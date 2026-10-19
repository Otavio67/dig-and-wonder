extends SceneTree

const SaveStore = preload("res://scripts/save/save_store.gd")

func _initialize() -> void:
	var root := ProjectSettings.globalize_path("user://")
	DirAccess.make_dir_recursive_absolute(root)
	DirAccess.remove_absolute(root.path_join("dig_wonder.save.json"))
	DirAccess.remove_absolute(root.path_join("dig_wonder.save.bak.json"))
	var data := SaveStore.fresh()
	data.coins = 42
	data.dug = ["0:0"]
	data.damaged = {"0:1": 1}
	assert(SaveStore.save_data(data))
	var next := data.duplicate(true)
	next.coins = 20
	assert(SaveStore.save_data(next))
	assert(SaveStore.load_data().coins == 20)
	var main_file := FileAccess.open(SaveStore.PATH, FileAccess.WRITE)
	main_file.store_string("{broken")
	main_file = null
	assert(SaveStore.load_data().coins == 42)
	assert(not SaveStore.save_data({"version": 4}))
	var malformed := SaveStore.fresh()
	malformed.dug = [42]
	assert(not SaveStore.save_data(malformed))
	malformed = SaveStore.fresh()
	malformed.damaged = {"0:1": 0}
	assert(not SaveStore.save_data(malformed))
	var legacy := SaveStore.fresh()
	legacy.erase("damaged")
	legacy.erase("first_sale_bonus_claimed")
	assert(SaveStore.save_data(legacy))
	assert(not SaveStore.load_data().get("first_sale_bonus_claimed", true))
	print("SAVE_OK valid=20 backup_recovery=42 invalid_version_cells_and_damage_rejected=true legacy_save_migrates=true")
	quit()
