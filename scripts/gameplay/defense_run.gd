extends RefCounted
## Deterministic simulation. The renderer owns no combat or reward rules.
const CONFIG_PATH := "res://content/defense/prototype.json"
var config: Dictionary
var profile: Dictionary
var status := "ready"
var layer_index := 0
var layer_hp := 0.0
var layer_max := 0.0
var cart_hp := 0.0
var cooldown := 3.0
var support_cooldown := 20.0
var clock := 0.0
var first_impact_paid := false
var wave_timer := 0.0
var timed_waves := 0
var pending_spawns := 0
var final_encounter := false
var next_id := 1
var cannon_timers := [0.0, 0.28]
var enemies: Array[Dictionary] = []
var units: Array[Dictionary] = []
var projectiles: Array[Dictionary] = []
var events: Array[Dictionary] = []
const FRONT_Y := 605.0

func _init(data: Dictionary) -> void:
	config = JSON.parse_string(FileAccess.get_file_as_string(CONFIG_PATH))
	profile = data
	restart()

func stats() -> Dictionary:
	var multiplier := 1.0 + int(profile.damage_level) * float(config.upgrades.damage.bonus)
	return {"hp": float(config.cart.hp) + int(profile.hull_level) * int(config.upgrades.hull.bonus), "cannon_damage": float(config.cart.cannon_damage) * multiplier, "interval": float(config.cart.cannon_interval), "drill_damage": float(config.pioneer.damage) * multiplier}

func restart() -> void:
	enemies.clear()
	units.clear()
	projectiles.clear()
	events.clear()
	layer_index = 0
	clock = 0
	first_impact_paid = false
	timed_waves = 0
	pending_spawns = 0
	final_encounter = false
	next_id = 1
	wave_timer = float(config.phase.wave_interval)
	cannon_timers = [0.0, 0.28]
	cart_hp = stats().hp
	cooldown = float(config.pioneer.cooldown) # Also charged on reopen/retry.
	support_cooldown = float(config.excavator.cooldown)
	status = "active"
	_new_layer()

func _new_layer() -> void:
	layer_max = float(config.phase.layer_hp) + layer_index * float(config.phase.hp_per_layer)
	layer_hp = layer_max

func launch() -> bool:
	if status != "active" or cooldown > 0 or units.size() >= int(config.pioneer.max_simultaneous) or projectiles.size() >= int(config.limits.projectiles) or final_encounter and enemies.is_empty():
		return false
	var id := _id()
	var victim := 0
	for enemy in enemies:
		if absf(float(enemy.x) - 270.0) <= 38.0 and (victim == 0 or int(enemy.id) < victim):
			victim = int(enemy.id)
	var unit := {"id": id, "kind": "pioneer", "role": "impact", "age": 0.0, "duration": float(config.pioneer.flight), "layer": layer_index, "enemy": victim, "damage": stats().drill_damage, "hits": 0}
	units.append(unit)
	cooldown = float(config.pioneer.cooldown)
	events.append({"type": "launch", "id": id})
	return true

func launch_support() -> bool:
	if status != "active" or support_cooldown > 0 or units.size() >= int(config.limits.units): return false
	for unit in units:
		if unit.get("role", "") == "defender": return false
	units.append({"id": _id(), "kind": "excavator", "role": "defender", "age": 0.0, "duration": float(config.excavator.flight), "hp": float(config.excavator.hp), "at_front": false, "timer": 0.0})
	support_cooldown = float(config.excavator.cooldown)
	events.append({"type": "support_launch"})
	return true

func _id() -> int:
	var value := next_id
	next_id += 1
	return value

