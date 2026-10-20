extends Control

const Economy = preload("res://scripts/progression/economy.gd")
const DrillSpec = preload("res://scripts/progression/drill.gd")
const DigEngine = preload("res://scripts/gameplay/dig_engine.gd")
const SaveStore = preload("res://scripts/save/save_store.gd")
const EquipmentSystem = preload("res://scripts/progression/equipment_system.gd")
const PhaseSystem = preload("res://scripts/progression/phase_system.gd")
const Layout = preload("res://scripts/gameplay/ui_layout.gd")
const AUTOSAVE_INTERVAL := 1.0
const BIOME_PATH := "res://content/biomes/prado_cristalino.json"
const PHASE_PATH := "res://content/phases/catalog.json"

const BG := Color("#82dfd4")
const INK := Color("#294456")
const PANEL := Color("#fff9e8")

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

func _ready() -> void:
	get_tree().auto_accept_quit = false
	get_tree().quit_on_go_back = false
	custom_minimum_size = Vector2(300, 560)
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
	_cancel_pointer()
	if not active:
		_commit_save()

func _cancel_pointer() -> void:
	held = false
	attacking_boss = false
	camera_dragging = false
	charge = 0.0
	active_touch = -1

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
	var changed := false
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
	if result.broken <= 0:
		if result.worked > 0:
			_request_save()
		return
	state.inventory += int(result.coins)
	for target in result.cells:
		var old_depth := int(state.depth)
		state.depth = maxi(old_depth, target.y + 1)
		_spawn_particles(_cell_center(target), result.broken)
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

func _spawn_particles(origin: Vector2, count: int) -> void:
	var remaining := 26 - fx.size()
	for i in range(mini(count * 3, remaining)):
		fx.append({"p": origin, "velocity": Vector2(randf_range(-115.0, 115.0), randf_range(-190.0, -45.0)), "life": randf_range(0.25, 0.58), "color": [Color("#ffe08b"), Color("#fff5cf"), Color("#79ede0")][randi() % 3], "radius": randf_range(2.0, 4.5)})

func _sell() -> void:
	var amount := int(state.inventory)
	if amount <= 0:
		_toast("A mochila ainda está vazia.")
		return
	var first_sale_bonus := 8 if not state.get("first_sale_bonus_claimed", false) else 0
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
	_commit_save()

func _toast(message: String) -> void:
	toast = message
	toast_time = 2.4

func _draw() -> void:
	var s := size
	_draw_background(s)
	if show_album:
		_draw_album(s)
	elif show_workshop:
		_draw_workshop(s)
	elif show_start_screen:
		_draw_start_screen(s)
	else:
		_draw_hud(s)
		_draw_world(s)
		_draw_controls(s)
	_draw_particles()
	if toast_time > 0.0:
		_draw_panel(Rect2(12, s.y - 310, s.x - 24, 64), INK, 16)
		_wrapped_text(Rect2(24, s.y - 299, s.x - 48, 48), toast, 16, PANEL)

func _draw_background(s: Vector2) -> void:
	var phase := _active_phase()
	draw_rect(Rect2(Vector2.ZERO, s), Color(str(phase.get("sky", "#82dfd4"))))
	# Sol, nuvens e colinas: shapes nativas, custo baixo e contornos redondos.
	draw_circle(Vector2(s.x - 47.0, 61.0), 35.0, Color("#fff1ae"))
	draw_circle(Vector2(65.0, 115.0), 56.0, Color("#b8efdb"))
	draw_circle(Vector2(9.0, 137.0), 47.0, Color("#b8efdb"))
	draw_circle(Vector2(s.x - 15.0, 130.0), 45.0, Color("#b8efdb"))
	draw_rect(Rect2(0, 131, s.x, s.y - 131), Color(str(phase.get("surface", "#f5d488"))).lightened(0.33))
	# Faixa de superfície com costura sinuosa.
	draw_rect(Rect2(0, 138, s.x, 16), Color("#a8dc79"))
	for i in range(0, ceili(s.x / 24.0)):
		var x := float(i) * 24.0
		draw_circle(Vector2(x + 7.0, 138.0 + float(i % 2) * 3.0), 3.5, Color("#78ba61"))

