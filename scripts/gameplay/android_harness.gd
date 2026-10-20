extends SceneTree

const Game = preload("res://scripts/gameplay/game.gd")
const Layout = preload("res://scripts/gameplay/ui_layout.gd")
const Surface = preload("res://scripts/gameplay/safe_surface.gd")
const Store = preload("res://scripts/save/save_store.gd")

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var game = Game.new()
	game.size = Layout.DESIGN_SIZE
	root.add_child(game)
	game.state = Store.fresh()
	game.engine.setup(game.biome, 7042, [], {}, game.phase_catalog, game.state.phase_sequence)
	game.drill.equip("basic")
	game.drill.level = 100
	game._start_game()
	game.state.inventory = 5
	game._press_at(Vector2(270, 830)) # Centro do botão Vender, antes executava upgrade.
	assert(game.state.coins == 13 and game.state.inventory == 0 and game.drill.id == "basic")
	game.state.coins = 30
	game._press_at(Vector2(270, 910)) # Centro do botão de transformação, antes não respondia.
	assert(game.drill.id == "prism" and game.state.coins == 0)
	assert(not Layout.button("sell", game.size).intersects(Layout.button("upgrade", game.size)))
	game.position = Vector2(38, 54)
	game.scale = Vector2(0.5, 0.5)
	game.state.inventory = 7
	var mouse := InputEventMouseButton.new()
	mouse.button_index = MOUSE_BUTTON_LEFT
	mouse.pressed = true
	mouse.position = game.get_global_transform_with_canvas() * Layout.button("sell", game.size).get_center()
	game._input(mouse)
	assert(game.state.coins == 7 and game.state.inventory == 0)
	game.position = Vector2.ZERO
	game.scale = Vector2.ONE
	var touch := InputEventScreenTouch.new()
	touch.index = 0
	touch.pressed = true
	touch.position = Vector2(270, 229)
	game._input(touch)
	assert(game.held and game.active_touch == 0)
	var other := InputEventScreenTouch.new()
	other.index = 1
	other.pressed = true
	other.position = Vector2(270, 830)
	var coins_before: int = game.state.coins
	game._input(other)
	assert(game.held and game.active_touch == 0 and game.state.coins == coins_before)
	other.pressed = false
	game._input(other)
	assert(game.held)
	touch.canceled = true
	game._input(touch)
	assert(not game.held and game.active_touch == -1 and not game.save_pending)
	game.held = true
	game.attacking_boss = true
	game.charge = 2.0
	game._set_app_active(false)
	var dug_before: int = game.engine.dug.size()
	game._process(2.0)
	assert(game.app_suspended and not game.held and game.engine.dug.size() == dug_before)
	game._set_app_active(true)
	assert(not game.app_suspended and not game.held and game.charge == 0.0)
	game._perform_dig(Vector2i(2, 3))
	assert(game.save_pending)
	game._process(0.5)
	assert(game.save_pending)
	game._process(0.6)
	assert(not game.save_pending and Store.load_data().dug.has("2:3"))
	game._perform_dig(Vector2i(2, 5))
	game._set_app_active(false)
	assert(not game.save_pending and Store.load_data().dug.has("2:5"))
	game._set_app_active(true)
	game.show_workshop = true
	game.menu_tab = "equipment"
	game.gear_rarity = "Mística"
	var gear_ids: Array = game.equipment.gear_ids_for_rarity("Mística")
	game.state.gear_inventory.append(gear_ids[9])
	for i in range(3):
		game._menu_press(Layout.page(true, game.size).get_center())
	assert(game.gear_page == 2)
	game._menu_press(Layout.item(1, game.size, true).get_center())
	assert(game.state.equipped_gear.has(gear_ids[9]))
	game.menu_tab = "character"
	game._menu_press(Layout.button("skins", game.size).get_center())
	var skin_ids: Array = game.equipment.skin_ids()
	game.state.skin_inventory.append(skin_ids[12])
	for i in range(5):
		game._menu_press(Layout.page(true, game.size).get_center())
	assert(game.skin_page == 4)
	game._menu_press(Layout.item(0, game.size).get_center())
	assert(game.state.equipped_skin == skin_ids[12])
	assert(game._navigate_back() and not game.show_workshop and not game.show_start_screen)
	assert(game._navigate_back() and game.show_start_screen)
	assert(not game._navigate_back())
	for available in [Rect2(0, 48, 1080, 2200), Rect2(0, 0, 1920, 1080), Rect2(4, 24, 312, 448)]:
		var fitted: Rect2 = Surface.fit_rect(available)
		assert(available.grow(0.01).encloses(fitted))
		assert(is_equal_approx(fitted.size.x / fitted.size.y, 540.0 / 960.0))
	print("ANDROID_UI_OK hitboxes=true transformed_input=true single_touch=true canceled_touch=true focus_pause=true autosave_timer=true focus_flush=true pagination=true back=true safe_area_geometry=true")
	quit()
