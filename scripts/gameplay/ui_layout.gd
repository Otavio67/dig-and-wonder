extends RefCounted

const DESIGN_SIZE := Vector2(540, 960)
const WORLD_TOP := 300.0
const FOOTER_HEIGHT := 226.0
const GEAR_PER_PAGE := 4
const CHARACTER_PER_PAGE := 3

static func button(id: String, s: Vector2) -> Rect2:
	match id:
		"start": return Rect2(28, s.y - 168, s.x - 56, 72)
		"album": return Rect2(s.x - 166, 8, 148, 72)
		"sell": return Rect2(18, s.y - 166, s.x - 36, 72)
		"upgrade": return Rect2(18, s.y - 86, s.x - 36, 72)
		"back": return Rect2(28, s.y - 92, s.x - 56, 72)
		"chest": return Rect2(22, s.y - 174, s.x - 44, 72)
		"models": return Rect2(20, 416, (s.x - 48) * 0.5, 72)
		"skins": return Rect2(s.x * 0.5 + 4, 416, (s.x - 48) * 0.5, 72)
	return Rect2()

static func tab(index: int, s: Vector2, workshop := false) -> Rect2:
	var width := (s.x - 36.0) / 3.0
	return Rect2(18 + index * width, 164 if workshop else 123, width - 6, 72)

static func rarity(index: int, s: Vector2) -> Rect2:
	var width := (s.x - 36.0) / 3.0
	return Rect2(18 + (index % 3) * width, 280 + (index / 3) * 80, width - 6, 72)

static func item(index: int, s: Vector2, gear := false) -> Rect2:
	return Rect2(22, (470 if gear else 500) + index * 76, s.x - 44, 72)

static func page(next: bool, s: Vector2) -> Rect2:
	return Rect2(s.x * 0.5 + 8 if next else 22, 780, s.x * 0.5 - 30, 72)
