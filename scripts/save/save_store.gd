extends RefCounted
class_name SaveStore

const PATH := "user://dig_wonder.save.json"
const BACKUP := "user://dig_wonder.save.bak.json"
const VERSION := 3

static func fresh() -> Dictionary:
	return {"version": VERSION, "coins": 0, "depth": 0, "inventory": 0, "drill": "basic", "drill_level": 1, "dug": [], "damaged": {}, "discoveries": [], "first_sale_bonus_claimed": false, "chests": 0, "keys": 0, "drill_inventory": ["basic"], "gear_inventory": [], "equipped_gear": [], "skin_inventory": ["pioneira"], "equipped_skin": "pioneira", "chest_open_count": 0, "below_epic_streak": 0, "below_legendary_streak": 0, "key_milestones": 0, "chest_milestones": 0, "last_loot": {}}

static func _valid(data: Variant) -> bool:
	if not data is Dictionary or int(data.get("version", -1)) != VERSION:
		return false
	for key in ["coins", "depth", "inventory", "drill_level"]:
		var amount: Variant = data.get(key, null)
		if (not amount is float and not amount is int) or int(amount) != float(amount) or int(amount) < 0:
			return false
	if not data.get("drill", null) is String or data.drill.is_empty() or data.drill.length() > 40:
		return false
	if not data.get("equipped_skin", null) is String or data.equipped_skin.is_empty() or data.equipped_skin.length() > 40:
		return false
	if not data.get("dug", null) is Array or not data.get("discoveries", null) is Array:
		return false
	if data.has("first_sale_bonus_claimed") and not data.first_sale_bonus_claimed is bool:
		return false
	for cell in data.dug:
		if not cell is String or cell.is_empty() or cell.length() > 16:
			return false
	for discovery_id in data.discoveries:
		if not discovery_id is String or discovery_id.is_empty() or discovery_id.length() > 64:
			return false
	var damaged: Variant = data.get("damaged", {})
	if not damaged is Dictionary:
		return false
	for cell_key in damaged:
		var remaining: Variant = damaged[cell_key]
		if not cell_key is String or cell_key.is_empty() or cell_key.length() > 16:
			return false
		if not remaining is int and not remaining is float:
			return false
		if int(remaining) <= 0 or float(remaining) != int(remaining):
			return false
	for key in ["chests", "keys", "chest_open_count", "below_epic_streak", "below_legendary_streak", "key_milestones", "chest_milestones"]:
		var count: Variant = data.get(key, null)
		if (not count is int and not count is float) or int(count) < 0 or float(count) != int(count):
			return false
	for key in ["drill_inventory", "gear_inventory", "equipped_gear", "skin_inventory"]:
		if not data.get(key, null) is Array or data[key].size() > 100:
			return false
		for item_id in data[key]:
			if not item_id is String or item_id.is_empty() or item_id.length() > 40:
				return false
	if not data.get("last_loot", {}) is Dictionary:
		return false
	return true

static func _migrate(data: Dictionary) -> Dictionary:
	var version := int(data.get("version", -1))
	if version == VERSION:
		return data
	if version < 1 or version > VERSION:
		return {}
	var migrated := data.duplicate(true)
	migrated.version = VERSION
	if version == 1:
		migrated.chests = 0
		migrated.keys = 0
		migrated.drill_inventory = ["basic"]
		if str(migrated.get("drill", "basic")) == "prism":
			migrated.drill_inventory.append("prism")
		migrated.gear_inventory = []
		migrated.equipped_gear = []
		migrated.chest_open_count = 0
		migrated.below_epic_streak = 0
		migrated.below_legendary_streak = 0
		migrated.key_milestones = 0
		migrated.chest_milestones = 0
		migrated.last_loot = {}
	if not migrated.has("skin_inventory"):
		migrated.skin_inventory = ["pioneira"]
	if not migrated.has("equipped_skin"):
		migrated.equipped_skin = "pioneira"
	return migrated

static func _read(path: String) -> Dictionary:
	if not FileAccess.file_exists(path):
		return {}
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return {}
	var parser := JSON.new()
	if parser.parse(file.get_as_text()) != OK:
		return {}
	var parsed: Variant = parser.data
	if not parsed is Dictionary:
		return {}
	var migrated := _migrate(parsed)
	return migrated if _valid(migrated) else {}

static func load_data() -> Dictionary:
	var current := _read(PATH)
	return current if not current.is_empty() else _read(BACKUP)

static func save_data(data: Dictionary) -> bool:
	var copy := data.duplicate(true)
	copy["version"] = VERSION
	for key in ["damaged"]:
		if not copy.has(key):
			copy[key] = {}
	for key in ["first_sale_bonus_claimed"]:
		if not copy.has(key):
			copy[key] = false
	for key in ["chests", "keys", "chest_open_count", "below_epic_streak", "below_legendary_streak", "key_milestones", "chest_milestones"]:
		if not copy.has(key):
			copy[key] = 0
	for key in ["drill_inventory", "gear_inventory", "equipped_gear", "skin_inventory"]:
		if not copy.has(key):
			copy[key] = ["basic"] if key == "drill_inventory" else (["pioneira"] if key == "skin_inventory" else [])
	if not copy.has("equipped_skin"):
		copy["equipped_skin"] = "pioneira"
	if not copy.has("last_loot"):
		copy["last_loot"] = {}
	if not _valid(copy):
		return false
	var temp := PATH + ".tmp"
	var file := FileAccess.open(temp, FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(JSON.stringify(copy))
	file.flush()
	file = null
	if FileAccess.file_exists(PATH):
		var previous := _read(PATH)
		if not previous.is_empty():
			var backup_file := FileAccess.open(BACKUP, FileAccess.WRITE)
			if backup_file != null:
				backup_file.store_string(JSON.stringify(previous))
				backup_file.flush()
				backup_file = null
		var remove_err := DirAccess.remove_absolute(ProjectSettings.globalize_path(PATH))
		if remove_err != OK:
			DirAccess.remove_absolute(ProjectSettings.globalize_path(temp))
			return false
	var rename_err := DirAccess.rename_absolute(ProjectSettings.globalize_path(temp), ProjectSettings.globalize_path(PATH))
	return rename_err == OK
