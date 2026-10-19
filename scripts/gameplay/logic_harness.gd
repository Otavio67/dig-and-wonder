extends SceneTree

const Economy = preload("res://scripts/progression/economy.gd")
const DrillSpec = preload("res://scripts/progression/drill.gd")
const DigEngine = preload("res://scripts/gameplay/dig_engine.gd")

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
	print("LOGIC_OK economy=pass insufficient_funds=pass basic_pattern=1 prism_pattern=3 broken=%d/%d value=%d/%d" % [basic_result.broken, prism_result.broken, basic_result.coins, prism_result.coins])
	quit()
