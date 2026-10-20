extends Node2D
## Cosmetic cart rig. AnimationPlayer owns idle/death; cannon Tweens own recoil.

const Cannon = preload("res://scripts/visual/cannon_rig.gd")
const MACHINE_ATLAS := "res://assets/ilustracoes/maquina-baus-atlas.png"
const BODY_REGION := Rect2(15, 15, 685, 625)

var chassis: Node2D
var body: Sprite2D
var animation_player: AnimationPlayer
var cannons: Array[Node2D] = []
var hit_tween: Tween
var dead := false

func _ready() -> void:
	name = "CarrinhoVisual"
	chassis = Node2D.new()
	chassis.name = "Chassi"
	add_child(chassis)
	body = Sprite2D.new()
	body.name = "Ilustracao"
	var atlas := AtlasTexture.new()
	atlas.atlas = load(MACHINE_ATLAS)
	atlas.region = BODY_REGION
	body.texture = atlas
	body.centered = true
	body.scale = Vector2(198.0 / BODY_REGION.size.x, 180.0 / BODY_REGION.size.y)
	chassis.add_child(body)
	for side in 2:
		var cannon := Cannon.new()
		cannon.name = "CanhaoEsquerdo" if side == 0 else "CanhaoDireito"
		cannon.position = Vector2(-95 if side == 0 else 95, 7)
		chassis.add_child(cannon)
		cannons.append(cannon)
	animation_player = AnimationPlayer.new()
	animation_player.name = "AnimationPlayer"
	animation_player.root_node = NodePath("..")
	add_child(animation_player)
	_build_animations()
	animation_player.play("idle")

func _build_animations() -> void:
	var library := AnimationLibrary.new()
	var idle := Animation.new()
	idle.resource_name = "idle"
	idle.length = 3.2
	idle.loop_mode = Animation.LOOP_LINEAR
	_add_value_track(idle, NodePath("Chassi:position"), [
		[0.0, Vector2.ZERO], [1.6, Vector2(0, -1.15)], [3.2, Vector2.ZERO],
	])
	_add_value_track(idle, NodePath("Chassi:rotation"), [
		[0.0, 0.0], [1.6, -0.009], [3.2, 0.0],
	])
	var defeat := Animation.new()
	defeat.resource_name = "defeat"
	defeat.length = 0.42
	_add_value_track(defeat, NodePath("Chassi:scale"), [
		[0.0, Vector2.ONE], [0.10, Vector2(1.04, 0.92)], [0.22, Vector2(0.97, 1.04)], [0.42, Vector2(1.0, 0.88)],
	])
	_add_value_track(defeat, NodePath("Chassi:position"), [
		[0.0, chassis.position], [0.10, Vector2(0, 6)], [0.22, Vector2(0, 3)], [0.42, Vector2(0, 20)],
	])
	_add_value_track(defeat, NodePath("Chassi/Ilustracao:modulate"), [
		[0.0, Color.WHITE], [0.10, Color("#FFD5A4")], [0.22, Color("#91A0A1")], [0.42, Color("#79898D")],
	])
	library.add_animation("idle", idle)
	library.add_animation("defeat", defeat)
	animation_player.add_animation_library("", library)
	animation_player.animation_finished.connect(_on_animation_finished)

func _add_value_track(animation: Animation, path: NodePath, keys: Array) -> void:
	var track := animation.add_track(Animation.TYPE_VALUE)
	animation.track_set_path(track, path)
	for key in keys:
		animation.track_insert_key(track, float(key[0]), key[1])

func aim(target_local: Vector2) -> void:
	if dead: return
	for cannon in cannons:
		var mount := position + chassis.position + cannon.position.rotated(chassis.rotation)
		cannon.aim_direction(target_local - mount, chassis.rotation)

func fire(side: int) -> void:
	if dead or side < 0 or side >= cannons.size(): return
	cannons[side].fire()

func hit() -> void:
	if dead or not is_instance_valid(body): return
	if hit_tween != null and hit_tween.is_valid(): hit_tween.kill()
	body.modulate = Color(1.0, 0.64, 0.62)
	hit_tween = create_tween()
	hit_tween.set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	hit_tween.tween_property(body, "modulate", Color.WHITE, 0.22)

func defeat() -> void:
	if dead: return
	dead = true
	stop_attack_responses()
	animation_player.play("defeat")

func reset_visual() -> void:
	stop_attack_responses()
	dead = false
	chassis.position = Vector2.ZERO
	chassis.rotation = 0.0
	chassis.scale = Vector2.ONE
	if is_instance_valid(body): body.modulate = Color.WHITE
	for cannon in cannons: cannon.reset_visual()
	animation_player.play("idle")

func suspend(value: bool) -> void:
	process_mode = Node.PROCESS_MODE_DISABLED if value else Node.PROCESS_MODE_INHERIT

func stop_attack_responses() -> void:
	if hit_tween != null and hit_tween.is_valid(): hit_tween.kill()
	if is_instance_valid(body): body.modulate = Color.WHITE
	for cannon in cannons: cannon.stop_visuals()

func _on_animation_finished(animation_name: StringName) -> void:
	if animation_name == "defeat":
		# Hold the final defeated pose; retry resets every visual channel.
		animation_player.pause()
