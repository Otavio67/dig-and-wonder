extends SceneTree
const Game = preload("res://scripts/gameplay/game.gd")
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
	game.legacy = {}
	game.error = ""
	game.sim = Run.new(game.profile)
	game.screen = "start"
	game._press_at(game.Layout.button("start").get_center())
	assert(game.screen == "fight")
	game.sim.cooldown = 0
	game._press_at(Vector2(270, 640))
	assert(game.sim.units.is_empty(), "Terrain is no longer a manual mining control")
	game._press_at(game.Layout.button("launch").get_center())
	assert(game.sim.units.size() == 1 and game.profile.gold == 0)
	game._press_at(game.Layout.button("launch").get_center())
	assert(game.sim.units.size() == 1)
	game.profile.gold = 50
	game._press_at(game.Layout.button("damage").get_center())
	assert(game.profile.damage_level == 1 and game.profile.gold == 32)
	game._press_at(game.Layout.button("hull").get_center())
	assert(game.profile.hull_level == 1 and game.profile.gold == 12)
	assert(Store.load_profile().profile.hull_level == 1)
	game.sim.damage_cart(10000)
	game._consume_events()
	assert(game.sim.status == "defeat")
	game._press_at(game.Layout.button("retry").get_center())
	assert(game.sim.status == "active" and game.sim.cart_hp == 155 and game.sim.cooldown == 3)
	assert(game.profile.gold == 12 and game.sim.units.is_empty())
	game._set_app_active(false)
	var clock: float = game.sim.clock
	game._process(4)
	assert(game.sim.clock == clock)
	game._set_app_active(true)
	assert(not game.app_suspended)
	var reopened := Game.new()
	reopened.size = Vector2(540, 960)
	root.add_child(reopened)
	reopened.set_process(false)
	assert(reopened.profile.gold == 12 and reopened.profile.hull_level == 1 and reopened.profile.damage_level == 1)
	assert(reopened.sim.layer_index == 0 and reopened.sim.cart_hp == 155 and reopened.sim.cooldown == 3 and reopened.screen == "start")
	reopened.screen = "fight"
	reopened.profile.gold = 100
	reopened.sim.damage_cart(10000)
	reopened._consume_events()
	reopened._press_at(reopened.Layout.button("damage").get_center())
	assert(reopened.profile.damage_level == 2 and reopened.profile.gold == 68 and reopened.sim.status == "defeat")
	assert(Store.load_profile().profile.damage_level == 2)
	print("GAME_OK defense_entry=true no_manual_grid=true launch_button=true upgrades=true retry_preserves_profile=true pause=true reopen_game=true")
	reopened.queue_free()
	game.queue_free()
	quit()
