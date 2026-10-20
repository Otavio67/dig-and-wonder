extends SceneTree
const CartRig = preload("res://scripts/visual/cart_rig.gd")
const PioneerRig = preload("res://scripts/visual/pioneer_rig.gd")

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var cart = CartRig.new()
	root.add_child(cart)
	await process_frame
	assert(cart.animation_player.current_animation == "idle")
	var left = cart.cannons[0]
	left.fire()
	var first_recoil: Tween = left.recoil_tween
	left.fire()
	assert(not first_recoil.is_valid(), "A repeated fire must interrupt its old recoil tween")
	assert(left.recoil_tween.is_valid() and left.flash_tween.is_valid())
	var cannon_tween: Tween = left.recoil_tween
	cart.hit()
	var hit_tween: Tween = cart.hit_tween
	cart.defeat()
	assert(not cannon_tween.is_valid() and not hit_tween.is_valid())
	assert(cart.animation_player.current_animation == "defeat")
	assert(left.disabled and cart.dead)
	await create_timer(0.5).timeout
	assert(not cart.animation_player.is_playing(), "Defeat sequence should end and hold")
	cart.reset_visual()
	assert(cart.animation_player.current_animation == "idle")
	assert(not left.disabled and cart.chassis.position == Vector2.ZERO and cart.chassis.scale == Vector2.ONE)

	var pioneer = PioneerRig.new()
	root.add_child(pioneer)
	await process_frame
	pioneer.sync_unit({"age": 0.4, "duration": 0.8})
	assert(pioneer.visible and pioneer.anim_state == "flight_spin")
	assert(is_equal_approx(pioneer.position.y, 370.0), "Flight position follows simulation age")
	var strike := {"age": 1.0, "duration": 0.8}
	pioneer.sync_unit(strike)
	assert(pioneer.anim_state == "strike_cycle")
	assert(is_equal_approx(pioneer.animation_player.current_animation_position, 0.2))
	assert(pioneer.position.y < 580.0, "Anticipation lift is phase locked to the next simulation strike")
	pioneer.confirm_impact()
	var first_impact: Tween = pioneer.impact_tween
	pioneer.confirm_impact()
	assert(not first_impact.is_valid(), "An impact replaces, rather than stacks, its response tween")
	var pending_impact: Tween = pioneer.impact_tween
	pioneer.remove_unit()
	assert(not pending_impact.is_valid() and not pioneer.visible and not pioneer.animation_player.is_playing())
	print("ANIMATION_OK cart_idle=true per_cannon_tween_replaced=true defeat_interrupts=true retry_resets=true pioneer_phase_locked=true impact_tween_bounded=true removal_cancels=true")
	quit()