func _draw_hud(s: Vector2) -> void:
	var phase := _active_phase()
	_draw_pill(Rect2(17, 16, 150, 43), PANEL, 18)
	draw_string(ThemeDB.fallback_font, Vector2(31, 44), "◉  %d" % state.coins, HORIZONTAL_ALIGNMENT_LEFT, 110, 21, Color("#c18431"))
	_draw_pill(Layout.button("album", s), PANEL, 18)
	draw_string(ThemeDB.fallback_font, Vector2(s.x - 156, 50), "ÁLBUM  ✦ %d/%d" % [state.discoveries.size(), phase_system.phases.size()], HORIZONTAL_ALIGNMENT_CENTER, 128, 14, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 116), "FASE %d  ·  %s  ·  %dm" % [int(state.phase_index) + 1, phase.get("name", "Mina"), state.depth * 2], HORIZONTAL_ALIGNMENT_CENTER, s.x - 28, 13, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 92), _goal_text(), HORIZONTAL_ALIGNMENT_CENTER, s.x - 44, 12, Color("#416878"))
	var labels := ["BAÚS %d · 🔑%d" % [state.get("chests", 0), state.get("keys", 0)], "EQUIPAMENTOS", "PERSONAGEM"]
	for i in range(3):
		var rect := Layout.tab(i, s)
		_draw_button(rect, Color("#e6f4db"), str(labels[i]), INK)

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
	draw_circle(Vector2(s.x * 0.5, 286), 88, Color("#b8efdb"))
	draw_circle(Vector2(s.x * 0.5, 279), 68, Color("#d5f2dd"))
	_draw_drill(Vector2(s.x * 0.5, 257))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 402), "DIG & WONDER", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 36, Color("#3d5c6d"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 438), "O que será que existe logo abaixo?", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 16, Color("#416878"))
	_draw_pill(Rect2(28, 478, s.x - 56, 66), PANEL, 18)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 505), "PRADO CRISTALINO", HORIZONTAL_ALIGNMENT_CENTER, s.x - 84, 16, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 528), "Escave · colete · descubra", HORIZONTAL_ALIGNMENT_CENTER, s.x - 84, 12, Color("#66808a"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 592), "Segure no terreno para cavar", HORIZONTAL_ALIGNMENT_CENTER, s.x - 60, 19, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 622), "Arraste para explorar a profundidade", HORIZONTAL_ALIGNMENT_CENTER, s.x - 60, 16, Color("#416878"))
	var button_label := "CONTINUAR" if has_existing_save else "INICIAR ESCAVAÇÃO"
	_draw_button(Layout.button("start", s), Color("#37cbb9"), button_label, Color("#173d4b"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 77), "PROGRESSO SALVO AUTOMATICAMENTE", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 11, Color("#55707a"))

