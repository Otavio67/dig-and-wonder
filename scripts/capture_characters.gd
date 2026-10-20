extends SceneTree
## Actual desktop rendering in a phone viewport; simulated input, isolated user data.
const OUTPUT := "res://builds/review/etapa1-20261020/depois/"

func _initialize() -> void:
	call_deferred("_run")

func _snapshot(file: String) -> void:
	var capture := root.get_texture().get_image()
	assert(not capture.is_empty())
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
	for frame in range(96):
		if frame >= 12 and frame < 60 and frame % 3 == 0:
			game._release_pointer()
			var target := Vector2i(posmod(frame / 3, 7) - 3, 1 + frame / 24)
			game._on_press(game._cell_center(target))
		if frame == 60:
			game._release_pointer()
		await process_frame
		await RenderingServer.frame_post_draw
		_snapshot("sequencia/frame-%03d.png" % frame)
		if frame == 4:
			_snapshot("08-idle.png")
		elif frame == 22:
			_snapshot("09-escavando.png")
	game._release_pointer()
	print("CHARACTER_CAPTURE_OK frames=96 fixed_fps=12 viewport=540x960 desktop=true")
	quit()
