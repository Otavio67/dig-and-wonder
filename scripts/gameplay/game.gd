extends Control
## Main entry: defense and excavation. Legacy manual-grid flow is inactive.
signal combat_event(event: Dictionary)
const Run = preload("res://scripts/gameplay/defense_run.gd")
const Store = preload("res://scripts/save/defense_save.gd")
const Layout = preload("res://scripts/gameplay/defense_layout.gd")
const Art = preload("res://scripts/visual/art_direction.gd")
const CartRig = preload("res://scripts/visual/cart_rig.gd")
const PioneerRig = preload("res://scripts/visual/pioneer_rig.gd")
const INK := Color("#17303D")
const CREAM := Color("#FFF4D8")
const TEAL := Color("#39D4C1")
const BRASS := Color("#F2B84B")
const CORAL := Color("#F27C67")
var profile: Dictionary = {}
var legacy: Dictionary = {}
var legacy_source := ""
var sim
var art := Art.new()
var screen := "start"
var app_suspended := false
var active_touch := -1
var pointer := Vector2(-999, -999)
var accumulator := 0.0
var error := ""
var layer_flash := 0.0
var notices: Array[Dictionary] = []
var terrain: Texture2D
var soil: Texture2D
var spawn_flash := 0.0
var mole_atlas: Texture2D
var robot_atlas: Texture2D
var effects: Array[Dictionary] = []
var impact_shake := 0.0
var icons: Dictionary = {}
var styles: Dictionary = {}
var cart_visual: Node2D
var pioneer_visuals: Dictionary = {}
var persistence_enabled := true # Captures set false; normal play always persists.

func _ready() -> void:
	get_tree().auto_accept_quit = false
	get_tree().quit_on_go_back = false
	custom_minimum_size = Vector2(300, 560)
	texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	terrain = load("res://assets/terreno/materiais-atlas.png")
	soil = load("res://assets/terreno/solo-torroes-raizes.png")
	mole_atlas = load("res://assets/characters/torrao-poses.png")
	robot_atlas = load("res://assets/characters/escavador-poses.png")
	cart_visual = CartRig.new()
	cart_visual.position = Vector2(270, 235)
	add_child(cart_visual)
	var loaded := Store.load_profile()
	profile = loaded.profile
	legacy = loaded.legacy
	legacy_source = loaded.source
	error = loaded.error
	if not legacy.is_empty(): screen = "import"
	elif not profile.is_empty(): sim = Run.new(profile)
	else: screen = "error"
	queue_redraw()

func start_run() -> void:
	if profile.is_empty() or not error.is_empty(): return
	if sim == null: sim = Run.new(profile)
	else: sim.restart()
	_reset_visuals()
	notices.clear()
	impact_shake = 0
	screen = "fight"
	accumulator = 0
	_save()
	queue_redraw()

func _save() -> bool:
	if not persistence_enabled: return true
	if profile.is_empty(): return false
	if Store.save_profile(profile): return true
	error = "Não foi possível salvar. Combate pausado; libere espaço e reabra o jogo."
	return false

func _notification(what: int) -> void:
	if not is_node_ready(): return
	if what in [NOTIFICATION_APPLICATION_PAUSED, NOTIFICATION_APPLICATION_FOCUS_OUT]:
		_set_app_active(false)
	elif what in [NOTIFICATION_APPLICATION_RESUMED, NOTIFICATION_APPLICATION_FOCUS_IN]:
		_set_app_active(true)
	elif what == NOTIFICATION_WM_GO_BACK_REQUEST:
		if screen == "fight":
			screen = "start"
			_save()
		else:
			_save()
			get_tree().quit()
	elif what == NOTIFICATION_WM_CLOSE_REQUEST:
		_save()
		get_tree().quit()

func _set_app_active(active: bool) -> void:
	app_suspended = not active
	_release_pointer()
	accumulator = 0
	if is_instance_valid(cart_visual): cart_visual.suspend(not active)
	for visual in pioneer_visuals.values():
		if is_instance_valid(visual): visual.suspend(not active)
	if not active: _save()

func _release_pointer() -> void:
	active_touch = -1
	pointer = Vector2(-999, -999)
	queue_redraw()

func _local_pointer(pos: Vector2) -> Vector2:
	return get_global_transform_with_canvas().affine_inverse() * pos

