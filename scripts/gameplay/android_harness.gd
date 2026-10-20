extends SceneTree
const Game = preload("res://scripts/gameplay/game.gd")
const Surface = preload("res://scripts/gameplay/safe_surface.gd")
const Store = preload("res://scripts/save/defense_save.gd")
const Run = preload("res://scripts/gameplay/defense_run.gd")

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	if OS.get_environment("D_W_TESTING") != "1" or not ProjectSettings.globalize_path("user://").contains("/.tools/"):
		push_error("Run with isolated test data via check-project.ps1 or run-harnesses.sh")
		quit(1)
		return
	var game := Game.new()
	game.size = Vector2(540, 960)
	root.add_child(game)
	game.set_process(false)
	game.profile = Store.fresh()
	game.error = ""
	game.legacy = {}
	game.sim = Run.new(game.profile)
	game.screen = "fight"
	game.sim.cooldown = 0
	game.position = Vector2(38, 54)
	game.scale = Vector2(0.5, 0.5)
	var touch := InputEventScreenTouch.new()
	touch.index = 0
	touch.pressed = true
	touch.position = game.get_global_transform_with_canvas() * game.Layout.button("launch").get_center()
	game._input(touch)
	assert(game.active_touch == 0 and game.sim.units.size() == 1)
	var other := InputEventScreenTouch.new()
	other.index = 1
	other.pressed = true
	other.position = touch.position
	game.sim.cooldown = 0
	game._input(other)
	assert(game.sim.units.size() == 1)
	touch.canceled = true
	game._input(touch)
	assert(game.active_touch == -1)
	game._set_app_active(false)
	var before: float = game.sim.clock
	game._process(2)
	assert(game.sim.clock == before)
	game._set_app_active(true)
	assert(game.accumulator == 0 and game.active_touch == -1)
	for a in [Rect2(0, 48, 1080, 2200), Rect2(0, 0, 1920, 1080), Rect2(4, 24, 312, 448)]:
		var fitted := Surface.fit_rect(a)
		assert(a.grow(0.01).encloses(fitted))
		assert(is_equal_approx(fitted.size.x / fitted.size.y, 540.0 / 960.0))
	for id in ["launch", "damage", "hull", "retry", "import"]:
		assert(game.Layout.button(id).size.y >= 64)
	assert(not game.Layout.button("launch").intersects(game.Layout.button("damage")))
	print("ANDROID_UI_OK transformed_input=true single_touch=true canceled_touch=true focus_pause=true no_resume_burst=true safe_area_geometry=true controls_64_logical=true")
	game.queue_free()
	quit()
