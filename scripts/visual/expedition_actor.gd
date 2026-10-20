extends Node2D
## Decorative rig. All pivots are local: no input, collision, economy or save access.

class Eyes extends Node2D:
	var mechanical := false
	func _draw() -> void:
		for side in [-1.0, 1.0]:
			var center := Vector2(side * 17.0, 0)
			draw_set_transform(center, 0, Vector2(0.8, 1.0))
			draw_circle(Vector2.ZERO, 12, Color("#39D4C1") if mechanical else Color("#FFF4D8"))
			draw_circle(Vector2(2, 1), 7, Color("#17303D"))
			draw_circle(Vector2(0, -3), 2.5, Color.WHITE)
		draw_set_transform(Vector2.ZERO, 0, Vector2.ONE)

var kind := "lia"
var idle_pivot: Node2D
var impact_pivot: Node2D
var head_pivot: Node2D
var front_arm: Node2D
var tool: Sprite2D
var eyes: Eyes
var tip_drive: Node2D
var core: Sprite2D
var model_details: Node2D
var extra_tips: Array[Sprite2D] = []
var idle_tween: Tween
var blink_tween: Tween
var sway_tween: Tween
var action_tween: Tween
var squash_tween: Tween
var tip_tween: Tween
var mining_left := 0.0
var event_count := 0
var break_count := 0
const TIP_REST := 0.0
const ARM_REST := -0.8

func _ready() -> void:
	texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	idle_pivot = _pivot(self, "Respiracao")
	impact_pivot = _pivot(idle_pivot, "Impacto")
	if kind == "drill":
		_build_drill()
	else:
		_build_miner()
	_start_idle()
	_start_blink()

func _pivot(parent: Node, title: String, at := Vector2.ZERO) -> Node2D:
	var pivot := Node2D.new()
	pivot.name = title
	pivot.position = at
	parent.add_child(pivot)
	return pivot

func _part(parent: Node, file: String, offset: Vector2) -> Sprite2D:
	var sprite := Sprite2D.new()
	sprite.name = file.get_basename()
	sprite.texture = load("res://assets/personagens/" + file)
	sprite.centered = false
	sprite.position = offset
	# SVG sources are rasterized at 2x their logical size to keep contours smooth.
	sprite.scale = Vector2(0.5, 0.5)
	parent.add_child(sprite)
	return sprite

func _build_miner() -> void:
	var back_arm := _pivot(impact_pivot, "BracoTraseiro", Vector2(-36, 1))
	back_arm.rotation = 0.2
	_part(back_arm, kind + "_braco.svg", Vector2(-16, -12))
	_part(impact_pivot, kind + "_corpo.svg", Vector2(-65, -15))
	front_arm = _pivot(impact_pivot, "BracoFerramenta", Vector2(36, 1))
	front_arm.rotation = ARM_REST
	_part(front_arm, kind + "_braco.svg", Vector2(-16, -12))
	var tool_pivot := _pivot(front_arm, "Ferramenta", Vector2(17, 58))
	tool_pivot.rotation = 0.8
	tool_pivot.scale = Vector2(0.58, 0.58)
	tool = _part(tool_pivot, "cristal.svg" if kind == "lia" else "chave.svg", Vector2(-27, -40) if kind == "lia" else Vector2(-29, -47))
	head_pivot = _pivot(impact_pivot, "Cabeca", Vector2(0, -54))
	_part(head_pivot, kind + "_cabeca.svg", Vector2(-70, -85))
	eyes = Eyes.new()
	eyes.name = "Olhos"
	eyes.position = Vector2(0, 8)
	head_pivot.add_child(eyes)

