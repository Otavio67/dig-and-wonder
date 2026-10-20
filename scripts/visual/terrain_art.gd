extends RefCounted
## Original illustrated atlas. Coordinates choose variants without touching RNG.
static var atlas: Texture2D

static func draw_cell(canvas: CanvasItem, rect: Rect2, layer: Dictionary, col: int, depth: int, damage: int) -> void:
	if atlas == null:
		atlas = load("res://assets/terreno/materiais-atlas.png")
	var name := str(layer.get("name", "")).to_lower()
	var material := 0
	if name.contains("cristal") or name.contains("quartzo") or name.contains("aurora"):
		material = 3
	elif int(layer.hardness) > 1:
		material = 2
	elif name.contains("argila"):
		material = 1
	var variant := posmod(col * 17 + depth * 31, 3)
	var cell := atlas.get_size() / Vector2(4, 3)
	var source := Rect2(Vector2(material * cell.x, variant * cell.y), cell)
	var bases := [Color("#ca8052"), Color("#a95e59"), Color("#547b91"), Color("#54d9cb")]
	var base: Color = bases[material]
	var fill := Color(str(layer.color))
	var tint := Color(clampf(fill.r / base.r, 0.35, 1.5), clampf(fill.g / base.g, 0.35, 1.5), clampf(fill.b / base.b, 0.35, 1.5))
	canvas.draw_texture_rect_region(atlas, rect.grow(-0.5), source, tint)
	if damage > 0:
		var p := rect.position
		canvas.draw_polyline(PackedVector2Array([p + Vector2(28, 4), p + Vector2(22, 22), p + Vector2(34, 30), p + Vector2(24, 49)]), Color("#17303D"), 2.5, true)
		canvas.draw_line(p + Vector2(22, 22), p + Vector2(7, 29), Color("#17303D"), 2, true)
