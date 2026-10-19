extends RefCounted
class_name DrillSpec

var id := "basic"
var title := "Broca Pioneira"
var pattern: Array[Vector2i] = [Vector2i.ZERO]
var accent := Color("fff1c2")
var level := 1

func transform_to_prism() -> void:
	id = "prism"
	title = "Broca Prisma"
	pattern = [Vector2i.ZERO, Vector2i(-1, 1), Vector2i(1, 1)]
	accent = Color("55f0df")
