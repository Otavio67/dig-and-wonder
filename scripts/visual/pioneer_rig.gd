extends Node2D
## A render-only drill. Its root position comes from the simulation's unit age.
## AnimationPlayer is manually sought from that same age so visual attack beats
## cannot drift away from the deterministic combat events.

const MACHINE_ATLAS := "res://assets/ilustracoes/maquina-baus-atlas.png"
const BODY_REGION := Rect2(15, 15, 685, 625)
const TIP_REGION := Rect2(730, 35, 490, 615)

var body_pivot: Node2D
var tip_pivot: Node2D
var animation_player: AnimationPlayer
var hit_interval := 0.7
var flight_duration := 0.8
var anim_state := ""
var impact_tween: Tween
var _ring_alpha := 0.0
var _ring_radius := 14.0

func _ready() -> void:
	name = "PioneiraVisual"
	visible = false
	body_pivot = Node2D.new()
	body_pivot.name = "CorpoPivot"
	add_child(body_pivot)
	var body := _sprite(BODY_REGION, Vector2(0, -14.5), Vector2(48, 45))
	body.name = "Carcaca"
	body_pivot.add_child(body)
	var eyes := EyePair.new()
	eyes.name = "Olhos"
	eyes.position = Vector2(0, -24)
	body_pivot.add_child(eyes)
	var spring := SpringVisual.new()
	spring.name = "Mola"
	add_child(spring)
	tip_pivot = Node2D.new()
	tip_pivot.name = "PivoBroca"
	tip_pivot.position = Vector2(0, 7)
	add_child(tip_pivot)
	var tip := _sprite(TIP_REGION, Vector2(0, 17), Vector2(38, 34))
	tip.name = "Ponta"
	tip_pivot.add_child(tip)
	animation_player = AnimationPlayer.new()
	animation_player.name = "AnimationPlayer"
	animation_player.root_node = NodePath("..")
	animation_player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
	add_child(animation_player)
	_build_animations()

class EyePair extends Node2D:
	func _draw() -> void:
		for side in [-1.0, 1.0]:
			var center := Vector2(side * 5.0, 0)
			draw_circle(center, 3.3, Color("#39D4C1"))
			draw_circle(center + Vector2(0.7, 0.3), 1.6, Color("#17303D"))
			draw_circle(center + Vector2(0, -0.7), 0.8, Color("#FFF4D8"))

class SpringVisual extends Node2D:
	func _draw() -> void:
		draw_line(Vector2(0, 3), Vector2(0, 6), Color("#17303D"), 3, true)
		var points := PackedVector2Array([Vector2(-8, 6), Vector2(8, 9), Vector2(-8, 12), Vector2(8, 15), Vector2(-8, 18)])
		draw_polyline(points, Color("#F2B84B"), 2.6, true)
		draw_line(Vector2(0, 18), Vector2(0, 22), Color("#17303D"), 2.5, true)

func _sprite(region: Rect2, at: Vector2, target_size: Vector2) -> Sprite2D:
	var atlas := AtlasTexture.new()
	atlas.atlas = load(MACHINE_ATLAS)
	atlas.region = region
	var sprite := Sprite2D.new()
	sprite.texture = atlas
	sprite.centered = true
	sprite.position = at
	sprite.scale = target_size / region.size
	return sprite

func configure(flight: float, strike_interval: float) -> void:
	flight_duration = flight
	hit_interval = strike_interval
	if is_node_ready(): _build_animations()

func _build_animations() -> void:
	if not is_instance_valid(animation_player) or not is_instance_valid(body_pivot) or not is_instance_valid(tip_pivot): return
	var library := AnimationLibrary.new()
	var spin := Animation.new()
	spin.resource_name = "flight_spin"
	spin.length = 0.42
	spin.loop_mode = Animation.LOOP_LINEAR
	_add_track(spin, NodePath("PivoBroca:rotation"), [[0.0, -0.15], [0.21, TAU * 1.5], [0.42, TAU * 3.0 - 0.15]])
	var cycle := Animation.new()
	cycle.resource_name = "strike_cycle"
	cycle.length = maxf(0.12, hit_interval)
	cycle.loop_mode = Animation.LOOP_LINEAR
	_add_track(cycle, NodePath("CorpoPivot:scale"), [
		[0.0, Vector2(1.08, 0.88)],
		[0.09, Vector2(0.94, 1.08)],
		[0.22, Vector2(1.03, 0.97)],
		[0.38, Vector2.ONE],
		[cycle.length * 0.68, Vector2(0.96, 1.05)],
		[cycle.length * 0.86, Vector2(1.02, 0.97)],
		[cycle.length, Vector2(1.08, 0.88)],
	])
	_add_track(cycle, NodePath("PivoBroca:rotation"), [
		[0.0, 0.0], [0.08, 0.12], [0.24, 0.18], [0.42, 0.0],
		[cycle.length * 0.70, -0.18], [cycle.length * 0.88, -0.12], [cycle.length, 0.0],
	])
	library.add_animation("flight_spin", spin)
	library.add_animation("strike_cycle", cycle)
	if animation_player.has_animation_library(""):
		animation_player.clear_animation_library("")
	animation_player.add_animation_library("", library)
	anim_state = ""

func _add_track(animation: Animation, path: NodePath, keys: Array) -> void:
	var track := animation.add_track(Animation.TYPE_VALUE)
	animation.track_set_path(track, path)
	for key in keys:
		animation.track_insert_key(track, float(key[0]), key[1])

func sync_unit(unit: Dictionary) -> void:
	visible = true
	var age := float(unit.age)
	var duration := float(unit.duration)
	if age < duration:
		var t := clampf(age / maxf(0.001, duration), 0.0, 1.0)
		position = Vector2(270, lerpf(300, 580, t * t))
		if anim_state != "flight_spin":
			anim_state = "flight_spin"
			animation_player.play(anim_state)
		animation_player.seek(fposmod(age, 0.42), true)
	else:
		var elapsed := maxf(0.0, age - duration)
		var phase := fposmod(elapsed, hit_interval)
		var hop := sin(phase / maxf(0.001, hit_interval) * PI) * 9.0
		position = Vector2(270, 580 - hop)
		if anim_state != "strike_cycle":
			anim_state = "strike_cycle"
			animation_player.play(anim_state)
		animation_player.seek(phase, true)

func confirm_impact() -> void:
	if not visible: return
	if impact_tween != null and impact_tween.is_valid(): impact_tween.kill()
	_ring_alpha = 1.0
	_ring_radius = 13.0
	queue_redraw()
	impact_tween = create_tween()
	impact_tween.set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	impact_tween.tween_property(self, "_ring_radius", 45.0, 0.22)
	impact_tween.parallel().tween_property(self, "_ring_alpha", 0.0, 0.22)

func remove_unit() -> void:
	if not visible: return
	_cancel_effects()
	visible = false
	anim_state = ""
	if animation_player.is_playing(): animation_player.stop()
	body_pivot.scale = Vector2.ONE
	tip_pivot.rotation = 0.0

func _cancel_effects() -> void:
	if impact_tween != null and impact_tween.is_valid(): impact_tween.kill()
	_ring_alpha = 0.0
	_ring_radius = 13.0
	queue_redraw()

func _draw() -> void:
	if _ring_alpha > 0.01:
		draw_arc(Vector2(0, 42), _ring_radius, 0, TAU, 32, Color("#FFC965", _ring_alpha), 2.0, true)

func suspend(value: bool) -> void:
	process_mode = Node.PROCESS_MODE_DISABLED if value else Node.PROCESS_MODE_INHERIT
