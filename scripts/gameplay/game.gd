extends Control

const Economy = preload("res://scripts/progression/economy.gd")
const DrillSpec = preload("res://scripts/progression/drill.gd")
const DigEngine = preload("res://scripts/gameplay/dig_engine.gd")
const SaveStore = preload("res://scripts/save/save_store.gd")
const EquipmentSystem = preload("res://scripts/progression/equipment_system.gd")
const PhaseSystem = preload("res://scripts/progression/phase_system.gd")
const Layout = preload("res://scripts/gameplay/ui_layout.gd")
const CrewVisuals = preload("res://scripts/visual/expedition_visuals.gd")
const ArtDirection = preload("res://scripts/visual/art_direction.gd")
const TerrainArt = preload("res://scripts/visual/terrain_art.gd")
const ExpeditionTheme = preload("res://assets/ui/expedition_theme.tres")
const AUTOSAVE_INTERVAL := 1.0
const BIOME_PATH := "res://content/biomes/prado_cristalino.json"
const PHASE_PATH := "res://content/phases/catalog.json"

const BG := Color("#123448")
const INK := Color("#173746")
const PANEL := Color("#fff4d8")
const OUTLINE := Color("#17303d")
const TEAL := Color("#39d4c1")
const CORAL := Color("#f27c67")
const BRASS := Color("#f2b84b")

var biome: Dictionary
var state: Dictionary
var engine := DigEngine.new()
var drill := DrillSpec.new()
var equipment := EquipmentSystem.new()
var phase_system := PhaseSystem.new()
var phase_catalog: Dictionary = {}
var cell_size := 54.0
var camera_depth := 0
var charge := 0.0
var held := false
var attacking_boss := false
var camera_dragging := false
var pointer_start := Vector2.ZERO
var camera_start_depth := 0
var mining := false
var mine_target := Vector2i.ZERO
var fx: Array[Dictionary] = []
var toast := ""
var toast_time := 0.0
var show_album := false
var show_workshop := false
var menu_tab := "chests"
var gear_rarity := "Ruim"
var gear_page := 0
var character_view := "models"
var skin_page := 0
var show_start_screen := true
var has_existing_save := false
var press_scale := 0.0
var grid_origin_y := Layout.WORLD_TOP
var world_bottom := 0.0
var active_touch := -1
var app_suspended := false
var save_pending := false
var save_elapsed := 0.0
var save_failed := false
var panel_styles: Dictionary = {}
var crew
var art := ArtDirection.new()
var sale_float := 0.0
var sale_value := 0
var impact_left := 0.0
var radio_text := ""
var radio_left := 0.0
var last_radio := -30.0
var ui_press := Vector2(-1000, -1000)
var icon_cache: Dictionary = {}
var ui_styles: Dictionary = {}
var block_echoes: Array[Dictionary] = []
var loot_flash := 0.0
var upgrade_flash := 0.0

func _ready() -> void:
	get_tree().auto_accept_quit = false
	get_tree().quit_on_go_back = false
	theme = ExpeditionTheme
	texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	custom_minimum_size = Vector2(300, 560)
	crew = CrewVisuals.new()
	add_child(crew)
	var biome_file := FileAccess.open(BIOME_PATH, FileAccess.READ)
	biome = JSON.parse_string(biome_file.get_as_text())
	var phase_file := FileAccess.open(PHASE_PATH, FileAccess.READ)
	phase_catalog = JSON.parse_string(phase_file.get_as_text())
	state = SaveStore.load_data()
	has_existing_save = not state.is_empty()
	if state.is_empty():
		state = SaveStore.fresh()
	if not phase_system.valid_sequence(state.get("phase_sequence", [])):
		state.merge(phase_system.initial_state(), true)
	if not drill.equip(str(state.get("drill", "basic"))):
		state.drill = "basic"
		drill.equip("basic")
	if not equipment.skins.has(str(state.get("equipped_skin", "pioneira"))):
		state.equipped_skin = "pioneira"
	_apply_drill_skin()
	var dug_cells: Array = state.dug
	engine.setup(biome, 7042, dug_cells, state.get("damaged", {}), phase_catalog, state.phase_sequence)
	drill.level = maxi(1, int(state.get("drill_level", 1)))
	camera_depth = maxi(0, int(state.depth) - 8)
	world_bottom = size.y - Layout.FOOTER_HEIGHT
	set_process(true)
	queue_redraw()

func _notification(what: int) -> void:
	if state.is_empty():
		return
	if what == NOTIFICATION_APPLICATION_PAUSED or what == NOTIFICATION_APPLICATION_FOCUS_OUT:
		_set_app_active(false)
	elif what == NOTIFICATION_APPLICATION_RESUMED or what == NOTIFICATION_APPLICATION_FOCUS_IN:
		_set_app_active(true)
	elif what == NOTIFICATION_WM_GO_BACK_REQUEST:
		if not _navigate_back():
			get_tree().quit()
	elif what == NOTIFICATION_WM_CLOSE_REQUEST:
		_commit_save()
		get_tree().quit()

func _set_app_active(active: bool) -> void:
	app_suspended = not active
	if is_instance_valid(crew):
		crew.suspend(not active)
	_cancel_pointer()
	if not active:
		_commit_save()

func _cancel_pointer() -> void:
	held = false
	attacking_boss = false
	camera_dragging = false
	charge = 0.0
	active_touch = -1
	ui_press = Vector2(-1000, -1000)
	queue_redraw()

func _release_pointer() -> void:
	_cancel_pointer()
	if save_pending:
		_commit_save()

func _navigate_back() -> bool:
	_release_pointer()
	_commit_save()
	if show_workshop:
		show_workshop = false
	elif show_album:
		show_album = false
	elif not show_start_screen:
		show_start_screen = true
	else:
		return false
	queue_redraw()
	return true

func _request_save() -> void:
	if not save_pending:
		save_elapsed = 0.0
	save_pending = true

func _commit_save() -> bool:
	if state.is_empty():
		return false
	state.depth = maxi(int(state.depth), camera_depth)
	state.drill = drill.id
	state.drill_level = drill.level
	state.equipped_gear = state.get("equipped_gear", [])
	state.dug = engine.dug_keys()
	state.damaged = engine.durability.duplicate(true)
	if SaveStore.save_data(state):
		has_existing_save = true
		save_pending = false
		save_failed = false
		save_elapsed = 0.0
		return true
	save_pending = true
	save_elapsed = 0.0
	if not save_failed:
		_toast("Não foi possível salvar. Verifique o espaço do aparelho.")
		queue_redraw()
	save_failed = true
	return false