func _input(event: InputEvent) -> void:
	if app_suspended: return
	if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
		if screen == "fight":
			screen = "start"
			_save()
		else: get_tree().quit()
	elif event is InputEventScreenTouch:
		if event.canceled:
			if event.index == active_touch: _release_pointer()
		elif event.pressed and active_touch == -1:
			active_touch = event.index
			_press_at(_local_pointer(event.position))
		elif not event.pressed and active_touch == event.index: _release_pointer()
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and active_touch == -1:
		if event.pressed: _press_at(_local_pointer(event.position))
		else: _release_pointer()

func _press_at(pos: Vector2) -> void:
	if not Rect2(Vector2.ZERO, size).has_point(pos) or not error.is_empty(): return
	pointer = pos
	if screen == "import":
		if Layout.button("import").has_point(pos):
			if not persistence_enabled: return
			var imported := Store.confirm_import(legacy, legacy_source)
			if imported.is_empty(): error = "Importação não concluída. Arquivo antigo preservado; reabra para tentar a recuperação."
			else:
				profile = imported
				legacy = {}
				sim = Run.new(profile)
				screen = "start"
	elif screen == "start":
		if Layout.button("start").has_point(pos): start_run()
	elif sim != null and sim.status != "active":
		if Layout.button("retry").has_point(pos): start_run()
		elif Layout.button("damage").has_point(pos): sim.purchase("damage")
		elif Layout.button("hull").has_point(pos): sim.purchase("hull")
		_consume_events()
	elif screen == "fight":
		if Layout.button("launch").has_point(pos): sim.launch()
		elif Layout.button("support").has_point(pos): sim.launch_support()
		elif Layout.button("damage").has_point(pos): sim.purchase("damage")
		elif Layout.button("hull").has_point(pos): sim.purchase("hull")
		_consume_events()
	queue_redraw()

func _process(delta: float) -> void:
	if app_suspended: return
	art.update(delta, 0 if sim == null else sim.layer_index * 6, "prado_cristalino")
	layer_flash = maxf(0, layer_flash - delta)
	spawn_flash = maxf(0, spawn_flash - delta)
	impact_shake = maxf(0, impact_shake - delta)
	for i in range(effects.size() - 1, -1, -1):
		effects[i].age += delta
		if effects[i].age > effects[i].duration: effects.remove_at(i)
	for i in range(notices.size() - 1, -1, -1):
		notices[i].life -= delta
		if notices[i].life <= 0: notices.remove_at(i)
	if screen == "fight" and sim != null and error.is_empty():
		accumulator += minf(delta, 0.25)
		while accumulator >= 0.05:
			accumulator -= 0.05
			sim.step(0.05)
		_consume_events()
	_sync_visuals()
	queue_redraw()

func _consume_events() -> void:
	var save_needed := false
	for event in sim.take_events():
		combat_event.emit(event)
		match str(event.type):
			"gold":
				save_needed = true
				var source := str(event.source)
				notices.append({"text": "+%d OURO%s" % [event.amount, " / TORRÃO" if source == "mole" else ""], "life": 1.8, "x": clampf(float(event.get("x", 270)), 125, 415), "y": 512.0})
			"spawn": spawn_flash = 1.5
			"launch": _ensure_pioneer(int(event.id))
			"drill_strike":
				var pioneer: Node2D = pioneer_visuals.get(int(event.id))
				if is_instance_valid(pioneer): pioneer.confirm_impact()
				impact_shake = 0.16
			"support_hit":
				impact_shake = 0.12
				_add_effect(str(event.type), float(event.x), 0.45)
			"drill_break": _remove_pioneer(int(event.id))
			"mole_defeat", "support_end": _add_effect(str(event.type), float(event.x), 0.65)
			"purchase": save_needed = true
			"victory":
				save_needed = true
				_clear_pioneer_visuals()
				cart_visual.stop_attack_responses()
			"defeat":
				save_needed = true
				_clear_pioneer_visuals()
				effects.clear()
				cart_visual.defeat()
			"cannon": cart_visual.fire(int(event.side))
			"cart_hit":
				cart_visual.hit()
			"impact": layer_flash = 0.18
			"hit": notices.append({"text": "✦", "life": 0.18, "x": event.x, "y": 564.0})
		if notices.size() > 32: notices.pop_front()
	if save_needed: _save()

