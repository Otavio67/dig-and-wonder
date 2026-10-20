extends SceneTree
const Store = preload("res://scripts/save/defense_save.gd")
const Legacy = preload("res://scripts/save/save_store.gd")

func _initialize() -> void:
	if OS.get_environment("D_W_TESTING") != "1" or not ProjectSettings.globalize_path("user://").contains("/.tools/"):
		push_error("Use check-project.ps1 or run-harnesses.sh: isolated user data is required.")
		quit(1)
		return
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://"))
	for path in [Store.PATH, Store.TEMP, Store.BACKUP, Legacy.PATH, Legacy.TEMP, Legacy.BACKUP]:
		if FileAccess.file_exists(path): DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
	var initial := Store.load_profile()
	assert(initial.profile.gold == 0)
	var old := Legacy.fresh()
	old.coins = 42
	old.inventory = 13
	old.drill_level = 3
	old.chests = 4
	old.keys = 2
	assert(Legacy.save_data(old))
	var bytes := FileAccess.get_file_as_bytes(Legacy.PATH)
	var waiting := Store.load_profile()
	assert(waiting.profile.is_empty() and waiting.legacy.coins == 42 and not FileAccess.file_exists(Store.PATH))
	var imported := Store.confirm_import(waiting.legacy, waiting.source)
	assert(imported.gold == 55 and imported.damage_level == 2 and imported.hull_level == 0)
	assert(FileAccess.get_file_as_bytes(imported.legacy_snapshot) == bytes)
	assert(FileAccess.get_file_as_bytes(Legacy.PATH) == bytes)
	assert(Store.load_profile().profile.gold == 55)
	# Save actual new upgrade/wallet data. Temporary combat fields are absent.
	var next := imported.duplicate(true)
	next.gold = 37
	next.hull_level = 1
	assert(Store.save_profile(next))
	assert(Store.load_profile().profile.hull_level == 1)
	var malformed := next.duplicate(true)
	malformed.gold = -1
	assert(not Store.save_profile(malformed))
	malformed = next.duplicate(true)
	malformed.damage_level = 11
	assert(not Store.save_profile(malformed))
	var file := FileAccess.open(Store.PATH, FileAccess.WRITE)
	file.store_string("{broken")
	file = null
	assert(Store.load_profile().profile.gold == 55) # Last valid backup.
	assert(Store._write_checked(Store.TEMP, next))
	assert(Store.load_profile().profile.gold == 37) # Valid temporary before backup.
	assert(Store.save_profile(next))
	assert(not FileAccess.file_exists(Store.TEMP))
	for version in range(1, 5):
		for path in [Store.PATH, Store.TEMP, Store.BACKUP]:
			if FileAccess.file_exists(path): DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
		var versioned := old.duplicate(true)
		versioned.version = version
		file = FileAccess.open(Legacy.PATH, FileAccess.WRITE)
		file.store_string(JSON.stringify(versioned))
		file = null
		var before := FileAccess.get_file_as_bytes(Legacy.PATH)
		var candidate := Store.load_profile()
		assert(candidate.profile.is_empty() and not candidate.legacy.is_empty())
		var converted := Store.confirm_import(candidate.legacy, candidate.source)
		assert(converted.gold == 55 and converted.damage_level == 2)
		assert(FileAccess.get_file_as_bytes(converted.legacy_snapshot) == before)
		assert(FileAccess.get_file_as_bytes(Legacy.PATH) == before)
		assert(Store.confirm_import(candidate.legacy, candidate.source).is_empty())
	print("DEFENSE_SAVE_OK import_confirmation=true original_bytes_preserved=true snapshot_before_conversion=true wallet_1_to_1=true upgrade_mapping=true reopen=true invalid_rejected=true backup_recovery=true temp_recovery=true")
	quit()