func _process(delta: float) -> void:
	if app_suspended:
		return
	if save_pending:
		save_elapsed += delta
		if save_elapsed >= AUTOSAVE_INTERVAL:
			_commit_save()
	var previous_environment: int = art.target
	art.update(delta, int(state.depth) * 2, phase_system.current_id(state))
	if previous_environment != art.target and not show_start_screen:
		if art.target == 4:
			_radio("Lia", "Isso não parece uma caverna comum...")
		else:
			_radio("Bento", "Rocha nova. Vamos com calma.")
	radio_left = maxf(0, radio_left - delta)
	sale_float = maxf(0, sale_float - delta)
	impact_left = maxf(0, impact_left - delta)
	var changed := true
	if loot_flash > 0.0 or upgrade_flash > 0.0:
		loot_flash = maxf(0, loot_flash - delta)
		upgrade_flash = maxf(0, upgrade_flash - delta)
		changed = true
	for i in range(block_echoes.size() - 1, -1, -1):
		block_echoes[i].life -= delta
		if block_echoes[i].life <= 0:
			block_echoes.remove_at(i)
		changed = true
	if held and not camera_dragging and not show_start_screen and not show_album and not show_workshop:
		var boosts := equipment.bonuses(state.get("equipped_gear", []), str(state.get("equipped_skin", "pioneira")))
		var dig_interval := clampf(0.17 - float(boosts.speed), 0.07, 0.42)
		charge += delta
		if charge >= dig_interval:
			charge -= dig_interval
			if attacking_boss and _boss_is_active():
				_damage_boss()
			else:
				_perform_dig(mine_target)
			changed = true
	if toast_time > 0.0:
		toast_time -= delta
		changed = true
	if not fx.is_empty():
		for i in range(fx.size() - 1, -1, -1):
			var particle: Dictionary = fx[i]
			particle.p += particle.velocity * delta
			particle.velocity.y += 280.0 * delta
			particle.angle = float(particle.angle) + float(particle.spin) * delta
			particle.life -= delta
			if particle.life <= 0.0:
				fx.remove_at(i)
			changed = true
	if changed:
		queue_redraw()

func _input(event: InputEvent) -> void:
	if app_suspended:
		return
	if event is InputEventKey and event.pressed and not event.echo and event.keycode == KEY_ESCAPE:
		if not _navigate_back():
			get_tree().quit()
		get_viewport().set_input_as_handled()
	elif event is InputEventScreenTouch:
		if event.canceled:
			if event.index == active_touch:
				_release_pointer()
		elif event.pressed and active_touch == -1:
			active_touch = event.index
			_press_at(_local_pointer(event.position))
		elif not event.pressed and event.index == active_touch:
			_release_pointer()
		get_viewport().set_input_as_handled()
	elif event is InputEventScreenDrag and event.index == active_touch and held:
		_track_pointer(_local_pointer(event.position))
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and active_touch == -1:
		if event.pressed:
			_press_at(_local_pointer(event.position))
		else:
			_release_pointer()
		get_viewport().set_input_as_handled()
	elif event is InputEventMouseMotion and held and active_touch == -1:
		_track_pointer(_local_pointer(event.position))

func _local_pointer(viewport_position: Vector2) -> Vector2:
	return get_global_transform_with_canvas().affine_inverse() * viewport_position

func _press_at(pos: Vector2) -> void:
	ui_press = pos
	queue_redraw()
	if not Rect2(Vector2.ZERO, size).has_point(pos):
		return
	if show_start_screen:
		if Layout.button("start", size).has_point(pos):
			_start_game()
	elif show_workshop:
		_menu_press(pos)
	elif show_album:
		if Layout.button("back", size).has_point(pos):
			show_album = false
			queue_redraw()
	else:
		_on_press(pos)

func _on_press(pos: Vector2) -> void:
	held = false
	attacking_boss = false
	camera_dragging = false
	pointer_start = pos
	camera_start_depth = camera_depth
	for i in range(3):
		if Layout.tab(i, size).has_point(pos):
			show_workshop = true
			menu_tab = ["chests", "equipment", "character"][i]
			queue_redraw()
			return
	if Layout.button("sell", size).has_point(pos):
		_sell()
	elif Layout.button("upgrade", size).has_point(pos):
		_upgrade()
	elif Layout.button("album", size).has_point(pos):
		show_album = true
	elif _in_world(pos) and _boss_is_active() and _boss_hit_rect().has_point(pos):
		held = true
		attacking_boss = true
		charge = 0.0
		_damage_boss()
	elif _in_world(pos):
		mine_target = _cell_at(pos)
		held = true
		charge = 0.0
		_perform_dig(mine_target)
	queue_redraw()

func _track_pointer(pos: Vector2) -> void:
	var movement := pos - pointer_start
	if not camera_dragging and absf(movement.y) >= 30.0 and absf(movement.y) > absf(movement.x) * 1.25:
		camera_dragging = true
		attacking_boss = false
		charge = 0.0
	if camera_dragging:
		var visible_rows := maxi(1, floori((world_bottom - grid_origin_y) / cell_size))
		var minimum := phase_system.start_depth(state)
		var maximum := maxi(minimum, int(state.depth) - visible_rows + 2)
		var drag_rows := roundi(-movement.y / cell_size)
		camera_depth = clampi(camera_start_depth + drag_rows, minimum, maximum)
		queue_redraw()
	elif _in_world(pos):
		mine_target = _cell_at(pos)
	else:
		held = false
		charge = 0.0

func _in_world(pos: Vector2) -> bool:
	return pos.x >= 0 and pos.x < size.x and pos.y >= grid_origin_y and pos.y < world_bottom

func _cell_at(pos: Vector2) -> Vector2i:
	var col := floori((pos.x - (size.x * 0.5 - cell_size * 0.5)) / cell_size)
	var row := maxi(0, floori((pos.y - grid_origin_y) / cell_size) + camera_depth)
	return Vector2i(clampi(col, -4, 4), row)

func _perform_dig(cell: Vector2i) -> void:
	var boosts := equipment.bonuses(state.get("equipped_gear", []), str(state.get("equipped_skin", "pioneira")))
	var result := engine.dig(drill.pattern, cell.x, cell.y, maxi(1, drill.level + int(boosts.power)), int(boosts.reward), phase_system.end_depth(state))
	if result.worked > 0 or result.broken > 0:
		impact_left = 0.10
		if is_instance_valid(crew):
			crew.mining_event(result.broken > 0)
	if result.broken <= 0:
		if result.worked > 0:
			var hit_layer: Dictionary = engine.layer_for_cell(cell.y, cell.x)
			_spawn_particles(_cell_center(cell), 2, Color(str(hit_layer.get("color", "#c98964"))).darkened(0.12))
			_request_save()
		return
	state.inventory += int(result.coins)
	for target in result.cells:
		var old_depth := int(state.depth)
		state.depth = maxi(old_depth, target.y + 1)
		var broken_layer: Dictionary = engine.layer_for_cell(target.y, target.x)
		if str(broken_layer.get("name", "")).to_lower().contains("cristal"):
			_radio("Lia", "Olha esse brilho. Tem mais coisa aqui.")
		_spawn_particles(_cell_center(target), 4, Color(str(broken_layer.get("color", "#c98964"))))
		block_echoes.append({"cell": target, "life": 0.22, "color": Color(str(broken_layer.color))})
		if block_echoes.size() > 12:
			block_echoes.pop_front()
		_collect_phase_marker(target)
		var active_phase: Dictionary = phase_system.phases.get(phase_system.phase_id_at(state, target.y), {})
		var discovery: Dictionary = active_phase.get("discovery", biome.discovery)
		if not state.discoveries.has(str(discovery.id)) and posmod(target.y, phase_system.cells_per_phase) >= int(discovery.get("depth", biome.discovery.depth)):
			state.discoveries.append(str(discovery.id))
			_toast("Descoberta: %s!" % discovery.get("name", "Novo achado"))
	var visible_rows := maxi(1, floori((world_bottom - grid_origin_y) / cell_size))
	camera_depth = maxi(camera_depth, state.depth - visible_rows + 2)
	_request_save()
	queue_redraw()

func _start_game() -> void:
	show_start_screen = false
	queue_redraw()

