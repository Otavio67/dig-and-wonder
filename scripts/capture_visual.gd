extends SceneTree

func _initialize() -> void:
	call_deferred("_run")

func _capture(name: String) -> void:
	await process_frame
	await process_frame
	await RenderingServer.frame_post_draw
	var image := root.get_texture().get_image()
	assert(not image.is_empty())
	assert(image.save_png("res://builds/review/" + name + ".png") == OK)

func _run() -> void:
	root.size = Vector2i(540, 960)
	var scene = load("res://main.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	var game = scene.get_node("Game")
	game.state = game.SaveStore.fresh()
	game.has_existing_save = false
	game.drill.equip("basic")
	game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
	game.queue_redraw()
	await _capture("01-inicio")
	game._start_game()
	await _capture("02-escavacao")
	game.show_workshop = true
	game.menu_tab = "equipment"
	game.queue_redraw()
	await _capture("03-equipamentos")
	game.menu_tab = "character"
	game.queue_redraw()
	await _capture("04-personagem")
	game.menu_tab = "chests"
	game.queue_redraw()
	await _capture("05-baus")
	game.show_workshop = false
	game.show_album = true
	game.queue_redraw()
	await _capture("06-album")
	game.show_album = false
	root.size = Vector2i(960, 540)
	game.queue_redraw()
	await _capture("07-paisagem")
	print("VISUAL_CAPTURE_OK")
	quit()