func step(delta: float) -> void:
	if status != "active" or delta <= 0: return
	var dt := minf(delta, 0.05)
	clock += dt
	cooldown = maxf(0, cooldown - dt)
	support_cooldown = maxf(0, support_cooldown - dt)
	for enemy in enemies:
		enemy.hurt = maxf(0, float(enemy.get("hurt", 0)) - dt)
		enemy.throw_pose = maxf(0, float(enemy.get("throw_pose", 0)) - dt)
		enemy.emerge = minf(1, float(enemy.get("emerge", 1)) + dt * 3)
	if not final_encounter and timed_waves < int(config.phase.timed_waves):
		wave_timer -= dt
		if wave_timer <= 0:
			wave_timer += float(config.phase.wave_interval)
			timed_waves += 1
			_queue_moles(int(config.phase.late_wave_size) if timed_waves > int(config.phase.late_wave_after) else 1)
	_drain_spawns()
	var fired_left := false
	for side in range(2):
		cannon_timers[side] = maxf(0, float(cannon_timers[side]) - dt)
		var target := cannon_target()
		if cannon_timers[side] <= 0 and target != 0 and projectiles.size() < int(config.limits.projectiles):
			if side == 1 and fired_left:
				cannon_timers[side] = 0.2
				continue
			cannon_timers[side] = stats().interval
			if side == 0: fired_left = true
			projectiles.append({"id": _id(), "kind": "cannon", "side": side, "target": target, "age": 0.0, "duration": 0.34, "damage": stats().cannon_damage, "x": float(_enemy(target).get("x", 270))})
			events.append({"type": "cannon", "side": side})
	for i in range(units.size() - 1, -1, -1):
		var unit := units[i]
		# Legacy harness fixtures may contain a stationary defender without age.
		if not unit.has("age"): continue
		unit.age += dt
		if unit.get("role", "") == "defender":
			unit.hurt = maxf(0, float(unit.get("hurt", 0)) - dt)
			unit.at_front = float(unit.age) >= float(unit.duration)
			if not unit.at_front: continue
			if float(unit.age) >= float(unit.duration) + float(config.excavator.duration):
				units.remove_at(i)
				events.append({"type": "support_end", "x": 320.0})
				continue
			unit.timer -= dt
			if unit.timer <= 0:
				unit.timer = float(config.excavator.attack_interval)
				var target := cannon_target()
				if target != 0: _damage_enemy(target, float(config.excavator.damage))
				elif not final_encounter: damage_layer(layer_index, float(config.excavator.damage))
				events.append({"type": "support_hit", "x": 320.0})
		else:
			if unit.age < unit.duration: continue
			if int(unit.layer) != layer_index and _enemy(int(unit.enemy)).is_empty() or final_encounter and _enemy(int(unit.enemy)).is_empty():
				events.append({"type": "drill_break", "id": int(unit.id), "x": 270.0})
				units.remove_at(i)
				continue
			var due := float(unit.duration) + int(unit.hits) * float(config.pioneer.hit_interval)
			if float(unit.age) + 0.0001 >= due:
				unit.hits += 1
				var strike_damage := float(unit.damage) / int(config.pioneer.hits)
				if not _enemy(int(unit.enemy)).is_empty(): _damage_enemy(int(unit.enemy), strike_damage)
				else: damage_layer(int(unit.layer), strike_damage)
				events.append({"type": "drill_strike", "id": int(unit.id), "x": 270.0})
				if int(unit.hits) >= int(config.pioneer.hits):
					units.remove_at(i)
					events.append({"type": "drill_break", "id": int(unit.id), "x": 270.0})
		if status != "active": return
	for enemy in enemies:
		enemy.timer -= dt
		enemy.telegraph = float(enemy.timer) <= float(config.mole.telegraph)
		if enemy.timer <= 0 and projectiles.size() < int(config.limits.projectiles):
			enemy.timer = float(config.mole.attack_interval)
			enemy.telegraph = false
			enemy.throw_pose = 0.3
			projectiles.append({"id": _id(), "kind": "soil", "target": defender_target(), "age": 0.0, "duration": 0.65, "damage": float(config.mole.damage), "x": float(enemy.x)})
	for i in range(projectiles.size() - 1, -1, -1):
		var shot := projectiles[i]
		shot.age += dt
		if shot.age < shot.duration: continue
		projectiles.remove_at(i)
		if shot.kind == "cannon":
			_damage_enemy(int(shot.target), float(shot.damage))
		else:
			var defender := _unit(int(shot.target))
			if not defender.is_empty() and str(defender.get("role", "")) == "defender" and float(defender.get("hp", 0)) > 0:
				defender.hp = maxf(0, float(defender.hp) - float(shot.damage))
				defender.hurt = 0.3
				if defender.hp <= 0:
					units.erase(defender)
					events.append({"type": "support_end", "x": 320.0})
			else:
				damage_cart(float(shot.damage))
		if status != "active": return
	_drain_spawns()
	if final_encounter and enemies.is_empty() and pending_spawns == 0:
		_finish("victory")

func defender_target() -> int:
	# Impact drills never become shields. Future defenders must be alive at front.
	for unit in units:
		if unit.get("role", "") == "defender" and float(unit.get("hp", 0)) > 0 and bool(unit.get("at_front", false)):
			return int(unit.id)
	return 0

