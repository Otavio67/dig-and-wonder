extends Node2D
## One set of rigs is reused by the start screen, mine and existing character menu.
const Actor = preload("res://scripts/visual/expedition_actor.gd")
var lia: Node2D
var bento: Node2D
var companion: Node2D
var suspended := false

func _ready() -> void:
	name = "EquipeVisual"
	lia = _actor("lia", "Lia")
	bento = _actor("bento", "Bento")
	companion = _actor("drill", "BrocaCompanheira")

func _actor(kind: String, title: String) -> Node2D:
	var actor := Actor.new()
	actor.kind = kind
	actor.name = title
	add_child(actor)
	return actor

func present(mode: String, viewport: Vector2, world_top: float, model: String, accent: Color) -> void:
	if not is_node_ready():
		return
	visible = mode != "hidden"
	process_mode = Node.PROCESS_MODE_DISABLED if suspended or not visible else Node.PROCESS_MODE_INHERIT
	if not visible:
		return
	var drill_center := Vector2(viewport.x * 0.5, world_top - 44)
	var drill_size := Vector2(122, 101)
	var left := Rect2(34, world_top - 104, 55, 82)
	var right := Rect2(viewport.x - 89, world_top - 104, 55, 82)
	if mode == "start":
		drill_center = Vector2(viewport.x * 0.5, 296)
		drill_size = Vector2(224, 185)
		left = Rect2(50, 251, 67, 101)
		right = Rect2(viewport.x - 117, 251, 67, 101)
	elif mode == "character":
		drill_center = Vector2(viewport.x * 0.5, 306)
		drill_size = Vector2(102, 84)
		left = Rect2(49, 270, 43, 65)
		right = Rect2(viewport.x - 92, 270, 43, 65)
	_place_miner(lia, left)
	_place_miner(bento, right)
	companion.position = drill_center
	companion.scale = drill_size / Vector2(240, 190)
	companion.set_model(model, accent)

func _place_miner(actor: Node2D, rect: Rect2) -> void:
	var factor := rect.size.y / 260.0
	actor.position = Vector2(rect.get_center().x, rect.position.y + 140.0 * factor)
	actor.scale = Vector2(factor, factor)

func mining_event(broke_block: bool) -> void:
	if not is_node_ready():
		return
	lia.mining_event(broke_block)
	bento.mining_event(broke_block)
	companion.mining_event(broke_block)

func suspend(value: bool) -> void:
	suspended = value
	process_mode = Node.PROCESS_MODE_DISABLED if value or not visible else Node.PROCESS_MODE_INHERIT