func _marker_save_id(phase_index: int, kind: String, cell: Vector2i) -> String:
	return "%d:%s:%d:%d" % [phase_index, kind, cell.x, cell.y]

func _has_phase_marker(phase: Dictionary, kind: String, phase_index: int, cell: Vector2i) -> bool:
	var marker_key := "key_cells" if kind == "key" else "chest_cells"
	for raw_cell in phase.get(marker_key, []):
		if raw_cell is Array and raw_cell.size() == 2 and int(raw_cell[0]) == cell.x and int(raw_cell[1]) == posmod(cell.y, phase_system.cells_per_phase):
			return not state.found_markers.has(_marker_save_id(phase_index, kind, cell))
	return false

func _collect_phase_marker(cell: Vector2i) -> void:
	var phase_index := floori(float(cell.y) / float(phase_system.cells_per_phase))
	var phase_id := phase_system.phase_id_at(state, cell.y)
	var phase: Dictionary = phase_system.phases.get(phase_id, {})
	for kind in ["key", "chest"]:
		if not _has_phase_marker(phase, kind, phase_index, cell):
			continue
		state.found_markers.append(_marker_save_id(phase_index, kind, cell))
		if kind == "key":
			state.keys = int(state.get("keys", 0)) + 1
			_toast("Chave antiga encontrada! Abra um baú.")
		else:
			state.chests = int(state.get("chests", 0)) + 1
			_toast("Baú escondido encontrado! Procure uma chave.")

func _active_phase() -> Dictionary:
	return phase_system.current(state)

func _boss_is_active() -> bool:
	return int(state.get("boss_health", 0)) > 0 and int(state.depth) >= phase_system.end_depth(state)

func _boss_center() -> Vector2:
	var y := grid_origin_y + (float(phase_system.end_depth(state) - camera_depth) + 0.5) * cell_size
	return Vector2(size.x * 0.5, y)

func _boss_hit_rect() -> Rect2:
	var center := _boss_center()
	return Rect2(center.x - 62.0, center.y - 50.0, 124.0, 100.0)

func _damage_boss() -> void:
	if not _boss_is_active():
		return
	var phase := _active_phase()
	var boss: Dictionary = phase.get("boss", {})
	var boosts := equipment.bonuses(state.get("equipped_gear", []), str(state.get("equipped_skin", "pioneira")))
	var damage := maxi(1, drill.level + int(boosts.power) + floori(float(drill.pattern.size() - 1) / 2.0))
	state.boss_health = maxi(0, int(state.boss_health) - damage)
	_spawn_particles(_boss_center(), 3)
	if int(state.boss_health) == 0:
		state.coins = int(state.coins) + int(boss.get("coins", 50))
		state.bosses_defeated = int(state.get("bosses_defeated", 0)) + 1
		var previous_id := phase_system.current_id(state)
		var next_id := phase_system.next_id(int(state.phase_index), previous_id, 7042 + int(state.depth))
		state.phase_index = int(state.phase_index) + 1
		state.phase_sequence.append(next_id)
		engine.phase_sequence = state.phase_sequence
		state.boss_health = int(phase_system.phases.get(next_id, {}).get("boss", {}).get("health", 12))
		state.key_milestones = 0
		state.chest_milestones = 0
		camera_depth = maxi(camera_depth, int(state.depth) - 8)
		held = false
		attacking_boss = false
		_toast("Chefe derrotado! +%d moedas · %s" % [boss.get("coins", 50), phase_system.phases.get(next_id, {}).get("name", "Nova fase")])
	else:
		_toast("%s · %d/%d" % [boss.get("name", "Chefe"), state.boss_health, boss.get("health", 1)])
	_commit_save()
	queue_redraw()

func _apply_drill_skin() -> void:
	var skin_id := str(state.get("equipped_skin", "pioneira"))
	if equipment.skins.has(skin_id):
		drill.accent = Color(str(equipment.skins[skin_id].color))

func _menu_press(pos: Vector2) -> void:
	for i in range(3):
		if Layout.tab(i, size, true).has_point(pos):
			menu_tab = ["chests", "equipment", "character"][i]
			queue_redraw()
			return
	if Layout.button("back", size).has_point(pos):
		show_workshop = false
	elif menu_tab == "chests" and Layout.button("chest", size).has_point(pos):
		_open_chest()
	elif menu_tab == "equipment":
		var rarities := ["Ruim", "Boa", "Épica", "Lendária", "Única", "Mística"]
		for i in range(rarities.size()):
			if Layout.rarity(i, size).has_point(pos):
				gear_rarity = rarities[i]
				gear_page = 0
		var ids := equipment.gear_ids_for_rarity(gear_rarity)
		var last_page := maxi(0, ceili(float(ids.size()) / Layout.GEAR_PER_PAGE) - 1)
		if Layout.page(false, size).has_point(pos):
			gear_page = maxi(0, gear_page - 1)
		elif Layout.page(true, size).has_point(pos):
			gear_page = mini(last_page, gear_page + 1)
		else:
			for row in range(Layout.GEAR_PER_PAGE):
				var index := gear_page * Layout.GEAR_PER_PAGE + row
				if index < ids.size() and Layout.item(row, size, true).has_point(pos):
					if state.gear_inventory.has(ids[index]):
						_equip_gear(ids[index])
					else:
						_toast("Esse achado ainda está escondido em um baú.")
	elif menu_tab == "character":
		if Layout.button("models", size).has_point(pos) or Layout.button("skins", size).has_point(pos):
			character_view = "models" if Layout.button("models", size).has_point(pos) else "skins"
			skin_page = 0
		else:
			var ids: Array = _character_ids()
			var last_page := maxi(0, ceili(float(ids.size()) / Layout.CHARACTER_PER_PAGE) - 1)
			if Layout.page(false, size).has_point(pos):
				skin_page = maxi(0, skin_page - 1)
			elif Layout.page(true, size).has_point(pos):
				skin_page = mini(last_page, skin_page + 1)
			else:
				for row in range(Layout.CHARACTER_PER_PAGE):
					var index := skin_page * Layout.CHARACTER_PER_PAGE + row
					if index < ids.size() and Layout.item(row, size).has_point(pos):
						if character_view == "models":
							_equip_drill(str(ids[index]))
						else:
							_equip_skin(str(ids[index]))
	queue_redraw()

func _character_ids() -> Array:
	if character_view == "skins":
		return equipment.skin_ids()
	var ids: Array = equipment.drills.keys()
	ids.sort()
	return ids

func _open_chest() -> void:
	if int(state.get("chests", 0)) < 1:
		_toast("Continue escavando para encontrar um baú.")
		return
	if int(state.get("keys", 0)) < 1:
		_toast("Baú guardado! Encontre uma chave para abrir.")
		return
	var boosts := equipment.bonuses(state.get("equipped_gear", []), str(state.get("equipped_skin", "pioneira")))
	var result: Dictionary = equipment.open_chest(state, int(state.depth), 7042, int(boosts.luck))
	if result.ok:
		loot_flash = 1.1
		var loot: Dictionary = result.item
		if loot.duplicate:
			_toast("%s repetido · +%d moedas" % [loot.name, loot.coins])
		else:
			_toast("%s: %s!" % [loot.rarity, loot.name])
		_commit_save()

func _equip_drill(drill_id: String) -> void:
	if state.drill_inventory.has(drill_id) and drill.equip(drill_id):
		state.drill = drill_id
		_apply_drill_skin()
		_toast("Broca-personagem equipada: %s" % drill.title)
		_commit_save()

