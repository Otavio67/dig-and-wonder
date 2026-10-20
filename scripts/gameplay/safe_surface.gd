extends Control

const Layout = preload("res://scripts/gameplay/defense_layout.gd")
var last_safe := Rect2i()
var elapsed := 0.0

func _ready() -> void:
	resized.connect(_fit)
	_fit()

func _process(delta: float) -> void:
	if OS.get_name() != "Android":
		return
	elapsed += delta
	if elapsed >= 0.5:
		elapsed = 0.0
		if DisplayServer.get_display_safe_area() != last_safe:
			_fit()

static func fit_rect(available: Rect2) -> Rect2:
	var factor := minf(available.size.x / Layout.SIZE.x, available.size.y / Layout.SIZE.y)
	var fitted := Layout.SIZE * maxf(factor, 0.001)
	return Rect2(available.position + (available.size - fitted) * 0.5, fitted)

func _fit() -> void:
	if not is_node_ready():
		return
	var available := Rect2(Vector2.ZERO, size)
	if OS.get_name() == "Android":
		var physical := DisplayServer.get_display_safe_area()
		last_safe = physical
		if physical.size.x > 0 and physical.size.y > 0:
			var logical := get_viewport().get_screen_transform().affine_inverse() * Rect2(physical)
			available = available.intersection(logical)
	# Pequena folga adicional para bordas/gestos; a área segura nativa continua sendo a referência.
	available = available.grow(-minf(8.0, available.size.x * 0.02))
	if not available.has_area():
		return
	var fitted := fit_rect(available)
	var game: Control = $Game
	game._release_pointer()
	game.position = fitted.position
	game.size = Layout.SIZE
	game.scale = Vector2.ONE * (fitted.size.x / Layout.SIZE.x)
	game.queue_redraw()

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_RESUMED:
		call_deferred("_fit")