func _sync_visuals() -> void:
	if is_instance_valid(cart_visual):
		var target := Vector2(270, 578)
		if sim != null:
			var target_id: int = int(sim.cannon_target())
			if target_id != 0:
				var enemy: Dictionary = sim._enemy(target_id)
				if not enemy.is_empty(): target.x = float(enemy.x)
			cart_visual.aim(target)
	if sim == null or sim.status != "active": return
	for unit in sim.units:
		if str(unit.get("role", "")) != "impact": continue
		var visual: Node2D = _ensure_pioneer(int(unit.id))
		visual.sync_unit(unit)

func _ensure_pioneer(unit_id: int) -> Node2D:
	if pioneer_visuals.has(unit_id) and is_instance_valid(pioneer_visuals[unit_id]):
		return pioneer_visuals[unit_id]
	var visual = PioneerRig.new()
	if sim != null:
		visual.flight_duration = float(sim.config.pioneer.flight)
		visual.hit_interval = float(sim.config.pioneer.hit_interval)
	add_child(visual)
	pioneer_visuals[unit_id] = visual
	return visual

func _remove_pioneer(unit_id: int) -> void:
	var visual: Node2D = pioneer_visuals.get(unit_id)
	pioneer_visuals.erase(unit_id)
	if not is_instance_valid(visual): return
	visual.remove_unit()
	visual.queue_free()

func _clear_pioneer_visuals() -> void:
	for visual in pioneer_visuals.values():
		if not is_instance_valid(visual): continue
		visual.remove_unit()
		visual.queue_free()
	pioneer_visuals.clear()

func _reset_visuals() -> void:
	_clear_pioneer_visuals()
	effects.clear()
	if is_instance_valid(cart_visual): cart_visual.reset_visual()

func _add_effect(kind: String, x: float, duration: float) -> void:
	effects.append({"kind": kind, "x": x, "age": 0.0, "duration": duration})
	if effects.size() > 24: effects.pop_front()

func _draw() -> void:
	art.background(self, size, 0 if sim == null else sim.layer_index * 2)
	draw_rect(Rect2(0, 0, 540, 144), Color("#0B202D", 0.8))
	_text(Vector2(270, 37), "DIG & WONDER", 23, CREAM, 500)
	_panel(Rect2(20, 51, 138, 42), INK, 12)
	_icon("coin", Rect2(28, 58, 27, 27))
	draw_string(ThemeDB.fallback_font, Vector2(63, 81), str(profile.get("gold", 0)), HORIZONTAL_ALIGNMENT_LEFT, 85, 21, BRASS)
	_text(Vector2(350, 77), "FASE 1 / POÇO DO ACAMPAMENTO", 12, CREAM, 335)
	var stats: Dictionary = {"hp": 120} if sim == null else sim.stats()
	var life: float = float(stats.hp) if sim == null else float(sim.cart_hp)
	_text(Vector2(270, 111), "CARRINHO  /  %d de %d PV" % [ceili(life), int(stats.hp)], 13, CREAM, 480)
	_bar(Rect2(36, 121, 468, 13), life / float(stats.hp), CORAL)
	draw_set_transform(Vector2(sin(art.clock * 90) * impact_shake * 10, 0))
	_well()
	if sim != null:
		for enemy in sim.enemies: _mole(enemy)
		for unit in sim.units:
			if str(unit.get("role", "")) == "defender": _robot(Vector2(320, lerpf(300, 592, clampf(float(unit.age) / float(unit.duration), 0.0, 1.0))), unit)
		for shot in sim.projectiles: _shot(shot)
	_draw_effects()
	draw_set_transform(Vector2.ZERO)
	_controls()
	for notice in notices:
		var at := Vector2(notice.x, notice.y - (1.8 - float(notice.life)) * 18)
		_text(at + Vector2(1, 2), str(notice.text), 17, INK, 230)
		_text(at, str(notice.text), 17, Color(BRASS, minf(1, float(notice.life) * 3)), 230)
	if screen == "start": _start_panel()
	elif screen == "import": _import_panel()
	elif sim != null and sim.status != "active": _result_panel()
	if not error.is_empty():
		draw_rect(Rect2(0, 0, 540, 960), Color("#0B202D", 0.92))
		_panel(Rect2(26, 340, 488, 230), INK, 18)
		_text(Vector2(270, 390), "PROGRESSO PRESERVADO", 20, CORAL, 450)
		_wrap(Rect2(48, 418, 444, 120), error, 18, CREAM)