func _equip_skin(skin_id: String) -> void:
	if state.skin_inventory.has(skin_id) and equipment.skins.has(skin_id):
		state.equipped_skin = skin_id
		_apply_drill_skin()
		_toast("Nova aparência e habilidade: %s" % equipment.skins[skin_id].name)
		_commit_save()
	else:
		_toast("Essa aparência pode aparecer em um baú.")

func _equip_gear(gear_id: String) -> void:
	var spec: Dictionary = equipment.gear[gear_id]
	var equipped: Array = state.get("equipped_gear", []).duplicate()
	if equipped.has(gear_id):
		equipped.erase(gear_id)
	else:
		for current_id in equipped.duplicate():
			if str(equipment.gear.get(current_id, {}).get("slot", "")) == str(spec.slot):
				equipped.erase(current_id)
		equipped.append(gear_id)
	state.equipped_gear = equipped
	_toast("Equipamento atualizado: %s" % spec.name)
	_commit_save()

func _cell_center(cell: Vector2i) -> Vector2:
	return Vector2(size.x * 0.5 + cell.x * cell_size, grid_origin_y + (cell.y - camera_depth + 0.5) * cell_size)

func _spawn_particles(origin: Vector2, count: int, material_color: Color = Color("#ca8052")) -> void:
	var remaining := 26 - fx.size()
	for i in range(mini(count * 3, remaining)):
		var shade := material_color.lightened(randf_range(0.08, 0.28))
		fx.append({"p": origin, "velocity": Vector2(randf_range(-115.0, 115.0), randf_range(-190.0, -45.0)), "life": randf_range(0.25, 0.48), "color": shade, "radius": randf_range(2.0, 4.5), "angle": randf_range(-PI, PI), "spin": randf_range(-8.0, 8.0)})

func _sell() -> void:
	var amount := int(state.inventory)
	if amount <= 0:
		_toast("A mochila ainda está vazia.")
		return
	var first_sale_bonus := 8 if not state.get("first_sale_bonus_claimed", false) else 0
	sale_value = Economy.sell_coins(amount, first_sale_bonus)
	sale_float = 0.8
	state.coins += Economy.sell_coins(amount, first_sale_bonus)
	state.inventory = 0
	if first_sale_bonus > 0:
		state.first_sale_bonus_claimed = true
		_toast("Primeira venda! +%d moedas · bônus +%d" % [amount, first_sale_bonus])
	else:
		_toast("Venda feita: +%d moedas" % amount)
	_commit_save()

func _upgrade() -> void:
	if drill.id == "basic":
		if state.get("drill_inventory", []).has("prism"):
			drill.equip("prism")
			state.drill = drill.id
			_toast("Broca Prisma equipada! Agora abre 3 túneis.")
			upgrade_flash = 0.8
			_commit_save()
			return
		var cost := 30
		if not Economy.can_afford(int(state.coins), cost):
			_toast("Faltam %d moedas para a Broca Prisma." % (cost - int(state.coins)))
			return
		state.coins = Economy.buy(int(state.coins), cost)
		drill.transform_to_prism()
		_apply_drill_skin()
		state.drill_inventory.append("prism")
		state.drill = drill.id
		_toast("Broca Prisma! Agora abre 3 túneis.")
	else:
		var cost := 18 + (drill.level - 1) * 16
		if not Economy.can_afford(int(state.coins), cost):
			_toast("Faltam %d moedas para reforçar a broca." % (cost - int(state.coins)))
			return
		state.coins = Economy.buy(int(state.coins), cost)
		drill.level += 1
		_toast("Broca reforçada: quebra pedra mais resistente.")
	upgrade_flash = 0.8
	_commit_save()

func _toast(message: String) -> void:
	toast = message
	toast_time = 2.4

func _radio(speaker: String, message: String) -> void:
	if art.clock - last_radio < 30.0:
		return
	last_radio = art.clock
	radio_text = speaker + ": " + message
	radio_left = 3.0

func _draw() -> void:
	var s := size
	var mode := "mining"
	if show_album:
		mode = "hidden"
	elif show_workshop:
		mode = "character" if menu_tab == "character" else "workshop"
	elif show_start_screen:
		mode = "start"
	crew.present(mode, s, Layout.WORLD_TOP, drill.id, drill.accent)
	_draw_background(s)
	if show_album:
		_draw_album(s)
	elif show_workshop:
		_draw_workshop(s)
	elif show_start_screen:
		_draw_start_screen(s)
	else:
		_draw_hud(s)
		var shake := Vector2(sin(impact_left * 190) * impact_left * 15, cos(impact_left * 150) * impact_left * 9)
		draw_set_transform(shake)
		_draw_world(s)
		draw_set_transform(Vector2.ZERO)
		_draw_controls(s)
	_draw_particles()
	if sale_float > 0 and not show_workshop and not show_album:
		var y := s.y - 166 - (0.8 - sale_float) * 42
		_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.27, y), "+%d moedas" % sale_value, HORIZONTAL_ALIGNMENT_CENTER, 200, 20, Color(BRASS, minf(1, sale_float * 3)))
	if not show_workshop and not show_album and not show_start_screen:
		for echo in block_echoes:
			var k := float(echo.life) / 0.22
			var center := _cell_center(echo.cell)
			if center.y >= grid_origin_y and center.y < world_bottom:
				draw_arc(center, (1 - k) * 30 + 7, 0, TAU, 20, Color(echo.color, k), 3 * k, true)
	if toast_time > 0.0:
		_draw_panel(Rect2(12, s.y - 310, s.x - 24, 64), INK, 16)
		_wrapped_text(Rect2(24, s.y - 299, s.x - 48, 48), toast, 16, PANEL)

func _draw_background(s: Vector2) -> void:
	art.background(self, s, camera_depth)
	# A translucent visor supports the compact HUD without hiding the environment.
	draw_rect(Rect2(0, 0, s.x, 153), Color("#0B202D", 0.42))

func _draw_hud(s: Vector2) -> void:
	_draw_panel(Rect2(16, 14, 145, 50), Color("#204A58"), 17, true)
	_icon("coin", Rect2(24, 21, 35, 35))
	draw_string(ThemeDB.fallback_font, Vector2(65, 47), str(state.coins), HORIZONTAL_ALIGNMENT_LEFT, 82, 24, PANEL)
	_draw_button(Layout.button("album", s), Color("#204A58"), "ÁLBUM  %d/%d" % [state.discoveries.size(), phase_system.phases.size()], PANEL)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 91), str(_active_phase().get("name", "Mina")).to_upper(), HORIZONTAL_ALIGNMENT_CENTER, s.x - 38, 18, PANEL)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 113), "%d m  /  EXPEDIÇÃO %02d" % [int(state.depth) * 2, int(state.phase_index) + 1], HORIZONTAL_ALIGNMENT_CENTER, s.x - 38, 12, TEAL)
	_draw_panel(Rect2(28, 120, s.x - 56, 24), Color("#204A58"), 10)
	var message := radio_text if radio_left > 0 and toast_time <= 0 else _goal_text()
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 136), message, HORIZONTAL_ALIGNMENT_CENTER, s.x - 68, 10, Color("#B7FFF1"))

