extends RefCounted
class_name EquipmentSystem

const DRILLS_PATH := "res://content/drills/catalog.json"
const GEAR_PATH := "res://content/equipment/catalog.json"
const TIERS := ["Ruim", "Boa", "Épica", "Lendária", "Única", "Mística"]

var drills: Dictionary = {}
var gear: Dictionary = {}

func _init() -> void:
	drills = _read_catalog(DRILLS_PATH)
	gear = _read_catalog(GEAR_PATH)

func _read_catalog(path: String) -> Dictionary:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	return parsed if parsed is Dictionary else {}

func bonuses(equipped: Array) -> Dictionary:
	var result := {"power": 0, "reward": 0}
	for item_id in equipped:
		if gear.has(str(item_id)):
			var spec: Dictionary = gear[str(item_id)]
			result.power += int(spec.get("power", 0))
			result.reward += int(spec.get("reward", 0))
	return result

func open_chest(state: Dictionary, depth: int, seed_value: int) -> Dictionary:
	if int(state.get("chests", 0)) < 1 or int(state.get("keys", 0)) < 1:
		return {"ok": false}
	state.chests -= 1
	state.keys -= 1
	var rng := RandomNumberGenerator.new()
	rng.seed = seed_value + int(state.get("chest_open_count", 0)) * 7919 + depth * 41
	var rarity := _roll_rarity(rng, depth, int(state.get("below_epic_streak", 0)), int(state.get("below_legendary_streak", 0)))
	var candidates := _items_for_rarity(rarity)
	if candidates.is_empty():
		return {"ok": false}
	var owned_candidates: Array[Dictionary] = []
	var new_candidates: Array[Dictionary] = []
	for item in candidates:
		var owned := _is_owned(state, item)
		(new_candidates if not owned else owned_candidates).append(item)
	var reward: Dictionary
	if not new_candidates.is_empty():
		reward = new_candidates[rng.randi_range(0, new_candidates.size() - 1)]
	else:
		reward = owned_candidates[rng.randi_range(0, owned_candidates.size() - 1)]
	var duplicate := _is_owned(state, reward)
	var coins := 0
	if duplicate:
		coins = [5, 9, 18, 34, 60, 100][TIERS.find(rarity)]
		state.coins = int(state.coins) + coins
	else:
		if reward.kind == "drill":
			state.drill_inventory.append(reward.id)
		else:
			state.gear_inventory.append(reward.id)
	state.chest_open_count = int(state.get("chest_open_count", 0)) + 1
	var tier_index := TIERS.find(rarity)
	state.below_epic_streak = 0 if tier_index >= 2 else int(state.get("below_epic_streak", 0)) + 1
	state.below_legendary_streak = 0 if tier_index >= 3 else int(state.get("below_legendary_streak", 0)) + 1
	state.last_loot = {"id": reward.id, "kind": reward.kind, "rarity": rarity, "name": reward.name, "duplicate": duplicate, "coins": coins}
	return {"ok": true, "item": state.last_loot.duplicate(true)}

func _roll_rarity(rng: RandomNumberGenerator, depth: int, epic_streak: int, legendary_streak: int) -> String:
	if legendary_streak >= 11:
		return "Lendária"
	if epic_streak >= 4:
		return "Épica"
	var weights := [28.0, 36.0, 22.0, 9.0, 3.5, 1.5]
	var progress := float(depth) / 200.0
	weights[0] = maxf(8.0, weights[0] - progress * 10.0)
	weights[1] = maxf(15.0, weights[1] - progress * 4.0)
	weights[2] += progress * 5.0
	weights[3] += progress * 5.0
	weights[4] += progress * 2.5
	weights[5] += progress * 1.5
	var total := 0.0
	for weight in weights:
		total += weight
	var roll := rng.randf_range(0.0, total)
	for index in range(weights.size()):
		roll -= weights[index]
		if roll <= 0.0:
			return TIERS[index]
	return "Mística"

func _items_for_rarity(rarity: String) -> Array[Dictionary]:
	var result: Array[Dictionary] = []
	for item_id in drills:
		if str(drills[item_id].get("rarity", "Simples")) == rarity and item_id != "basic":
			result.append({"id": str(item_id), "kind": "drill", "name": str(drills[item_id].name)})
	for item_id in gear:
		if str(gear[item_id].rarity) == rarity:
			result.append({"id": str(item_id), "kind": "gear", "name": str(gear[item_id].name)})
	return result

func _is_owned(state: Dictionary, item: Dictionary) -> bool:
	if item.kind == "drill":
		return state.drill_inventory.has(item.id)
	return state.gear_inventory.has(item.id)