func _well() -> void:
	# Open shaft: receding walls, upper-left lip light and depth fog.
	for band in range(48):
		var t := band / 47.0
		draw_rect(Rect2(32, 315 + band * 8.9, 476, 9.5), Color("#25444C").lerp(Color("#07151F"), t))
	for side in range(2):
		var points := PackedVector2Array([Vector2(27, 315), Vector2(74, 338), Vector2(96, 737), Vector2(27, 737)])
		if side == 1:
			for i in range(points.size()): points[i].x = 540 - points[i].x
		draw_colored_polygon(points, Color("#5B4B3D") if side == 0 else Color("#3B3935"))
		for ledge in range(8):
			var y := 341.0 + ledge * 50
			var x := 73.0 + ledge * 2.8
			if side == 1: x = 540 - x
			var outward := -1.0 if side == 0 else 1.0
			draw_line(Vector2(x, y), Vector2(x + outward * 39, y - 15), Color("#9B7850", 0.45 - ledge * 0.035), 3, true)
		draw_line(points[1], points[2], Color("#C59A62", 0.35 if side == 0 else 0.12), 3, true)
	# Reuse granular earth along the shaft walls, avoiding bare geometric faces.
	draw_set_transform(Vector2(24, 325), PI * 0.5)
	draw_texture_rect(soil, Rect2(0, -64, 414, 64), false, Color(0.60, 0.63, 0.63))
	draw_set_transform(Vector2(516, 325), PI * 0.5)
	draw_texture_rect(soil, Rect2(0, 0, 414, 64), false, Color(0.39, 0.46, 0.49))
	draw_set_transform(Vector2(sin(art.clock * 90) * impact_shake * 10, 0))
	var index: int = 0 if sim == null else int(sim.layer_index)
	var complete: bool = sim != null and sim.final_encounter
	_text(Vector2(270, 358), "POÇO / %d DE 6 CAMADAS" % index, 14, CREAM, 360)
	_text(Vector2(270, 385), "TORRÃO EM CAMPO" if sim != null and not sim.enemies.is_empty() else "CANHÕES EM VIGÍLIA", 12, CORAL if spawn_flash > 0 else TEAL, 350)
	var material := "terra" if sim == null else str(sim.material())
	_text(Vector2(270, 650), "ENCONTRO FINAL" if complete else "FRENTE / " + material.to_upper(), 14, CREAM, 420)
	if not complete:
		var hp := 44.0 if sim == null else float(sim.layer_hp)
		var maximum := 44.0 if sim == null else float(sim.layer_max)
		var column: int = {"terra": 0, "argila": 1, "pedra": 2, "cristal": 3}.get(material, 0)
		var tile := terrain.get_size() / Vector2(4, 3)
		var source := Rect2(Vector2(int(column) * tile.x, posmod(index, 3) * tile.y), tile)
		if material in ["terra", "argila"]:
			draw_texture_rect(soil, Rect2(36, 595, 468, 156), false, Color(1, 0.83, 0.68) if layer_flash > 0 else Color.WHITE)
		else:
			draw_texture_rect_region(terrain, Rect2(44, 607, 452, 130), source, Color(1, 0.8, 0.7) if layer_flash > 0 else Color.WHITE)
		_panel(Rect2(97, 653, 346, 65), Color("#0B202D", 0.88), 10)
		_text(Vector2(270, 673), "FRENTE / " + material.to_upper(), 13, CREAM, 330)
		_bar(Rect2(111, 683, 318, 9), hp / maximum, TEAL)
		_text(Vector2(270, 709), "%d / %d RESISTÊNCIA" % [ceili(hp), int(maximum)], 12, CREAM, 330)
		if hp < maximum * 0.6:
			draw_polyline(PackedVector2Array([Vector2(210, 612), Vector2(242, 642), Vector2(225, 673), Vector2(260, 712)]), INK, 4, true)
	else:
		_text(Vector2(270, 673), "ÚLTIMA DEFESA", 24, TEAL, 410)

