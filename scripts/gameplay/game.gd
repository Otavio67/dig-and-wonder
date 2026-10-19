extends Control

const Economy = preload("res://scripts/progression/economy.gd")
const DrillSpec = preload("res://scripts/progression/drill.gd")
const DigEngine = preload("res://scripts/gameplay/dig_engine.gd")
const SaveStore = preload("res://scripts/save/save_store.gd")
const BIOME_PATH := "res://content/biomes/prado_cristalino.json"

const BG := Color("#82dfd4")
const INK := Color("#294456")
const PANEL := Color("#fff9e8")

var biome: Dictionary
var state: Dictionary
var engine := DigEngine.new()
var drill := DrillSpec.new()
var cell_size := 54.0
var camera_depth := 0
var charge := 0.0
var held := false
var mining := false
var mine_target := Vector2i.ZERO
var fx: Array[Dictionary] = []
var toast := ""
var toast_time := 0.0
var show_album := false
var show_start_screen := true
var has_existing_save := false
var press_scale := 0.0
var grid_origin_y := 154.0
var world_bottom := 0.0

func _ready() -> void:
	custom_minimum_size = Vector2(300, 560)
	var biome_file := FileAccess.open(BIOME_PATH, FileAccess.READ)
	biome = JSON.parse_string(biome_file.get_as_text())
	state = SaveStore.load_data()
	has_existing_save = not state.is_empty()
	if state.is_empty():
		state = SaveStore.fresh()
	var dug_cells: Array = state.dug
	engine.setup(biome, 7042, dug_cells, state.get("damaged", {}))
	if state.drill == "prism":
		drill.transform_to_prism()
	drill.level = maxi(1, int(state.get("drill_level", 1)))
	camera_depth = maxi(0, int(state.depth) - 8)
	set_process(true)
	queue_redraw()

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_PAUSED or what == NOTIFICATION_WM_CLOSE_REQUEST:
		_commit_save()

func _commit_save() -> void:
	state.depth = maxi(int(state.depth), camera_depth)
	state.drill = drill.id
	state.drill_level = drill.level
	state.dug = engine.dug_keys()
	state.damaged = engine.durability.duplicate(true)
	if SaveStore.save_data(state):
		has_existing_save = true

func _process(delta: float) -> void:
	var changed := false
	if held and not show_album:
		charge += delta
		if charge >= 0.17:
			charge = 0.0
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
	if show_start_screen:
		if event is InputEventScreenTouch and event.pressed or event is InputEventMouseButton and event.pressed:
			var start_top := size.y - 168.0
			if event.position.y >= start_top and event.position.y <= size.y - 104.0 and event.position.x >= 30.0 and event.position.x <= size.x - 30.0:
				_start_game()
			get_viewport().set_input_as_handled()
		return
	if show_album:
		if event is InputEventScreenTouch and event.pressed or event is InputEventMouseButton and event.pressed:
			show_album = false
			queue_redraw()
		get_viewport().set_input_as_handled()
		return
	if event is InputEventScreenTouch:
		if event.pressed:
			_on_press(event.position)
		else:
			held = false
		get_viewport().set_input_as_handled()
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		if event.pressed:
			_on_press(event.position)
		else:
			held = false
		get_viewport().set_input_as_handled()
	elif event is InputEventMouseMotion and held:
		if _in_world(event.position):
			mine_target = _cell_at(event.position)

func _on_press(pos: Vector2) -> void:
	var size := get_rect().size
	if pos.y >= size.y - 168.0 and pos.y <= size.y - 108.0:
		_sell()
	elif pos.y >= size.y - 103.0 and pos.y <= size.y - 53.0:
		_upgrade()
	elif pos.y < 96.0 and pos.x > size.x - 132.0:
		show_album = true
	elif _in_world(pos):
		mine_target = _cell_at(pos)
		held = true
		charge = 0.16
		_perform_dig(mine_target)
	queue_redraw()

func _in_world(pos: Vector2) -> bool:
	return pos.y >= grid_origin_y and pos.y < world_bottom

func _cell_at(pos: Vector2) -> Vector2i:
	var col := floori((pos.x - (size.x * 0.5 - cell_size * 0.5)) / cell_size)
	var row := maxi(0, floori((pos.y - grid_origin_y) / cell_size) + camera_depth)
	return Vector2i(clampi(col, -14, 14), row)

