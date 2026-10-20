extends RefCounted
class_name DigEngine

const CHUNK_CELLS := 12

var biome: Dictionary
var dug := {}
var durability := {}
var seed_value := 0
var phase_catalog: Dictionary = {}
var phase_sequence: Array = []
var cells_per_phase := 24

func setup(data: Dictionary, seed_value: int, dug_cells: Array, damaged_cells: Dictionary = {}, phase_data: Dictionary = {}, phases: Array = []) -> void:
	biome = data
	self.seed_value = seed_value
	phase_catalog = phase_data
	phase_sequence = phases
	cells_per_phase = maxi(1, int(phase_catalog.get("cells_per_phase", 24)))
	dug.clear()
	durability.clear()
	for cell in dug_cells:
		dug[str(cell)] = true
	durability = damaged_cells.duplicate(true)

func is_dug(x: int, y: int) -> bool:
	return dug.has(_key(x, y))

func _key(x: int, y: int) -> String:
	return "%d:%d" % [x, y]

func layer_for_cell(depth: int, cell_x: int) -> Dictionary:
	var layer_biome := biome
	var local_depth := depth
	if not phase_catalog.is_empty() and not phase_sequence.is_empty():
		var phase_index := maxi(0, floori(float(depth) / float(cells_per_phase)))
		var phase_map: Dictionary = phase_catalog.get("phases", {})
		if phase_index < phase_sequence.size() and phase_map.has(str(phase_sequence[phase_index])):
			layer_biome = phase_map[str(phase_sequence[phase_index])]
			local_depth = posmod(depth, cells_per_phase)
	var layers: Array = layer_biome.layers
	var candidates: Array[int]
	if local_depth < 5:
		candidates = [0, 1]
	elif local_depth < 12:
		candidates = [0, 1, 2]
	elif local_depth < 24:
		candidates = [1, 2, 3]
	else:
		candidates = [2, 3]
	var total_weight := 0.0
	for index in candidates:
		total_weight += float(layers[index].chance)
	var roll := float(posmod(seed_value + cell_x * 73 + depth * 37, 100)) / 100.0 * total_weight
	for index in candidates:
		roll -= float(layers[index].chance)
		if roll < 0.0:
			return layers[index]
	return layers[candidates.back()]

func dig(pattern: Array[Vector2i], center_x: int, center_y: int, power: int, reward_bonus: int = 0, max_depth: int = 2147483647) -> Dictionary:
	var result := {"broken": 0, "coins": 0, "cells": [], "worked": 0}
	for offset in pattern:
		var x := center_x + offset.x
		var y := center_y + offset.y
		if y < 0 or y >= max_depth or abs(x) > 14:
			continue
		var cell_key := _key(x, y)
		if dug.has(cell_key):
			continue
		var layer: Dictionary = layer_for_cell(y, x)
		if not durability.has(cell_key):
			durability[cell_key] = int(layer.hardness)
		var remaining: int = int(durability.get(cell_key, int(layer.hardness))) - maxi(1, power)
		if remaining > 0:
			durability[cell_key] = remaining
			result.worked += 1
			continue
		durability.erase(cell_key)
		dug[cell_key] = true
		result.broken += 1
		result.coins += maxi(0, int(layer.reward) + reward_bonus)
		result.cells.append(Vector2i(x, y))
	return result

func dug_keys() -> Array:
	var result: Array = dug.keys()
	result.sort()
	return result