func _mole(enemy: Dictionary) -> void:
	var p := Vector2(float(enemy.x), 615 + sin(art.clock * 3 + float(enemy.id)) * 1.5)
	_oval(p + Vector2(0, 14), Vector2(83, 20), Color("#07151F", 0.75))
	var pose := 3 if float(enemy.get("hurt", 0)) > 0 else (2 if float(enemy.get("throw_pose", 0)) > 0 else (1 if enemy.telegraph else 0))
	var emergence := clampf(float(enemy.get("emerge", 1)), 0.05, 1)
	var breath := sin(art.clock * 3.5 + float(enemy.id)) * 1.3
	_mole_sprite(p + Vector2(0, (1 - emergence) * 16), pose, Color.WHITE, Vector2(112 + breath, (146 - breath) * emergence))
	if spawn_flash > 0: _text(p - Vector2(0, 105), "TORRÃO", 12, BRASS, 100)
	if enemy.telegraph:
		_text(p - Vector2(0, 88), "! ARREMESSO", 12, CORAL, 125)
	_bar(Rect2(p.x - 31, p.y + 6, 62, 5), float(enemy.hp) / float(enemy.max_hp), CORAL)

func _mole_sprite(feet: Vector2, pose: int, tint: Color, dimensions: Vector2) -> void:
	var tile := mole_atlas.get_size() / Vector2(4, 1)
	draw_texture_rect_region(mole_atlas, Rect2(feet - Vector2(dimensions.x * 0.5, dimensions.y * 0.86), dimensions), Rect2(Vector2(pose * tile.x, 0), tile), tint)

func _robot(p: Vector2, unit: Dictionary) -> void:
	var bob := sin(art.clock * 6) * 2
	p.y += bob
	var pose := 2 if float(unit.get("hurt", 0)) > 0 else (1 if unit.get("at_front", false) and float(unit.timer) > 0.72 else 0)
	var tile := robot_atlas.get_size() / Vector2(3, 1)
	_oval(p + Vector2(0, 21), Vector2(70, 11), Color("#07151F", 0.6))
	draw_texture_rect_region(robot_atlas, Rect2(p - Vector2(49, 73), Vector2(98, 98)), Rect2(Vector2(pose * tile.x, 0), tile))
	_bar(Rect2(p.x - 24, p.y + 28, 48, 5), float(unit.hp) / float(sim.config.excavator.hp), TEAL)
	if unit.get("at_front", false): _text(p - Vector2(0, 80), "DEFENSOR / %.0fs" % maxf(0, float(sim.config.excavator.duration) - float(unit.age) + float(unit.duration)), 10, TEAL, 130)

func _draw_effects() -> void:
	for fx in effects:
		var t := float(fx.age) / float(fx.duration)
		var p := Vector2(float(fx.x), 609)
		if fx.kind == "mole_defeat": _mole_sprite(p + Vector2(0, t * 17), 3, Color(1, 1, 1, 1 - t), Vector2(112, 146))
		for particle in range(9):
			var angle := PI + particle * PI / 8.0
			var velocity := Vector2(cos(angle), sin(angle)) * (28 + particle % 3 * 17)
			var at := p + velocity * t + Vector2(0, t * t * 34)
			draw_circle(at, 4.5 * (1 - t), Color("#AF8150", 1 - t))

func _shot(shot: Dictionary) -> void:
	var t := clampf(float(shot.age) / float(shot.duration), 0, 1)
	var a := Vector2(175 if int(shot.get("side", 0)) == 0 else 365, 292)
	var b := Vector2(float(shot.x), 578)
	if shot.kind == "soil":
		a = Vector2(float(shot.x), 573)
		b = Vector2(320, 580) if sim != null and not sim._unit(int(shot.target)).is_empty() else Vector2(270, 245)
	var p := a.lerp(b, t)
	if shot.kind == "soil":
		p.x += sin(t * PI) * 25
		draw_circle(p, 6, Color("#9D603D"))
	else:
		draw_line(p, p - (b - a).normalized() * 14, BRASS, 4, true)