func _perform_dig(cell: Vector2i) -> void:
	var result := engine.dig(drill.pattern, cell.x, cell.y, drill.level)
	if result.broken <= 0:
		if result.worked > 0:
			_commit_save()
		return
	state.inventory += int(result.coins)
	for target in result.cells:
		state.depth = maxi(int(state.depth), target.y + 1)
		_spawn_particles(_cell_center(target), result.broken)
		if not state.discoveries.has(biome.discovery.id) and target.y >= int(biome.discovery.depth):
			state.discoveries.append(biome.discovery.id)
			_toast("Descoberta: Semente lunar!")
	var visible_rows := maxi(1, floori((world_bottom - grid_origin_y) / cell_size))
	camera_depth = maxi(camera_depth, state.depth - visible_rows + 2)
	_commit_save()
	queue_redraw()

func _start_game() -> void:
	show_start_screen = false
	queue_redraw()

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
	state.coins += Economy.sell_coins(amount)
	state.inventory = 0
	_toast("Venda feita: +%d moedas" % amount)
	_commit_save()

func _upgrade() -> void:
	if drill.id == "basic":
		var cost := 30
		if not Economy.can_afford(int(state.coins), cost):
			_toast("Faltam %d moedas para a Broca Prisma." % (cost - int(state.coins)))
			return
		state.coins = Economy.buy(int(state.coins), cost)
		drill.transform_to_prism()
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
	elif show_start_screen:
		_draw_start_screen(s)
	else:
		_draw_hud(s)
		_draw_world(s)
		_draw_controls(s)
	_draw_particles()
	if toast_time > 0.0:
		draw_string(ThemeDB.fallback_font, Vector2(18, 160), toast, HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 15, PANEL)

func _draw_background(s: Vector2) -> void:
	draw_rect(Rect2(Vector2.ZERO, s), BG)
	# Sol, nuvens e colinas: shapes nativas, custo baixo e contornos redondos.
	draw_circle(Vector2(s.x - 47.0, 61.0), 35.0, Color("#fff1ae"))
	draw_circle(Vector2(65.0, 115.0), 56.0, Color("#b8efdb"))
	draw_circle(Vector2(9.0, 137.0), 47.0, Color("#b8efdb"))
	draw_circle(Vector2(s.x - 15.0, 130.0), 45.0, Color("#b8efdb"))
	draw_rect(Rect2(0, 131, s.x, s.y - 131), Color("#f5d488"))
	# Faixa de superfície com costura sinuosa.
	draw_rect(Rect2(0, 138, s.x, 16), Color("#a8dc79"))
	for i in range(0, ceili(s.x / 24.0)):
		var x := float(i) * 24.0
		draw_circle(Vector2(x + 7.0, 138.0 + float(i % 2) * 3.0), 3.5, Color("#78ba61"))

func _draw_hud(s: Vector2) -> void:
	_draw_pill(Rect2(17, 16, 150, 43), PANEL, 18)
	draw_string(ThemeDB.fallback_font, Vector2(31, 44), "◉  %d" % state.coins, HORIZONTAL_ALIGNMENT_LEFT, 110, 21, Color("#c18431"))
	_draw_pill(Rect2(s.x - 134, 16, 117, 43), PANEL, 18)
	draw_string(ThemeDB.fallback_font, Vector2(s.x - 122, 43), "ÁLBUM  ✦ %d/1" % state.discoveries.size(), HORIZONTAL_ALIGNMENT_CENTER, 92, 14, INK)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 116), "PRADO CRISTALINO  ·  %dm" % (state.depth * 2), HORIZONTAL_ALIGNMENT_CENTER, s.x - 50, 14, INK)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 68), "TOQUE E SEGURE PARA ESCAVAR", HORIZONTAL_ALIGNMENT_CENTER, s.x - 30, 12, Color("#416878"))

