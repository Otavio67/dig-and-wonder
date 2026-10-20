extends SceneTree
## Desktop portrait evidence through real inputs. No player writes.
var game
var output := "res://builds/review/revisao-marco1/antes/"
func _initialize() -> void:
	call_deferred("_run")
func snap(name: String) -> void:
	game.queue_redraw()
	await process_frame
	await RenderingServer.frame_post_draw
	assert(root.get_texture().get_image().save_png(output + name + ".png") == OK)
func _run() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() > 0: output = args[0]
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(output))
	root.size = Vector2i(540, 960)
	var scene = load("res://main.tscn").instantiate()
	game = scene.get_node("Game")
	game.persistence_enabled = false
	root.add_child(scene)
	await process_frame
	game.set_process(false)
	game.profile = game.Store.fresh()
	game.sim = game.Run.new(game.profile)
	game.error = ""
	game.legacy = {}
	game.app_suspended = false
	game.start_run()
	await snap("01-inicio")
	for tick in range(310):
		if tick == 62: game._press_at(game.Layout.button("launch").get_center())
		game._process(0.05)
		game._release_pointer()
		if tick == 80: await snap("02-primeiro-impacto")
		if tick == 202:
			assert(game.sim.enemies.size() > 0)
			await snap("03-torrao-apareceu")
		if tick == 295: await snap("04-recompensa-torrao")
	print("REVIEW_CAPTURE_OK desktop_portrait=true real_spawn=true persistence_disabled=true")
	quit()
