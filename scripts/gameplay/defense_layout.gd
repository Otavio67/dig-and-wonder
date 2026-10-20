extends RefCounted
const SIZE := Vector2(540, 960)

static func button(id: String) -> Rect2:
	match id:
		"launch": return Rect2(20, 758, 242, 88)
		"damage": return Rect2(20, 870, 244, 68)
		"hull": return Rect2(276, 870, 244, 68)
		"start": return Rect2(56, 636, 428, 76)
		"retry": return Rect2(56, 620, 428, 76)
		"import": return Rect2(46, 650, 448, 76)
	return Rect2()