func _draw_start_screen(s: Vector2) -> void:
	draw_circle(Vector2(s.x * 0.5, 286), 88, Color("#b8efdb"))
	draw_circle(Vector2(s.x * 0.5, 279), 68, Color("#d5f2dd"))
	_draw_drill(Vector2(s.x * 0.5, 257))
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 402), "DIG & WONDER", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 36, Color("#3d5c6d"))
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 438), "O que será que existe logo abaixo?", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 16, Color("#416878"))
	_draw_pill(Rect2(28, 478, s.x - 56, 66), PANEL, 18)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 505), "PRADO CRISTALINO", HORIZONTAL_ALIGNMENT_CENTER, s.x - 84, 16, INK)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 528), "Escave · colete · descubra", HORIZONTAL_ALIGNMENT_CENTER, s.x - 84, 12, Color("#66808a"))
	var button_label := "CONTINUAR" if has_existing_save else "INICIAR ESCAVAÇÃO"
	_draw_button(Rect2(28, s.y - 168, s.x - 56, 58), Color("#37cbb9"), button_label, Color("#173d4b"))
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 77), "PROGRESSO SALVO AUTOMATICAMENTE", HORIZONTAL_ALIGNMENT_CENTER, s.x - 36, 11, Color("#55707a"))

func _draw_world(s: Vector2) -> void:
	var top := 154.0
	var bottom := s.y - 174.0
	grid_origin_y = top
	world_bottom = bottom
	draw_rect(Rect2(0, top, s.x, bottom - top), Color("#f0cf91"))
	var visible_rows := ceili((bottom - top) / cell_size) + 1
	for screen_row in range(visible_rows):
		var depth := camera_depth + screen_row
		for col in range(-4, 5):
			var x := s.x * 0.5 - cell_size * 0.5 + float(col) * cell_size
			var y := top + float(screen_row) * cell_size
			var rect := Rect2(x, y, cell_size, cell_size)
			if engine.is_dug(col, depth):
				draw_rect(rect, Color("#f4d896"))
				draw_rect(Rect2(x + 2, y + 3, cell_size - 4, cell_size - 5), Color("#f8e8bd"))
			else:
				var layer: Dictionary = _layer_visual(depth, col)
				var fill: Color = Color(layer.color)
				draw_rect(rect, fill)
				draw_rect(Rect2(x + 3, y + 3, cell_size - 6, cell_size - 7), fill.lightened(0.10))
				draw_circle(Vector2(x + 12 + float(posmod(col * 13 + depth * 7, 27)), y + 16 + float(posmod(depth * 11 + col * 5, 22))), 2.4, fill.darkened(0.14))
				if depth >= int(biome.discovery.depth) and col == 0 and posmod(depth, 13) == 0:
					draw_colored_polygon(PackedVector2Array([Vector2(x + 19, y + 36), Vector2(x + 26, y + 11), Vector2(x + 35, y + 34)]), Color("#9affed"))
	# Mina lateral, fita e broca central.
	draw_rect(Rect2(0, top, 5, bottom - top), Color("#ddae68"))
	var surface_line := PackedVector2Array([Vector2(0, 154), Vector2(s.x * 0.5 - 39, 154), Vector2(s.x * 0.5 - 25, 149), Vector2(s.x * 0.5 + 27, 149), Vector2(s.x * 0.5 + 42, 154), Vector2(s.x, 154)])
	draw_polyline(surface_line, Color("#75b85d"), 6.0, true)
	_draw_drill(Vector2(s.x * 0.5, top + 2))
	_draw_pill(Rect2(s.x - 105, top + 11, 89, 30), PANEL, 13)
	draw_string(ThemeDB.fallback_font, Vector2(s.x - 97, top + 32), "%d ◉" % state.inventory, HORIZONTAL_ALIGNMENT_CENTER, 73, 14, INK)

func _layer_visual(depth: int, col: int) -> Dictionary:
	var layers: Array = biome.layers
	var index := 0
	if depth > 5:
		index = 1
	if depth > 12:
		index = 2
	if depth > 23:
		index = 3
	if index == 3 and posmod(depth + col * 3, 7) != 0:
		index = 2
	return layers[index]