func _goal_text() -> String:
	if _boss_is_active():
		var boss: Dictionary = _active_phase().get("boss", {})
		return "CHEFE: %s · TOQUE E SEGURE PARA ATACAR" % boss.get("name", "Guardião")
	var remaining_to_boss := maxi(0, phase_system.end_depth(state) - int(state.depth)) * 2
	if drill.id == "basic":
		var progress_coins := int(state.coins) + int(state.inventory)
		if progress_coins >= 30:
			if int(state.coins) >= 30:
				return "OBJETIVO: TRANSFORME A BROCA POR 30 ◉"
			return "OBJETIVO: VENDA PEDRAS E TRANSFORME A BROCA"
		if int(state.inventory) > 0:
			return "OBJETIVO: VENDA AS PEDRAS · FALTAM %d ◉" % (30 - progress_coins)
		if int(state.coins) > 0:
			return "OBJETIVO: JUNTE %d ◉ PARA A BROCA PRISMA" % (30 - int(state.coins))
		return "OBJETIVO: JUNTE 30 ◉ PARA A BROCA PRISMA"
	var phase_discovery: Dictionary = _active_phase().get("discovery", biome.discovery)
	var discovery_id := str(phase_discovery.get("id", biome.discovery.id))
	if not state.discoveries.has(discovery_id):
		var remaining_depth := maxi(0, int(phase_discovery.get("depth", 16)) - (int(state.depth) - phase_system.start_depth(state))) * 2
		return "DESCUBRA %s · %d m" % [str(phase_discovery.get("name", "UM ACHADO")).to_upper(), remaining_depth]
	var next_cost := 18 + (drill.level - 1) * 16
	if int(state.coins) >= next_cost:
		return "REFORÇO DISPONÍVEL · %d m ATÉ O CHEFE" % remaining_to_boss
	return "JUNTE %d ◉ · %d m ATÉ O CHEFE" % [next_cost, remaining_to_boss]

func _draw_start_screen(s: Vector2) -> void:
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 83), "EXPEDIÇÃO SUBTERRÂNEA", HORIZONTAL_ALIGNMENT_CENTER, s.x - 56, 12, Color("#d6bd81"))
	draw_circle(Vector2(s.x * 0.5, 287), 117, Color("#1e5a64", 0.82))
	draw_circle(Vector2(s.x * 0.5, 282), 88, Color("#2d8f83", 0.82))
	draw_arc(Vector2(s.x * 0.5, 282), 102, 0.3, 3.05, 36, Color("#edbd61"), 3.0, true)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 445), "DIG & WONDER", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 37, Color("#fff3d4"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 478), "Pequenas pistas. Grandes descobertas.", HORIZONTAL_ALIGNMENT_CENTER, s.x - 42, 15, Color("#b9d9cf"))
	_draw_pill(Rect2(31, 516, s.x - 62, 68), Color("#f6edda"), 18)
	draw_circle(Vector2(59, 550), 11, TEAL)
	draw_circle(Vector2(59, 550), 5, Color("#f6edda"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5 + 23, 548), "PRADO CRISTALINO", HORIZONTAL_ALIGNMENT_CENTER, s.x - 112, 15, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5 + 23, 568), "Sua primeira expedição", HORIZONTAL_ALIGNMENT_CENTER, s.x - 112, 11, Color("#5d7778"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 637), "Segure para escavar", HORIZONTAL_ALIGNMENT_CENTER, s.x - 60, 20, Color("#fff3d4"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 665), "Arraste para descobrir o que vem depois", HORIZONTAL_ALIGNMENT_CENTER, s.x - 60, 13, Color("#b9d9cf"))
	var button_label := "CONTINUAR" if has_existing_save else "INICIAR ESCAVAÇÃO"
	_draw_button(Layout.button("start", s), BRASS, button_label, Color("#3b2b1c"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 77), "PROGRESSO SALVO AUTOMATICAMENTE", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 11, Color("#bed2c6"))

func _draw_world(s: Vector2) -> void:
	var top := Layout.WORLD_TOP
	var bottom := s.y - Layout.FOOTER_HEIGHT
	grid_origin_y = top
	world_bottom = bottom
	var phase := _active_phase()
	var rock_color := Color(str(phase.get("rock", "#826277")))
	draw_rect(Rect2(23, top, s.x - 46, bottom - top), Color("#17303D"))
	draw_line(Vector2(28, top - 3), Vector2(s.x - 28, top - 3), Color("#9D603D"), 5, true)
	var visible_rows := ceili((bottom - top) / cell_size) + 1
	for screen_row in range(visible_rows):
		var depth := camera_depth + screen_row
		if depth >= phase_system.end_depth(state):
			continue
		for col in range(-4, 5):
			var x := s.x * 0.5 - cell_size * 0.5 + float(col) * cell_size
			var y := top + float(screen_row) * cell_size
			var rect := Rect2(x, y, cell_size, cell_size)
			if engine.is_dug(col, depth):
				draw_rect(rect, Color("#192f3e"))
				draw_line(Vector2(x + 6, y + cell_size - 4), Vector2(x + cell_size - 7, y + cell_size - 4), Color("#20404D"), 1.5, true)
			else:
				var layer: Dictionary = engine.layer_for_cell(depth, col)
				var damage_key := "%d:%d" % [col, depth]
				TerrainArt.draw_cell(self, rect, layer, col, depth, int(engine.durability.get(damage_key, 0)))
				var phase_index := floori(float(depth) / float(phase_system.cells_per_phase))
				var phase_id := phase_system.phase_id_at(state, depth)
				var marker_phase: Dictionary = phase_system.phases.get(phase_id, {})
				var discovery: Dictionary = marker_phase.get("discovery", biome.discovery)
				if posmod(depth, phase_system.cells_per_phase) >= int(discovery.get("depth", biome.discovery.depth)) and col == 0 and posmod(depth, 7) == 0:
					draw_colored_polygon(PackedVector2Array([Vector2(x + 19, y + 36), Vector2(x + 26, y + 11), Vector2(x + 35, y + 34)]), Color("#9affed"))
				var cell := Vector2i(col, depth)
				if _has_phase_marker(marker_phase, "key", phase_index, cell):
					_draw_marker_icon(Vector2(x + cell_size * 0.5, y + cell_size * 0.5), "key")
				elif _has_phase_marker(marker_phase, "chest", phase_index, cell):
					_draw_marker_icon(Vector2(x + cell_size * 0.5, y + cell_size * 0.5), "chest")
	if held and not camera_dragging and not attacking_boss:
		var selected := Rect2(_cell_center(mine_target) - Vector2.ONE * cell_size * 0.5, Vector2.ONE * cell_size)
		if selected.position.y >= top and selected.end.y <= bottom:
			draw_rect(selected.grow(-2), Color("#B7FFF1"), false, 2, true)
			if not engine.is_dug(mine_target.x, mine_target.y):
				var contact := selected.get_center()
				var shaft := PackedVector2Array([Vector2(s.x * 0.5, top - 13), Vector2(s.x * 0.5, top + 6), contact])
				draw_polyline(shaft, OUTLINE, 8, true)
				draw_polyline(shaft, TEAL, 3, true)
	_draw_pill(Rect2(s.x - 112, top + 8, 96, 32), Color("#fff2d0"), 14)
	draw_string(ThemeDB.fallback_font, Vector2(s.x - 105, top + 29), "◈ %d" % state.inventory, HORIZONTAL_ALIGNMENT_CENTER, 82, 14, INK)
	if _boss_is_active():
		var boss_center := _boss_center()
		if boss_center.y >= top + 50.0 and boss_center.y <= bottom - 50.0:
			_draw_boss(boss_center)

