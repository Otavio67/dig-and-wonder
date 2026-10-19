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
	game.drill.equip("basic")
	game.drill.level = 1
	game.engine.setup(game.biome, 7042, [])
	game.grid_origin_y = 154.0
	game.world_bottom = 786.0
	game._start_game()
	assert(not game.show_start_screen)
	assert(game._cell_at(Vector2(270, 181)) == Vector2i(0, 0))
	game._on_press(Vector2(270, 100))
	assert(not game.held and game.engine.dug.is_empty())
	game.state.inventory = 5
	game._sell()
	assert(game.state.coins == 13 and game.state.first_sale_bonus_claimed)
	game.state.inventory = 4
	game._sell()
	assert(game.state.coins == 17)
	assert(game._goal_text().contains("JUNTE 13"))
	game._upgrade()
	assert(game.drill.id == "basic" and game.state.coins == 17)
	game.state.coins = 30
	assert(game._goal_text().contains("TRANSFORME A BROCA"))
	game._upgrade()
	assert(game.drill.id == "prism" and game.drill.pattern.size() == 3 and game.state.coins == 0)
	game._check_depth_rewards(4)
	assert(game.state.keys == 1 and game.state.chests == 0)
	game._check_depth_rewards(8)
	assert(game.state.keys == 1 and game.state.chests == 1)
	game._on_press(Vector2(90, 140))
	assert(game.show_workshop and game.menu_tab == "chests")
	game._menu_press(Vector2(270, 800))
	assert(game.state.keys == 0 and game.state.chests == 0 and not game.state.last_loot.is_empty())
	if game.state.last_loot.kind == "drill":
		assert(game.state.drill_inventory.has(game.state.last_loot.id))
	elif game.state.last_loot.kind == "skin":
		assert(game.state.skin_inventory.has(game.state.last_loot.id))
	else:
		assert(game.state.gear_inventory.has(game.state.last_loot.id))
	game._menu_press(Vector2(100, 880))
	game._on_press(Vector2(450, 140))
	assert(game.menu_tab == "character")
	game._menu_press(Vector2(100, 420))
	game._menu_press(Vector2(270, 504))
	assert(game.drill.id == "basic")
	game._menu_press(Vector2(270, 630))
	assert(game.drill.id == "prism")
	game._menu_press(Vector2(400, 420))
	game._menu_press(Vector2(270, 462))
	assert(game.state.equipped_skin == "pioneira")
	game._menu_press(Vector2(100, 880))
	game._on_press(Vector2(270, 140))
	assert(game.menu_tab == "equipment")
	game._menu_press(Vector2(100, 880))
	assert(not game.show_workshop)
	assert(game._goal_text().contains("VEIO BRILHANTE"))
	game.drill.level = 100
	for depth in range(17):
		game._perform_dig(Vector2i(0, depth))
	assert(game.state.depth >= 17)
	assert(game.state.discoveries.has(game.biome.discovery.id))
	assert(game.camera_depth > 0)
	assert(game._goal_text().contains("REFORÇAR"))
	game.drill.level = 3
	game.state.skin_inventory.append("safira_veloz")
	game._equip_skin("safira_veloz")
	game.state.gear_inventory.append("peneira")
	game._equip_gear("peneira")
	assert(game.state.equipped_gear.has("peneira"))
	var active_bonuses: Dictionary = game.equipment.bonuses(game.state.equipped_gear, game.state.equipped_skin)
	assert(active_bonuses.reward == 1 and active_bonuses.power == -1 and is_equal_approx(active_bonuses.speed, 0.06))
	game._commit_save()
	var saved := SaveStore.load_data()
	assert(saved.drill_level == 3 and saved.keys == 1 and saved.chests == 0 and saved.chest_open_count == 1 and saved.equipped_skin == "safira_veloz")
	game.engine.setup(game.biome, 7042, [])
	game.drill.id = "basic"
	var basic_pattern: Array[Vector2i] = [Vector2i.ZERO]
	game.drill.pattern = basic_pattern
	game.drill.level = 1
	var hard_depth := 0
	for depth in range(24):
		if int(game.engine.layer_for_cell(depth, 0).hardness) > 1:
			hard_depth = depth
			break
	game._perform_dig(Vector2i(0, hard_depth))
	saved = SaveStore.load_data()
	assert(saved.damaged.has("0:%d" % hard_depth))
	print("GAME_OK start_button=true first_sale_bonus_once=true grid_center=0:0 hud_does_not_dig=true discovery_depth=17 scroll=%d saved_partial_damage=true" % game.camera_depth)
	quit()
