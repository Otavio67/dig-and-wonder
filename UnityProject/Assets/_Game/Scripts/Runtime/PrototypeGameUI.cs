using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>Hub e HUD do protótipo: telas separadas, contraste alto, retrato, toque e mouse.</summary>
    public sealed class PrototypeGameUI : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.035f, 0.11f, 0.14f);
        private static readonly Color Deep = new Color(0.055f, 0.17f, 0.2f);
        private static readonly Color Panel = new Color(0.09f, 0.24f, 0.27f);
        private static readonly Color Sand = new Color(0.98f, 0.79f, 0.43f);
        private static readonly Color Mint = new Color(0.36f, 0.86f, 0.68f);
        private static readonly Color Coral = new Color(1f, 0.42f, 0.31f);
        private static readonly Color White = new Color(1f, 0.96f, 0.83f);

        private Transform root;
        private AutoBattleDriver driver;
        private TextMeshProUGUI stageLabel;
        private TextMeshProUGUI walletLabel;
        private TextMeshProUGUI missionLabel;
        private TextMeshProUGUI statusLabel;
        private TextMeshProUGUI healthLabel;
        private TextMeshProUGUI combatLog;
        private TextMeshProUGUI menuTitle;
        private TextMeshProUGUI menuBody;
        private RectTransform attackUpgradeButton;
        private RectTransform healthUpgradeButton;
        private RectTransform restartButton;
        private RectTransform continueButton;
        private Button attackUpgradeUiButton;
        private Button healthUpgradeUiButton;
        private TextMeshProUGUI attackUpgradeButtonLabel;
        private TextMeshProUGUI healthUpgradeButtonLabel;
        private PooledCombatFeedback feedback;
        private Image healthFill;
        private TextMeshProUGUI drillIcon;
        private ProceduralAnimator2D drillAnimator;
        private GroundDrillMotion2D drillMotion;
        private DrillSpriteSheetPlayer2D drillFrames;
        private TextMeshProUGUI[] enemyIcons;
        private ProceduralAnimator2D[] enemyAnimators;
        private RectTransform[] caveSeams;
        private Tween[] caveMotionTweens;
        private TextMeshProUGUI[] enemyHealth;
        private RectTransform menuPanel;
        private RectTransform hubPanel;
        private RectTransform combatRoot;
        private RectTransform progressionPanel;
        private RectTransform worldPanel;
        private RectTransform bottomBar;
        private RectTransform hubEnterCaveButton;
        private RectTransform hubReturnButton;
        private TextMeshProUGUI hubStageLabel;
        private TextMeshProUGUI hubWalletLabel;
        private TextMeshProUGUI hubMissionLabel;
        private TextMeshProUGUI hubLockLabel;
        private TextMeshProUGUI hubEnterCaveButtonLabel;
        private DrillSpriteSheetPlayer2D hubDrillFrames;
        private TextMeshProUGUI progressionWalletLabel;
        private TextMeshProUGUI progressionPowerLabel;
        private TextMeshProUGUI progressionStatusLabel;
        private TextMeshProUGUI progressionAttackValue;
        private TextMeshProUGUI progressionHealthValue;
        private TextMeshProUGUI progressionAttackButtonLabel;
        private TextMeshProUGUI progressionHealthButtonLabel;
        private Button progressionAttackButton;
        private Button progressionHealthButton;
        private bool inGame;
        private bool saveWasBlocked;
        private string currentTab = "MELHORIAS";
        private float refreshTimer;
        private int displayedGold = -1;
        private readonly Dictionary<string, Image> tabButtonImages = new Dictionary<string, Image>();
        private ProgressionDefinition progression;
        private AbilityDefinition[] abilityCatalog;

        public void Initialize(Transform canvasRoot, AutoBattleDriver battleDriver)
        {
            root = canvasRoot;
            driver = battleDriver;
            progression = Resources.Load<ProgressionDefinition>("Data/Progression");
            abilityCatalog = Resources.LoadAll<AbilityDefinition>("Data/Abilities");
            driver.CombatEventRaised += OnCombatEvent;
            BuildInterface();
            feedback = gameObject.AddComponent<PooledCombatFeedback>();
            feedback.Initialize(worldPanel);
            if (driver.Simulation != null)
                EnterGame(true);
            else
                ShowHub();
        }

        private void OnDestroy()
        {
            if (driver != null) driver.CombatEventRaised -= OnCombatEvent;
            StopCaveMotion();
        }

        private void BuildInterface()
        {
            var background = MakePanel("Background", root, Ink, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            hubPanel = MakePanel("Expedition hub", background, new Color(0.055f, 0.17f, 0.2f), new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            BuildHubInterface(hubPanel);
            progressionPanel = MakePanel("Progression screen", background, new Color(0.12f, 0.19f, 0.24f), new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            BuildProgressionInterface(progressionPanel);
            combatRoot = MakePanel("Combat screen", background, new Color(0, 0, 0, 0), new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            combatRoot.GetComponent<Image>().raycastTarget = false;
            MakeLabel("Title", combatRoot, "DIG & WONDER", 40, Sand, new Vector2(0.04f, 0.91f), new Vector2(0.7f, 0.975f), TextAlignmentOptions.Left);
            stageLabel = MakeLabel("Stage", combatRoot, "CAVERNA", 25, White, new Vector2(0.04f, 0.865f), new Vector2(0.68f, 0.91f), TextAlignmentOptions.Left);
            walletLabel = MakeLabel("Wallet", combatRoot, "◉ 0", 30, Sand, new Vector2(0.68f, 0.905f), new Vector2(0.96f, 0.97f), TextAlignmentOptions.Right);
            var missionPanel = MakePanel("Mission banner", combatRoot, new Color(0.12f, 0.2f, 0.25f), new Vector2(0.04f, 0.785f), new Vector2(0.96f, 0.845f), Vector2.zero, Vector2.zero);
            missionLabel = MakeLabel("Mission", missionPanel, "MISSÃO 01  •  DERROTE O GUARDIÃO", 17, White,
                new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f), TextAlignmentOptions.Center);

            worldPanel = MakePanel("Cave view", combatRoot, Deep, new Vector2(0.04f, 0.325f), new Vector2(0.96f, 0.85f), Vector2.zero, Vector2.zero);
            MakePanel("Cave glow", worldPanel, new Color(0.13f, 0.33f, 0.34f), new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
            MakeLabel("Depth label", worldPanel, "POÇO DO ACAMPAMENTO  •  CAMADA 01", 21, Mint,
                new Vector2(0.06f, 0.83f), new Vector2(0.94f, 0.96f), TextAlignmentOptions.Center);
            // Faixas da caverna dão sensação de avanço sem animar posição lógica ou colisores.
            caveSeams = new RectTransform[8];
            caveMotionTweens = new Tween[caveSeams.Length];
            for (int i = 0; i < caveSeams.Length; i++)
            {
                float x = 0.05f + i * 0.13f;
                caveSeams[i] = MakePanel("Rock seam " + i, worldPanel, i % 2 == 0 ? new Color(0.16f, 0.38f, 0.37f) : new Color(0.12f, 0.3f, 0.33f),
                    new Vector2(x, 0.16f), new Vector2(x + 0.045f, 0.79f), Vector2.zero, Vector2.zero);
            }
            drillIcon = MakeLabel("Drill", worldPanel, "⛏", 96, Sand,
                new Vector2(0.09f, 0.35f), new Vector2(0.36f, 0.66f), TextAlignmentOptions.Center);
            var drillVisualObject = new GameObject("Drill Visual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            drillVisualObject.transform.SetParent(drillIcon.transform, false);
            var drillVisualRect = (RectTransform)drillVisualObject.transform;
            drillVisualRect.anchorMin = Vector2.zero;
            drillVisualRect.anchorMax = Vector2.one;
            drillVisualRect.offsetMin = Vector2.zero;
            drillVisualRect.offsetMax = Vector2.zero;
            drillFrames = drillVisualObject.AddComponent<DrillSpriteSheetPlayer2D>();
            drillAnimator = drillVisualObject.AddComponent<ProceduralAnimator2D>();
            drillMotion = drillVisualObject.AddComponent<GroundDrillMotion2D>();
            if (drillFrames.IsReady) drillIcon.text = string.Empty;
            enemyIcons = new TextMeshProUGUI[4];
            enemyAnimators = new ProceduralAnimator2D[4];
            enemyHealth = new TextMeshProUGUI[4];
            for (int i = 0; i < enemyIcons.Length; i++)
            {
                var icon = MakeLabel("Enemy " + i, worldPanel, "●", 58, Coral,
                    new Vector2(0.54f + i * 0.09f, 0.37f), new Vector2(0.70f + i * 0.09f, 0.58f), TextAlignmentOptions.Center);
                enemyIcons[i] = icon;
                enemyAnimators[i] = icon.gameObject.AddComponent<ProceduralAnimator2D>();
                enemyHealth[i] = MakeLabel("Enemy HP " + i, worldPanel, "", 17, White,
                    new Vector2(0.53f + i * 0.09f, 0.30f), new Vector2(0.72f + i * 0.09f, 0.38f), TextAlignmentOptions.Center);
                icon.gameObject.SetActive(false);
                enemyHealth[i].gameObject.SetActive(false);
            }
            healthLabel = MakeLabel("Health", worldPanel, "BROCA  120 / 120", 21, White,
                new Vector2(0.06f, 0.10f), new Vector2(0.72f, 0.19f), TextAlignmentOptions.Left);
            MakePanel("Health track", worldPanel, Ink, new Vector2(0.06f, 0.055f), new Vector2(0.94f, 0.1f), Vector2.zero, Vector2.zero);
            healthFill = MakePanel("Health fill", worldPanel, Mint, new Vector2(0.06f, 0.055f), new Vector2(0.94f, 0.1f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            statusLabel = MakeLabel("Status", combatRoot, "", 20, Mint,
                new Vector2(0.04f, 0.275f), new Vector2(0.96f, 0.32f), TextAlignmentOptions.Center);
            combatLog = MakeLabel("Combat log", combatRoot, "", 18, White,
                new Vector2(0.06f, 0.235f), new Vector2(0.94f, 0.275f), TextAlignmentOptions.Center);

            bottomBar = MakePanel("Bottom navigation", combatRoot, new Color(0.04f, 0.13f, 0.16f), new Vector2(0.02f, 0.025f), new Vector2(0.98f, 0.125f), Vector2.zero, Vector2.zero);
            string[] tabs = { "MINA", "MELHORIAS", "BROCAS", "BAÚS", "PERFIL" };
            for (int i = 0; i < tabs.Length; i++)
            {
                string tab = tabs[i];
                float left = i / 5f;
                RectTransform tabButton = MakeButton("Tab " + tab, bottomBar, tab, 18, left + 0.006f, 0.04f, left + 0.194f, 0.96f,
                    () => SelectTab(tab), tab == currentTab ? Sand : White);
                tabButtonImages.Add(tab, tabButton.GetComponent<Image>());
            }

            menuPanel = MakePanel("Tab panel", combatRoot, Panel, new Vector2(0.06f, 0.135f), new Vector2(0.94f, 0.315f), Vector2.zero, Vector2.zero);
            menuTitle = MakeLabel("Tab title", menuPanel, "MELHORIAS", 22, Sand, new Vector2(0.03f, 0.74f), new Vector2(0.97f, 0.98f), TextAlignmentOptions.Center);
            menuBody = MakeLabel("Tab info", menuPanel, "", 15, White, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.74f), TextAlignmentOptions.Center);
            attackUpgradeButton = MakeButton("Upgrade attack", menuPanel, "MELHORAR", 17, 0.04f, 0.03f, 0.47f, 0.28f,
                () => { if (!driver.TryPurchaseUpgrade(UpgradeKind.Attack)) statusLabel.text = "OURO INSUFICIENTE OU NÍVEL MÁXIMO"; Refresh(); }, Mint).GetComponent<RectTransform>();
            attackUpgradeUiButton = attackUpgradeButton.GetComponent<Button>();
            attackUpgradeButtonLabel = attackUpgradeButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            healthUpgradeButton = MakeButton("Upgrade health", menuPanel, "MELHORAR", 17, 0.53f, 0.03f, 0.96f, 0.28f,
                () => { if (!driver.TryPurchaseUpgrade(UpgradeKind.Health)) statusLabel.text = "OURO INSUFICIENTE OU NÍVEL MÁXIMO"; Refresh(); }, Sand).GetComponent<RectTransform>();
            healthUpgradeUiButton = healthUpgradeButton.GetComponent<Button>();
            healthUpgradeButtonLabel = healthUpgradeButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            restartButton = MakeButton("Restart run", menuPanel, "RECOMEÇAR", 17, 0.25f, 0.03f, 0.75f, 0.28f,
                () => { driver.StartNewGame(); EnterGame(false); }, Coral).GetComponent<RectTransform>();
            restartButton.gameObject.SetActive(false);
            continueButton = MakeButton("Continue stage", menuPanel, "AVANÇAR FASE", 17, 0.2f, 0.03f, 0.8f, 0.28f,
                () => { driver.StartNewGame(); EnterGame(false); }, Mint).GetComponent<RectTransform>();
            continueButton.gameObject.SetActive(false);
            hubEnterCaveButton = MakeButton("Hub enter cave", hubPanel, "ENTRAR NA CAVERNA", 20, 0.22f, 0.07f, 0.78f, 0.16f,
                () =>
                {
                    if (driver.Simulation != null && driver.Simulation.IsActive) EnterGame(true);
                    else if (driver.TryStartNewGame()) EnterGame(false);
                    else hubMissionLabel.text = "Não foi possível iniciar — o progresso foi preservado";
                }, Mint).GetComponent<RectTransform>();
            hubEnterCaveButtonLabel = hubEnterCaveButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            hubReturnButton = MakeButton("Return to hub", combatRoot, "ACAMPAMENTO", 14, 0.72f, 0.86f, 0.96f, 0.91f,
                ShowHub, new Color(0.16f, 0.35f, 0.36f)).GetComponent<RectTransform>();
            hubReturnButton.gameObject.SetActive(false);
            menuPanel.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
            worldPanel.gameObject.SetActive(false);
            stageLabel.gameObject.SetActive(false);
            walletLabel.gameObject.SetActive(false);
            missionLabel.gameObject.SetActive(false);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
        }

        private void BuildHubInterface(RectTransform panel)
        {
            MakeLabel("Hub title", panel, "ACAMPAMENTO", 38, Sand,
                new Vector2(0.06f, 0.9f), new Vector2(0.66f, 0.98f), TextAlignmentOptions.Left);
            hubWalletLabel = MakeLabel("Hub wallet", panel, "◉ 0", 28, Sand,
                new Vector2(0.68f, 0.91f), new Vector2(0.94f, 0.98f), TextAlignmentOptions.Right);
            hubStageLabel = MakeLabel("Hub stage", panel, "EXPEDIÇÃO 01", 22, White,
                new Vector2(0.06f, 0.84f), new Vector2(0.62f, 0.9f), TextAlignmentOptions.Left);
            hubMissionLabel = MakeLabel("Hub mission", panel, "Derrote o guardião para avançar", 17, Mint,
                new Vector2(0.06f, 0.79f), new Vector2(0.94f, 0.84f), TextAlignmentOptions.Left);

            var drillCard = MakePanel("Hub drill card", panel, new Color(0.08f, 0.25f, 0.28f),
                new Vector2(0.06f, 0.49f), new Vector2(0.94f, 0.77f), Vector2.zero, Vector2.zero);
            MakeLabel("Hub drill title", drillCard, "BROCA PIONEIRA", 18, Sand,
                new Vector2(0.52f, 0.72f), new Vector2(0.96f, 0.92f), TextAlignmentOptions.Center);
            MakeLabel("Hub drill stats", drillCard, "NÍVEL 1\nATQ 6  •  VIDA 120", 17, White,
                new Vector2(0.52f, 0.28f), new Vector2(0.96f, 0.7f), TextAlignmentOptions.Center);
            var hubDrillObject = new GameObject("Hub Drill Visual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hubDrillObject.transform.SetParent(drillCard, false);
            var hubDrillRect = (RectTransform)hubDrillObject.transform;
            hubDrillRect.anchorMin = new Vector2(0.05f, 0.08f);
            hubDrillRect.anchorMax = new Vector2(0.5f, 0.95f);
            hubDrillRect.offsetMin = Vector2.zero;
            hubDrillRect.offsetMax = Vector2.zero;
            hubDrillFrames = hubDrillObject.AddComponent<DrillSpriteSheetPlayer2D>();
            hubDrillObject.AddComponent<ProceduralAnimator2D>();

            MakeHubCard(panel, "Oficina", "ATQ  •  VIDA\nMelhorias disponíveis", new Vector2(0.06f, 0.3f), new Vector2(0.47f, 0.45f), Mint);
            MakeHubCard(panel, "Garagem", "Brocas alternativas\n🔒 FASE 2", new Vector2(0.53f, 0.3f), new Vector2(0.94f, 0.45f), new Color(0.25f, 0.28f, 0.36f));
            MakeHubCard(panel, "Baús", "Baús 0  •  Chaves 0\nRecompensas da fase", new Vector2(0.06f, 0.17f), new Vector2(0.47f, 0.29f), Sand);
            MakeHubCard(panel, "Álbum", "Descobertas da caverna\n🔒 FASE 2", new Vector2(0.53f, 0.17f), new Vector2(0.94f, 0.29f), new Color(0.25f, 0.28f, 0.36f));
            hubLockLabel = MakeLabel("Hub lock status", panel, "Equipamentos e álbum desbloqueiam na Fase 2", 15, White,
                new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.17f), TextAlignmentOptions.Center);
        }

        private void BuildProgressionInterface(RectTransform panel)
        {
            MakeLabel("Progression title", panel, "OFICINA DA BROCA", 34, Sand,
                new Vector2(0.06f, 0.91f), new Vector2(0.68f, 0.98f), TextAlignmentOptions.Left);
            progressionWalletLabel = MakeLabel("Progression wallet", panel, "⚙ 0", 27, Sand,
                new Vector2(0.69f, 0.92f), new Vector2(0.94f, 0.98f), TextAlignmentOptions.Right);
            MakeLabel("Progression drill", panel, "BROCA PIONEIRA  •  PERSONAGEM PRINCIPAL", 18, White,
                new Vector2(0.06f, 0.84f), new Vector2(0.7f, 0.9f), TextAlignmentOptions.Left);
            progressionPowerLabel = MakeLabel("Progression power", panel, "PODER 6,0", 20, Mint,
                new Vector2(0.7f, 0.84f), new Vector2(0.94f, 0.9f), TextAlignmentOptions.Right);

            RectTransform attackRow = MakePanel("Attack upgrade row", panel, new Color(0.24f, 0.35f, 0.45f),
                new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero);
            MakeLabel("Attack icon", attackRow, "⚔", 39, Sand, new Vector2(0.03f, 0.12f), new Vector2(0.15f, 0.88f), TextAlignmentOptions.Center);
            progressionAttackValue = MakeLabel("Attack value", attackRow, "ATQ\nNv. 0\n6,0", 18, White,
                new Vector2(0.17f, 0.14f), new Vector2(0.48f, 0.9f), TextAlignmentOptions.Left);
            progressionAttackButton = MakeButton("Progression attack", attackRow, "MELHORAR", 17, 0.57f, 0.12f, 0.96f, 0.88f,
                () => PurchaseFromProgression(UpgradeKind.Attack), Mint).GetComponent<Button>();
            progressionAttackButtonLabel = progressionAttackButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();

            RectTransform healthRow = MakePanel("Health upgrade row", panel, new Color(0.27f, 0.36f, 0.44f),
                new Vector2(0.04f, 0.53f), new Vector2(0.96f, 0.67f), Vector2.zero, Vector2.zero);
            MakeLabel("Health icon", healthRow, "♥", 39, Coral, new Vector2(0.03f, 0.12f), new Vector2(0.15f, 0.88f), TextAlignmentOptions.Center);
            progressionHealthValue = MakeLabel("Health value", healthRow, "PV\nNv. 0\n120", 18, White,
                new Vector2(0.17f, 0.14f), new Vector2(0.48f, 0.9f), TextAlignmentOptions.Left);
            progressionHealthButton = MakeButton("Progression health", healthRow, "MELHORAR", 17, 0.57f, 0.12f, 0.96f, 0.88f,
                () => PurchaseFromProgression(UpgradeKind.Health), Sand).GetComponent<Button>();
            progressionHealthButtonLabel = progressionHealthButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();

            MakeLockedProgressionRow(panel, "🔧  RECUPERAÇÃO DE PV", "Requer VIDA Nv. 10  •  EM BALANCEAMENTO", new Vector2(0.04f, 0.39f), new Vector2(0.96f, 0.52f));
            MakeLockedProgressionRow(panel, "🎯  CHANCE DE CRÍTICO", "Requer ATQ Nv. 10  •  EM BALANCEAMENTO", new Vector2(0.04f, 0.25f), new Vector2(0.96f, 0.38f));
            MakeLockedProgressionRow(panel, "🔥  DANO CRÍTICO / ESPECIAL", "Requer FASE 2  •  EM BALANCEAMENTO", new Vector2(0.04f, 0.11f), new Vector2(0.96f, 0.24f));
            progressionStatusLabel = MakeLabel("Progression status", panel, "Melhorias são salvas automaticamente", 15, White,
                new Vector2(0.05f, 0.05f), new Vector2(0.58f, 0.1f), TextAlignmentOptions.Left);
            MakeButton("Back to hub", panel, "VOLTAR AO ACAMPAMENTO", 15, 0.6f, 0.04f, 0.95f, 0.11f,
                ShowHub, new Color(0.16f, 0.35f, 0.36f));
        }

        private void MakeLockedProgressionRow(RectTransform parent, string title, string body, Vector2 min, Vector2 max)
        {
            RectTransform row = MakePanel("Locked progression row", parent, new Color(0.12f, 0.19f, 0.25f), min, max, Vector2.zero, Vector2.zero);
            MakeLabel("Locked title", row, title, 17, White, new Vector2(0.04f, 0.48f), new Vector2(0.58f, 0.92f), TextAlignmentOptions.Left);
            MakeLabel("Locked body", row, body, 13, new Color(0.72f, 0.76f, 0.78f), new Vector2(0.04f, 0.08f), new Vector2(0.94f, 0.48f), TextAlignmentOptions.Left);
        }

        private void MakeHubCard(RectTransform parent, string title, string body, Vector2 min, Vector2 max, Color color)
        {
            RectTransform card = MakePanel("Hub card " + title, parent, color, min, max, Vector2.zero, Vector2.zero);
            MakeLabel("Title", card, title.ToUpperInvariant(), 17, Ink,
                new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.94f), TextAlignmentOptions.Center);
            MakeLabel("Body", card, body, 13, Ink,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.56f), TextAlignmentOptions.Center);
            if (title == "Oficina")
            {
                Button button = card.gameObject.AddComponent<Button>();
                button.onClick.AddListener(OpenProgression);
            }
        }

        private void OpenProgression()
        {
            if (driver.Simulation == null)
            {
                hubMissionLabel.text = "Inicie uma expedição antes de usar a oficina";
                return;
            }

            inGame = false;
            StopCaveMotion();
            hubPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
            progressionPanel.gameObject.SetActive(true);
            RefreshProgression();
        }

        private void PurchaseFromProgression(UpgradeKind kind)
        {
            bool purchased = driver.TryPurchaseUpgradeAtBase(kind);
            progressionStatusLabel.text = purchased ? "MELHORIA COMPRADA — PROGRESSO SALVO" : "OURO INSUFICIENTE OU NÍVEL MÁXIMO";
            RefreshProgression();
        }

        private void RefreshProgression()
        {
            IdleCombatSimulation sim = driver.Simulation;
            if (sim == null)
            {
                progressionWalletLabel.text = "⚙ 0";
                progressionPowerLabel.text = "PODER —";
                progressionAttackValue.text = "ATQ\nINICIE A EXPEDIÇÃO";
                progressionHealthValue.text = "PV\nINICIE A EXPEDIÇÃO";
                progressionAttackButton.interactable = false;
                progressionHealthButton.interactable = false;
                return;
            }

            int attackCost = sim.Profile.AttackLevel < driver.CurrentAttackMaximum ? driver.GetAttackUpgradeCost() : -1;
            int healthCost = sim.Profile.HealthLevel < driver.CurrentHealthMaximum ? driver.GetHealthUpgradeCost() : -1;
            progressionWalletLabel.text = "⚙ " + sim.Gold;
            progressionPowerLabel.text = "PODER " + driver.CurrentAttackDamage.ToString("0.0");
            progressionAttackValue.text = "ATQ\nNv. " + sim.Profile.AttackLevel + "/" + driver.CurrentAttackMaximum +
                "\n" + driver.CurrentAttackDamage.ToString("0.0");
            progressionHealthValue.text = "PV\nNv. " + sim.Profile.HealthLevel + "/" + driver.CurrentHealthMaximum +
                "\n" + sim.PlayerMaximumHealth.ToString("0");
            progressionAttackButtonLabel.text = attackCost < 0 ? "ATQ  MÁXIMO" : "MELHORAR\n⚙ " + attackCost;
            progressionHealthButtonLabel.text = healthCost < 0 ? "PV  MÁXIMO" : "MELHORAR\n⚙ " + healthCost;
            progressionAttackButton.interactable = attackCost >= 0 && !driver.IsSaveBlocked;
            progressionHealthButton.interactable = healthCost >= 0 && !driver.IsSaveBlocked;
        }

        private void ShowHub()
        {
            inGame = false;
            StopCaveMotion();
            hubPanel.gameObject.SetActive(true);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
            hubEnterCaveButton.gameObject.SetActive(driver.IsReady);
            if (hubEnterCaveButtonLabel != null)
                hubEnterCaveButtonLabel.text = driver.Simulation == null || !driver.Simulation.IsActive ? "NOVA TENTATIVA" : "CONTINUAR EXPEDIÇÃO";
            RefreshHub();
        }

        private void RefreshHub()
        {
            if (driver == null || driver.Simulation == null)
            {
                hubStageLabel.text = "NOVA EXPEDIÇÃO";
                hubWalletLabel.text = "◉ 0";
                hubMissionLabel.text = "Inicie a escavação para construir seu acampamento";
                hubLockLabel.text = "Equipamentos e álbum desbloqueiam na Fase 2";
                return;
            }

            IdleCombatSimulation sim = driver.Simulation;
            int stageNumber = GetStageNumber(sim.Profile.UnlockedStageId);
            string stageName = sim.CurrentStageDisplayName == null ? "NOVA EXPEDIÇÃO" : sim.CurrentStageDisplayName.ToUpperInvariant();
            hubStageLabel.text = "EXPEDIÇÃO " + stageNumber.ToString("00") + "  •  " + stageName;
            hubWalletLabel.text = "◉ " + sim.Gold;
            hubMissionLabel.text = sim.Run.IsPlayerDefeated ? "Tentativa encerrada — melhore a broca e retorne" :
                sim.Run.IsStageCleared ? "Guardião derrotado — próxima fase disponível" : "Derrote o guardião para avançar";
            hubLockLabel.text = stageNumber < 2 ? "Equipamentos e álbum  🔒  desbloqueiam na Fase 2" : "Equipamentos e álbum  •  sistema em breve";
        }

        private void EnterGame(bool resumed)
        {
            if (driver.Simulation == null) return;
            inGame = true;
            hubPanel.gameObject.SetActive(false);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(true);
            worldPanel.gameObject.SetActive(true);
            menuPanel.gameObject.SetActive(true);
            bottomBar.gameObject.SetActive(true);
            stageLabel.gameObject.SetActive(true);
            walletLabel.gameObject.SetActive(true);
            missionLabel.gameObject.SetActive(true);
            statusLabel.gameObject.SetActive(true);
            combatLog.gameObject.SetActive(true);
            hubReturnButton.gameObject.SetActive(false);
            if (resumed) statusLabel.text = "PROGRESSO RECUPERADO — CONTINUANDO DA ÚLTIMA POSIÇÃO";
            else statusLabel.text = "A broca começou a avançar!";
            drillAnimator.ResetForRun();
            drillFrames.ResetForRun();
            hubDrillFrames.ResetForRun();
            StartCaveMotion();
            Refresh();
        }

        private void Update()
        {
            if (!inGame || driver == null || driver.Simulation == null) return;
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= 0.12f)
            {
                refreshTimer = 0f;
                Refresh();
            }
        }

        private void Refresh()
        {
            IdleCombatSimulation sim = driver.Simulation;
            if (sim == null) return;
            stageLabel.text = sim.CurrentStageDisplayName == null ? "NOVA EXPEDIÇÃO" : sim.CurrentStageDisplayName.ToUpperInvariant();
            int stageNumber = GetStageNumber(sim.Profile.UnlockedStageId);
            missionLabel.text = "MISSÃO " + stageNumber.ToString("00") + "  •  DERROTE O GUARDIÃO  •  " + (sim.Run.IsStageCleared ? "CONCLUÍDA" : "EM ANDAMENTO");
            if (displayedGold != sim.Gold)
            {
                displayedGold = sim.Gold;
                walletLabel.text = "◉ " + sim.Gold;
                walletLabel.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0), 0.22f, 5, 0.7f);
            }
            healthLabel.text = "BROCA  " + Mathf.CeilToInt(sim.PlayerHealth) + " / " + Mathf.CeilToInt(sim.PlayerMaximumHealth);
            float healthRatio = sim.PlayerMaximumHealth <= 0 ? 0 : sim.PlayerHealth / sim.PlayerMaximumHealth;
            healthFill.rectTransform.anchorMax = new Vector2(0.06f + 0.88f * healthRatio, 0.1f);
            for (int i = 0; i < enemyIcons.Length; i++)
            {
                bool active = i < sim.Enemies.Count;
                enemyIcons[i].gameObject.SetActive(active);
                enemyHealth[i].gameObject.SetActive(active);
                if (active)
                {
                    EnemySnapshot enemy = sim.Enemies[i];
                    enemyIcons[i].text = enemy.IsBoss ? "♛" : "●";
                    enemyIcons[i].color = enemy.IsBoss ? Sand : Coral;
                    enemyHealth[i].text = Mathf.CeilToInt(enemy.Health) + " PV";
                }
            }
            UpdateTab();
            if (sim.Run.IsPlayerDefeated)
            {
                statusLabel.text = "A broca parou. Seu ouro está salvo.";
                currentTab = "MINA";
                menuTitle.text = "TENTATIVA ENCERRADA";
                menuBody.text = "Melhore a broca e tente novamente.";
            }
            else if (sim.Run.IsStageCleared)
            {
                statusLabel.text = "GUARDIÃO DERROTADO — FASE CONCLUÍDA!";
            }
            if (driver.IsSaveBlocked)
            {
                if (!saveWasBlocked) StopCaveMotion();
                statusLabel.text = driver.LastSaveError;
            }
            else if (saveWasBlocked && sim.IsActive)
            {
                StartCaveMotion();
                statusLabel.text = "PROGRESSO SALVO — COMBATE RETOMADO";
            }
            hubReturnButton.gameObject.SetActive(!sim.IsActive && !driver.IsSaveBlocked);
            saveWasBlocked = driver.IsSaveBlocked;
        }

        private void SelectTab(string tab)
        {
            currentTab = tab;
            UpdateTab();
        }

        private void UpdateTab()
        {
            if (driver.Simulation == null) return;
            var sim = driver.Simulation;
            int attackCost = sim.Profile.AttackLevel < driver.CurrentAttackMaximum ? driver.GetAttackUpgradeCost() : -1;
            int healthCost = sim.Profile.HealthLevel < driver.CurrentHealthMaximum ? driver.GetHealthUpgradeCost() : -1;
            menuTitle.text = currentTab;
            switch (currentTab)
            {
                case "MINA":
                    menuTitle.text = "CENTRAL DA EXPEDIÇÃO";
                    menuBody.text = sim.Run.IsPlayerDefeated ? "A escavação terminou. Toque para recomeçar." : "A broca avança e luta automaticamente.\nO guardião é a única condição de avanço.\n\nFASE " + GetStageNumber(sim.CurrentStageId) + "  •  " + sim.CurrentStageDisplayName;
                    break;
                case "MELHORIAS":
                    menuTitle.text = "OFICINA DA BROCA";
                    menuBody.text = BuildUpgradeSummary(sim, attackCost, healthCost);
                    break;
                case "BROCAS":
                    menuTitle.text = "GARAGEM DE BROCAS";
                    menuBody.text = BuildAbilitySummary(GetStageNumber(sim.Profile.UnlockedStageId));
                    break;
                case "BAÚS":
                    menuTitle.text = "BAÚS E EQUIPAMENTOS";
                    menuBody.text = BuildChestSummary(sim, GetStageNumber(sim.Profile.UnlockedStageId));
                    break;
                default:
                    menuTitle.text = "PERFIL DA EXPEDIÇÃO";
                    menuBody.text = "FASE DESBLOQUEADA  " + sim.Profile.UnlockedStageId.Replace('_', ' ') + "\nATQ  Nv." + sim.Profile.AttackLevel + "   •   VIDA  Nv." + sim.Profile.HealthLevel + "\n\nPROGRESSO SALVO AUTOMATICAMENTE\nNovos módulos aparecem conforme a expedição avança.";
                    break;
            }
            bool defeated = sim.Run.IsPlayerDefeated;
            bool cleared = sim.Run.IsStageCleared;
            bool upgradesVisible = currentTab == "MELHORIAS" && !defeated && !cleared && !driver.IsSaveBlocked;
            attackUpgradeButton.gameObject.SetActive(upgradesVisible);
            healthUpgradeButton.gameObject.SetActive(upgradesVisible);
            if (upgradesVisible)
            {
                bool attackMaxed = attackCost < 0;
                bool healthMaxed = healthCost < 0;
                attackUpgradeButtonLabel.text = attackMaxed ? "ATQ  MÁXIMO" : "MELHORAR\n◉ " + attackCost;
                healthUpgradeButtonLabel.text = healthMaxed ? "VIDA  MÁXIMA" : "MELHORAR\n◉ " + healthCost;
                attackUpgradeUiButton.interactable = !attackMaxed;
                healthUpgradeUiButton.interactable = !healthMaxed;
                attackUpgradeButton.GetComponent<Image>().color = attackMaxed ? new Color(0.18f, 0.3f, 0.29f) : Mint;
                healthUpgradeButton.GetComponent<Image>().color = healthMaxed ? new Color(0.3f, 0.28f, 0.18f) : Sand;
            }
            restartButton.gameObject.SetActive(defeated && !driver.IsSaveBlocked);
            continueButton.gameObject.SetActive(cleared && !driver.IsSaveBlocked);
            foreach (KeyValuePair<string, Image> button in tabButtonImages)
                button.Value.color = button.Key == currentTab ? Sand : new Color(0.14f, 0.34f, 0.36f);
        }

        private string BuildUpgradeSummary(IdleCombatSimulation sim, int attackCost, int healthCost)
        {
            int attackMaximum = driver.CurrentAttackMaximum;
            int healthMaximum = driver.CurrentHealthMaximum;
            UpgradeDefinition critical = progression == null ? null : progression.criticalChanceUpgrade;
            UpgradeDefinition criticalDamage = progression == null ? null : progression.criticalDamageUpgrade;
            UpgradeDefinition special = progression == null ? null : progression.specialUpgrade;
            return "ATQ  Nv." + sim.Profile.AttackLevel + "/" + attackMaximum + "   •   " + (attackCost < 0 ? "MÁX." : attackCost + " ouro") +
                "\nVIDA  Nv." + sim.Profile.HealthLevel + "/" + healthMaximum + "   •   " + (healthCost < 0 ? "MÁX." : healthCost + " ouro") +
                "\n\n" + FormatLockedUpgrade("RECUPERAÇÃO", sim.Profile.HealthLevel, healthMaximum, "VIDA") +
                "\n" + FormatLockedUpgrade(critical == null ? "CRÍTICO" : critical.displayName, sim.Profile.AttackLevel, attackMaximum, "ATQ") +
                "\n" + FormatLockedUpgrade(criticalDamage == null ? "DANO CRÍTICO" : criticalDamage.displayName, sim.Profile.AttackLevel, attackMaximum, "ATQ") +
                "\n" + FormatLockedUpgrade(special == null ? "MELHORIA DO ESPECIAL" : special.displayName, GetStageNumber(sim.Profile.UnlockedStageId), 2, "FASE");
        }

        private string FormatLockedUpgrade(string displayName, int currentLevel, int requiredLevel, string source)
        {
            if (currentLevel < requiredLevel) return "🔒 " + displayName.ToUpperInvariant() + "  •  requer " + source + " Nv." + requiredLevel;
            return "🔒 " + displayName.ToUpperInvariant() + "  •  EM BALANCEAMENTO";
        }

        private string BuildAbilitySummary(int currentStage)
        {
            if (abilityCatalog == null || abilityCatalog.Length == 0)
                return "BROCA PIONEIRA  •  DISPONÍVEL\nEspecial: investida em quatro impactos.";

            StringBuilder available = new StringBuilder();
            StringBuilder[] lockedByPhase = { new StringBuilder(), new StringBuilder(), new StringBuilder(), new StringBuilder(), new StringBuilder() };
            for (int i = 0; i < abilityCatalog.Length; i++)
            {
                AbilityDefinition ability = abilityCatalog[i];
                if (ability == null) continue;
                if (ability.implemented)
                {
                    AppendCatalogName(available, ability.displayName);
                    continue;
                }
                int phase = (int)Mathf.Clamp(ability.unlockPhase, 1, lockedByPhase.Length - 1);
                AppendCatalogName(lockedByPhase[phase], ability.displayName);
            }

            StringBuilder summary = new StringBuilder("CATÁLOGO DE BROCAS\n");
            if (available.Length > 0) summary.Append("DISPONÍVEIS  ").Append(available).Append('\n');
            for (int phase = 2; phase < lockedByPhase.Length; phase++)
            {
                if (lockedByPhase[phase].Length == 0) continue;
                summary.Append("🔒 FASE ").Append(phase).Append("  ").Append(lockedByPhase[phase]);
                if (currentStage >= phase) summary.Append("  •  EM BREVE");
                summary.Append('\n');
            }
            return summary.ToString().TrimEnd();
        }

        private void AppendCatalogName(StringBuilder target, string displayName)
        {
            if (target.Length > 0) target.Append("  •  ");
            target.Append(displayName.ToUpperInvariant());
        }

        private string BuildChestSummary(IdleCombatSimulation sim, int currentStage)
        {
            string equipmentState = currentStage < 2 ? "🔒 FASE 2" : "DESBLOQUEADO  •  EM BREVE";
            return "BAÚS GUARDADOS  " + sim.Profile.Chests + "   •   CHAVES  " + sim.Profile.Keys +
                "\nO guardião entrega baús e chaves da fase.\n\nEQUIPAMENTOS  " + equipmentState +
                "\nÁLBUM DE DESCOBERTAS  " + equipmentState;
        }

        private int GetStageNumber(string stageId)
        {
            if (progression == null) return 1;
            if (progression.normalCycle != null)
                for (int i = 0; i < progression.normalCycle.Length; i++)
                    if (progression.normalCycle[i] != null && progression.normalCycle[i].id == stageId) return i + 1;
            return progression.surpriseStage != null && progression.surpriseStage.id == stageId
                ? (progression.normalCycle == null ? 1 : progression.normalCycle.Length + 1) : 1;
        }

        private void OnCombatEvent(CombatEvent e)
        {
            if (!inGame) return;
            feedback.Handle(e);
            switch (e.Type)
            {
                case CombatEventType.EnemySpawned: statusLabel.text = "UMA CRIATURA SURGIU!"; break;
                case CombatEventType.BossSpawned: statusLabel.text = "GUARDIÃO À FRENTE!"; statusLabel.transform.DOPunchScale(new Vector3(0.18f, 0.18f, 0), 0.35f, 4, 0.65f); break;
                case CombatEventType.SpecialStarted: combatLog.text = "ESPECIAL: PIONEIRA EM LANÇAMENTO"; break;
                case CombatEventType.SpecialStrike: combatLog.text = "IMPACTO DA PIONEIRA  •  " + Mathf.CeilToInt(e.Amount); break;
                case CombatEventType.GoldAwarded: combatLog.text = "+" + Mathf.CeilToInt(e.Amount) + " ouro"; break;
                case CombatEventType.UpgradePurchased: statusLabel.text = "MELHORIA COMPRADA — PROGRESSO SALVO"; break;
                case CombatEventType.ChestAwarded: combatLog.text = "BAÚ DE FASE ENCONTRADO!"; break;
                case CombatEventType.PlayerDefeated: statusLabel.text = "DERROTA — OURO E MELHORIAS PRESERVADOS"; break;
                case CombatEventType.StageCleared: statusLabel.text = "FASE CONCLUÍDA! PRÓXIMA: " + e.Text; break;
            }
            if (e.Type == CombatEventType.DrillAttack)
            {
                drillAnimator.PlayAttack(false);
                drillMotion.PlayAttack(false);
                drillFrames.PlayAttack(false);
            }
            else if (e.Type == CombatEventType.SpecialStarted)
            {
                drillAnimator.PlayAttack(true);
                drillMotion.PlayAttack(true);
                drillFrames.PlayAttack(true);
            }
            else if (e.Type == CombatEventType.PlayerDamaged)
            {
                drillAnimator.PlayImpact();
                drillMotion.PlayImpact();
                drillFrames.PlayImpact();
            }
            else if (e.Type == CombatEventType.PlayerDefeated)
            {
                drillAnimator.PlayDeath();
                drillMotion.PlayDeath();
                drillFrames.PlayDeath();
            }
            else if (e.Type == CombatEventType.RunStarted)
            {
                drillAnimator.ResetForRun();
                drillFrames.ResetForRun();
            }
            if (e.Type == CombatEventType.RunStarted && inGame) StartCaveMotion();
            else if (e.Type == CombatEventType.PlayerDefeated || e.Type == CombatEventType.StageCleared) StopCaveMotion();
            if (e.Type == CombatEventType.Hit)
            {
                int index = Mathf.Abs(e.EntityId) % enemyAnimators.Length;
                enemyAnimators[index].PlayImpact();
            }
        }

        private RectTransform MakePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var rect = (RectTransform)gameObject.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            var image = gameObject.GetComponent<Image>(); image.color = color;
            return rect;
        }

        private void StartCaveMotion()
        {
            StopCaveMotion();
            for (int i = 0; i < caveSeams.Length; i++)
                caveMotionTweens[i] = caveSeams[i].DOAnchorPosX(-1220f, 7.5f + i * 0.18f)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetDelay(i * 0.4f);
            drillMotion?.BeginWalk();
            drillFrames?.BeginWalk();
        }

        private void StopCaveMotion()
        {
            if (caveMotionTweens != null)
                foreach (Tween tween in caveMotionTweens) tween?.Kill(false);
            drillMotion?.StopWalking();
        }

        private TextMeshProUGUI MakeLabel(string name, Transform parent, string text, int size, Color color, Vector2 min, Vector2 max, TextAlignmentOptions alignment)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var label = rect.gameObject.GetComponent<TextMeshProUGUI>();
            label.text = text; label.fontSize = size; label.color = color; label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal; label.raycastTarget = false;
            return label;
        }

        private RectTransform MakeButton(string name, Transform parent, string text, int size, float left, float bottom, float right, float top, UnityEngine.Events.UnityAction action, Color color)
        {
            var rect = MakePanel(name, parent, color, new Vector2(left, bottom), new Vector2(right, top), Vector2.zero, Vector2.zero);
            var button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            var label = MakeLabel("Label", rect, text, size, Ink, Vector2.zero, new Vector2(1, 1), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            return rect;
        }
    }
}