func _draw_marker_icon(center: Vector2, kind: String) -> void:
	if kind == "key":
		draw_circle(center + Vector2(-5, -2), 7, Color("#fff0a1"))
		draw_circle(center + Vector2(-5, -2), 3, Color("#705528"))
		draw_line(center + Vector2(1, 3), center + Vector2(12, 14), Color("#fff0a1"), 4.0, true)
		draw_line(center + Vector2(8, 11), center + Vector2(5, 14), Color("#fff0a1"), 3.0, true)
	else:
		draw_rect(Rect2(center.x - 13, center.y - 9, 26, 20), Color("#f2b74c"), true)
		draw_rect(Rect2(center.x - 14, center.y - 3, 28, 5), Color("#fff0a1"), true)
		draw_rect(Rect2(center.x - 3, center.y - 1, 6, 11), Color("#8a5b2e"), true)

func _draw_boss(center: Vector2) -> void:
	var boss: Dictionary = _active_phase().get("boss", {})
	var color := Color(str(boss.get("color", "#e89b53")))
	var shape := str(boss.get("shape", "armored"))
	# Silhueta em primitivas leves: olhos grandes e formas diferentes por guardião.
	if shape == "cosmic":
		for angle in range(8):
			var direction := Vector2.RIGHT.rotated(float(angle) * TAU / 8.0)
			draw_colored_polygon(PackedVector2Array([center + direction * 28.0, center + direction * 55.0, center + direction.rotated(0.35) * 31.0]), color.lightened(0.12))
	else:
		draw_colored_polygon(PackedVector2Array([center + Vector2(-36, -16), center + Vector2(-52, -48), center + Vector2(-12, -32)]), color.darkened(0.10))
		draw_colored_polygon(PackedVector2Array([center + Vector2(36, -16), center + Vector2(52, -48), center + Vector2(12, -32)]), color.darkened(0.10))
	draw_circle(center + Vector2(0, 7), 39, Color("#294456"))
	draw_circle(center, 37, color)
	if shape == "claw":
		for side in [-1.0, 1.0]:
			draw_line(center + Vector2(side * 27, 12), center + Vector2(side * 57, 30), color.lightened(0.15), 8.0, true)
			draw_line(center + Vector2(side * 57, 30), center + Vector2(side * 44, 45), color.lightened(0.15), 6.0, true)
	elif shape == "horned":
		draw_colored_polygon(PackedVector2Array([center + Vector2(-20, -25), center + Vector2(-28, -58), center + Vector2(-4, -34)]), Color("#fff0b2"))
		draw_colored_polygon(PackedVector2Array([center + Vector2(20, -25), center + Vector2(28, -58), center + Vector2(4, -34)]), Color("#fff0b2"))
	draw_circle(center + Vector2(-12, -4), 6, Color.WHITE)
	draw_circle(center + Vector2(12, -4), 6, Color.WHITE)
	draw_circle(center + Vector2(-11, -3), 2.5, INK)
	draw_circle(center + Vector2(13, -3), 2.5, INK)
	draw_arc(center + Vector2(0, 0), 12, 0.35, 2.8, 10, INK, 2.4, true)
	var bar := Rect2(center.x - 62, center.y + 43, 124, 8)
	_draw_panel(bar, PANEL, 4)
	var ratio := clampf(float(state.boss_health) / maxf(1.0, float(boss.get("health", 1))), 0.0, 1.0)
	_draw_panel(Rect2(bar.position, Vector2(bar.size.x * ratio, bar.size.y)), CORAL, 4)
	_centered_string(ThemeDB.fallback_font, Vector2(center.x, center.y - 49), str(boss.get("name", "CHEFE")), HORIZONTAL_ALIGNMENT_CENTER, 200, 13, Color("#fff4d8"))

func _draw_controls(s: Vector2) -> void:
	_draw_panel(Rect2(0, s.y - Layout.FOOTER_HEIGHT, s.x, Layout.FOOTER_HEIGHT + 12), Color("#17303D"), 24)
	_icon("bag", Rect2(21, s.y - 169, 31, 31))
	draw_string(ThemeDB.fallback_font, Vector2(60, s.y - 147), "MOCHILA  /  %d MOEDAS" % state.inventory, HORIZONTAL_ALIGNMENT_LEFT, s.x - 95, 14, PANEL)
	var sell_rect := Layout.button("sell", s)
	_action(sell_rect, "coin", "VENDER PEDRAS", "+%d moedas" % state.inventory, BRASS, int(state.inventory) > 0)
	var cost := 18 + (drill.level - 1) * 16
	var title := "REFORÇAR NV.%d" % drill.level
	if drill.id == "basic":
		cost = 0 if state.get("drill_inventory", []).has("prism") else 30
		title = "EQUIPAR PRISMA" if cost == 0 else "BROCA PRISMA"
	_action(Layout.button("upgrade", s), "picareta", title, "%d moedas" % cost, TEAL, int(state.coins) >= cost)
	var labels := ["BAÚS %d" % state.get("chests", 0), "EQUIPAMENTOS", "PERSONAGEM"]
	for i in range(3):
		var r := Layout.tab(i, s)
		_draw_panel(r, Color("#244954"), 10)
		_icon(["chest", "gear", "drill"][i], Rect2(r.position + Vector2(8, 9), Vector2(30, 30)))
		draw_string(ThemeDB.fallback_font, r.position + Vector2(41, 32), labels[i], HORIZONTAL_ALIGNMENT_CENTER, r.size.x - 46, 11, PANEL)
	if upgrade_flash > 0:
		_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 185 - (0.8 - upgrade_flash) * 15), "BROCA MELHORADA!", HORIZONTAL_ALIGNMENT_CENTER, s.x - 40, 20, Color(TEAL, minf(1, upgrade_flash * 2)))

func _draw_album(s: Vector2) -> void:
	_draw_panel(Rect2(16, 174, s.x - 32, s.y - 202), PANEL, 24)
	var discovery: Dictionary = _active_phase().get("discovery", biome.discovery)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 218), "ÁLBUM DE DESCOBERTAS", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 20, INK)
	draw_circle(Vector2(s.x * 0.5, 310), 50, Color("#d5f2dd"))
	var found: bool = state.discoveries.has(str(discovery.get("id", biome.discovery.id)))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 327), "✦" if found else "?", HORIZONTAL_ALIGNMENT_CENTER, 65, 42, Color("#55bba5"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 408), str(discovery.get("name", "Achado")) if found else "Descoberta oculta", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 18, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 441), str(discovery.get("description", "Continue escavando para encontrar algo.")) if found else "Continue escavando para encontrar algo.", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 13, Color("#66808a"))
	_draw_button(Layout.button("back", s), BRASS, "VOLTAR À ESCAVAÇÃO", Color("#49351d"))

func _draw_workshop(s: Vector2) -> void:
	_draw_hud(s)
	draw_line(Vector2(26, 307), Vector2(s.x - 26, 307), BRASS, 5, true)
	_draw_panel(Rect2(8, 324, s.x - 16, s.y - 332), Color("#17303D"), 24, true)
	_draw_menu_tabs(s)
	match menu_tab:
		"equipment": _draw_equipment_menu(s)
		"character": _draw_character_menu(s)
		_: _draw_chest_menu(s)
	_draw_button(Layout.button("back", s), Color("#244954"), "VOLTAR À ESCAVAÇÃO", PANEL)