func _draw_drill(center: Vector2) -> void:
	var color := drill.accent
	if drill.id == "prism":
		for x_offset in [-21.0, 21.0]:
			draw_colored_polygon(PackedVector2Array([center + Vector2(x_offset, 15), center + Vector2(x_offset - 9, 32), center + Vector2(x_offset + 10, 31)]), color.darkened(0.08))
	# Cápsula principal e faixas metálicas.
	draw_circle(center + Vector2(0, 9), 23, Color("#33516a"))
	draw_circle(center, 19, color)
	draw_circle(center + Vector2(-5, -5), 5, Color("#ffffff"))
	draw_rect(Rect2(center.x - 5, center.y + 14, 10, 18), Color("#fff4cd"))
	var tip := PackedVector2Array([center + Vector2(-12, 27), center + Vector2(12, 27), center + Vector2(0, 44)])
	draw_colored_polygon(tip, Color("#3e5d71"))
	if drill.id == "prism":
		draw_line(center + Vector2(-30, 4), center + Vector2(30, 4), Color("#ffffff"), 2.0, true)

func _draw_controls(s: Vector2) -> void:
	draw_rect(Rect2(0, s.y - 168, s.x, 168), Color("#fff9e8"))
	draw_line(Vector2(18, s.y - 168), Vector2(s.x - 18, s.y - 168), Color("#e3d9bd"), 2.0)
	_draw_pill(Rect2(18, s.y - 153, s.x - 36, 40), Color("#e8f2d7"), 16)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 127), "VALOR NA MOCHILA  %d ◉" % state.inventory, HORIZONTAL_ALIGNMENT_CENTER, s.x - 55, 15, INK)
	_draw_button(Rect2(18, s.y - 102, s.x - 36, 47), Color("#f6c15f"), "VENDER PEDRAS  ·  +%d" % state.inventory, Color("#543e23"))
	var upgrade_text: String
	var cost: int
	if drill.id == "basic":
		upgrade_text = "BROCA PRISMA  ·  30 ◉"
		cost = 30
	else:
		cost = 18 + (drill.level - 1) * 16
		upgrade_text = "REFORÇAR NV.%d  ·  %d ◉" % [drill.level, cost]
	var enabled := int(state.coins) >= cost
	_draw_button(Rect2(18, s.y - 48, s.x - 36, 40), Color("#37cbb9") if enabled else Color("#c5d7ce"), upgrade_text, Color("#173d4b"))
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, s.y - 6), "Broca: %s  ·  quebradas %d" % [drill.title, engine.dug.size()], HORIZONTAL_ALIGNMENT_CENTER, s.x - 24, 11, Color("#55707a"))

func _draw_album(s: Vector2) -> void:
	draw_rect(Rect2(16, 174, s.x - 32, s.y - 202), Color("#fff9e8"), true, 24)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 218), "ÁLBUM DE DESCOBERTAS", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 20, INK)
	draw_circle(Vector2(s.x * 0.5, 310), 50, Color("#d5f2dd"))
	var found: bool = state.discoveries.has(biome.discovery.id)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 327), "✦" if found else "?", HORIZONTAL_ALIGNMENT_CENTER, 65, 42, Color("#55bba5"))
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 408), biome.discovery.name if found else "Descoberta oculta", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 18, INK)
	draw_string(ThemeDB.fallback_font, Vector2(s.x * 0.5, 441), biome.discovery.description if found else "Continue escavando para encontrar algo.", HORIZONTAL_ALIGNMENT_CENTER, s.x - 64, 13, Color("#66808a"))
	_draw_button(Rect2(35, s.y - 87, s.x - 70, 48), Color("#37cbb9"), "VOLTAR À ESCAVAÇÃO", Color("#173d4b"))

func _draw_pill(rect: Rect2, color: Color, radius: float) -> void:
	draw_rect(rect, Color("#314c59"), true)
	draw_rect(Rect2(rect.position.x + 1, rect.position.y - 2, rect.size.x - 2, rect.size.y - 2), color, true)

func _draw_button(rect: Rect2, color: Color, label: String, text_color: Color) -> void:
	draw_rect(Rect2(rect.position.x, rect.position.y + 3, rect.size.x, rect.size.y), Color("#354f5c"), true)
	draw_rect(rect, color, true)
	draw_string(ThemeDB.fallback_font, Vector2(rect.position.x, rect.position.y + rect.size.y * 0.67), label, HORIZONTAL_ALIGNMENT_CENTER, rect.size.x - 14, 13, text_color)

func _draw_particles() -> void:
	for particle in fx:
		draw_circle(particle.p, particle.radius, particle.color)