func _controls() -> void:
	_panel(Rect2(0, 747, 540, 213), INK, 17)
	var ready: bool = sim != null and sim.cooldown <= 0 and sim.status == "active" and sim.units.size() < int(sim.config.limits.units)
	var ended: bool = sim != null and sim.status != "active"
	var detail := "TENTATIVA ENCERRADA" if ended else ("4 GOLPES / PRONTA" if ready else ("FRENTE OCUPADA" if sim != null and sim.cooldown <= 0 else "%.1f s" % (3.0 if sim == null else sim.cooldown)))
	_button("launch", "LANÇAR PIONEIRA", detail, CORAL, ready)
	_bar(Rect2(35, 830, 212, 5), 0 if sim == null or ended else 1.0 - float(sim.cooldown) / 3.0, TEAL)
	var support_ready: bool = sim != null and sim.support_cooldown <= 0 and sim.status == "active" and sim.units.size() < int(sim.config.limits.units)
	var support_active := false
	if sim != null:
		for unit in sim.units:
			if unit.get("role", "") == "defender":
				support_ready = false
				support_active = true
	var support_detail := "PRONTO" if support_ready else ("ENCERRADO" if ended else ("EM CAMPO" if support_active else ("OCUPADO" if sim != null and sim.support_cooldown <= 0 else "%.1fs" % (20.0 if sim == null else sim.support_cooldown))))
	_button("support", "ESCAVADOR", "GRÁTIS / " + support_detail, TEAL, support_ready)
	for slot in [1]:
		var r := Rect2(276 + slot * 126, 758, 118, 88)
		_panel(r, Color("#24434D"), 10)
		_icon("lock", Rect2(r.get_center() - Vector2(12, 27), Vector2(24, 24)))
		_text(r.get_center() + Vector2(0, 22), "BLOQUEADO", 11, Color("#B7D6D3"), 110)
	if sim != null:
		for kind in ["damage", "hull"]:
			var level := int(profile.get(kind + "_level", 0))
			var cost: int = sim.upgrade_cost(kind)
			var current: Dictionary = sim.stats()
			var next_multiplier: float = 1.0 + mini(level + 1, 10) * float(sim.config.upgrades.damage.bonus)
			var label := "DANO / NV.%d" % level if kind == "damage" else "VIDA / NV.%d" % level
			var upgrade_detail := "Canhão %.2f → %.2f | Broca %.2f → %.2f" % [current.cannon_damage, float(sim.config.cart.cannon_damage) * next_multiplier, current.drill_damage, float(sim.config.pioneer.damage) * next_multiplier] if kind == "damage" else "Máxima %d → %d PV" % [int(current.hp), int(current.hp) + (0 if level >= 10 else int(sim.config.upgrades.hull.bonus))]
			var r := Layout.button(kind)
			var enabled: bool = level < 10 and int(profile.gold) >= cost
			art.plate(self, r, BRASS, r.has_point(pointer), enabled, false)
			_text(Vector2(r.get_center().x, r.position.y + 17), label, 12, CREAM, r.size.x - 20)
			_text(Vector2(r.get_center().x, r.position.y + 37), upgrade_detail, 10, CREAM, r.size.x - 12)
			_text(Vector2(r.get_center().x, r.position.y + 56), "NÍVEL MÁXIMO" if level >= 10 else "CUSTO: %d OURO / TEM: %d" % [cost, profile.gold], 11, BRASS if not enabled else INK, r.size.x - 20)
	_text(Vector2(270, 862), "MELHORIAS PERMANENTES", 11, TEAL, 490)

func _start_panel() -> void:
	draw_rect(Rect2(0, 330, 540, 416), Color("#0B202D", 0.88))
	_text(Vector2(270, 392), "DEFENDA A EXPEDIÇÃO", 26, CREAM, 485)
	_wrap(Rect2(45, 420, 450, 95), "A Pioneira salta e golpeia 4 vezes antes de quebrar. O Escavador luta, minera e protege por 10 s. Canhões automáticos defendem o carrinho.", 17, CREAM)
	_wrap(Rect2(45, 520, 450, 80), "Lançar não custa ouro. Na derrota, ouro e melhorias ficam com você; a tentativa recomeça.", 16, TEAL)
	_button("start", "INICIAR TENTATIVA", "PIONEIRA: 3 s DE COOLDOWN", CORAL, true)