func _draw_menu_tabs(s: Vector2) -> void:
	for i in range(3):
		var chosen: bool = menu_tab == ["chests", "equipment", "character"][i]
		var r := Layout.tab(i, s, true)
		_draw_panel(r, Color("#244954") if chosen else Color("#17303D"), 7)
		_icon(["chest", "gear", "drill"][i], Rect2(r.position + Vector2(8, 10), Vector2(29, 29)))
		draw_string(ThemeDB.fallback_font, r.position + Vector2(39, 32), ["BAÚS", "EQUIPAMENTOS", "PERSONAGEM"][i], HORIZONTAL_ALIGNMENT_CENTER, r.size.x - 43, 11, BRASS if chosen else PANEL)
		if chosen:
			draw_line(r.position + Vector2(11, r.size.y - 3), r.end - Vector2(11, 3), BRASS, 4, true)

func _draw_chest_menu(s: Vector2) -> void:
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 418), "BAÚS DE DESCOBERTA", HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 22, PANEL)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 443), "%d baú(s) guardado(s)   /   %d chave(s)" % [state.get("chests", 0), state.get("keys", 0)], HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 14, TEAL)
	var pulse := sin(loot_flash * 18) * loot_flash * 5
	var center := Vector2(s.x * 0.5, 527)
	draw_circle(center, 76, Color("#244954"))
	draw_arc(center, 79, 0.2, 2.7, 30, BRASS, 2, true)
	if loot_flash > 0:
		for ray in range(10):
			var direction := Vector2.from_angle(float(ray) * TAU / 10 + loot_flash)
			draw_line(center + direction * 79, center + direction * (84 + loot_flash * 18), Color(BRASS, minf(1, loot_flash)), 3, true)
		var rarity := str(state.get("last_loot", {}).get("rarity", ""))
		if rarity in ["Lendária", "Única", "Mística"]:
			var age := 1.1 - loot_flash
			for fragment in range(16):
				var direction := Vector2.from_angle(float(fragment) * TAU / 16)
				var at := center + direction * age * 120 + Vector2(0, age * age * 65)
				draw_line(at, at + Vector2(4, 6).rotated(fragment + age * 4), Color(_rarity_color(rarity), minf(1, loot_flash * 2)), 3, true)
	_icon("chest", Rect2(center - Vector2(104, 74) - Vector2.ONE * pulse, Vector2(208 + pulse * 2, 148 + pulse * 2)))
	_icon("key", Rect2(center + Vector2(58, 22), Vector2(40, 40)))
	_wrapped_text(Rect2(24, 611, s.x - 48, 32), _chest_rule_text(), 11, TEAL)
	var rarities := ["Ruim", "Boa", "Épica", "Lendária", "Única", "Mística"]
	var odds := ["28%", "36%", "22%", "9%", "3,5%", "1,5%"]
	for i in range(6):
		var r := Rect2(26 + (i % 2) * (s.x * 0.5 - 16), 645 + (i / 2) * 29, s.x * 0.5 - 36, 25)
		_draw_panel(r, Color("#244954"), 7)
		draw_circle(r.position + Vector2(13, 12), 4, _rarity_color(rarities[i]))
		draw_string(ThemeDB.fallback_font, r.position + Vector2(24, 17), rarities[i] + "   " + odds[i], HORIZONTAL_ALIGNMENT_LEFT, r.size.x - 30, 11, PANEL)
	var loot: Dictionary = state.get("last_loot", {})
	var detail := "%s / %s" % [loot.get("rarity", "Próximo achado"), loot.get("kind", "Equipamentos, brocas e skins")]
	if bool(loot.get("duplicate", false)):
		detail += " / repetido: +%d moedas" % int(loot.get("coins", 0))
	_draw_item_row(Rect2(22, 739, s.x - 44, 54), str(loot.get("name", "Seu próximo achado está aqui")), detail, not loot.is_empty(), false, _rarity_color(str(loot.get("rarity", "Boa"))))
	var can_open := int(state.get("chests", 0)) > 0 and int(state.get("keys", 0)) > 0
	_action(Layout.button("chest", s), "key", "ABRIR BAÚ" if can_open else "BAÚ SELADO", "1 chave / chaves e baús aparecem cavando", BRASS, can_open)

func _draw_equipment_menu(s: Vector2) -> void:
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 408), "EQUIPAMENTOS  ·  %d/60" % state.get("gear_inventory", []).size(), HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 19, PANEL)
	var rarities: Array[String] = ["Ruim", "Boa", "Épica", "Lendária", "Única", "Mística"]
	for i in range(rarities.size()):
		_draw_button(Layout.rarity(i, s), TEAL if rarities[i] == gear_rarity else _rarity_color(rarities[i]).lightened(0.55), rarities[i], INK)
	var ids := equipment.gear_ids_for_rarity(gear_rarity)
	draw_string(ThemeDB.fallback_font, Vector2(24, 535), "Toque no item encontrado para equipar", HORIZONTAL_ALIGNMENT_LEFT, s.x - 48, 13, PANEL)
	for row in range(Layout.GEAR_PER_PAGE):
		var index := gear_page * Layout.GEAR_PER_PAGE + row
		if index >= ids.size():
			break
		var id := ids[index]
		var spec: Dictionary = equipment.gear[id]
		_draw_item_row(Layout.item(row, s, true), str(spec.name), "%s · %s · %s" % [spec.slot, spec.skill, spec.description], state.gear_inventory.has(id), state.equipped_gear.has(id), Color(str(spec.color)))
	_draw_pager(gear_page, ceili(float(ids.size()) / Layout.GEAR_PER_PAGE), s)

func _draw_character_menu(s: Vector2) -> void:
	var skin: Dictionary = equipment.skins.get(str(state.get("equipped_skin", "pioneira")), {})
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 418), "%s / NV.%d" % [drill.title.to_upper(), drill.level], HORIZONTAL_ALIGNMENT_CENTER, s.x - 42, 21, PANEL)
	_wrapped_text(Rect2(24, 432, s.x - 48, 34), "Inata: %s — %s" % [skin.get("skill", "Curiosa"), skin.get("description", "")], 12, TEAL)
	var stats := equipment.bonuses(state.get("equipped_gear", []), str(state.get("equipped_skin", "pioneira")))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 485), "Força %+d / moedas %+d / velocidade %+d%% / sorte %+d" % [stats.power, stats.reward, roundi(float(stats.speed) * 100.0), stats.luck], HORIZONTAL_ALIGNMENT_CENTER, s.x - 42, 12, PANEL)
	var models := character_view == "models"
	_draw_button(Layout.button("models", s), TEAL if models else Color("#244954"), "MODELOS", INK if models else PANEL)
	_draw_button(Layout.button("skins", s), TEAL if not models else Color("#244954"), "SKINS %d/13" % state.get("skin_inventory", []).size(), INK if not models else PANEL)
	var ids := _character_ids()
	for row in range(Layout.CHARACTER_PER_PAGE):
		var index := skin_page * Layout.CHARACTER_PER_PAGE + row
		if index >= ids.size(): break
		var id := str(ids[index])
		var spec: Dictionary = equipment.drills[id] if models else equipment.skins[id]
		var owned: bool = state.drill_inventory.has(id) if models else state.skin_inventory.has(id)
		var equipped: bool = state.drill == id if models else state.equipped_skin == id
		_draw_item_row(Layout.item(row, s), str(spec.name), "%s / %s" % [spec.rarity, spec.description], owned, equipped, Color(str(spec.color)))
	_draw_pager(skin_page, ceili(float(ids.size()) / Layout.CHARACTER_PER_PAGE), s)

