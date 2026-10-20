extends SceneTree
## Isolated visual fixtures, not proof of earned progression or device performance.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	root.size = Vector2i(540, 960)
	var scene = load("res://main.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	var game = scene.get_node("Game")
	game.set_process(false)
	game.state = game.SaveStore.fresh()
	game.show_start_screen = false
	game.state.phase_sequence = ["prado_cristalino", "gruta_basaltica", "jardim_fossil", "gruta_basaltica", "prado_cristalino", "fenda_aurora"]
	var cases := [[0, 0, "superficie"], [1, 25, "raizes"], [2, 56, "cobre"], [4, 110, "abismo"], [5, 124, "aurora"]]
	for fixture in cases:
		game.state.phase_index = fixture[0]
		game.state.depth = fixture[1]
		game.camera_depth = fixture[1]
		game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
		game.art.update(2, int(fixture[1]) * 2, game.phase_system.current_id(game.state))
		game.queue_redraw()
		await process_frame
		await RenderingServer.frame_post_draw
		assert(root.get_texture().get_image().save_png("res://builds/review/aprovada-20261020/depois/ambiente-" + fixture[2] + ".png") == OK)
	print("ENVIRONMENT_CAPTURE_OK count=5 desktop=true isolated_fixtures=true")
	quit()
