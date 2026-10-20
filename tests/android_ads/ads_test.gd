extends Control

@onready var admob: Admob = $Admob
@onready var status_label: Label = %Status
@onready var load_button: Button = %LoadButton
@onready var show_button: Button = %ShowButton
@onready var reward_label: Label = %Reward

var rewarded_loaded := false
var awaiting_reward := false
var reward_received_this_show := false
var confirmed_rewards := 0

func _ready() -> void:
	admob.initialization_completed.connect(_on_initialized)
	admob.rewarded_ad_loaded.connect(_on_rewarded_loaded)
	admob.rewarded_ad_failed_to_load.connect(_on_rewarded_failed_to_load)
	admob.rewarded_ad_user_earned_reward.connect(_on_user_earned_reward)
	admob.rewarded_ad_dismissed_full_screen_content.connect(_on_rewarded_dismissed)
	admob.rewarded_ad_failed_to_show_full_screen_content.connect(_on_rewarded_failed_to_show)
	load_button.disabled = true
	show_button.disabled = true
	admob.initialize()

func _on_initialized(_status_data: InitializationStatus) -> void:
	status_label.text = "SDK pronto. Carregue o anúncio de teste."
	load_button.disabled = false

func _on_load_pressed() -> void:
	rewarded_loaded = false
	load_button.disabled = true
	show_button.disabled = true
	status_label.text = "Carregando anúncio de teste…"
	admob.load_rewarded_ad()

func _on_rewarded_loaded(_ad_info: AdInfo, _response_info: ResponseInfo) -> void:
	rewarded_loaded = true
	load_button.disabled = true
	show_button.disabled = false
	status_label.text = "Anúncio carregado. Toque em Assistir para testar."

func _on_rewarded_failed_to_load(_ad_info: AdInfo, error_data: LoadAdError) -> void:
	rewarded_loaded = false
	load_button.disabled = false
	show_button.disabled = true
	status_label.text = "Falha ao carregar: %s" % error_data.get_message()

func _on_show_pressed() -> void:
	if not rewarded_loaded or awaiting_reward:
		return
	rewarded_loaded = false
	awaiting_reward = true
	reward_received_this_show = false
	show_button.disabled = true
	status_label.text = "Anúncio aberto. Conclua para receber a recompensa de teste."
	admob.show_rewarded_ad()

func _on_user_earned_reward(_ad_info: AdInfo, reward_data: RewardItem) -> void:
	if not awaiting_reward or reward_received_this_show:
		return
	reward_received_this_show = true
	confirmed_rewards += maxi(1, reward_data.get_amount())
	reward_label.text = "Recompensas confirmadas: %d" % confirmed_rewards
	status_label.text = "Recompensa confirmada pelo callback do SDK (%d %s)." % [reward_data.get_amount(), reward_data.get_type()]

func _on_rewarded_dismissed(_ad_info: AdInfo) -> void:
	awaiting_reward = false
	if not reward_received_this_show:
		status_label.text = "Anúncio fechado sem completar. Nenhuma recompensa."
	load_button.disabled = false

func _on_rewarded_failed_to_show(_ad_info: AdInfo, error_data: AdError) -> void:
	awaiting_reward = false
	load_button.disabled = false
	status_label.text = "Falha ao exibir: %s" % error_data.get_message()
