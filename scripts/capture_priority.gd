extends SceneTree
## Real desktop rendering; isolated test data, not an Android/device benchmark.
const OUTPUT := "res://builds/review/aprovada-20261020/depois/"

func _initialize() -> void:
	call_deferred("_run")

func _snapshot(file: String) -> void:
	var capture := root.get_texture().get_image()
	assert(capture.save_png(OUTPUT + file) == OK)

func _run() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUTPUT + "sequencia"))
	root.size = Vector2i(540, 960)
	var scene = load("res://main.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	var game = scene.get_node("Game")
	game.state = game.SaveStore.fresh()
	game.has_existing_save = false
	game.drill.equip("basic")
	game.drill.level = 1
	game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
	game._start_game()
	for frame in range(144):
		if frame >= 12 and frame < 76 and frame % 2 == 0:
			game._release_pointer()
			var index := (frame - 12) / 2
			var target := Vector2i(posmod(index, 7) - 3, index / 7)
			game._press_at(game._cell_center(target))
		if frame == 76:
			game._release_pointer()
			game._press_at(game.Layout.button("sell", game.size).get_center())
		if frame == 80:
			game._release_pointer()
			game._press_at(game.Layout.button("upgrade", game.size).get_center())
		if frame == 88:
			game._release_pointer()
			game._press_at(game.Layout.tab(2, game.size).get_center())
		if frame == 103:
			# Exercise the existing buried-marker paths, not free inventory grants.
			game._perform_dig(Vector2i(0, 4))
			game._perform_dig(Vector2i(-2, 8))
			game._perform_dig(Vector2i(-2, 8))
			game._menu_press(game.Layout.tab(0, game.size, true).get_center())
		if frame == 112:
			game._menu_press(game.Layout.button("chest", game.size).get_center())
		game.queue_redraw()
		await process_frame
		await RenderingServer.frame_post_draw
		_snapshot("sequencia/frame-%03d.png" % frame)
		if frame in [4, 33, 83, 97, 108, 115]:
			_snapshot("demo-%03d.png" % frame)
	game._release_pointer()
	assert(game.drill.id == "prism", "Capture must demonstrate an actual affordable transformation")
	assert(int(game.state.chest_open_count) == 1, "Capture must demonstrate one actual chest opening")
	print("PRIORITY_CAPTURE_OK frames=144 fps=12 viewport=540x960 actual_purchase=true actual_chest=true desktop=true")
	quit()
