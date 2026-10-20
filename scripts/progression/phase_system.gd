extends RefCounted
class_name PhaseSystem

const CATALOG_PATH := "res://content/phases/catalog.json"

var catalog: Dictionary = {}
var phases: Dictionary = {}
var cells_per_phase := 24

func _init() -> void:
	var file := FileAccess.open(CATALOG_PATH, FileAccess.READ)
	if file != null:
		var parsed: Variant = JSON.parse_string(file.get_as_text())
		if parsed is Dictionary:
			catalog = parsed
			phases = parsed.get("phases", {})
			cells_per_phase = maxi(1, int(parsed.get("cells_per_phase", 24)))

func initial_state() -> Dictionary:
	var initial_id := str(catalog.get("normal_cycle", ["prado_cristalino"])[0])
	return {"phase_index": 0, "phase_sequence": [initial_id], "boss_health": int(phases.get(initial_id, {}).get("boss", {}).get("health", 10)), "bosses_defeated": 0}

func phase_id_at(state: Dictionary, depth: int) -> String:
	var phase_index := maxi(0, floori(float(depth) / float(cells_per_phase)))
	var sequence: Array = state.get("phase_sequence", [])
	if phase_index < sequence.size() and phases.has(str(sequence[phase_index])):
		return str(sequence[phase_index])
	return current_id(state)

func current_id(state: Dictionary) -> String:
	var sequence: Array = state.get("phase_sequence", [])
	var index := clampi(int(state.get("phase_index", 0)), 0, maxi(0, sequence.size() - 1))
	if not sequence.is_empty() and phases.has(str(sequence[index])):
		return str(sequence[index])
	return str(catalog.get("normal_cycle", ["prado_cristalino"])[0])

func current(state: Dictionary) -> Dictionary:
	return phases.get(current_id(state), phases.get("prado_cristalino", {}))

func start_depth(state: Dictionary) -> int:
	return int(state.get("phase_index", 0)) * cells_per_phase

func end_depth(state: Dictionary) -> int:
	return start_depth(state) + cells_per_phase

func next_id(phase_index: int, current_phase: String, seed_value: int) -> String:
	var rng := RandomNumberGenerator.new()
	rng.seed = seed_value + phase_index * 7919 + 0xD1A
	var surprise := str(catalog.get("surprise_id", "fenda_aurora"))
	var chance := int(catalog.get("surprise_chance_percent", 22))
	if current_phase != surprise and rng.randi_range(1, 100) <= chance and phases.has(surprise):
		return surprise
	var cycle: Array = catalog.get("normal_cycle", ["prado_cristalino"])
	var next_index := posmod(phase_index + 1, cycle.size())
	var next_phase := str(cycle[next_index])
	if next_phase == current_phase and cycle.size() > 1:
		next_phase = str(cycle[posmod(next_index + 1, cycle.size())])
	return next_phase

func valid_sequence(sequence: Array) -> bool:
	if sequence.is_empty() or sequence.size() > 1000:
		return false
	for phase_id in sequence:
		if not phase_id is String or not phases.has(phase_id):
			return false
	return true