func cannon_target() -> int:
	var chosen := 0
	var priority := 99
	for enemy in enemies:
		var score := 0 if bool(enemy.telegraph) and defender_target() == 0 else 1
		if score < priority or score == priority and int(enemy.id) < chosen:
			chosen = int(enemy.id)
			priority = score
	return chosen

func _enemy(id: int) -> Dictionary:
	for enemy in enemies:
		if int(enemy.id) == id: return enemy
	return {}

func _unit(id: int) -> Dictionary:
	for unit in units:
		if int(unit.id) == id: return unit
	return {}

func _queue_moles(count: int) -> void:
	pending_spawns = mini(int(config.limits.pending_spawns), pending_spawns + count)

func _drain_spawns() -> void:
	while pending_spawns > 0 and enemies.size() < int(config.limits.enemies):
		pending_spawns -= 1
		var id := _id()
		enemies.append({"id": id, "hp": float(config.mole.hp), "max_hp": float(config.mole.hp), "x": 120.0 + posmod(id * 73, 300), "timer": float(config.mole.first_attack), "telegraph": false, "hurt": 0.0, "throw_pose": 0.0, "emerge": 0.0})
		events.append({"type": "spawn"})

func damage_layer(index: int, amount: float) -> bool:
	if status != "active" or final_encounter or index != layer_index or amount <= 0: return false
	layer_hp = maxf(0, layer_hp - amount)
	if not first_impact_paid:
		first_impact_paid = true
		profile.gold += int(config.first_impact_gold)
		events.append({"type": "gold", "amount": int(config.first_impact_gold), "source": "first_impact", "x": 270.0})
	events.append({"type": "impact", "material": material()})
	if layer_hp > 0: return true
	var gold := int(config.phase.layer_gold) + layer_index * int(config.phase.gold_per_layer)
	profile.gold += gold
	events.append({"type": "gold", "amount": gold, "source": "layer"})
	layer_index += 1
	if layer_index >= int(config.phase.layers):
		final_encounter = true
		layer_max = 0
		pending_spawns += int(config.phase.final_moles) # Reserved: never drop the final encounter at the ordinary queue cap.
	else:
		_new_layer()
		if layer_index % 2 == 0: _queue_moles(1)
	return true

func material() -> String:
	return str(config.phase.materials[mini(layer_index, int(config.phase.layers) - 1)])

func _damage_enemy(id: int, amount: float) -> bool:
	var enemy := _enemy(id)
	if status != "active" or enemy.is_empty() or amount <= 0: return false
	enemy.hp = maxf(0, float(enemy.hp) - amount)
	enemy.hurt = 0.22
	events.append({"type": "hit", "x": enemy.x})
	if enemy.hp <= 0:
		events.append({"type": "mole_defeat", "x": enemy.x})
		enemies.erase(enemy)
		profile.gold += int(config.mole.gold)
		events.append({"type": "gold", "amount": int(config.mole.gold), "source": "mole", "x": enemy.x})
	return true

func damage_cart(amount: float) -> void:
	if status != "active" or amount <= 0: return
	cart_hp = maxf(0, cart_hp - amount)
	events.append({"type": "cart_hit"})
	if cart_hp <= 0: _finish("defeat")

func _finish(result: String) -> void:
	if status != "active": return
	status = result
	projectiles.clear()
	units.clear()
	enemies.clear()
	pending_spawns = 0
	if result == "victory":
		profile.unlocked_stage = maxi(2, int(profile.unlocked_stage))
		profile.wins += 1
	events.append({"type": result})

func upgrade_cost(kind: String) -> int:
	if not config.upgrades.has(kind): return -1
	var spec: Dictionary = config.upgrades[kind]
	return int(spec.base_cost) + int(profile[kind + "_level"]) * int(spec.cost_step)

func purchase(kind: String) -> bool:
	if not config.upgrades.has(kind): return false
	var level := int(profile[kind + "_level"])
	var cost := upgrade_cost(kind)
	if level >= int(config.upgrades[kind].max_level) or int(profile.gold) < cost: return false
	profile.gold -= cost
	profile[kind + "_level"] = level + 1
	if kind == "hull" and status == "active":
		cart_hp = minf(stats().hp, cart_hp + float(config.upgrades.hull.bonus))
	events.append({"type": "purchase", "kind": kind})
	return true

func take_events() -> Array[Dictionary]:
	var result := events.duplicate(true)
	events.clear()
	return result