func _draw_world(s: Vector2) -> void:
	var top := Layout.WORLD_TOP
	var bottom := s.y - Layout.FOOTER_HEIGHT
	grid_origin_y = top
	world_bottom = bottom
	draw_rect(Rect2(0, top, s.x, bottom - top), Color(str(_active_phase().get("rock", "#f0cf91"))))
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
				draw_rect(rect, Color("#f4d896"))
				draw_rect(Rect2(x + 2, y + 3, cell_size - 4, cell_size - 5), Color("#f8e8bd"))
			else:
				var layer: Dictionary = engine.layer_for_cell(depth, col)
				var fill: Color = Color(layer.color)
				draw_rect(rect, fill)
				draw_rect(Rect2(x + 3, y + 3, cell_size - 6, cell_size - 7), fill.lightened(0.10))
				draw_circle(Vector2(x + 12 + float(posmod(col * 13 + depth * 7, 27)), y + 16 + float(posmod(depth * 11 + col * 5, 22))), 2.4, fill.darkened(0.14))
				if int(layer.reward) >= 8:
					var ore_center := Vector2(x + cell_size * 0.5, y + cell_size * 0.5)
					draw_colored_polygon(PackedVector2Array([ore_center + Vector2(0, -13), ore_center + Vector2(11, 0), ore_center + Vector2(0, 13), ore_center + Vector2(-11, 0)]), fill.lightened(0.42))
					draw_line(ore_center + Vector2(0, -9), ore_center + Vector2(0, 7), Color.WHITE, 2.0, true)
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
	# Mina lateral, fita e broca central.
	draw_rect(Rect2(0, top, 5, bottom - top), Color("#ddae68"))
	var surface_line := PackedVector2Array([Vector2(0, top), Vector2(s.x * 0.5 - 39, top), Vector2(s.x * 0.5 - 25, top - 5), Vector2(s.x * 0.5 + 27, top - 5), Vector2(s.x * 0.5 + 42, top), Vector2(s.x, top)])
	draw_polyline(surface_line, Color(str(_active_phase().get("surface", "#75b85d"))).lightened(0.2), 6.0, true)
	_draw_drill(Vector2(s.x * 0.5, top + 2))
	_draw_pill(Rect2(s.x - 105, top + 11, 89, 30), PANEL, 13)
	draw_string(ThemeDB.fallback_font, Vector2(s.x - 97, top + 32), "%d ◉" % state.inventory, HORIZONTAL_ALIGNMENT_CENTER, 73, 14, INK)
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
	_draw_panel(Rect2(bar.position, Vector2(bar.size.x * ratio, bar.size.y)), Color("#ef6a65"), 4)
	_centered_string(ThemeDB.fallback_font, Vector2(center.x, center.y - 49), str(boss.get("name", "CHEFE")), HORIZONTAL_ALIGNMENT_CENTER, 200, 13, INK)

func _draw_drill(center: Vector2) -> void:
	var color := drill.accent
	if drill.id in ["prism", "trinca"]:
		for x_offset in [-21.0, 21.0]:
			draw_colored_polygon(PackedVector2Array([center + Vector2(x_offset, 15), center + Vector2(x_offset - 9, 32), center + Vector2(x_offset + 10, 31)]), color.darkened(0.08))
	# Cápsula principal e faixas metálicas.
	draw_circle(center + Vector2(0, 9), 23, Color("#33516a"))
	draw_circle(center, 19, color)
	draw_circle(center + Vector2(-5, -5), 5, Color("#ffffff"))
	# A broca também é a companheira: olhos e sorriso permanecem legíveis em tamanho pequeno.
	draw_circle(center + Vector2(-6, 2), 2.5, Color("#294456"))
	draw_circle(center + Vector2(6, 2), 2.5, Color("#294456"))
	draw_arc(center + Vector2(0, 3), 7, 0.35, 2.79, 12, Color("#294456"), 1.6, true)
	draw_rect(Rect2(center.x - 5, center.y + 14, 10, 18), Color("#fff4cd"))
	var tip := PackedVector2Array([center + Vector2(-12, 27), center + Vector2(12, 27), center + Vector2(0, 44)])
	draw_colored_polygon(tip, Color("#3e5d71"))
	if drill.id in ["prism", "dynamo", "echo", "aurora"]:
		draw_line(center + Vector2(-30, 4), center + Vector2(30, 4), Color("#ffffff"), 2.0, true)
	if drill.id == "aurora":
		draw_circle(center, 7, Color("#fff7d0"))

func _draw_controls(s: Vector2) -> void:
	draw_rect(Rect2(0, s.y - Layout.FOOTER_HEIGHT, s.x, Layout.FOOTER_HEIGHT), Color("#fff9e8"))
	draw_line(Vector2(18, s.y - Layout.FOOTER_HEIGHT), Vector2(s.x - 18, s.y - Layout.FOOTER_HEIGHT), Color("#e3d9bd"), 2.0)
	_draw_pill(Rect2(18, s.y - 218, s.x - 36, 40), Color("#e8f2d7"), 16)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 192), "VALOR NA MOCHILA  %d ◉" % state.inventory, HORIZONTAL_ALIGNMENT_CENTER, s.x - 55, 15, INK)
	_draw_button(Layout.button("sell", s), Color("#f6c15f"), "VENDER PEDRAS  ·  +%d" % state.inventory, Color("#543e23"))
	var upgrade_text: String
	var cost: int
	if drill.id == "basic":
		var has_prism: bool = state.get("drill_inventory", []).has("prism")
		upgrade_text = "EQUIPAR PRISMA" if has_prism else "BROCA PRISMA  ·  30 ◉"
		cost = 0 if has_prism else 30
	else:
		cost = 18 + (drill.level - 1) * 16
		upgrade_text = "REFORÇAR NV.%d  ·  %d ◉" % [drill.level, cost]
	var enabled := int(state.coins) >= cost
	_draw_button(Layout.button("upgrade", s), Color("#37cbb9") if enabled else Color("#c5d7ce"), upgrade_text, Color("#173d4b"))

