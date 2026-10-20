extends RefCounted
## New profile alongside the original file. Import is an explicit player action.
const Legacy = preload("res://scripts/save/save_store.gd")
const VERSION := 6
const PATH := "user://dig_wonder.defense.v6.json"
const TEMP := PATH + ".tmp"
const BACKUP := PATH + ".bak"

static func fresh() -> Dictionary:
	return {"version": VERSION, "mode": "defense", "gold": 0, "damage_level": 0, "hull_level": 0, "unlocked_stage": 1, "stage": 1, "wins": 0, "slots": ["pioneer", "", ""], "legacy_snapshot": ""}

static func valid(data: Variant) -> bool:
	if not data is Dictionary or data.get("version", -1) != VERSION or data.get("mode", "") != "defense": return false
	for key in ["gold", "damage_level", "hull_level", "unlocked_stage", "stage", "wins"]:
		var value: Variant = data.get(key)
		if not value is int and not value is float: return false
		if not is_finite(float(value)) or value < 0 or float(value) != int(value) or value > 2000000000: return false
	if data.damage_level > 10 or data.hull_level > 10 or data.stage != 1 or data.unlocked_stage < 1 or data.unlocked_stage > 2: return false
	if data.get("slots", []) != ["pioneer", "", ""]: return false
	if not data.get("legacy_snapshot", null) is String or str(data.legacy_snapshot).length() > 200: return false
	return true

static func read_profile(path: String) -> Dictionary:
	if not FileAccess.file_exists(path): return {}
	var parser := JSON.new()
	if parser.parse(FileAccess.get_file_as_string(path)) != OK: return {}
	var data: Variant = parser.data
	return data if valid(data) else {}

static func load_profile() -> Dictionary:
	var found_new := false
	for path in [PATH, TEMP, BACKUP]:
		found_new = found_new or FileAccess.file_exists(path)
		var data := read_profile(path)
		if not data.is_empty(): return {"profile": data, "legacy": {}, "source": path, "error": ""}
	if found_new:
		return {"profile": {}, "legacy": {}, "source": "", "error": "Perfil de defesa inválido. Arquivos preservados; restaure uma cópia válida."}
	var found_old := false
	for path in [Legacy.PATH, Legacy.TEMP, Legacy.BACKUP]:
		found_old = found_old or FileAccess.file_exists(path)
		var old := Legacy._read(path)
		if not old.is_empty(): return {"profile": {}, "legacy": old, "source": path, "error": ""}
	if found_old:
		return {"profile": {}, "legacy": {}, "source": "", "error": "Save antigo inválido. Arquivos preservados; restaure o backup para importar."}
	return {"profile": fresh(), "legacy": {}, "source": "", "error": ""}

static func _write_checked(path: String, data: Dictionary) -> bool:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null: return false
	file.store_string(JSON.stringify(data))
	file.flush()
	var error := file.get_error()
	file = null
	return error == OK and not read_profile(path).is_empty()

static func save_profile(data: Dictionary) -> bool:
	if not valid(data): return false
	if not _write_checked(TEMP, data): return false
	var previous := read_profile(PATH)
	if not previous.is_empty():
		if not _write_checked(BACKUP + ".tmp", previous): return false
		if DirAccess.rename_absolute(ProjectSettings.globalize_path(BACKUP + ".tmp"), ProjectSettings.globalize_path(BACKUP)) != OK: return false
	return DirAccess.rename_absolute(ProjectSettings.globalize_path(TEMP), ProjectSettings.globalize_path(PATH)) == OK

static func confirm_import(old: Dictionary, source: String) -> Dictionary:
	# Preserve the exact original bytes before converting or writing a v6 profile.
	if old.is_empty() or not FileAccess.file_exists(source): return {}
	if source not in [Legacy.PATH, Legacy.TEMP, Legacy.BACKUP] or Legacy._read(source) != old: return {}
	if not read_profile(PATH).is_empty(): return {}
	var original := FileAccess.get_file_as_bytes(source)
	var path := "user://dig_wonder.legacy-" + FileAccess.get_file_as_string(source).sha256_text().substr(0, 16) + ".json"
	if not FileAccess.file_exists(path):
		var file := FileAccess.open(path, FileAccess.WRITE)
		if file == null: return {}
		file.store_buffer(original)
		file.flush()
		var error := file.get_error()
		file = null
		if error != OK: return {}
	if FileAccess.get_file_as_bytes(path) != original: return {}
	var data := fresh()
	data.gold = int(old.coins) + int(old.inventory)
	data.damage_level = clampi(int(old.drill_level) - 1, 0, 10)
	data.legacy_snapshot = path
	return data if save_profile(data) else {}
