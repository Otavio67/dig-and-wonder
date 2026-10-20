extends SceneTree
const Run = preload("res://scripts/gameplay/defense_run.gd")
const Store = preload("res://scripts/save/defense_save.gd")

func advance(run, seconds: float) -> void:
	for i in range(ceili(seconds / 0.05)): run.step(0.05)

func _initialize() -> void:
	var data := Store.fresh()
	var run := Run.new(data)
	var abilities: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://content/defense/abilities.json"))
	assert(abilities.pioneer.implemented and abilities.pioneer.damage == run.config.pioneer.damage and abilities.pioneer.cooldown == run.config.pioneer.cooldown)
	for id in abilities:
		if id != "pioneer": assert(not abilities[id].implemented)
	assert(not run.launch() and run.units.is_empty())
	advance(run, 3.05)
	var hp: float = run.layer_hp
	assert(run.launch() and run.cooldown > 2.9)
	assert(run.defender_target() == 0, "A disposable drill is never a shield")
	assert(not run.launch() and data.gold == 0)
	advance(run, 0.85)
	assert(run.layer_hp < hp and run.units.is_empty())
	assert(data.gold == int(run.config.first_impact_gold))
	run.damage_layer(run.layer_index, 1)
	assert(data.gold == 2, "First-impact reward only once per attempt")
	var first := run.layer_index
	assert(run.damage_layer(first, 1000))
	var earned: int = data.gold
	assert(not run.damage_layer(first, 1000) and data.gold == earned)
	run._queue_moles(1)
	run._drain_spawns()
	var enemy_id: int = run.enemies[0].id
	assert(run.cannon_target() == enemy_id)
	run._queue_moles(1)
	run._drain_spawns()
	assert(run.cannon_target() == enemy_id)
	run.enemies[1].telegraph = true
	assert(run.cannon_target() == int(run.enemies[1].id))
	run.units.append({"id": 999, "role": "defender", "hp": 10, "at_front": true})
	assert(run.defender_target() == 999)
	run.units.clear()
	assert(run._damage_enemy(enemy_id, 1000))
	var gold_after: int = data.gold
	assert(not run._damage_enemy(enemy_id, 1000) and data.gold == gold_after)
	# Autonomous defense works with no launch; moles can reach the cart at range.
	var auto := Run.new(Store.fresh())
	auto._queue_moles(1)
	auto._drain_spawns()
	advance(auto, 2.6)
	assert(auto.cart_hp < auto.stats().hp)
	assert(float(auto.enemies[0].hp) < float(auto.config.mole.hp))
	advance(auto, 4)
	assert(auto.profile.gold >= int(auto.config.mole.gold))
	# Defeat is terminal; no delayed shot, layer reward or countdown may survive.
	data.gold = 100
	assert(run.purchase("damage") and data.damage_level == 1 and data.gold == 82)
	var base_stats: Dictionary = run.stats()
	assert(base_stats.drill_damage > 32 and base_stats.cannon_damage > 6)
	assert(run.purchase("hull") and run.cart_hp <= run.stats().hp)
	var kept := data.duplicate(true)
	run.damage_cart(100000)
	assert(run.status == "defeat" and run.units.is_empty() and run.projectiles.is_empty() and run.enemies.is_empty())
	advance(run, 10)
	assert(data == kept and not run.launch() and not run.damage_layer(run.layer_index, 1000))
	run.restart()
	assert(run.layer_index == 0 and run.cart_hp == run.stats().hp and run.cooldown == 3)
	assert(data == kept and run.units.is_empty() and run.projectiles.is_empty())
	assert(not run.first_impact_paid)
	# Per-event layer and final-wave rewards; no boss or skipped mandatory wave.
	var win := Run.new(Store.fresh())
	for i in range(int(win.config.phase.layers)):
		assert(win.damage_layer(i, 100000))
	assert(win.final_encounter and win.status == "active" and win.pending_spawns > 0)
	advance(win, 0.05)
	while not win.enemies.is_empty() or win.pending_spawns > 0:
		for enemy in win.enemies.duplicate(): win._damage_enemy(int(enemy.id), 10000)
		advance(win, 0.05)
	assert(win.status == "victory" and win.profile.unlocked_stage == 2 and win.profile.wins == 1)
	var final_gold: int = win.profile.gold
	advance(win, 10)
	assert(win.profile.gold == final_gold and win.profile.wins == 1)
	# Bounds and insufficient funds.
	var limits := Run.new(Store.fresh())
	assert(not limits.purchase("damage"))
	limits._queue_moles(100)
	advance(limits, 0.05)
	assert(limits.enemies.size() <= 4 and limits.pending_spawns <= 8)
	for i in range(3):
		limits.cooldown = 0
		var launched: bool = limits.launch()
		assert(launched == (i < 2))
	assert(limits.units.size() == 2)
	limits.profile.gold = 100000
	for i in range(10): assert(limits.purchase("damage"))
	assert(not limits.purchase("damage") and limits.profile.damage_level == 10)
	limits.units.clear()
	limits.projectiles.clear()
	for i in range(24):
		limits.projectiles.append({"id": 10000 + i, "kind": "cannon", "side": 0, "target": -1, "age": 0.0, "duration": 1000.0, "damage": 0.0, "x": 270.0})
	limits.cooldown = 0
	assert(not limits.launch())
	advance(limits, 0.1)
	assert(limits.projectiles.size() == 24)
	var reserved := Run.new(Store.fresh())
	reserved.pending_spawns = 8
	reserved.layer_index = 5
	assert(reserved.damage_layer(5, 10000))
	assert(reserved.pending_spawns == 10, "Mandatory final wave must not be lost at the queue limit")
	print("DEFENSE_OK cooldown=true one_reward=true stable_targets=true defenders=true auto_defense=true ranged_damage=true defeat_terminal=true cleanup=true permanent_upgrades=true finite_phase=true final_encounter=true bounds=true")
	quit()
