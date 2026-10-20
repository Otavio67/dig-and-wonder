extends RefCounted

const DESIGN_SIZE := Vector2(540, 960)
const WORLD_TOP := 360.0
const FOOTER_HEIGHT := 176.0
const GEAR_PER_PAGE := 4
const CHARACTER_PER_PAGE := 3

static func button(id: String, s: Vector2) -> Rect2:
	match id:
		"start": return Rect2(28, s.y - 168, s.x - 56, 72)
		"album": return Rect2(s.x - 142, 14, 126, 50)
		"sell": return Rect2(18, s.y - 128, (s.x - 48) * 0.5, 62)
		"upgrade": return Rect2(s.x * 0.5 + 6, s.y - 128, (s.x - 48) * 0.5, 62)
		"back": return Rect2(22, s.y - 80, s.x - 44, 58)
		"chest": return Rect2(22, 806, s.x - 44, 60)
		"models": return Rect2(22, 496, (s.x - 52) * 0.5, 54)
		"skins": return Rect2(s.x * 0.5 + 4, 496, (s.x - 52) * 0.5, 54)
	return Rect2()

static func tab(index: int, s: Vector2, workshop := false) -> Rect2:
	var width := (s.x - 36.0) / 3.0
	return Rect2(18 + index * width, 336 if workshop else s.y - 58, width - 6, 54)

static func rarity(index: int, s: Vector2) -> Rect2:
	var width := (s.x - 36.0) / 3.0
	return Rect2(18 + (index % 3) * width, 414 + (index / 3) * 54, width - 6, 48)

static func item(index: int, s: Vector2, gear := false) -> Rect2:
	return Rect2(22, (550 if gear else 560) + index * (56 if gear else 72), s.x - 44, 52 if gear else 66)

static func page(next: bool, s: Vector2) -> Rect2:
	return Rect2(s.x * 0.5 + 8 if next else 22, 798, s.x * 0.5 - 30, 56)
