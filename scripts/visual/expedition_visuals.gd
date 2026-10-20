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
	# Retained for future use; absent from all current game screens.
	lia.hide()
	bento.hide()
	lia.process_mode = Node.PROCESS_MODE_DISABLED
	bento.process_mode = Node.PROCESS_MODE_DISABLED

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
	var drill_center := Vector2(viewport.x * 0.5, world_top - 120)
	var drill_size := Vector2(270, 204)
	var left := Rect2(34, world_top - 125, 64, 96)
	var right := Rect2(viewport.x - 98, world_top - 125, 64, 96)
	if mode == "start":
		drill_center = Vector2(viewport.x * 0.5, 296)
		drill_size = Vector2(224, 185)
		left = Rect2(50, 251, 67, 101)
		right = Rect2(viewport.x - 117, 251, 67, 101)
	elif mode == "character" or mode == "workshop":
		drill_center = Vector2(viewport.x * 0.5, 218)
		drill_size = Vector2(204, 136)
		left = Rect2(38, 198, 61, 91)
		right = Rect2(viewport.x - 99, 198, 61, 91)
	_place_miner(lia, left)
	_place_miner(bento, right)
	companion.position = drill_center
	companion.scale = Vector2.ONE * drill_size.y / 240.0
	companion.set_model(model, accent)

func _place_miner(actor: Node2D, rect: Rect2) -> void:
	var factor := rect.size.y / 260.0
	actor.position = Vector2(rect.get_center().x, rect.position.y + 140.0 * factor)
	actor.scale = Vector2(factor, factor)

func mining_event(broke_block: bool) -> void:
	if not is_node_ready():
		return
	companion.mining_event(broke_block)

func suspend(value: bool) -> void:
	suspended = value
	process_mode = Node.PROCESS_MODE_DISABLED if value or not visible else Node.PROCESS_MODE_INHERIT