func _draw_album(s: Vector2) -> void:
	_draw_panel(Rect2(16, 174, s.x - 32, s.y - 202), PANEL, 24)
	var discovery: Dictionary = _active_phase().get("discovery", biome.discovery)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 218), "ÁLBUM DE DESCOBERTAS", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 20, INK)
	draw_circle(Vector2(s.x * 0.5, 310), 50, Color("#d5f2dd"))
	var found: bool = state.discoveries.has(str(discovery.get("id", biome.discovery.id)))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 327), "✦" if found else "?", HORIZONTAL_ALIGNMENT_CENTER, 65, 42, Color("#55bba5"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 408), str(discovery.get("name", "Achado")) if found else "Descoberta oculta", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 18, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 441), str(discovery.get("description", "Continue escavando para encontrar algo.")) if found else "Continue escavando para encontrar algo.", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 13, Color("#66808a"))
	_draw_button(Layout.button("back", s), Color("#37cbb9"), "VOLTAR À ESCAVAÇÃO", Color("#173d4b"))

func _draw_workshop(s: Vector2) -> void:
	draw_rect(Rect2(Vector2.ZERO, s), Color("#82dfd4"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 68), "OFICINA DE ACHADOS", HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 28, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 104), "Monte sua broca e descubra novas combinações", HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 16, INK)
	_draw_panel(Rect2(14, 154, s.x - 28, s.y - 174), PANEL, 22)
	_draw_menu_tabs(s)
	match menu_tab:
		"equipment":
			_draw_equipment_menu(s)
		"character":
			_draw_character_menu(s)
		_:
			_draw_chest_menu(s)
	_draw_button(Layout.button("back", s), Color("#37cbb9"), "VOLTAR À ESCAVAÇÃO", Color("#173d4b"))

func _draw_menu_tabs(s: Vector2) -> void:
	var width := (s.x - 42.0) / 3.0
	var names := ["BAÚS", "EQUIPAMENTOS", "PERSONAGEM"]
	var ids := ["chests", "equipment", "character"]
	for i in range(3):
		var color := Color("#37cbb9") if menu_tab == ids[i] else Color("#e8f2d7")
		_draw_button(Layout.tab(i, s, true), color, names[i], INK)

