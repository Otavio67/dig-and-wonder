extends SceneTree

const Game = preload("res://scripts/gameplay/game.gd")
const SaveStore = preload("res://scripts/save/save_store.gd")

func _initialize() -> void:
	var game = Game.new()
	game.size = Vector2(540, 960)
	root.add_child(game)
	var biome_file := FileAccess.open("res://content/biomes/prado_cristalino.json", FileAccess.READ)
	game.biome = JSON.parse_string(biome_file.get_as_text())
	game.state = SaveStore.fresh()
	game.engine.setup(game.biome, 7042, [])
	game.grid_origin_y = 154.0
	game.world_bottom = 786.0
	assert(game._cell_at(Vector2(270, 181)) == Vector2i(0, 0))
	game._on_press(Vector2(270, 100))
	assert(not game.held and game.engine.dug.is_empty())
	game.drill.level = 100
	for depth in range(17):
		game._perform_dig(Vector2i(0, depth))
	assert(game.state.depth >= 17)
	assert(game.state.discoveries.has(game.biome.discovery.id))
	assert(game.camera_depth > 0)
	game.drill.level = 3
	game._commit_save()
	var saved := SaveStore.load_data()
	assert(saved.drill_level == 3)
	print("GAME_OK grid_center=0:0 hud_does_not_dig=true discovery_at_depth=17 scroll=%d saved_drill_level=%d" % [game.camera_depth, saved.drill_level])
	quit()