func _result_panel() -> void:
	draw_rect(Rect2(0, 330, 540, 416), Color("#0B202D", 0.94))
	var victory: bool = sim.status == "victory"
	_text(Vector2(270, 397), "FASE SUPERADA!" if victory else "CARRINHO PAROU", 27, TEAL if victory else CORAL, 480)
	_wrap(Rect2(45, 432, 450, 90), "Ouro e melhorias estão salvos. Você pode repetir esta fase." if victory else "A tentativa terminou. Todo ouro recebido e cada melhoria comprada continuam com você.", 19, CREAM)
	_text(Vector2(270, 560), "%d OURO / DANO NV.%d / VIDA NV.%d" % [profile.gold, profile.damage_level, profile.hull_level], 16, BRASS, 480)
	_button("retry", "REPETIR FASE" if victory else "TENTAR NOVAMENTE", "VIDA RESTAURADA / COOLDOWN COMPLETO", CORAL, true)

func _import_panel() -> void:
	draw_rect(Rect2(0, 300, 540, 450), Color("#0B202D", 0.96))
	_text(Vector2(270, 350), "SEU PROGRESSO ANTIGO", 24, CREAM, 485)
	var gold := int(legacy.get("coins", 0)) + int(legacy.get("inventory", 0))
	var level := clampi(int(legacy.get("drill_level", 1)) - 1, 0, 10)
	_wrap(Rect2(40, 380, 460, 75), "A direção do jogo mudou. Antes de importar, será criada uma cópia integral do arquivo original.", 18, CREAM)
	_wrap(Rect2(40, 470, 460, 70), "%d moedas + %d da mochila = %d ouro. Reforços da broca: dano nível %d." % [int(legacy.get("coins", 0)), int(legacy.get("inventory", 0)), gold, level], 18, BRASS)
	_wrap(Rect2(40, 555, 460, 80), "Chaves, baús, skins, equipamentos e fases antigas ficam arquivados, sem conversão. Níveis acima de 10 ficam na cópia.", 15, TEAL)
	_button("import", "CRIAR CÓPIA E IMPORTAR", "O ARQUIVO ANTIGO NÃO SERÁ ALTERADO", CORAL, true)

func _panel(rect: Rect2, tint: Color, radius: int) -> void:
	var key := tint.to_html() + str(radius)
	if not styles.has(key):
		var style := StyleBoxFlat.new()
		style.bg_color = tint
		style.set_corner_radius_all(radius)
		styles[key] = style
	draw_style_box(styles[key], rect)

func _button(id: String, label: String, detail: String, tint: Color, enabled: bool) -> void:
	var r := Layout.button(id)
	art.plate(self, r, tint, r.has_point(pointer), enabled, id in ["launch", "start", "retry", "import"])
	_text(r.get_center() + Vector2(0, -3), label, 12 if id == "support" else (13 if id in ["damage", "hull"] else 17), INK if enabled else CREAM, r.size.x - 16)
	_text(r.get_center() + Vector2(0, 20), detail, 10 if id in ["damage", "hull", "support"] else 12, INK if enabled else CREAM, r.size.x - 14)

func _bar(rect: Rect2, ratio: float, tint: Color) -> void:
	_panel(rect, INK, 5)
	if ratio > 0: _panel(Rect2(rect.position, Vector2(rect.size.x * clampf(ratio, 0, 1), rect.size.y)), tint, 5)

func _text(at: Vector2, value: String, font_size: int, tint: Color, width: float) -> void:
	draw_string(ThemeDB.fallback_font, at - Vector2(width * 0.5, 0), value, HORIZONTAL_ALIGNMENT_CENTER, width, font_size, tint)

func _wrap(rect: Rect2, value: String, font_size: int, tint: Color) -> void:
	var paragraph := TextParagraph.new()
	paragraph.width = rect.size.x
	paragraph.alignment = HORIZONTAL_ALIGNMENT_CENTER
	paragraph.add_string(value, ThemeDB.fallback_font, font_size)
	paragraph.draw(get_canvas_item(), rect.position, tint)

func _icon(id: String, rect: Rect2) -> void:
	if not icons.has(id): icons[id] = load("res://assets/ui/" + id + ".svg")
	draw_texture_rect(icons[id], rect, false)

func _oval(center: Vector2, dimensions: Vector2, tint: Color) -> void:
	var points := PackedVector2Array()
	for i in range(24): points.append(center + Vector2(cos(i * TAU / 24), sin(i * TAU / 24)) * dimensions * 0.5)
	draw_colored_polygon(points, tint)