func _draw_pager(page_index: int, count: int, s: Vector2) -> void:
	_draw_button(Layout.page(false, s), Color("#244954"), "%d/%d · ANTERIOR" % [page_index + 1, count], PANEL)
	_draw_button(Layout.page(true, s), Color("#244954"), "PRÓXIMOS" if page_index + 1 < count else "ÚLTIMA PÁGINA", PANEL)

func _rarity_color(rarity: String) -> Color:
	var colors := {"Ruim": "#bd896e", "Boa": "#7fdc94", "Épica": "#55d9cb", "Lendária": "#f3ad54", "Única": "#bc91ec", "Mística": "#ff8dbe"}
	return Color(str(colors.get(rarity, "#fff1c2")))

func _draw_item_row(rect: Rect2, title: String, detail: String, owned: bool, equipped: bool, tint: Color) -> void:
	_draw_panel(rect, Color("#2D555D") if owned else Color("#203C49"), 10)
	_draw_panel(Rect2(rect.position + Vector2(4, 4), Vector2(44, rect.size.y - 8)), tint.darkened(0.28), 9)
	_icon("gear" if menu_tab == "equipment" else "drill", Rect2(rect.position + Vector2(9, 8), Vector2(34, 34)))
	var font_size := 15 if rect.size.y > 55 else 13
	draw_string(ThemeDB.fallback_font, rect.position + Vector2(57, 23), title, HORIZONTAL_ALIGNMENT_LEFT, rect.size.x - 157, font_size, PANEL)
	var marker := "EQUIPADO" if equipped else ("EQUIPAR" if owned else "NO BAÚ")
	_draw_panel(Rect2(rect.end.x - 96, rect.position.y + 7, 88, 28), TEAL if equipped else (BRASS if owned else Color("#33515A")), 8)
	draw_string(ThemeDB.fallback_font, Vector2(rect.end.x - 94, rect.position.y + 26), marker, HORIZONTAL_ALIGNMENT_CENTER, 84, 10, INK if owned else PANEL)
	draw_string(ThemeDB.fallback_font, rect.position + Vector2(57, rect.size.y - 10), detail, HORIZONTAL_ALIGNMENT_LEFT, rect.size.x - 68, 10, Color("#B7D6D3"))

func _chest_rule_text() -> String:
	if int(state.get("below_legendary_streak", 0)) >= 11:
		return "LENDÁRIO GARANTIDO NO PRÓXIMO BAÚ · sem compras ou anúncios"
	if int(state.get("below_epic_streak", 0)) >= 4:
		return "ÉPICO GARANTIDO NO PRÓXIMO BAÚ · sem compras ou anúncios"
	var remaining := 5 - int(state.get("below_epic_streak", 0))
	return "Chaves e baús vêm escavando · Épico garantido em até %d baú(s)" % remaining

func _draw_panel(rect: Rect2, color: Color, radius: int, shadow := false) -> void:
	var key := "%s:%d:%s" % [color.to_html(), radius, shadow]
	if not panel_styles.has(key):
		var style := StyleBoxFlat.new()
		style.bg_color = color
		style.set_corner_radius_all(radius)
		style.border_color = OUTLINE
		style.set_border_width_all(2)
		if shadow:
			style.shadow_color = Color(0.02, 0.08, 0.12, 0.52)
			style.shadow_size = 4
			style.shadow_offset = Vector2(0, 4)
		panel_styles[key] = style
	draw_style_box(panel_styles[key], rect)

func _draw_pill(rect: Rect2, color: Color, radius: float) -> void:
	_draw_panel(rect, color, int(radius), true)

func _draw_button(rect: Rect2, color: Color, label: String, text_color: Color) -> void:
	var primary := label.contains("INICIAR") or label == "CONTINUAR"
	var tint := CORAL if primary else BRASS
	art.plate(self, rect, tint, rect.has_point(ui_press), true, primary)
	var font_size := 14 if rect.size.x > 190 else 11
	draw_string(ThemeDB.fallback_font, rect.position + Vector2(10, rect.size.y * 0.5 + 5 + (2 if rect.has_point(ui_press) else 0)), label, HORIZONTAL_ALIGNMENT_CENTER, rect.size.x - 20, font_size, INK)

func _icon(id: String, rect: Rect2) -> void:
	var key := id
	if id == "chest" and loot_flash > 0.0 and show_workshop:
		key = "chest_open"
	if not icon_cache.has(key):
		if id == "chest" or id == "drill":
			var atlas := AtlasTexture.new()
			atlas.atlas = load("res://assets/ilustracoes/maquina-baus-atlas.png")
			atlas.region = Rect2(700, 670, 535, 545) if key == "chest_open" else Rect2(50, 720, 575, 495)
			if id == "drill":
				atlas.region = Rect2(15, 15, 685, 625)
			icon_cache[key] = atlas
		else:
			icon_cache[key] = load("res://assets/ui/" + id + ".svg")
	var texture: Texture2D = icon_cache[key]
	var factor := minf(rect.size.x / texture.get_width(), rect.size.y / texture.get_height())
	var fitted := texture.get_size() * factor
	draw_texture_rect(texture, Rect2(rect.get_center() - fitted * 0.5, fitted), false)

func _action(rect: Rect2, icon: String, title: String, detail: String, tint: Color, enabled: bool) -> void:
	art.plate(self, rect, CORAL, rect.has_point(ui_press), enabled, true)
	_icon(icon, Rect2(rect.position + Vector2(12, 12), Vector2(34, 34)))
	var color := INK if enabled else Color("#D1E2DB")
	draw_string(ThemeDB.fallback_font, rect.position + Vector2(49, 26), title, HORIZONTAL_ALIGNMENT_CENTER, rect.size.x - 62, 12, color)
	draw_string(ThemeDB.fallback_font, rect.position + Vector2(49, 44), detail, HORIZONTAL_ALIGNMENT_CENTER, rect.size.x - 62, 10, color)

func _wrapped_text(rect: Rect2, text: String, font_size: int, color: Color) -> void:
	var paragraph := TextParagraph.new()
	paragraph.width = rect.size.x
	paragraph.alignment = HORIZONTAL_ALIGNMENT_CENTER
	paragraph.add_string(text, ThemeDB.fallback_font, font_size)
	paragraph.draw(get_canvas_item(), rect.position, color)

func _centered_string(font: Font, pos: Vector2, text: String, alignment: HorizontalAlignment, width: float, font_size: int, color: Color) -> void:
	draw_string(font, pos - Vector2(width * 0.5, 0), text, alignment, width, font_size, color)

func _draw_particles() -> void:
	for particle in fx:
		var radius: float = particle.radius
		var angle: float = particle.angle
		var long_axis := Vector2(cos(angle), sin(angle)) * radius
		var short_axis := Vector2(-long_axis.y, long_axis.x) * radius * 0.52
		var points := PackedVector2Array([
			particle.p + long_axis,
			particle.p + short_axis,
			particle.p - long_axis,
			particle.p - short_axis
		])
		draw_colored_polygon(points, particle.color)
		draw_line(particle.p - long_axis * 0.35, particle.p + long_axis * 0.35, Color("#fff7e6", 0.8), 1.0, true)