func _draw_chest_menu(s: Vector2) -> void:
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 270), "BAÚS DE DESCOBERTA", HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 21, INK)
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 310), "%d baú(s) guardado(s)  ·  %d chave(s)" % [state.get("chests", 0), state.get("keys", 0)], HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 15, Color("#55707a"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 347), "Chaves e baús aparecem cavando. Nada expira.", HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 12, Color("#55707a"))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 383), _chest_rule_text(), HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 11, Color("#287d6d"))
	draw_string(ThemeDB.fallback_font, Vector2(32, 416), "CHANCES INICIAIS", HORIZONTAL_ALIGNMENT_LEFT, s.x - 84, 13, INK)
	var odds := ["Ruim 28%", "Boa 36%", "Épica 22%", "Lendária 9%", "Única 3,5%", "Mística 1,5%"]
	for i in range(odds.size()):
		var y := 440.0 + float(i / 2) * 43.0
		var x := 32.0 + float(i % 2) * (s.x * 0.5 - 22.0)
		_draw_pill(Rect2(x, y, s.x * 0.5 - 56, 34), _rarity_color(["Ruim", "Boa", "Épica", "Lendária", "Única", "Mística"][i]).lightened(0.66), 11)
		draw_string(ThemeDB.fallback_font, Vector2(x + 8, y + 22), odds[i], HORIZONTAL_ALIGNMENT_CENTER, s.x * 0.5 - 72, 12, INK)
	draw_string(ThemeDB.fallback_font, Vector2(32, 598), "ÚLTIMO ACHADO", HORIZONTAL_ALIGNMENT_LEFT, s.x - 84, 13, INK)
	var loot: Dictionary = state.get("last_loot", {})
	var loot_name := str(loot.get("name", "Ainda nenhum · o próximo pode ser seu!"))
	var kind_name := str({"gear": "Equipamento", "drill": "Broca", "skin": "Visual"}.get(str(loot.get("kind", "")), "Equipamento"))
	var loot_detail := "%s · %s" % [loot.get("rarity", "Baú"), kind_name]
	if bool(loot.get("duplicate", false)):
		loot_detail += " · repetido: +%d moedas" % int(loot.get("coins", 0))
	_draw_item_row(Rect2(28, 614, s.x - 56, 72), loot_name, loot_detail, not loot.is_empty(), false, _rarity_color(str(loot.get("rarity", "Boa"))))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 718), "Baús podem trazer equipamentos, brocas ou skins.", HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 12, Color("#55707a"))
	var can_open := int(state.get("chests", 0)) > 0 and int(state.get("keys", 0)) > 0
	_draw_button(Layout.button("chest", s), Color("#efb950") if can_open else Color("#d7d9c5"), "ABRIR BAÚ  ·  1 CHAVE" if can_open else "BAÚ SELADO  ·  PRECISA DE CHAVE", Color("#4b3822"))

func _draw_equipment_menu(s: Vector2) -> void:
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 263), "EQUIPAMENTOS  ·  %d/60" % state.get("gear_inventory", []).size(), HORIZONTAL_ALIGNMENT_CENTER, s.x - 48, 19, INK)
	var rarities: Array[String] = ["Ruim", "Boa", "Épica", "Lendária", "Única", "Mística"]
	for i in range(rarities.size()):
		_draw_button(Layout.rarity(i, s), Color("#37cbb9") if rarities[i] == gear_rarity else _rarity_color(rarities[i]).lightened(0.55), rarities[i], INK)
	var ids := equipment.gear_ids_for_rarity(gear_rarity)
	draw_string(ThemeDB.fallback_font, Vector2(24, 459), "Toque no item encontrado para equipar", HORIZONTAL_ALIGNMENT_LEFT, s.x - 48, 16, INK)
	for row in range(Layout.GEAR_PER_PAGE):
		var index := gear_page * Layout.GEAR_PER_PAGE + row
		if index >= ids.size():
			break
		var id := ids[index]
		var spec: Dictionary = equipment.gear[id]
		_draw_item_row(Layout.item(row, s, true), str(spec.name), "%s · %s · %s" % [spec.slot, spec.skill, spec.description], state.gear_inventory.has(id), state.equipped_gear.has(id), Color(str(spec.color)))
	_draw_pager(gear_page, ceili(float(ids.size()) / Layout.GEAR_PER_PAGE), s)

