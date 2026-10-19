extends RefCounted
class_name DigEngine

const CHUNK_CELLS := 12

var biome: Dictionary
var dug := {}
var durability := {}
var rng := RandomNumberGenerator.new()

func setup(data: Dictionary, seed_value: int, dug_cells: Array) -> void:
	biome = data
	rng.seed = seed_value
	for cell in dug_cells:
		dug[str(cell)] = true

func is_dug(x: int, y: int) -> bool:
	return dug.has(_key(x, y))

func _key(x: int, y: int) -> String:
	return "%d:%d" % [x, y]

func _layer_for_depth(depth: int) -> Dictionary:
	var layers: Array = biome.layers
	var choice := rng.randf()
	var sum := 0.0
	for layer in layers:
		sum += float(layer.chance)
		if choice <= sum:
			return layer
	return layers.back()

func dig(pattern: Array[Vector2i], center_x: int, center_y: int, power: int) -> Dictionary:
	var result := {"broken": 0, "coins": 0, "cells": []}
	for offset in pattern:
		var x := center_x + offset.x
		var y := center_y + offset.y
		if y < 0 or abs(x) > 14:
			continue
		var cell_key := _key(x, y)
		if dug.has(cell_key):
			continue
		var layer: Dictionary = _layer_for_depth(y)
		var remaining: int = int(durability.get(cell_key, int(layer.hardness))) - maxi(1, power)
		if remaining > 0:
			durability[cell_key] = remaining
			continue
		dug[cell_key] = true
		result.broken += 1
		result.coins += int(layer.reward)
		result.cells.append(Vector2i(x, y))
	return result

func dug_keys() -> Array:
	var result: Array = dug.keys()
	result.sort()
	return result
