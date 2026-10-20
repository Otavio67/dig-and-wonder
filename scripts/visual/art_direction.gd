extends RefCounted
## Presentation only. No gameplay RNG, input transforms, or saved properties.
const INK := Color("#17303D")
const CORAL := Color("#F27C67")
const BRASS := Color("#F2B84B")
var clock := 0.0
var weights: Array[float] = [1.0, 0.0, 0.0, 0.0, 0.0]
var from_weights: Array[float] = [1.0, 0.0, 0.0, 0.0, 0.0]
var target := 0
var transition := 1.5
var skies: Array[Texture2D] = []
var scene_atlas: Texture2D
var middle: Texture2D
var near: Texture2D
var fog_texture: GradientTexture2D

func _init() -> void:
	var pairs := [["#E7A06A", "#123448"], ["#704A38", "#0B202D"], ["#123448", "#0B202D"], ["#0B202D", "#123448"], ["#7966B2", "#123448"]]
	for pair in pairs:
		var gradient := Gradient.new()
		gradient.set_color(0, Color(pair[0]))
		gradient.set_color(1, Color(pair[1]))
		var texture := GradientTexture2D.new()
		texture.gradient = gradient
		texture.fill_from = Vector2(0, 0)
		texture.fill_to = Vector2(0, 1)
		skies.append(texture)
	if ResourceLoader.exists("res://assets/ambientes/ambientes-atlas.png"):
		scene_atlas = load("res://assets/ambientes/ambientes-atlas.png")
		middle = load("res://assets/ambientes/intermediario.png")
		near = load("res://assets/ambientes/proximo.png")
	var fog_gradient := Gradient.new()
	fog_gradient.set_color(0, Color("#39D4C1", 0.11))
	fog_gradient.set_color(1, Color("#39D4C1", 0))
	fog_texture = GradientTexture2D.new()
	fog_texture.gradient = fog_gradient
	fog_texture.fill = GradientTexture2D.FILL_RADIAL
	fog_texture.fill_from = Vector2(0.5, 0.5)
	fog_texture.fill_to = Vector2(1, 0.5)

func update(delta: float, meters: int, phase: String) -> void:
	clock += delta
	var next := 0 if meters < 40 else (1 if meters < 96 else (2 if meters < 192 else 3))
	if phase == "fenda_aurora":
		next = 4
	if next != target:
		from_weights = weights.duplicate()
		target = next
		transition = 0
	transition = minf(1.5, transition + delta)
	var blend := (1 - cos(PI * transition / 1.5)) * 0.5
	for i in range(5):
		weights[i] = lerpf(from_weights[i], 1.0 if i == target else 0.0, blend)

func background(canvas: CanvasItem, size: Vector2, camera: int) -> void:
	for i in range(5):
		if weights[i] < 0.003: continue
		canvas.draw_texture_rect(skies[i], Rect2(Vector2.ZERO, size), false, Color(1, 1, 1, weights[i]))
		if scene_atlas != null:
			var cell := scene_atlas.get_size() / Vector2(5, 1)
			var source := Rect2(Vector2(i * cell.x, 0), cell)
			var offset := fmod(float(camera) * 0.8, 14.0)
			canvas.draw_texture_rect_region(scene_atlas, Rect2(-9, -offset, size.x + 18, size.y + 22), source, Color(1, 1, 1, weights[i]))
	# Independently painted transparent layers preserve the central play space.
	for plane in range(2):
		var offset := fmod(float(camera) * (2.0 if plane == 0 else 3.4), 52.0)
		var layer := middle if plane == 0 else near
		if layer == null: continue
		canvas.draw_texture_rect(layer, Rect2(0, 160 - offset, size.x, size.y + 52), false, Color(1, 1, 1, 0.65 if plane == 0 else 0.8))
	var fog_center := Vector2(size.x * 0.5 + sin(clock * 0.13) * 50, size.y * 0.42)
	canvas.draw_texture_rect(fog_texture, Rect2(fog_center - Vector2(230, 230), Vector2(460, 460)), false)

func plate(canvas: CanvasItem, rect: Rect2, tint: Color, pressed: bool, enabled: bool, primary: bool) -> void:
	var amount := 0.97 if pressed and enabled else 1.0
	var r := Rect2(rect.get_center() - rect.size * amount * 0.5, rect.size * amount)
	var p := r.position + Vector2(0, 3 if pressed else 0)
	var w := r.size.x
	var h := r.size.y - 3
	var points := PackedVector2Array([p + Vector2(10, 0), p + Vector2(w - 10, 0), p + Vector2(w, 10), p + Vector2(w, h - 10), p + Vector2(w - 10, h), p + Vector2(10, h), p + Vector2(0, h - 10), p + Vector2(0, 10), p + Vector2(10, 0)])
	var shade := tint if enabled else Color("#617A80")
	canvas.draw_rect(Rect2(p + Vector2(3, 5), Vector2(w - 6, h)), Color("#0B202D", 0.8))
	canvas.draw_colored_polygon(points, shade.darkened(0.15) if pressed else shade)
	canvas.draw_polyline(points, INK, 3, true)
	var glint := (0.24 + 0.1 * sin(clock * TAU / 2.4)) if enabled and primary else 0.18
	if not enabled: glint = 0.06
	canvas.draw_line(p + Vector2(13, 5), p + Vector2(w - 13, 5), Color("#FFF4D8", glint), 2, true)
	canvas.draw_line(p + Vector2(12, h - 5), p + Vector2(w - 12, h - 5), Color(INK, 0.3), 3, true)
	for x in [9.0, w - 9]:
		for y in [12.0, h - 10]:
			canvas.draw_circle(p + Vector2(x, y), 2.8, INK)
			canvas.draw_circle(p + Vector2(x - 0.5, y - 0.7), 1.0, Color("#FFF4D8", 0.55))
