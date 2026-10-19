extends RefCounted
class_name SaveStore

const PATH := "user://dig_wonder.save.json"
const BACKUP := "user://dig_wonder.save.bak.json"
const VERSION := 1

static func fresh() -> Dictionary:
	return {"version": VERSION, "coins": 0, "depth": 0, "inventory": 0, "drill": "basic", "drill_level": 1, "dug": [], "discoveries": []}

static func _valid(data: Variant) -> bool:
	if not data is Dictionary or int(data.get("version", -1)) != VERSION:
		return false
	for key in ["coins", "depth", "inventory", "drill_level"]:
		if not data.get(key, null) is float and not data.get(key, null) is int:
			return false
		if int(data[key]) != float(data[key]) or int(data[key]) < 0:
			return false
	if not data.get("drill", null) is String or not data.get("dug", null) is Array or not data.get("discoveries", null) is Array:
		return false
	if not data.drill in ["basic", "prism"]:
		return false
	for cell in data.dug:
		if not cell is String or cell.is_empty() or cell.length() > 16:
			return false
	for discovery_id in data.discoveries:
		if not discovery_id is String or discovery_id.is_empty() or discovery_id.length() > 64:
			return false
	return true

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
	return parsed if _valid(parsed) else {}

static func load_data() -> Dictionary:
	var current := _read(PATH)
	return current if not current.is_empty() else _read(BACKUP)

static func save_data(data: Dictionary) -> bool:
	var copy := data.duplicate(true)
	copy["version"] = VERSION
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
