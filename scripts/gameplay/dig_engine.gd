extends RefCounted
class_name DigEngine

const CHUNK_CELLS := 12

var biome: Dictionary
var dug := {}
var durability := {}
var seed_value := 0

func setup(data: Dictionary, seed_value: int, dug_cells: Array, damaged_cells: Dictionary = {}) -> void:
	biome = data
	self.seed_value = seed_value
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
	var layers: Array = biome.layers
	var candidates: Array[int]
	if depth < 5:
		candidates = [0, 1]
	elif depth < 12:
		candidates = [0, 1, 2]
	elif depth < 24:
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

func dig(pattern: Array[Vector2i], center_x: int, center_y: int, power: int, reward_bonus: int = 0) -> Dictionary:
	var result := {"broken": 0, "coins": 0, "cells": [], "worked": 0}
	for offset in pattern:
		var x := center_x + offset.x
		var y := center_y + offset.y
		if y < 0 or abs(x) > 14:
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
		result.coins += int(layer.reward) + maxi(0, reward_bonus)
		result.cells.append(Vector2i(x, y))
	return result

func dug_keys() -> Array:
	var result: Array = dug.keys()
	result.sort()
	return result
