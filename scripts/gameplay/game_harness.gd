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
	game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
	game.grid_origin_y = game.Layout.WORLD_TOP
	game.world_bottom = 960.0 - game.Layout.FOOTER_HEIGHT
	game._start_game()
	assert(not game.show_start_screen)
	assert(game._cell_at(Vector2(270, 229)) == Vector2i(0, 0))
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
	game.drill.level = 100
	game._perform_dig(Vector2i(0, 4))
	assert(game.state.keys == 1 and game.state.chests == 0)
	game._perform_dig(Vector2i(-2, 8))
	assert(game.state.keys == 1 and game.state.chests == 1)
	game._perform_dig(Vector2i(0, 4))
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
	game._menu_press(Vector2(270, 612))
	assert(game.drill.id == "basic")
	game._menu_press(game.Layout.page(true, game.size).get_center())
	game._menu_press(Vector2(270, 612))
	assert(game.drill.id == "prism")
	game._menu_press(Vector2(400, 420))
	game._menu_press(Vector2(270, 536))
	assert(game.state.equipped_skin == "pioneira")
	game._menu_press(Vector2(100, 880))
	game._on_press(Vector2(270, 140))
	assert(game.menu_tab == "equipment")
	game._menu_press(Vector2(100, 880))
	assert(not game.show_workshop)
	assert(game._goal_text().contains("SEMENTE LUNAR"))
	game.drill.level = 100
	for depth in range(17):
		game._perform_dig(Vector2i(0, depth))
	assert(game.state.depth >= 17)
	assert(game.state.discoveries.has(game.biome.discovery.id))
	assert(game.camera_depth > 0)
	var camera_before: int = game.camera_depth
	game.pointer_start = Vector2(270, 480)
	game.camera_start_depth = camera_before
	game.held = true
	game._track_pointer(Vector2(270, 540))
	assert(game.camera_dragging and game.camera_depth < camera_before)
	game.held = false
	game.camera_dragging = false
	assert(game._goal_text().contains("ATÉ O CHEFE"))
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
	assert(saved.drill_level == 3 and saved.keys == 0 and saved.chests == 0 and saved.chest_open_count == 1 and saved.equipped_skin == "safira_veloz")
	assert(saved.found_markers.size() == 2)
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
	game._release_pointer()
	saved = SaveStore.load_data()
	assert(saved.damaged.has("0:%d" % hard_depth))
	var edge_pattern: Array[Vector2i] = [Vector2i.ZERO, Vector2i(0, 1)]
	var capped: Dictionary = game.engine.dig(edge_pattern, 0, 23, 100, 0, 24)
	assert(capped.broken == 1 and not game.engine.is_dug(0, 24))
	game.state = SaveStore.fresh()
	for raw_cell in game.phase_system.current(game.state).key_cells:
		game._collect_phase_marker(Vector2i(int(raw_cell[0]), int(raw_cell[1])))
	for raw_cell in game.phase_system.current(game.state).chest_cells:
		game._collect_phase_marker(Vector2i(int(raw_cell[0]), int(raw_cell[1])))
	assert(game.state.keys == 2 and game.state.chests == 2)
	var regular_marker_count: int = game.state.found_markers.size()
	game._collect_phase_marker(Vector2i(0, 4))
	assert(game.state.keys == 2 and game.state.chests == 2 and game.state.found_markers.size() == regular_marker_count)
	game.state = SaveStore.fresh()
	game.state.phase_sequence = ["fenda_aurora"]
	for raw_cell in game.phase_system.current(game.state).key_cells:
		game._collect_phase_marker(Vector2i(int(raw_cell[0]), int(raw_cell[1])))
	for raw_cell in game.phase_system.current(game.state).chest_cells:
		game._collect_phase_marker(Vector2i(int(raw_cell[0]), int(raw_cell[1])))
	assert(game.state.keys == 3 and game.state.chests == 3)
	game.state = SaveStore.fresh()
	game.state.depth = 24
	game.state.boss_health = 2
	game.camera_depth = 20
	game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
	game.drill.equip("basic")
	game.drill.level = 1
	game._on_press(game._boss_center())
	assert(game.state.phase_index == 0 and game.state.boss_health == 1)
	assert(game.attacking_boss)
	game.held = false
	game._on_press(Vector2(270, 229))
	assert(not game.attacking_boss and game.state.boss_health == 1)
	game.held = false
	game._on_press(game._boss_center())
	assert(game.state.phase_index == 1 and game.state.bosses_defeated == 1)
	assert(game.state.phase_sequence.size() == 2 and game.state.coins == 60)
	assert(game.state.phase_sequence[1] in ["gruta_basaltica", "jardim_fossil", "fenda_aurora"])
	game._commit_save()
	saved = SaveStore.load_data()
	assert(saved.phase_index == 1 and saved.phase_sequence == game.state.phase_sequence and saved.bosses_defeated == 1)
	print("GAME_OK start=true economy=true menus=true phase_cells_capped=true per_phase_chest_caps=true boss_combat=true boss_defeat_unlocks_next=true phase_saved=true partial_damage=true scroll=%d" % game.camera_depth)
	quit()