func _build_drill() -> void:
	for at in [Vector2(-53, 72), Vector2(53, 72)]:
		var mount := _pivot(impact_pivot, "PontaAuxiliar", at)
		mount.rotation = TIP_REST
		mount.scale = Vector2(0.6, 0.6)
		var extra := _illustration(mount, Rect2(730, 35, 490, 615), Rect2(-40, -5, 80, 108))
		extra.visible = false
		extra_tips.append(extra)
	_illustration(impact_pivot, Rect2(15, 15, 685, 625), Rect2(-110, -100, 220, 200))
	model_details = preload("res://scripts/visual/model_details.gd").new()
	impact_pivot.add_child(model_details)
	var core_pivot := _pivot(impact_pivot, "Nucleo", Vector2(0, -79))
	core_pivot.scale = Vector2(0.7, 0.7)
	core = _part(core_pivot, "broca_nucleo.svg", Vector2(-25, -30))
	core.hide()
	tip_drive = _pivot(impact_pivot, "EixoPonta", Vector2(0, 70))
	tip_drive.rotation = TIP_REST
	_illustration(tip_drive, Rect2(730, 35, 490, 615), Rect2(-40, -5, 80, 108))
	eyes = Eyes.new()
	eyes.name = "Olhos"
	eyes.mechanical = true
	eyes.position = Vector2(0, -31)
	impact_pivot.add_child(eyes)

func _illustration(parent: Node, region: Rect2, target: Rect2) -> Sprite2D:
	var atlas := AtlasTexture.new()
	atlas.atlas = load("res://assets/ilustracoes/maquina-baus-atlas.png")
	atlas.region = region
	var sprite := Sprite2D.new()
	sprite.texture = atlas
	sprite.centered = false
	sprite.position = target.position
	sprite.scale = target.size / region.size
	parent.add_child(sprite)
	return sprite

func _start_idle() -> void:
	idle_tween = create_tween().set_loops()
	idle_tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	idle_tween.tween_property(idle_pivot, "scale", Vector2(1.018, 0.98), 1.25)
	idle_tween.tween_property(idle_pivot, "scale", Vector2.ONE, 1.25)
	if kind == "drill":
		sway_tween = create_tween().set_loops()
		sway_tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		sway_tween.tween_property(idle_pivot, "rotation", 0.025, 1.5)
		sway_tween.tween_property(idle_pivot, "rotation", -0.025, 1.5)

func _start_blink() -> void:
	# Fixed stagger avoids consuming the world's/rewards' random stream.
	var delay := 3.4 if kind == "lia" else (4.1 if kind == "bento" else 4.7)
	blink_tween = create_tween().set_loops()
	blink_tween.tween_interval(delay)
	blink_tween.tween_property(eyes, "scale:y", 0.06, 0.09)
	blink_tween.tween_property(eyes, "scale:y", 1.0, 0.12)

func mining_event(broke_block: bool) -> void:
	event_count += 1
	if broke_block:
		break_count += 1
	if kind == "drill":
		mining_left = 0.24
		if tip_tween == null or not tip_tween.is_valid():
			tip_tween = create_tween().set_loops()
			tip_tween.tween_property(tip_drive, "rotation", TIP_REST - 0.2, 0.075)
			tip_tween.tween_property(tip_drive, "rotation", TIP_REST + 0.2, 0.075)
		if broke_block:
			if squash_tween != null and squash_tween.is_valid():
				squash_tween.kill()
			squash_tween = create_tween()
			squash_tween.tween_property(impact_pivot, "scale", Vector2(1.1, 0.86), 0.055)
			squash_tween.tween_property(impact_pivot, "scale", Vector2(0.96, 1.06), 0.075)
			squash_tween.tween_property(impact_pivot, "scale", Vector2.ONE, 0.11)
	else:
		if action_tween != null and action_tween.is_valid():
			action_tween.kill()
		action_tween = create_tween()
		action_tween.tween_property(front_arm, "rotation", ARM_REST - 0.7, 0.06)
		action_tween.tween_property(front_arm, "rotation", ARM_REST + 0.22, 0.08)
		action_tween.tween_property(front_arm, "rotation", ARM_REST, 0.1)

func _process(delta: float) -> void:
	if kind != "drill" or mining_left <= 0.0:
		return
	mining_left = maxf(0.0, mining_left - delta)
	if mining_left == 0.0:
		if tip_tween != null and tip_tween.is_valid():
			tip_tween.kill()
		tip_drive.rotation = TIP_REST

func set_model(model: String, accent: Color) -> void:
	if kind != "drill":
		return
	for extra in extra_tips:
		extra.visible = model in ["prism", "trinca"]
	model_details.set_model(model)
	# Skin accent stays visual; modifiers remain in the existing equipment system.
	core.modulate = Color(accent.r, accent.g, accent.b).lightened(0.25)
