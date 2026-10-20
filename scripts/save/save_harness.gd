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
	assert(not SaveStore.save_data({"version": 6}))
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
	var prior_v2 := SaveStore.fresh()
	prior_v2.version = 2
	prior_v2.gear_inventory = ["peneira"]
	prior_v2.equipped_gear = ["peneira"]
	prior_v2.erase("skin_inventory")
	prior_v2.erase("equipped_skin")
	prior_v2.erase("phase_sequence")
	prior_v2.erase("phase_index")
	prior_v2.erase("boss_health")
	prior_v2.erase("bosses_defeated")
	var v2_file := FileAccess.open(SaveStore.PATH, FileAccess.WRITE)
	v2_file.store_string(JSON.stringify(prior_v2))
	v2_file = null
	var migrated_v2 := SaveStore.load_data()
	assert(migrated_v2.version == 5 and migrated_v2.gear_inventory == ["peneira"])
	assert(migrated_v2.equipped_gear == ["peneira"] and migrated_v2.equipped_skin == "pioneira")
	assert(migrated_v2.phase_sequence == ["prado_cristalino"] and migrated_v2.boss_health == 12)
	assert(SaveStore.save_data(migrated_v2))
	var prior_v3 := SaveStore.fresh()
	prior_v3.version = 3
	prior_v3.depth = 50
	prior_v3.erase("phase_sequence")
	prior_v3.erase("phase_index")
	prior_v3.erase("boss_health")
	prior_v3.erase("bosses_defeated")
	var v3_file := FileAccess.open(SaveStore.PATH, FileAccess.WRITE)
	v3_file.store_string(JSON.stringify(prior_v3))
	v3_file = null
	var migrated_v3 := SaveStore.load_data()
	assert(migrated_v3.version == 5 and migrated_v3.phase_index == 2)
	assert(migrated_v3.phase_sequence == ["prado_cristalino", "gruta_basaltica", "jardim_fossil"])
	assert(migrated_v3.depth == 50 and migrated_v3.boss_health == 24)
	var prior_v4 := SaveStore.fresh()
	prior_v4.version = 4
	prior_v4.erase("found_markers")
	var v4_file := FileAccess.open(SaveStore.PATH, FileAccess.WRITE)
	v4_file.store_string(JSON.stringify(prior_v4))
	v4_file = null
	var migrated_v4 := SaveStore.load_data()
	assert(migrated_v4.version == 5 and migrated_v4.found_markers.is_empty())
	print("SAVE_OK valid=20 backup_recovery=42 invalid_schema_cells_damage_rejected=true v1_v2_v3_v4_migrate=true phase_progress_persisted=true")
	quit()
