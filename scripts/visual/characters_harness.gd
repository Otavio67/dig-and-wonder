extends SceneTree
const Game = preload("res://scripts/gameplay/game.gd")
const Actor = preload("res://scripts/visual/expedition_actor.gd")

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	seed(9071)
	var expected_random := randi()
	seed(9071)
	var isolated := Actor.new()
	root.add_child(isolated)
	isolated.mining_event(true)
	assert(randi() == expected_random, "Visual rigs must not consume gameplay randomness")
	isolated.queue_free()
	await process_frame
	var game = Game.new()
	game.size = Vector2(540, 960)
	root.add_child(game)
	game.set_process(false)
	game.state = game.SaveStore.fresh()
	game.drill.equip("basic")
	game.drill.level = 1
	game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
	game._start_game()
	await process_frame
	game.crew.present("mining", game.size, game.Layout.WORLD_TOP, game.drill.id, game.drill.accent)
	await create_timer(0.15).timeout
	var cell := Vector2i.ZERO
	var hit_position: Vector2 = game._cell_center(cell)
	var input_before: Vector2i = game._cell_at(hit_position)
	var actor_origin: Vector2 = game.crew.companion.position
	game._perform_dig(cell)
	assert(game.crew.lia.event_count == 0 and game.crew.bento.event_count == 0)
	assert(not game.crew.lia.visible and not game.crew.bento.visible)
	assert(game.crew.companion.break_count == 1)
	var inventory_after: int = game.state.inventory
	game._perform_dig(cell)
	assert(game.crew.companion.event_count == 1, "Empty cells must not trigger a mining animation")
	assert(game.state.inventory == inventory_after)
	await create_timer(0.05).timeout
	assert(absf(game.crew.companion.tip_drive.rotation - game.crew.companion.TIP_REST) > 0.01)
	assert(not game.crew.companion.impact_pivot.scale.is_equal_approx(Vector2.ONE))
	assert(game._cell_at(hit_position) == input_before and game.crew.companion.position == actor_origin)
	await create_timer(0.4).timeout
	assert(is_equal_approx(game.crew.companion.tip_drive.rotation, game.crew.companion.TIP_REST))
	assert(game.crew.companion.impact_pivot.scale.is_equal_approx(Vector2.ONE))
	var before_hits: int = game.crew.companion.event_count
	for y in range(5, 20):
		if int(game.engine.layer_for_cell(y, 0).hardness) > 1:
			game._perform_dig(Vector2i(0, y))
			break
	assert(game.crew.companion.event_count == before_hits + 1)
	assert(game.crew.companion.break_count == 1, "Damage without destruction must not squash the drill")
	for i in range(20):
		game.crew.mining_event(true)
	await process_frame # Killed Tweens leave SceneTree's list on the next frame.
	assert(get_processed_tweens().size() <= 12, "Repeated hits must replace temporary Tweens")
	await create_timer(0.5).timeout
	assert(get_processed_tweens().size() <= 7)
	game._set_app_active(false)
	var frozen_scale: Vector2 = game.crew.companion.idle_pivot.scale
	await create_timer(0.15).timeout
	assert(game.crew.companion.idle_pivot.scale.is_equal_approx(frozen_scale))
	game._set_app_active(true)
	assert(game.state.keys() == game.SaveStore.fresh().keys())
	print("CHARACTERS_OK event_sync=true empty_cells_idle=true damage_vs_break=true local_pivots=true bounded_tweens=true focus_pause=true rng_unchanged=true save_schema_unchanged=true")
	game.queue_free()
	await process_frame
	quit()
