extends SceneTree
## Drives the live desktop game through normal controls while ffmpeg records X11.
## Only the capture encounter's wave pacing/HP/damage are accelerated.
const FPS := 30
var game
var event_counts: Dictionary = {}

func _initialize() -> void:
	call_deferred("_run")

func _wait_for(seconds: float) -> void:
	for _frame in range(ceili(seconds * FPS)):
		await process_frame

func _tap(button_id: String) -> void:
	game._press_at(game.Layout.button(button_id).get_center())
	game._release_pointer()

func _run() -> void:
	Engine.max_fps = FPS
	root.size = Vector2i(540, 960)
	var shell = load("res://main.tscn").instantiate()
	root.add_child(shell)
	game = shell.get_node("Game")
	await process_frame
	game.persistence_enabled = false
	game.profile = game.Store.fresh()
	game.legacy = {}
	game.error = ""
	game.sim = game.Run.new(game.profile)
	game.screen = "start"
	game.combat_event.connect(_on_combat_event)
	game.queue_redraw()
	await _wait_for(0.8) # Quiet cart idle on the start screen.
	_tap("start")
	# Capture-only values shorten the encounter, not the visual or event pipeline.
	game.sim.config.mole.hp = 500
	game.sim.config.mole.first_attack = 1.7
	game.sim.config.mole.attack_interval = 0.55
	game.sim.config.mole.damage = 40
	game.sim.config.phase.timed_waves = 4
	game.sim.config.phase.late_wave_after = 1
	game.sim.config.phase.late_wave_size = 1
	game.sim.config.phase.wave_interval = 0.5
	game.sim.config.limits.enemies = 4
	game.sim.wave_timer = 3.5
	await _wait_for(3.1) # Preserve the game's normal 3 s launch cooldown.
	assert(game.screen == "fight" and game.sim.cooldown <= 0.0)
	_tap("launch")
	assert(not game.sim.units.is_empty(), "Launch button must create the simulation unit")
	var pioneer: Node2D = game.pioneer_visuals.values()[0]
	var frame_limit := FPS * 8
	while game.sim.status == "active" and frame_limit > 0:
		await process_frame
		frame_limit -= 1
	print("ANIMATION_CAPTURE_DEFEAT status=%s frames_left=%d cannon=%d strikes=%d hits=%d pioneer_visuals=%d" % [game.sim.status, frame_limit, event_counts.get("cannon", 0), event_counts.get("drill_strike", 0), event_counts.get("cart_hit", 0), game.pioneer_visuals.size()])
	assert(game.sim.status == "defeat", "Test encounter must end through incoming projectile damage")
	assert(int(event_counts.get("cannon", 0)) >= 6, "Show repeated automatic cannon fire")
	assert(int(event_counts.get("drill_strike", 0)) == 4, "Show all four Pioneer impact events")
	assert(int(event_counts.get("cart_hit", 0)) >= 3, "Defeat must follow actual incoming projectiles")
	assert(int(event_counts.get("defeat", 0)) == 1)
	await _wait_for(0.55) # Death pose and result panel.
	assert(game.pioneer_visuals.is_empty() and not is_instance_valid(pioneer))
	_tap("retry")
	assert(game.sim.status == "active" and game.sim.cart_hp == game.sim.stats().hp)
	assert(game.pioneer_visuals.is_empty() and game.effects.is_empty())
	assert(game.cart_visual.animation_player.current_animation == "idle")
	assert(not game.cart_visual.dead)
	await _wait_for(0.8) # Fresh idle: no old recoil, impact ring or defeat pose.
	print("ANIMATION_CAPTURE_OK cannon_events=%d drill_strikes=%d cart_hits=%d defeat_retry=true persistence_disabled=true" % [event_counts.cannon, event_counts.drill_strike, event_counts.cart_hit])
	quit()

func _on_combat_event(event: Dictionary) -> void:
	var key := str(event.type)
	event_counts[key] = int(event_counts.get(key, 0)) + 1
