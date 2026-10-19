extends RefCounted
class_name DrillSpec

var id := "basic"
var title := "Broca Pioneira"
var pattern: Array[Vector2i] = [Vector2i.ZERO]
var accent := Color("fff1c2")
var level := 1
var rarity := "Simples"
var description := "Abre um túnel por vez."
var catalog: Dictionary = {}

func _init() -> void:
	load_catalog()
	equip("basic")

func load_catalog() -> void:
	var file := FileAccess.open("res://content/drills/catalog.json", FileAccess.READ)
	if file == null:
		return
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if parsed is Dictionary:
		catalog = parsed

func equip(drill_id: String) -> bool:
	if not catalog.has(drill_id):
		return false
	var spec: Dictionary = catalog[drill_id]
	id = drill_id
	title = str(spec.name)
	rarity = str(spec.get("rarity", "Simples"))
	description = str(spec.get("description", ""))
	pattern.clear()
	for offset in spec.pattern:
		pattern.append(Vector2i(int(offset[0]), int(offset[1])))
	accent = Color(str(spec.color))
	return true

func transform_to_prism() -> void:
	equip("prism")
