extends Node2D
## Model-specific decorative silhouettes. No changes to mining patterns.
var model := "basic"

func set_model(value: String) -> void:
	if model != value:
		model = value
		queue_redraw()

func _draw() -> void:
	var ink := Color("#17303D")
	var mint := Color("#B7FFF1")
	var teal := Color("#39D4C1")
	if model == "dynamo":
		for side in [-1.0, 1.0]:
			var at := Vector2(side * 68, -28)
			draw_style_box(_coil_style(), Rect2(at - Vector2(11, 25), Vector2(22, 50)))
			for i in range(4):
				draw_line(at + Vector2(-12, -18 + i * 12), at + Vector2(12, -18 + i * 12), mint, 4, true)
	elif model == "echo":
		for side in [-1.0, 1.0]:
			var at := Vector2(side * 72, -17)
			draw_circle(at, 23, ink)
			draw_circle(at, 17, teal)
			draw_circle(at, 11, ink)
			draw_circle(at, 5, mint)
	elif model == "aurora":
		for side in [-1.0, 1.0]:
			var points := PackedVector2Array([Vector2(side * 62, 3), Vector2(side * 103, -52), Vector2(side * 94, 32), Vector2(side * 69, 39)])
			draw_colored_polygon(points, teal)
			draw_polyline(points, ink, 5, true)
			draw_line(Vector2(side * 75, 12), Vector2(side * 95, -29), mint, 4, true)
	elif model == "prism":
		for side in [-1.0, 1.0]:
			var at := Vector2(side * 64, -24)
			draw_colored_polygon(PackedVector2Array([at + Vector2(0, -24), at + Vector2(12, 0), at + Vector2(0, 17), at + Vector2(-12, 0)]), mint)
	elif model == "trinca":
		draw_line(Vector2(-67, 35), Vector2(67, 35), ink, 16, true)
		draw_line(Vector2(-67, 35), Vector2(67, 35), Color("#F2B84B"), 8, true)

func _coil_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color("#17303D")
	style.set_corner_radius_all(6)
	return style
