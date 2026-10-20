extends Node2D
## Independent visual mount. Recoil is cosmetic and never feeds the simulation.

class Barrel extends Node2D:
	var flash_alpha := 0.0

	func _process(_delta: float) -> void:
		if flash_alpha > 0.0:
			queue_redraw()

	func _draw() -> void:
		draw_rect(Rect2(-12, 4, 24, 52), Color("#17303D"))
		draw_rect(Rect2(-8, 8, 16, 42), Color("#7D9390"))
		draw_rect(Rect2(-10, 4, 20, 7), Color("#F2B84B"))
		draw_rect(Rect2(-14, 52, 28, 10), Color("#17303D"))
		draw_rect(Rect2(-10, 54, 20, 4), Color("#C8D6C8"))
		if flash_alpha > 0.01:
			var c := Color("#FFC965", flash_alpha)
			draw_circle(Vector2(0, 61), 8.0, Color("#FF9A53", flash_alpha * 0.55))
			draw_colored_polygon(PackedVector2Array([Vector2(-14, 57), Vector2(-8, 66), Vector2(0, 62), Vector2(8, 66), Vector2(14, 57), Vector2(0, 52)]), c)
			draw_circle(Vector2.ZERO + Vector2(0, 58), 3.0, Color("#FFF4D8", flash_alpha))

var barrel: Barrel
var recoil_tween: Tween
var flash_tween: Tween
var disabled := false

func _ready() -> void:
	barrel = Barrel.new()
	barrel.name = "Cano"
	barrel.position = Vector2.ZERO
	add_child(barrel)
	queue_redraw()

func _draw() -> void:
	# Fixed brass mount, separate from the aiming/recoiling tube.
	draw_circle(Vector2.ZERO, 19, Color("#17303D"))
	draw_circle(Vector2.ZERO, 14, Color("#F2B84B"))
	draw_circle(Vector2.ZERO, 8, Color("#506C70"))
	for angle in [0.0, PI * 0.5, PI, PI * 1.5]:
		draw_circle(Vector2(cos(angle), sin(angle)) * 11, 1.8, Color("#FFF4D8"))

func aim_direction(direction: Vector2, parent_rotation := 0.0) -> void:
	if not is_instance_valid(barrel): return
	if direction.length_squared() <= 0.0001: return
	barrel.rotation = direction.angle() - PI * 0.5 - parent_rotation

func fire() -> void:
	if disabled or not is_instance_valid(barrel): return
	_cancel_tweens()
	barrel.flash_alpha = 1.0
	var recoil := Vector2.UP.rotated(barrel.rotation) * 12.0
	recoil_tween = create_tween()
	recoil_tween.set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	recoil_tween.tween_property(barrel, "position", recoil, 0.06)
	recoil_tween.set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	recoil_tween.tween_property(barrel, "position", Vector2.ZERO, 0.14)
	flash_tween = create_tween()
	flash_tween.tween_property(barrel, "flash_alpha", 0.0, 0.12)

func reset_visual() -> void:
	disabled = false
	_cancel_tweens()
	if is_instance_valid(barrel):
		barrel.rotation = 0.0
		barrel.position = Vector2.ZERO
		barrel.flash_alpha = 0.0
	queue_redraw()

func stop_visuals() -> void:
	disabled = true
	_cancel_tweens()
	if is_instance_valid(barrel):
		barrel.position = Vector2.ZERO
		barrel.flash_alpha = 0.0
	queue_redraw()

func _cancel_tweens() -> void:
	if recoil_tween != null and recoil_tween.is_valid(): recoil_tween.kill()
	if flash_tween != null and flash_tween.is_valid(): flash_tween.kill()