func _draw_character_menu(s: Vector2) -> void:
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 263), "MINHA BROCA É MINHA COMPANHEIRA", HORIZONTAL_ALIGNMENT_CENTER, s.x - 46, 16, INK)
	_draw_drill(Vector2(s.x * 0.5, 284))
	var skin: Dictionary = equipment.skins.get(str(state.get("equipped_skin", "pioneira")), equipment.skins.get("pioneira", {}))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 346), "%s · skin %s" % [drill.title, skin.get("name", "Pioneira")], HORIZONTAL_ALIGNMENT_CENTER, s.x - 42, 16, INK)
	_wrapped_text(Rect2(24, 352, s.x - 48, 40), "Inata: %s — %s" % [skin.get("skill", "Curiosa"), skin.get("description", "")], 13, INK)
	var stats := equipment.bonuses(state.get("equipped_gear", []), str(state.get("equipped_skin", "pioneira")))
	_centered_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 405), "Força %+d · moedas %+d · velocidade %+d%% · sorte %+d" % [stats.power, stats.reward, roundi(float(stats.speed) * 100.0), stats.luck], HORIZONTAL_ALIGNMENT_CENTER, s.x - 42, 14, Color("#287d6d"))
	var models := character_view == "models"
	_draw_button(Layout.button("models", s), Color("#37cbb9") if models else Color("#e8f2d7"), "MODELOS", INK)
	_draw_button(Layout.button("skins", s), Color("#e8f2d7") if models else Color("#37cbb9"), "SKINS %d/13" % state.get("skin_inventory", []).size(), INK)
	var ids := _character_ids()
	for row in range(Layout.CHARACTER_PER_PAGE):
		var index := skin_page * Layout.CHARACTER_PER_PAGE + row
		if index >= ids.size():
			break
		var id := str(ids[index])
		var spec: Dictionary = equipment.drills[id] if models else equipment.skins[id]
		var owned: bool = state.drill_inventory.has(id) if models else state.skin_inventory.has(id)
		var equipped: bool = state.drill == id if models else state.equipped_skin == id
		var detail := "%s · %s" % [spec.rarity, spec.description]
		_draw_item_row(Layout.item(row, s), str(spec.name), detail, owned, equipped, Color(str(spec.color)))
	_draw_pager(skin_page, ceili(float(ids.size()) / Layout.CHARACTER_PER_PAGE), s)

func _draw_pager(page_index: int, count: int, s: Vector2) -> void:
	_draw_button(Layout.page(false, s), Color("#e8f2d7"), "%d/%d · ANTERIOR" % [page_index + 1, count], INK)
	_draw_button(Layout.page(true, s), Color("#e8f2d7"), "PRÓXIMOS" if page_index + 1 < count else "ÚLTIMA PÁGINA", INK)

func _rarity_color(rarity: String) -> Color:
	var colors := {"Ruim": "#bd896e", "Boa": "#7fdc94", "Épica": "#55d9cb", "Lendária": "#f3ad54", "Única": "#bc91ec", "Mística": "#ff8dbe"}
	return Color(str(colors.get(rarity, "#fff1c2")))

func _draw_item_row(rect: Rect2, title: String, detail: String, owned: bool, equipped: bool, tint: Color) -> void:
	_draw_panel(rect, tint.lightened(0.76) if owned else Color("#eee9d9"), 12)
	_draw_panel(Rect2(rect.position.x, rect.position.y, 5, rect.size.y), tint, 3)
	var marker := "EQUIPADO" if equipped else ("ENCONTRADO" if owned else "???")
	draw_string(ThemeDB.fallback_font, Vector2(rect.position.x + 12, rect.position.y + 25), title, HORIZONTAL_ALIGNMENT_LEFT, rect.size.x - 112, 17, INK if owned else Color("#546a70"))
	draw_string(ThemeDB.fallback_font, Vector2(rect.position.x + 12, rect.position.y + 51), detail, HORIZONTAL_ALIGNMENT_LEFT, rect.size.x - 24, 13, Color("#607a7b"))
	draw_string(ThemeDB.fallback_font, Vector2(rect.end.x - 92, rect.position.y + 25), marker, HORIZONTAL_ALIGNMENT_RIGHT, 82, 9, Color("#287d6d") if equipped else Color("#718082"))

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
		if shadow:
			style.shadow_color = Color(0.12, 0.24, 0.29, 0.22)
			style.shadow_size = 2
			style.shadow_offset = Vector2(0, 3)
		panel_styles[key] = style
	draw_style_box(panel_styles[key], rect)

func _draw_pill(rect: Rect2, color: Color, radius: float) -> void:
	_draw_panel(rect, color, int(radius), true)

func _draw_button(rect: Rect2, color: Color, label: String, text_color: Color) -> void:
	_draw_panel(rect, color, 16, true)
	draw_string(ThemeDB.fallback_font, Vector2(rect.position.x + 7, rect.position.y + rect.size.y * 0.5 + 6), label, HORIZONTAL_ALIGNMENT_CENTER, rect.size.x - 14, 16, text_color)

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
		draw_circle(particle.p, particle.radius, particle.color)
