extends SceneTree

const Economy = preload("res://scripts/progression/economy.gd")
const DrillSpec = preload("res://scripts/progression/drill.gd")
const DigEngine = preload("res://scripts/gameplay/dig_engine.gd")
const EquipmentSystem = preload("res://scripts/progression/equipment_system.gd")
const SaveStore = preload("res://scripts/save/save_store.gd")
const PhaseSystem = preload("res://scripts/progression/phase_system.gd")

func _initialize() -> void:
	assert(Economy.sell_coins(12) == 12)
	assert(Economy.sell_coins(-2, -3) == 0)
	assert(not Economy.can_afford(29, 30))
	assert(Economy.buy(29, 30) == -1)
	assert(Economy.buy(30, 30) == 0)
	var drill := DrillSpec.new()
	var basic := drill.pattern.size()
	drill.transform_to_prism()
	assert(drill.pattern.size() == 3 and basic == 1)
	var biome_file := FileAccess.open("res://content/biomes/prado_cristalino.json", FileAccess.READ)
	var biome: Dictionary = JSON.parse_string(biome_file.get_as_text())
	var engine := DigEngine.new()
	engine.setup(biome, 72, [])
	var one: Array[Vector2i] = [Vector2i.ZERO]
	var three: Array[Vector2i] = [Vector2i.ZERO, Vector2i(-1, 1), Vector2i(1, 1)]
	var basic_result := engine.dig(one, 0, 0, 4)
	assert(basic_result.broken == 1)
	engine.setup(biome, 72, [])
	var prism_result := engine.dig(three, 0, 0, 4)
	assert(prism_result.broken > basic_result.broken)
	assert(prism_result.coins > basic_result.coins)
	var hard_depth := 0
	for depth in range(24):
		if int(engine.layer_for_cell(depth, 0).hardness) > 1:
			hard_depth = depth
			break
	engine.setup(biome, 72, [])
	var center: Array[Vector2i] = [Vector2i.ZERO]
	var partial := engine.dig(center, 0, hard_depth, 1)
	assert(partial.broken == 0 and partial.worked == 1)
	var damaged := engine.durability.duplicate(true)
	engine.setup(biome, 72, [], damaged)
	var resumed := engine.dig(center, 0, hard_depth, 1)
	assert(resumed.broken == 1)
	var phase_system := PhaseSystem.new()
	assert(phase_system.phases.size() == 4)
	for phase_id in phase_system.phases:
		var phase_data: Dictionary = phase_system.phases[phase_id]
		assert(phase_data.key_cells.size() == int(phase_data.keys))
		assert(phase_data.chest_cells.size() == int(phase_data.chests))
		assert(int(phase_data.enemy_count) == 1 and phase_data.has("boss"))
	var surprise_found := false
	for phase_seed in range(200):
		if phase_system.next_id(0, "prado_cristalino", phase_seed) == "fenda_aurora":
			surprise_found = true
			break
	assert(surprise_found)
	var phase_state: Dictionary = phase_system.initial_state()
	phase_state.phase_sequence.append("gruta_basaltica")
	var phase_engine := DigEngine.new()
	phase_engine.setup(biome, 7042, [], {}, phase_system.catalog, phase_state.phase_sequence)
	assert(phase_engine.layer_for_cell(0, 0).name != phase_engine.layer_for_cell(24, 0).name)
	print("LOGIC_OK economy=pass insufficient_funds=pass basic_pattern=1 prism_pattern=3 broken=%d/%d value=%d/%d partial_damage_resume=true" % [basic_result.broken, prism_result.broken, basic_result.coins, prism_result.coins])
	var equipment := EquipmentSystem.new()
	for rarity in EquipmentSystem.TIERS:
		assert(equipment.gear_ids_for_rarity(rarity).size() == 10)
	assert(equipment.skins.size() == 13)
	var normal_rank := 0
	var lucky_rank := 0
	for sample in range(512):
		var normal_rng := RandomNumberGenerator.new()
		normal_rng.seed = sample + 17
		var lucky_rng := RandomNumberGenerator.new()
		lucky_rng.seed = sample + 17
		normal_rank += EquipmentSystem.TIERS.find(equipment._roll_rarity(normal_rng, 0, 0, 0, 0))
		lucky_rank += EquipmentSystem.TIERS.find(equipment._roll_rarity(lucky_rng, 0, 0, 0, 12))
	assert(lucky_rank > normal_rank)
	var state := SaveStore.fresh()
	state.chests = 1
	state.keys = 1
	state.below_legendary_streak = 11
	var legendary := equipment.open_chest(state, 120, 321)
	assert(legendary.ok and legendary.item.rarity == "Lendária")
	assert(state.chests == 0 and state.keys == 0)
	assert(legendary.item.kind in ["drill", "gear", "skin"])
	var boosts := equipment.bonuses(["manopla", "peneira"], "safira_veloz")
	assert(boosts.power == 1 and boosts.reward == 1 and is_equal_approx(boosts.speed, 0.08) and boosts.luck == 2)
	state = SaveStore.fresh()
	state.chests = 1
	state.keys = 1
	state.below_legendary_streak = 11
	state.gear_inventory = equipment.gear_ids_for_rarity("Lendária")
	state.drill_inventory.append("dynamo")
	for skin_id in equipment.skin_ids():
		if str(equipment.skins[skin_id].rarity) == "Lendária":
			state.skin_inventory.append(skin_id)
	var duplicate := equipment.open_chest(state, 120, 321)
	assert(duplicate.ok and duplicate.item.duplicate and duplicate.item.coins == 34)
	assert(state.coins == 34)
	var trinca := DrillSpec.new()
	assert(trinca.equip("trinca") and trinca.pattern.size() == 3)
	assert(trinca.equip("aurora") and trinca.pattern.size() == 8)
	print("CHEST_OK rarities=6 equipment_per_rarity=10 pity_legendary=true luck=raises_tier duplicate_gold=34 skin_passives=true drills=3/8 patterns surprise_phase_seeded=true phase_ores_distinct=true")
	quit()
