extends SceneTree
const Crew = preload("res://scripts/visual/expedition_visuals.gd")

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	seed(9071)
	var expected := randi()
	seed(9071)
	var crew := Crew.new()
	root.add_child(crew)
	assert(randi() == expected)
	crew.present("mining", Vector2(540, 960), 360, "basic", Color("#39D4C1"))
	await create_timer(0.15).timeout
	var origin := crew.companion.position
	crew.mining_event(true)
	assert(crew.companion.event_count == 1 and crew.companion.break_count == 1)
	assert(crew.lia.event_count == 0 and crew.bento.event_count == 0)
	assert(not crew.lia.visible and not crew.bento.visible)
	await create_timer(0.05).timeout
	assert(absf(crew.companion.tip_drive.rotation - crew.companion.TIP_REST) > 0.01)
	assert(crew.companion.position == origin)
	await create_timer(0.4).timeout
	assert(crew.companion.tip_drive.rotation == crew.companion.TIP_REST)
	assert(crew.companion.impact_pivot.scale.is_equal_approx(Vector2.ONE))
	for i in range(20): crew.mining_event(true)
	await process_frame
	assert(get_processed_tweens().size() <= 12)
	await create_timer(0.5).timeout
	assert(get_processed_tweens().size() <= 7)
	crew.suspend(true)
	var frozen: Vector2 = crew.companion.idle_pivot.scale
	await create_timer(0.15).timeout
	assert(crew.companion.idle_pivot.scale == frozen)
	print("CHARACTERS_OK preserved_rigs=true miners_hidden=true rng_unchanged=true bounded_tweens=true local_pivots=true pause=true")
	crew.queue_free()
	quit()
