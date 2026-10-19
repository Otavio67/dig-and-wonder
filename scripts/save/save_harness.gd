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
	var legacy := {"version": 1, "coins": 19, "depth": 11, "inventory": 3, "drill": "prism", "drill_level": 2, "dug": ["0:0"], "damaged": {}, "discoveries": [], "first_sale_bonus_claimed": true}
	var legacy_file := FileAccess.open(SaveStore.PATH, FileAccess.WRITE)
	legacy_file.store_string(JSON.stringify(legacy))
	legacy_file = null
	var migrated := SaveStore.load_data()
	assert(migrated.version == SaveStore.VERSION and migrated.coins == 19)
	assert(migrated.drill_inventory.has("prism") and migrated.equipped_gear.is_empty())
	assert(migrated.keys == 0 and migrated.chests == 0)
	assert(SaveStore.save_data(migrated))
	print("SAVE_OK valid=20 backup_recovery=42 invalid_schema_cells_damage_rejected=true v1_to_v2_preserves_prism=true")
	quit()
