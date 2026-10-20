extends SceneTree
## Actual desktop renderer, normal-speed 12 fps capture, no writes to player saves.
const OUTPUT := "res://builds/review/marco2-20261020/"
var game

func _initialize() -> void:
	call_deferred("_run")

func _image(name: String) -> void:
	await process_frame
	await RenderingServer.frame_post_draw
	assert(root.get_texture().get_image().save_png(OUTPUT + name + ".png") == OK)

func _play_step(delta: float) -> void:
	if game.sim.status != "active": return
	if game.sim.cooldown <= 0: game._press_at(game.Layout.button("launch").get_center())
	if game.profile.damage_level < 1 and game.profile.gold >= game.sim.upgrade_cost("damage"):
		game._press_at(game.Layout.button("damage").get_center())
	elif game.profile.hull_level < 2 and game.profile.gold >= game.sim.upgrade_cost("hull"):
		game._press_at(game.Layout.button("hull").get_center())
	if game.sim.support_cooldown <= 0: game._press_at(game.Layout.button("support").get_center())
	game._process(delta)
	game._release_pointer()

func _run() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUTPUT + "sequencia"))
	root.size = Vector2i(540, 960)
	var scene = load("res://main.tscn").instantiate()
	game = scene.get_node("Game")
	game.persistence_enabled = false
	root.add_child(scene)
	await process_frame
	game.set_process(false)
	game.profile = game.Store.fresh()
	game.legacy = {}
	game.error = ""
	game.app_suspended = false
	game.sim = game.Run.new(game.profile)
	game.screen = "start"
	if "--import-preview" in OS.get_cmdline_user_args():
		game.legacy = {"coins": 42, "inventory": 13, "drill_level": 3}
		game.screen = "import"
		game.queue_redraw()
		await _image("09-importacao-fixture")
		print("IMPORT_PREVIEW_OK fixture=true persistence_disabled=true")
		quit()
		return
	game.queue_redraw()
	await _image("01-inicio")
	game.start_run()
	for frame in range(336):
		_play_step(1.0 / 12.0)
		game.queue_redraw()
		await _image("sequencia/frame-%03d" % frame)
		if frame == 44: await _image("02-lancamento")
		if frame == 135: await _image("03-combate")
		if frame == 225: await _image("04-melhorias")
		if frame == 48: await _image("10-broca-salto")
		if frame == 74: await _image("11-broca-quebra")
		if frame == 145: await _image("12-torrao-arremesso")
		if frame == 264: await _image("13-escavador")
	# Continue the same legitimate attempt without capturing every frame.
	for tick in range(6000):
		_play_step(0.05)
		if game.sim.status != "active": break
	assert(game.sim.status == "victory", "Normal launches and affordable purchases must be able to win")
	game.queue_redraw()
	await _image("05-vitoria")
	var permanent: Dictionary = game.profile.duplicate(true)
	game.start_run()
	for tick in range(5000):
		game._process(0.05) # Idle: the cart is defended but can eventually die.
		if game.sim.status != "active": break
	assert(game.sim.status == "defeat")
	assert(game.profile.gold >= permanent.gold and game.profile.damage_level == permanent.damage_level and game.profile.hull_level == permanent.hull_level)
	game.queue_redraw()
	await _image("06-derrota")
	game._press_at(game.Layout.button("retry").get_center())
	game._release_pointer()
	assert(game.sim.cart_hp == game.sim.stats().hp and game.sim.layer_index == 0 and game.sim.cooldown == 3)
	await _image("07-reinicio")
	root.size = Vector2i(960, 540)
	await _image("08-paisagem")
	print("MARCO2_CAPTURE_OK frames=336 fps=12 normal_speed=true real_launches=true affordable_upgrades=true victory=true defeat=true retry=true desktop=true persistence_disabled=true")
	quit()
