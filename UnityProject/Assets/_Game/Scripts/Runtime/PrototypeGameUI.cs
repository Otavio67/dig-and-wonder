using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>Menu inicial e HUD: mineração contínua, contraste alto, retrato, toque e mouse.</summary>
    public sealed class PrototypeGameUI : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.035f, 0.11f, 0.14f);
        private static readonly Color Deep = new Color(0.055f, 0.17f, 0.2f);
        private static readonly Color Panel = new Color(0.09f, 0.24f, 0.27f);
        private static readonly Color Sand = new Color(0.98f, 0.79f, 0.43f);
        private static readonly Color Mint = new Color(0.36f, 0.86f, 0.68f);
        private static readonly Color Coral = new Color(1f, 0.42f, 0.31f);
        private static readonly Color White = new Color(1f, 0.96f, 0.83f);
        private static readonly Color Cream = new Color(1f, 0.97f, 0.88f);
        private static readonly Color Steel = new Color(0.26f, 0.36f, 0.43f);
        private static readonly Color Dim = new Color(0.62f, 0.70f, 0.74f);
        private static readonly Color Cyan = new Color(0f, 0.96f, 0.83f);
        private static readonly Color Gold = new Color(1f, 0.72f, 0.08f);

        private Transform root;
        private AutoBattleDriver driver;
        private TextMeshProUGUI stageLabel;
        private TextMeshProUGUI walletLabel;
        private TextMeshProUGUI crystalLabel;
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
        private Button autoToggleButton;
        private TextMeshProUGUI autoStateLabel;
        private PooledCombatFeedback feedback;
        private Image healthFill;
        private TextMeshProUGUI drillIcon;
        private ProceduralAnimator2D drillAnimator;
        private GroundDrillMotion2D drillMotion;
        private DrillSpriteSheetPlayer2D drillFrames;
        private RectTransform[] enemyVisualRoots;
        private Image[] enemyImages;
        private EnemySpritePresenter[] enemySprites;
        private ProceduralAnimator2D[] enemyAnimators;
        private RectTransform[] caveSeams;
        private Tween[] caveMotionTweens;
        private TextMeshProUGUI[] enemyHealth;
        private Image[] enemyHealthFills;
        private int[] enemyIds;
        private float[] enemyMaximumHealth;
        private RectTransform menuPanel;
        private RectTransform upgradeRowsRoot;
        private ScrollRect upgradeScroll;
        private RectTransform startPanel;
        private RectTransform combatRoot;
        private RectTransform progressionPanel;
        private RectTransform worldPanel;
        private RectTransform bottomBar;
        private UnityEngine.UI.Button startButton;
        private TextMeshProUGUI startStatusLabel;
        private TMP_FontAsset startFont;
        private RectTransform loadingRoot;
        private RectTransform loadingFill;
        private TextMeshProUGUI loadingLabel;
        private bool startupComplete;
        private string startupFailure;
        private float loadingProgress;
        private Image stageBackdrop;
        private AspectRatioFitter backdropAspect;
        private string renderedStageId;
        private bool showingStartMenu;
        private float nextStageTimer;
        private const float StageVictorySeconds = 1.8f;
        private readonly Dictionary<string, Sprite> mapBackgroundSprites = new Dictionary<string, Sprite>();
        private CharacterWorkshopView workshop;
        private bool workshopOpenedFromCombat;
        private bool inGame;
        private bool saveWasBlocked;
        private string currentTab = "MINA";
        private float refreshTimer;
        private int displayedGold = -1;
        private int displayedCrystals = -1;
        private string lastCharacterReward;
        private readonly Dictionary<string, Image> tabButtonImages = new Dictionary<string, Image>();
        private ProgressionDefinition progression;
        private AbilityDefinition[] abilityCatalog;
        private readonly Dictionary<string, Sprite> uiSprites = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Sprite> uiAssetSprites = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Sprite> navIconSprites = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, TextMeshProUGUI> upgradeLevelLabels = new Dictionary<string, TextMeshProUGUI>();

        public void Initialize(Transform canvasRoot)
        {
            if (root != null) return;
            root = canvasRoot;
            startPanel = MakePanel("Start menu", root.parent, Ink, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            startPanel.gameObject.AddComponent<RectMask2D>();
            BuildStartMenu(startPanel);
            showingStartMenu = true;
            StartCoroutine(PrepareGame());
        }

        private IEnumerator PrepareGame()
        {
            // Primeiro frame pertence à arte e ao loading, antes dos trabalhos síncronos.
            yield return null;
            for (int i = 0; i < StartupContent.ResourcePaths.Count; i++)
            {
                ResourceRequest request = null;
                if (!TryPrepareStep(() => request = Resources.LoadAsync<UnityEngine.Object>(StartupContent.ResourcePaths[i])))
                    yield break;
                while (!request.isDone)
                {
                    SetLoadingProgress(0.75f * (i + request.progress) / StartupContent.ResourcePaths.Count);
                    yield return null;
                }
                if (request.asset == null)
                {
                    FailStartup(new InvalidOperationException("Recurso de abertura ausente: " + StartupContent.ResourcePaths[i]));
                    yield break;
                }
                SetLoadingProgress(0.75f * (i + 1f) / StartupContent.ResourcePaths.Count);
            }

            if (!TryPrepareStep(() =>
            {
                var driverObject = new GameObject("Combat Driver");
                driverObject.transform.SetParent(transform.parent, false);
                driver = driverObject.AddComponent<AutoBattleDriver>();
                driver.SetMenuPaused(true);
                if (!driver.IsReady) throw new InvalidOperationException("Driver ou perfil não está pronto. " + driver.LastSaveError);
                progression = Resources.Load<ProgressionDefinition>("Data/Progression");
                abilityCatalog = Resources.LoadAll<AbilityDefinition>("Data/Abilities");
                driver.CombatEventRaised += OnCombatEvent;
            })) yield break;
            SetLoadingProgress(0.83f);
            yield return null;

            if (!TryPrepareStep(BuildInterface)) yield break;
            SetLoadingProgress(0.94f);
            yield return null;

            if (!TryPrepareStep(() =>
            {
                feedback = gameObject.AddComponent<PooledCombatFeedback>();
                feedback.Initialize(worldPanel);
            })) yield break;
            SetLoadingProgress(1f);
            yield return null;

            startupComplete = true;
            loadingRoot.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            ShowStartMenu();
        }

        private bool TryPrepareStep(Action operation)
        {
            try { operation(); return true; }
            catch (Exception exception) { FailStartup(exception); return false; }
        }

        private void FailStartup(Exception exception)
        {
            startupFailure = "Não foi possível carregar. Progresso preservado. Reabra o jogo.";
            loadingLabel.text = "CARREGAMENTO INTERROMPIDO";
            startStatusLabel.text = startupFailure;
            startButton.interactable = false;
            Debug.LogError("Falha na preparação inicial: " + exception, this);
        }

        private void SetLoadingProgress(float value)
        {
            // Progresso monotônico das tarefas reais; sem duração artificial ou porcentagem de bytes.
            loadingProgress = Mathf.Max(loadingProgress, Mathf.Clamp01(value));
            loadingFill.anchorMax = new Vector2(loadingProgress, 1f);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (driver != null) driver.CombatEventRaised -= OnCombatEvent;
            StopCaveMotion();
            foreach (Sprite sprite in mapBackgroundSprites.Values) Destroy(sprite);
            if (startPanel != null) Destroy(startPanel.gameObject);
            if (startFont != null)
            {
                foreach (Texture2D atlas in startFont.atlasTextures) if (atlas != null) Destroy(atlas);
                if (startFont.material != null) Destroy(startFont.material);
                Destroy(startFont);
            }
            foreach (Sprite sprite in uiAssetSprites.Values)
                if (sprite != null) Destroy(sprite);
            foreach (Sprite sprite in navIconSprites.Values)
                if (sprite != null) Destroy(sprite);
        }

        private void BuildInterface()
        {
            BuildUiAssetAtlases();
            var background = MakePanel("Background", root, Ink, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            progressionPanel = MakePanel("Progression screen", background, new Color(0.12f, 0.19f, 0.24f), new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            BuildProgressionInterface(progressionPanel);

            combatRoot = MakePanel("Combat screen", background, Color.clear, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            combatRoot.GetComponent<Image>().raycastTarget = false;
            MakePanel("Combat shade", combatRoot, new Color(0.015f, 0.045f, 0.065f, 0.48f), new Vector2(0, 0.84f), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            RectTransform avatar = MakeArtPanel("Pilot avatar", combatRoot, Steel, "plate", 0.025f, 0.858f, 0.205f, 0.982f);
            MakeIconSprite(avatar, FindResourceSprite("Art/Drill/drill_pioneira", "idle_00"),
                0.08f, 0.08f, 0.92f, 0.92f, Color.white);
            MakeArtPanel("Gold wallet", combatRoot, Color.white, "wallet_panel", 0.225f, 0.89f, 0.505f, 0.978f);
            walletLabel = MakeLabel("Wallet", combatRoot, "0", 29, White, new Vector2(0.295f, 0.9f), new Vector2(0.49f, 0.966f), TextAlignmentOptions.Center);
            MakeArtPanel("Crystal wallet", combatRoot, Color.white, "crystal_wallet", 0.52f, 0.89f, 0.77f, 0.978f);
            crystalLabel = MakeLabel("Crystals", combatRoot, "0", 29, White, new Vector2(0.59f, 0.9f), new Vector2(0.755f, 0.966f), TextAlignmentOptions.Center);
            MakeArtPanel("Options", combatRoot, Color.white, "menu_button", 0.79f, 0.89f, 0.975f, 0.978f);

            RectTransform stagePlate = MakeArtPanel("Stage plate", combatRoot, Color.white, "stage_plate", 0.20f, 0.805f, 0.80f, 0.885f);
            stageLabel = MakeLabel("Stage", stagePlate, "PRADO CRISTALINO  •  1-1", 28, Cream,
                new Vector2(0.04f, 0.34f), new Vector2(0.96f, 0.92f), TextAlignmentOptions.Center);
            MakeLabel("Stage progress", stagePlate, "●   ○   ○   ○   ◆", 22, Cyan,
                new Vector2(0.15f, 0.02f), new Vector2(0.85f, 0.36f), TextAlignmentOptions.Center);
            MakeLabel("Title", combatRoot, "DIG & WONDER", 20, Cream, new Vector2(0.025f, 0.82f), new Vector2(0.19f, 0.86f), TextAlignmentOptions.Center);

            var missionPanel = MakeArtPanel("Mission banner", combatRoot, Color.white, "mission_banner", 0.025f, 0.397f, 0.975f, 0.458f);
            missionLabel = MakeLabel("Mission", missionPanel, "MISSÃO 01   DERROTE 10 INIMIGOS", 24, Cream,
                new Vector2(0.13f, 0.16f), new Vector2(0.76f, 0.86f), TextAlignmentOptions.Left);
            MakeLabel("Mission reward", missionPanel, "CRISTAIS  20", 21, Cyan,
                new Vector2(0.76f, 0.16f), new Vector2(0.97f, 0.86f), TextAlignmentOptions.Right);

            worldPanel = MakePanel("Cave view", combatRoot, Color.clear, new Vector2(0, 0.455f), new Vector2(1, 0.805f), Vector2.zero, Vector2.zero);
            worldPanel.GetComponent<Image>().raycastTarget = false;
            worldPanel.gameObject.AddComponent<RectMask2D>();
            RectTransform backdrop = Region("Stage backdrop", worldPanel, 0, 0, 1, 1);
            stageBackdrop = backdrop.gameObject.AddComponent<Image>();
            stageBackdrop.raycastTarget = false;
            stageBackdrop.preserveAspect = true;
            backdropAspect = backdrop.gameObject.AddComponent<AspectRatioFitter>();
            backdropAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            MakePanel("Ground shade", worldPanel, new Color(0.02f, 0.06f, 0.08f, 0.28f), new Vector2(0, 0), new Vector2(1, 0.34f), Vector2.zero, Vector2.zero);
            MakePanel("Combat ground track", worldPanel, new Color(0.03f, 0.10f, 0.12f, 0.45f),
                new Vector2(0, 0.345f), new Vector2(1, 0.36f), Vector2.zero, Vector2.zero);
            MakePanel("Combat ground glow", worldPanel, new Color(0.00f, 0.86f, 0.74f, 0.18f),
                new Vector2(0, 0.36f), new Vector2(1, 0.367f), Vector2.zero, Vector2.zero);
            MakeIcon(worldPanel, "chassis", 0.06f, 0.42f, 0.15f, 0.59f, new Color(0.65f, 0.41f, 0.23f, 0.55f));
            MakeIcon(worldPanel, "gear", 0.83f, 0.46f, 0.93f, 0.62f, new Color(0.65f, 0.41f, 0.23f, 0.45f));
            MakeLabel("Depth label", worldPanel, "CAMADA 01", 18, Cream,
                new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.98f), TextAlignmentOptions.Center);

            caveSeams = new RectTransform[6];
            caveMotionTweens = new Tween[caveSeams.Length];
            for (int i = 0; i < caveSeams.Length; i++)
            {
                float x = 0.02f + i * 0.18f;
                float y = 0.38f + (i % 3) * 0.12f;
                caveSeams[i] = MakePanel("World wind " + i, worldPanel, new Color(0.62f, 0.96f, 0.88f, 0.18f),
                    new Vector2(x, y), new Vector2(x + 0.075f, y + 0.008f), Vector2.zero, Vector2.zero);
                caveSeams[i].GetComponent<Image>().raycastTarget = false;
            }

            drillIcon = MakeLabel("Drill", worldPanel, "", 96, Sand,
                new Vector2(0.055f, 0.34f), new Vector2(0.29f, 0.57f), TextAlignmentOptions.Center);
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

            enemyVisualRoots = new RectTransform[4];
            enemyImages = new Image[4];
            enemySprites = new EnemySpritePresenter[4];
            enemyAnimators = new ProceduralAnimator2D[4];
            enemyHealth = new TextMeshProUGUI[4];
            enemyHealthFills = new Image[4];
            enemyIds = new int[4];
            enemyMaximumHealth = new float[4];
            for (int i = 0; i < enemyVisualRoots.Length; i++)
            {
                float left = 0.565f + i * 0.108f;
                var visualRootObject = new GameObject("Enemy " + i, typeof(RectTransform));
                visualRootObject.transform.SetParent(worldPanel, false);
                var visualRoot = (RectTransform)visualRootObject.transform;
                visualRoot.anchorMin = new Vector2(left, 0.33f);
                visualRoot.anchorMax = new Vector2(left + 0.095f, 0.54f);
                visualRoot.offsetMin = Vector2.zero;
                visualRoot.offsetMax = Vector2.zero;
                var visualObject = new GameObject("Sprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                visualObject.transform.SetParent(visualRoot, false);
                var visualRect = (RectTransform)visualObject.transform;
                visualRect.anchorMin = Vector2.zero;
                visualRect.anchorMax = Vector2.one;
                visualRect.offsetMin = Vector2.zero;
                visualRect.offsetMax = Vector2.zero;
                enemyVisualRoots[i] = visualRoot;
                enemyImages[i] = visualObject.GetComponent<Image>();
                enemySprites[i] = visualRootObject.AddComponent<EnemySpritePresenter>();
                enemyAnimators[i] = visualRootObject.AddComponent<ProceduralAnimator2D>();
                enemyHealth[i] = MakeLabel("Enemy HP " + i, worldPanel, "", 18, Cream,
                    new Vector2(left - 0.01f, 0.555f), new Vector2(left + 0.105f, 0.585f), TextAlignmentOptions.Center);
                MakePanel("Enemy HP track " + i, worldPanel, new Color(0.03f, 0.06f, 0.08f, 0.92f),
                    new Vector2(left - 0.005f, 0.535f), new Vector2(left + 0.10f, 0.55f), Vector2.zero, Vector2.zero);
                enemyHealthFills[i] = MakePanel("Enemy HP fill " + i, worldPanel, Coral,
                    new Vector2(left - 0.005f, 0.535f), new Vector2(left + 0.10f, 0.55f), Vector2.zero, Vector2.zero).GetComponent<Image>();
                visualRoot.gameObject.SetActive(false);
                enemyHealth[i].gameObject.SetActive(false);
                enemyHealthFills[i].gameObject.SetActive(false);
            }
            healthLabel = MakeLabel("Health", combatRoot, "BROCA 120 / 120", 18, Cream,
                new Vector2(0.04f, 0.462f), new Vector2(0.38f, 0.493f), TextAlignmentOptions.Left);
            MakePanel("Health track", combatRoot, Ink, new Vector2(0.04f, 0.448f), new Vector2(0.38f, 0.46f), Vector2.zero, Vector2.zero);
            healthFill = MakePanel("Health fill", combatRoot, Mint, new Vector2(0.04f, 0.448f), new Vector2(0.38f, 0.46f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            statusLabel = MakeLabel("Status", combatRoot, "", 18, Mint,
                new Vector2(0.42f, 0.462f), new Vector2(0.96f, 0.493f), TextAlignmentOptions.Right);
            combatLog = MakeLabel("Combat log", combatRoot, "", 18, Cream,
                new Vector2(0.04f, 0.385f), new Vector2(0.96f, 0.402f), TextAlignmentOptions.Center);
            BuildPowerBar(combatRoot);
            BuildNavIconAtlas();

            // Keep the upgrade plate below the power bar; the previous bounds hid the five power slots.
            menuPanel = MakeArtPanel("Upgrade panel", combatRoot, new Color(0.12f, 0.22f, 0.30f), "plate", 0.025f, 0.105f, 0.975f, 0.315f);
            menuTitle = MakeLabel("Tab title", menuPanel, "MELHORIAS DA BROCA", 24, Cream, new Vector2(0.035f, 0.90f), new Vector2(0.965f, 0.99f), TextAlignmentOptions.Center);
            menuBody = MakeLabel("Tab info", menuPanel, "", 18, Cream, new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.82f), TextAlignmentOptions.Center);
            menuBody.gameObject.SetActive(false);
            upgradeRowsRoot = BuildUpgradeScroll(menuPanel);
            attackUpgradeButton = BuildUpgradeRow(upgradeRowsRoot, "ATQ", "attack", "6 de ATQ", 0,
                () => { if (!driver.TryPurchaseUpgrade(UpgradeKind.Attack)) statusLabel.text = "OURO INSUFICIENTE OU NÍVEL MÁXIMO"; Refresh(); }, out attackUpgradeUiButton, out attackUpgradeButtonLabel);
            healthUpgradeButton = BuildUpgradeRow(upgradeRowsRoot, "VIDA", "health", "120 de vida", 1,
                () => { if (!driver.TryPurchaseUpgrade(UpgradeKind.Health)) statusLabel.text = "OURO INSUFICIENTE OU NÍVEL MÁXIMO"; Refresh(); }, out healthUpgradeUiButton, out healthUpgradeButtonLabel);
            BuildLockedUpgradeRow(upgradeRowsRoot, "AUTORREPARO", "repair", "Recupera vida", "LIBERA NO NV. 10");
            BuildLockedUpgradeRow(upgradeRowsRoot, "CADÊNCIA", "bolt", "Acelera os ataques", "LIBERA NO NV. 20");
            BuildLockedUpgradeRow(upgradeRowsRoot, "CRÍTICO", "attack", "Chance de dano crítico", "LIBERA NO NV. 35");
            BuildLockedUpgradeRow(upgradeRowsRoot, "BROCA DUPLA", "drill", "Dispara dois projéteis", "LIBERA NO NV. 50");
            restartButton = MakeButton("Restart run", menuPanel, "RECOMEÇAR", 20, 0.25f, 0.25f, 0.75f, 0.54f,
                () => { driver.StartNewGame(); EnterGame(false); }, Coral).GetComponent<RectTransform>();
            restartButton.gameObject.SetActive(false);
            continueButton = MakeButton("Continue stage", menuPanel, "REPETIR ÚLTIMA FASE", 20, 0.2f, 0.25f, 0.8f, 0.54f,
                () => { driver.StartNewGame(); EnterGame(false); }, Mint).GetComponent<RectTransform>();
            continueButton.gameObject.SetActive(false);

            bottomBar = MakeArtPanel("Bottom navigation", combatRoot, Color.white, "bottom_nav_structure", 0.01f, 0.015f, 0.99f, 0.105f);
            string[] tabs = { "MINA", "MODOS", "OFICINA", "PETS", "BAÚS", "LOJA" };
            string[] icons = { "nav_mine", "nav_modes", "nav_workshop", "nav_pets", "nav_chests", "nav_shop" };
            for (int i = 0; i < tabs.Length; i++)
            {
                string tab = tabs[i];
                float left = i / 6f + 0.004f;
                RectTransform tabButton = MakeNavButton("Tab " + tab, bottomBar, tab, icons[i], left, 0.05f, left + 0.159f, 0.97f,
                    () => { if (tab == "OFICINA") OpenProgression(); else if (tab == "BAÚS") OpenTreasury(); else SelectTab(tab); }, tab == currentTab ? Cyan : Steel);
                tabButtonImages.Add(tab, tabButton.GetComponent<Image>());
            }

            menuPanel.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
            worldPanel.gameObject.SetActive(false);
            stageLabel.gameObject.SetActive(false);
            walletLabel.gameObject.SetActive(false);
            crystalLabel.gameObject.SetActive(false);
            missionLabel.gameObject.SetActive(false);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
        }

        private void BuildStartMenu(RectTransform panel)
        {
            // A ilustração alcança as bordas; logo e controles respeitam a área segura.
            RectTransform artwork = Region("Mine illustration", panel, 0, 0, 1, 1);
            var image = artwork.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = Resources.Load<Sprite>("Art/StartMenu/mine-start-v2");
            image.raycastTarget = false;
            if (image.sprite != null)
            {
                var fit = artwork.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
            }
            RectTransform controls = Region("Start safe area", panel, 0, 0, 1, 1);
            controls.gameObject.AddComponent<SafeAreaFitter>();
            MakeIconSprite(controls, Resources.Load<Sprite>("Art/StartMenu/dig-and-wonder-logo-v3"),
                0.06f, 0.735f, 0.94f, 0.96f, Color.white);

            Font source = Resources.Load<Font>("Fonts/Workshop/TitanOne-Regular");
            if (source != null)
            {
                startFont = TMP_FontAsset.CreateFontAsset(source, 64, 8,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
                if (startFont != null) startFont.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ÁÀÂÃÉÊÍÓÔÕÚÜÇáàâãéêíóôõúüç.,:/!?—•-()");
            }
            RectTransform buttonRoot = MakePanel("Start mining", controls, Color.clear,
                new Vector2(0.09f, 0.14f), new Vector2(0.91f, 0.223f), Vector2.zero, Vector2.zero);
            var hitArea = buttonRoot.GetComponent<UnityEngine.UI.Image>();
            hitArea.raycastTarget = true;
            startButton = buttonRoot.gameObject.AddComponent<UnityEngine.UI.Button>();
            startButton.targetGraphic = hitArea;
            startButton.onClick.AddListener(BeginMining);
            TextMeshProUGUI title = MakeLabel("Start label", buttonRoot, "INICIAR MINERAÇÃO", 42, Color.white,
                new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.9f), TextAlignmentOptions.Center);
            if (startFont != null) title.font = startFont;
            startButton.gameObject.AddComponent<WorkshopButtonFeedback>().Initialize(title.rectTransform, startButton);
            startButton.gameObject.SetActive(false);

            loadingRoot = MakePanel("Loading", controls, Color.clear,
                new Vector2(0.09f, 0.14f), new Vector2(0.91f, 0.223f), Vector2.zero, Vector2.zero);
            loadingRoot.GetComponent<Image>().raycastTarget = false;
            loadingLabel = MakeLabel("Loading label", loadingRoot, "CARREGANDO...", 30, Color.white,
                new Vector2(0, 0.55f), Vector2.one, TextAlignmentOptions.Center);
            if (startFont != null) loadingLabel.font = startFont;
            RectTransform track = MakePanel("White loading track", loadingRoot, new Color(1, 1, 1, 0.2f),
                new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.3f), Vector2.zero, Vector2.zero);
            track.GetComponent<Image>().raycastTarget = false;
            loadingFill = MakePanel("White loading fill", track, Color.white,
                Vector2.zero, new Vector2(0, 1), Vector2.zero, Vector2.zero);
            loadingFill.GetComponent<Image>().raycastTarget = false;
            startStatusLabel = MakeLabel("Start status", controls, "", 26, Cream,
                new Vector2(0.09f, 0.084f), new Vector2(0.91f, 0.133f), TextAlignmentOptions.Center);
            if (startFont != null) startStatusLabel.font = startFont;
            TextMeshProUGUI version = MakeLabel("Version", controls, "v" + Application.version, 22, Dim,
                new Vector2(0.15f, 0.022f), new Vector2(0.85f, 0.055f), TextAlignmentOptions.Center);
            if (startFont != null) version.font = startFont;
        }

        private void BuildProgressionInterface(RectTransform panel)
        {
            workshop = panel.gameObject.AddComponent<CharacterWorkshopView>();
            workshop.Initialize(panel, driver, CloseProgression);
        }

        private void BeginMining()
        {
            if (!startupComplete || driver == null) return;
            if (!driver.IsReady) { RefreshStartMenu(); return; }
            // Só liberar o relógio do combate depois da ação explícita do jogador.
            if (driver.Simulation != null && driver.Simulation.IsActive) EnterGame(true);
            else if (driver.TryStartNewGame()) EnterGame(false);
            else RefreshStartMenu();
        }

        private void OpenProgression()
        {
            workshopOpenedFromCombat = inGame;
            driver.SetMenuPaused(true);
            inGame = false;
            StopCaveMotion();
            startPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
            // The workshop lives inside an initially hidden root. Activate the
            // screen explicitly before CharacterWorkshopView reveals its own UI.
            progressionPanel.gameObject.SetActive(true);
            workshop.Show();
        }

        private void OpenTreasury()
        {
            OpenProgression();
            workshop.Show(2);
        }

        private void CloseProgression()
        {
            driver.SetMenuPaused(false);
            if (workshopOpenedFromCombat) EnterGame(true);
            else ShowStartMenu();
        }

        private void ShowStartMenu()
        {
            inGame = false;
            showingStartMenu = true;
            driver.SetMenuPaused(true);
            StopCaveMotion();
            startPanel.gameObject.SetActive(true);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
            RefreshStartMenu();
        }

        private void RefreshStartMenu()
        {
            if (startupFailure != null) { startStatusLabel.text = startupFailure; return; }
            if (!startupComplete || driver == null) return;
            startButton.interactable = driver.IsReady;
            if (driver.IsSaveBlocked) startStatusLabel.text = driver.LastSaveError;
            else if (!driver.IsReady) startStatusLabel.text = "Não foi possível carregar. Seu progresso foi preservado.";
            else startStatusLabel.text = driver.Profile.ActiveRun != null
                ? "Seu progresso está salvo. Continue de onde parou."
                : "Explore, evolua e descubra as profundezas.";
        }

        private void RefreshStageBackdrop()
        {
            string id = driver.Simulation.CurrentStageId;
            if (renderedStageId == id) return;
            renderedStageId = id;
            string resource = id == "prado_cristalino" ? "fundo-prado-cristalino-v1" :
                id == "jardim_fossil" ? "fundo-jardim-fossil-v1" : "fundo-gruta-basaltica-v1";
            if (!mapBackgroundSprites.TryGetValue(resource, out Sprite sprite))
            {
                Texture2D texture = Resources.Load<Texture2D>("Art/Backgrounds/" + resource);
                if (texture == null) return;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                mapBackgroundSprites.Add(resource, sprite);
            }
            stageBackdrop.sprite = sprite;
            stageBackdrop.color = id == "fenda_aurora" ? new Color(0.78f, 0.65f, 1f) : Color.white;
            backdropAspect.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        private void EnterGame(bool resumed)
        {
            if (driver.Simulation == null) return;
            if (driver.Simulation.IsActive) lastCharacterReward = null;
            ApplyCharacterColors();
            driver.SetMenuPaused(false);
            inGame = true;
            showingStartMenu = false;
            nextStageTimer = 0f;
            RefreshStageBackdrop();
            startPanel.gameObject.SetActive(false);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(true);
            worldPanel.gameObject.SetActive(true);
            menuPanel.gameObject.SetActive(true);
            bottomBar.gameObject.SetActive(true);
            if (upgradeScroll != null) upgradeScroll.verticalNormalizedPosition = 1f;
            stageLabel.gameObject.SetActive(true);
            walletLabel.gameObject.SetActive(true);
            crystalLabel.gameObject.SetActive(true);
            missionLabel.gameObject.SetActive(true);
            statusLabel.gameObject.SetActive(true);
            combatLog.gameObject.SetActive(true);
            if (resumed) statusLabel.text = "PROGRESSO RECUPERADO — CONTINUANDO DA ÚLTIMA POSIÇÃO";
            else statusLabel.text = "A broca começou a avançar!";
            RefreshAutoToggle();
            drillAnimator.ResetForRun();
            drillFrames.ResetForRun();
            drillMotion.ResetForRun();
            if (driver.Simulation.IsActive && !driver.IsSaveBlocked)
                StartCaveMotion();
            else
            {
                StopCaveMotion();
                if (driver.Simulation.Run.IsPlayerDefeated)
                {
                    drillAnimator.PlayDeath();
                    drillMotion.PlayDeath();
                    drillFrames.PlayDeath();
                }
            }
            Refresh();
        }

        private void Update()
        {
            if (driver == null) return;
            if (!inGame)
            {
                if (showingStartMenu) RefreshStartMenu();
                return;
            }
            if (driver.Simulation == null) return;
            // Avanço fora do evento de vitória: o driver já terminou o passo e salvou as recompensas.
            if (driver.CanContinueCampaign && !driver.IsSaveBlocked)
            {
                nextStageTimer += Time.unscaledDeltaTime;
                if (nextStageTimer >= StageVictorySeconds && driver.TryContinueCampaign())
                {
                    string reward = lastCharacterReward;
                    EnterGame(false);
                    statusLabel.text = string.IsNullOrEmpty(reward) ? "Nova fase — a mineração continua!" : reward + " LIBERADA! CONFIRA A OFICINA.";
                }
            }
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
            stageLabel.text = (sim.CurrentStageDisplayName == null ? "NOVA EXPEDIÇÃO" : sim.CurrentStageDisplayName.ToUpperInvariant()) + "  •  " + sim.Run.CampaignStage + "-1";
            int stageNumber = sim.Run.CampaignStage;
            missionLabel.text = "FASE " + stageNumber.ToString("00") + "  •  AMEAÇA " + (1 + driver.Economy.DifficultyTier(stageNumber)) + "  •  " + (sim.Run.IsStageCleared ? "CONCLUÍDA" : "DERROTE O GUARDIÃO");
            if (displayedGold != sim.Gold || displayedCrystals != sim.Profile.Crystals)
            {
                displayedGold = sim.Gold;
                displayedCrystals = sim.Profile.Crystals;
                walletLabel.text = sim.Gold.ToString("N0");
                crystalLabel.text = sim.Profile.Crystals.ToString("N0");
                walletLabel.transform.DOKill(false);
                walletLabel.transform.localScale = Vector3.one;
                walletLabel.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0), 0.22f, 5, 0.7f);
            }
            healthLabel.text = "BROCA NV. " + driver.CurrentCharacterLevel.Level + "  •  " + Mathf.CeilToInt(sim.PlayerHealth) + " / " + Mathf.CeilToInt(sim.PlayerMaximumHealth);
            float healthRatio = sim.PlayerMaximumHealth <= 0 ? 0 : sim.PlayerHealth / sim.PlayerMaximumHealth;
            healthFill.rectTransform.anchorMax = new Vector2(0.04f + 0.34f * healthRatio, 0.46f);
            for (int i = 0; i < enemyVisualRoots.Length; i++)
            {
                bool active = i < sim.Enemies.Count;
                enemyVisualRoots[i].gameObject.SetActive(active);
                enemyHealth[i].gameObject.SetActive(active);
                enemyHealthFills[i].gameObject.SetActive(active);
                if (active)
                {
                    EnemySnapshot enemy = sim.Enemies[i];
                    if (enemyIds[i] != enemy.Id)
                    {
                        enemyIds[i] = enemy.Id;
                        enemyMaximumHealth[i] = Mathf.Max(1f, enemy.Health);
                    }
                    enemyMaximumHealth[i] = Mathf.Max(enemyMaximumHealth[i], enemy.Health);
                    // Saves antigos não têm VisualId; nesse caso o presenter usa o visual de fallback.
                    enemySprites[i].Configure(string.IsNullOrEmpty(enemy.VisualId) ? "torrao" : enemy.VisualId, enemy.IsBoss);
                    enemyHealth[i].text = Mathf.CeilToInt(enemy.Health) + " PV";
                    float ratio = Mathf.Clamp01(enemy.Health / enemyMaximumHealth[i]);
                    enemyHealthFills[i].rectTransform.anchorMax = new Vector2(
                        enemyHealthFills[i].rectTransform.anchorMin.x + 0.105f * ratio,
                        enemyHealthFills[i].rectTransform.anchorMax.y);
                }
                else
                {
                    enemyIds[i] = 0;
                    enemyMaximumHealth[i] = 0f;
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
                statusLabel.text = driver.CanContinueCampaign
                    ? (string.IsNullOrEmpty(lastCharacterReward) ? "Guardião derrotado!" : lastCharacterReward + " LIBERADA!") + " Próximo mapa..."
                    : "CAMPANHA CONCLUÍDA! Abra a Oficina ou repita a última fase.";
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
                    menuTitle.text = "MELHORIAS DA BROCA";
                    menuBody.text = sim.Run.IsPlayerDefeated ? "A escavação terminou. Toque em recomeçar para tentar novamente." : "A broca avança e luta automaticamente.";
                    break;
                case "MELHORIAS":
                    menuTitle.text = "MELHORIAS DA BROCA";
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
                case "MODOS":
                    menuTitle.text = "MODOS DE EXPEDIÇÃO";
                    menuBody.text = "MINA PRINCIPAL\nA broca avança e enfrenta ondas de inimigos.\n\nTORRE INVERTIDA\nDesça por setores cada vez mais perigosos.\nRecompensas especiais — EM PREPARAÇÃO.";
                    break;
                case "PETS":
                    menuTitle.text = "PETS DE APOIO";
                    menuBody.text = "Nenhum pet equipado.\n\nEncontre companheiros mecânicos em baús e marcos de exploração.\nCada pet poderá trazer um bônus passivo para a broca.";
                    break;
                case "LOJA":
                    menuTitle.text = "LOJA DE SUPRIMENTOS";
                    menuBody.text = "Suprimentos da oficina\n\nOuro e cristais serão usados para reforços, visuais e itens especiais.\nCatálogo em preparação.";
                    break;
                default:
                    menuTitle.text = "PERFIL DA EXPEDIÇÃO";
                    menuBody.text = "FASE DESBLOQUEADA  " + sim.Profile.UnlockedStageId.Replace('_', ' ') + "\nATQ  Nv." + sim.Profile.AttackLevel + "   •   VIDA  Nv." + sim.Profile.HealthLevel + "\n\nPROGRESSO SALVO AUTOMATICAMENTE\nNovos módulos aparecem conforme a expedição avança.";
                    break;
            }
            bool defeated = sim.Run.IsPlayerDefeated;
            bool cleared = sim.Run.IsStageCleared;
            bool upgradesVisible = (currentTab == "MINA" || currentTab == "MELHORIAS") && !defeated && !cleared && !driver.IsSaveBlocked;
            if (upgradeRowsRoot != null) upgradeRowsRoot.gameObject.SetActive(upgradesVisible);
            if (menuBody != null) menuBody.gameObject.SetActive(!upgradesVisible);
            if (upgradeLevelLabels.TryGetValue("ATQ", out TextMeshProUGUI attackLevelLabel))
                attackLevelLabel.text = "NV. " + sim.Profile.AttackLevel;
            if (upgradeLevelLabels.TryGetValue("VIDA", out TextMeshProUGUI healthLevelLabel))
                healthLevelLabel.text = "NV. " + sim.Profile.HealthLevel;
            attackUpgradeButton.gameObject.SetActive(upgradesVisible);
            healthUpgradeButton.gameObject.SetActive(upgradesVisible);
            if (upgradesVisible)
            {
                bool attackMaxed = attackCost < 0;
                bool healthMaxed = healthCost < 0;
                attackUpgradeButtonLabel.text = attackMaxed ? "ATQ  MÁXIMO" : "MELHORAR\nOURO " + attackCost;
                healthUpgradeButtonLabel.text = healthMaxed ? "VIDA  MÁXIMA" : "MELHORAR\nOURO " + healthCost;
                attackUpgradeUiButton.interactable = !attackMaxed;
                healthUpgradeUiButton.interactable = !healthMaxed;
                attackUpgradeButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                healthUpgradeButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            }
            restartButton.gameObject.SetActive(defeated && !driver.IsSaveBlocked);
            continueButton.gameObject.SetActive(cleared && !driver.CanContinueCampaign && !driver.IsSaveBlocked);
            if (cleared && driver.CanContinueCampaign)
                menuBody.text = driver.IsSaveBlocked ? "Aguardando salvar para continuar. Seu progresso foi preservado."
                    : "Baú, chave e progresso salvos. Avançando para o próximo mapa...";
            foreach (KeyValuePair<string, Image> button in tabButtonImages)
                button.Value.color = new Color(1f, 1f, 1f, 0f);
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
            if (currentLevel < requiredLevel) return "BLOQUEADO  " + displayName.ToUpperInvariant() + "  •  requer " + source + " Nv." + requiredLevel;
            return "BLOQUEADO  " + displayName.ToUpperInvariant() + "  •  EM BALANCEAMENTO";
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
                summary.Append("BLOQUEADO  FASE ").Append(phase).Append("  ").Append(lockedByPhase[phase]);
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
            string equipmentState = currentStage < 2 ? "BLOQUEADO  FASE 2" : "DESBLOQUEADO  •  EM BREVE";
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
                case CombatEventType.SpecialStarted: combatLog.text = "ESPECIAL: " + driver.EquippedCharacter.DisplayName.ToUpperInvariant(); break;
                case CombatEventType.SpecialStrike: combatLog.text = "IMPACTO ESPECIAL  •  " + Mathf.CeilToInt(e.Amount); break;
                case CombatEventType.CharacterUnlocked:
                    CharacterCombatData reward = Array.Find(driver.CharacterCatalog, item => item.Id == e.Text);
                    lastCharacterReward = reward == null ? e.Text : reward.DisplayName.ToUpperInvariant();
                    break;
                case CombatEventType.GoldAwarded: combatLog.text = "+" + Mathf.CeilToInt(e.Amount) + " ouro"; break;
                case CombatEventType.CharacterExperienceAwarded: combatLog.text = "+" + Mathf.CeilToInt(e.Amount) + " XP para sua broca"; break;
                case CombatEventType.CharacterLeveledUp: statusLabel.text = "SUA BROCA SUBIU PARA O NÍVEL " + Mathf.CeilToInt(e.Amount) + "!"; break;
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

        private void ApplyCharacterColors()
        {
            CharacterCombatData character = driver.EquippedCharacter;
            if (character == null) return;
            var tint = new Color(character.TintR, character.TintG, character.TintB);
            if (drillFrames != null) drillFrames.GetComponent<Image>().color = tint;
        }

        private void BuildPowerBar(RectTransform parent)
        {
            MakeArtPanel("Power lane", parent, new Color(0.025f, 0.07f, 0.10f, 0.96f), "plate", 0.018f, 0.316f, 0.982f, 0.403f);
            MakeArtPanel("Power slots structure", parent, Color.white, "power_lane_structure", 0.19f, 0.316f, 0.75f, 0.403f);
            RectTransform auto = MakeArtPanel("Auto attack", parent, Color.white, "auto_control", 0.025f, 0.327f, 0.185f, 0.392f);
            autoToggleButton = auto.gameObject.AddComponent<Button>();
            autoToggleButton.targetGraphic = auto.GetComponent<Image>();
            autoToggleButton.onClick.AddListener(ToggleAutoAttack);
            MakeLabel("Auto label", auto, "AUTO", 26, Cream, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.96f), TextAlignmentOptions.Center);
            autoStateLabel = MakeLabel("Auto state", auto, "ON", 19, Ink, new Vector2(0.16f, 0.08f), new Vector2(0.84f, 0.50f), TextAlignmentOptions.Center);

            string[] powers = { "drill", "bolt", "shield", "repair", "core" };
            for (int i = 0; i < powers.Length; i++)
            {
                float left = 0.20f + i * 0.105f;
                RectTransform slot = MakeArtPanel("Power slot " + i, parent, Color.white, i == 0 ? "power_active" : "power_locked",
                    left, 0.327f, left + 0.09f, 0.392f);
                if (i == 0) MakeIcon(slot, powers[i], 0.16f, 0.16f, 0.84f, 0.84f, Cyan);
            }
            RectTransform force = MakeArtPanel("Force badge", parent, Color.white, "force_badge", 0.74f, 0.327f, 0.975f, 0.392f);
            MakeIcon(force, "drill", 0.05f, 0.12f, 0.36f, 0.88f, Gold);
            MakeLabel("Force title", force, "FORÇA", 15, Cream, new Vector2(0.37f, 0.53f), new Vector2(0.96f, 0.94f), TextAlignmentOptions.Center);
            MakeLabel("Force value", force, "128", 25, Gold, new Vector2(0.37f, 0.08f), new Vector2(0.96f, 0.58f), TextAlignmentOptions.Center);
            RefreshAutoToggle();
        }

        private void ToggleAutoAttack()
        {
            bool enabled = driver == null || driver.Simulation == null || driver.Simulation.AutoAttackEnabled;
            driver?.SetAutoAttackEnabled(!enabled);
            RefreshAutoToggle();
        }

        private void RefreshAutoToggle()
        {
            if (autoStateLabel == null || autoToggleButton == null) return;
            bool enabled = driver == null || driver.Simulation == null || driver.Simulation.AutoAttackEnabled;
            autoStateLabel.text = enabled ? "ON" : "OFF";
            autoStateLabel.color = enabled ? Ink : Cream;
            autoToggleButton.targetGraphic.color = enabled ? Mint : new Color(0.26f, 0.30f, 0.34f);
        }

        private RectTransform BuildUpgradeScroll(RectTransform parent)
        {
            RectTransform viewport = Region("Upgrade viewport", parent, 0.025f, 0.035f, 0.975f, 0.895f);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0.08f);
            viewportImage.raycastTarget = false;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            RectTransform content = Region("Upgrade content", viewport, 0, 1, 1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 7f;
            layout.padding = new RectOffset(0, 0, 0, 8);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // The scrollbar is part of the visual language, not a default Unity scrollbar.
            // Keep it outside the masked content so the blue steel rail remains visible while scrolling.
            RectTransform rail = MakeArtPanel("Upgrade scroll rail", parent, Color.white,
                "scroll_rail_structure", 0.955f, 0.04f, 0.985f, 0.895f);
            rail.GetComponent<Image>().raycastTarget = false;

            upgradeScroll = parent.gameObject.AddComponent<ScrollRect>();
            upgradeScroll.viewport = viewport;
            upgradeScroll.content = content;
            upgradeScroll.horizontal = false;
            upgradeScroll.vertical = true;
            upgradeScroll.movementType = ScrollRect.MovementType.Clamped;
            upgradeScroll.scrollSensitivity = 52f;
            upgradeScroll.inertia = true;
            upgradeScroll.verticalNormalizedPosition = 1f;
            return content;
        }

        private RectTransform BuildUpgradeRow(RectTransform content, string title, string icon, string value, int row,
            UnityEngine.Events.UnityAction action, out Button buttonComponent, out TextMeshProUGUI buttonLabel)
        {
            var rowObject = new GameObject("Upgrade row " + title, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rowObject.transform.SetParent(content, false);
            RectTransform rowRect = (RectTransform)rowObject.transform;
            var element = rowObject.GetComponent<LayoutElement>();
            element.preferredHeight = 92f;
            element.minHeight = 92f;
            var rowImage = rowObject.GetComponent<Image>();
            rowImage.sprite = SpriteFor("upgrade_row_structure");
            rowImage.type = Image.Type.Simple;
            rowImage.color = Color.white;
            rowImage.raycastTarget = false;
            RectTransform iconFrame = Region("Upgrade icon", rowRect, 0.012f, 0.08f, 0.17f, 0.92f);
            MakeIcon(iconFrame, icon, 0.13f, 0.13f, 0.87f, 0.87f, Color.white);
            TextMeshProUGUI levelLabel = MakeLabel("Upgrade level", iconFrame, "NV. 0", 13, Cream,
                new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.27f), TextAlignmentOptions.Center);
            levelLabel.fontStyle = FontStyles.Bold;
            levelLabel.outlineWidth = 0.15f;
            levelLabel.outlineColor = Ink;
            upgradeLevelLabels[title] = levelLabel;
            MakeLabel("Upgrade title", rowRect, title, 25, Cream, new Vector2(0.20f, 0.52f), new Vector2(0.60f, 0.94f), TextAlignmentOptions.Left);
            MakeLabel("Upgrade value", rowRect, value, 18, Dim, new Vector2(0.20f, 0.10f), new Vector2(0.60f, 0.55f), TextAlignmentOptions.Left);
            RectTransform button = MakeOverlayButton("Upgrade action", rowRect, "MELHORAR", 20, 0.68f, 0.13f, 0.98f, 0.87f, action);
            buttonComponent = button.GetComponent<Button>();
            buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();
            return button;
        }

        private void BuildLockedUpgradeRow(RectTransform content, string title, string icon, string value, string requirement)
        {
            var rowObject = new GameObject("Locked upgrade row " + title, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rowObject.transform.SetParent(content, false);
            RectTransform rowRect = (RectTransform)rowObject.transform;
            var element = rowObject.GetComponent<LayoutElement>();
            element.preferredHeight = 92f;
            element.minHeight = 92f;
            var rowImage = rowObject.GetComponent<Image>();
            rowImage.sprite = SpriteFor("upgrade_row_structure");
            rowImage.type = Image.Type.Simple;
            rowImage.color = Color.white;
            rowImage.raycastTarget = false;
            RectTransform iconFrame = Region("Locked icon", rowRect, 0.012f, 0.08f, 0.17f, 0.92f);
            MakeIcon(iconFrame, icon, 0.13f, 0.13f, 0.87f, 0.87f, Dim);
            TextMeshProUGUI levelLabel = MakeLabel("Locked level", iconFrame, "NV. 0", 13, Dim,
                new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.27f), TextAlignmentOptions.Center);
            levelLabel.fontStyle = FontStyles.Bold;
            levelLabel.outlineWidth = 0.15f;
            levelLabel.outlineColor = Ink;
            upgradeLevelLabels[title] = levelLabel;
            MakeLabel("Locked title", rowRect, title, 25, Dim, new Vector2(0.20f, 0.52f), new Vector2(0.62f, 0.94f), TextAlignmentOptions.Left);
            MakeLabel("Locked value", rowRect, value, 18, new Color(0.55f, 0.63f, 0.68f), new Vector2(0.20f, 0.10f), new Vector2(0.62f, 0.55f), TextAlignmentOptions.Left);
            RectTransform lockedAction = MakeArtPanel("Locked action", rowRect, new Color(0.08f, 0.13f, 0.18f), "slot", 0.68f, 0.13f, 0.98f, 0.87f);
            MakeIcon(lockedAction, "lock", 0.04f, 0.16f, 0.24f, 0.84f, Cream);
            MakeLabel("Requirement", lockedAction, requirement, 16, Dim, new Vector2(0.23f, 0.12f), new Vector2(0.96f, 0.88f), TextAlignmentOptions.Center);
        }

        private RectTransform MakeArtPanel(string name, Transform parent, Color color, string art, float x0, float y0, float x1, float y1)
        {
            RectTransform rect = MakePanel(name, parent, color, new Vector2(x0, y0), new Vector2(x1, y1), Vector2.zero, Vector2.zero);
            Image image = rect.GetComponent<Image>();
            image.sprite = SpriteFor(art);
            image.type = uiAssetSprites.ContainsKey(art) ? Image.Type.Simple : Image.Type.Sliced;
            image.raycastTarget = false;
            return rect;
        }

        private Image MakeIcon(Transform parent, string art, float x0, float y0, float x1, float y1, Color color)
        {
            RectTransform rect = Region("Icon " + art, parent, x0, y0, x1, y1);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = SpriteFor(art);
            image.preserveAspect = true;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Image MakeIconSprite(Transform parent, Sprite sprite, float x0, float y0, float x1, float y1, Color color)
        {
            RectTransform rect = Region("Icon sprite", parent, x0, y0, x1, y1);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite FindResourceSprite(string resourcePath, string spriteName)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
            for (int i = 0; i < sprites.Length; i++)
                if (sprites[i] != null && sprites[i].name == spriteName) return sprites[i];
            return sprites.Length > 0 ? sprites[0] : null;
        }

        private RectTransform MakeArtButton(string name, Transform parent, string text, string icon, float left, float bottom, float right, float top,
            UnityEngine.Events.UnityAction action, Color color)
        {
            RectTransform rect = MakeArtPanel(name, parent, color, "slot", left, bottom, right, top);
            var button = rect.gameObject.AddComponent<Button>();
            Image buttonImage = rect.GetComponent<Image>();
            buttonImage.raycastTarget = true;
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(action);
            MakeIcon(rect, icon, 0.18f, 0.36f, 0.82f, 0.91f, Color.white);
            TextMeshProUGUI label = MakeLabel("Tab label", rect, text, 16, Cream, new Vector2(0.02f, 0.03f), new Vector2(0.98f, 0.35f), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            return rect;
        }

        private RectTransform MakeNavButton(string name, Transform parent, string text, string icon, float left, float bottom,
            float right, float top, UnityEngine.Events.UnityAction action, Color stateColor)
        {
            RectTransform rect = MakePanel(name, parent, new Color(1f, 1f, 1f, 0f),
                new Vector2(left, bottom), new Vector2(right, top), Vector2.zero, Vector2.zero);
            Button button = rect.gameObject.AddComponent<Button>();
            Image image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            if (stateColor == Cyan)
                MakePanel("Active nav glow", rect, Cyan, new Vector2(0.12f, 0.01f), new Vector2(0.88f, 0.055f), Vector2.zero, Vector2.zero);
            MakeIcon(rect, icon, 0.18f, 0.33f, 0.82f, 0.91f, Color.white);
            TextMeshProUGUI label = MakeLabel("Tab label", rect, text, 16, Cream,
                new Vector2(0.02f, 0.03f), new Vector2(0.98f, 0.34f), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            return rect;
        }

        private Sprite SpriteFor(string name)
        {
            if (name.StartsWith("nav_", StringComparison.Ordinal))
                return NavIconFor(name);
            if (uiAssetSprites.TryGetValue(name, out Sprite atlasSprite))
                return atlasSprite;
            if (!uiSprites.TryGetValue(name, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>("Art/Workshop/" + name);
                uiSprites.Add(name, sprite);
            }
            return sprite;
        }

        private void BuildUiAssetAtlases()
        {
            SliceUiAtlas("Art/UI/dw_hud_top_atlas", new[]
            {
                "wallet_panel", "crystal_wallet", "menu_button", "stage_plate", "mission_banner", "upgrade_button"
            }, new[]
            {
                new RectInt(20, 587, 489, 165),
                new RectInt(512, 575, 509, 185),
                new RectInt(1024, 747, 493, 181),
                new RectInt(12, 239, 620, 173),
                new RectInt(620, 239, 540, 173),
                new RectInt(1190, 205, 305, 210)
            });
            SliceUiAtlas("Art/UI/dw_hud_combat_atlas", new[]
            {
                "mission_banner_combat", "auto_control", "power_active", "power_locked", "force_badge", "upgrade_row"
            }, new[]
            {
                new RectInt(46, 723, 680, 140),
                new RectInt(765, 633, 325, 318),
                new RectInt(1132, 618, 359, 331),
                new RectInt(190, 349, 309, 277),
                new RectInt(620, 339, 579, 215),
                new RectInt(46, 65, 1441, 218)
            });
            SliceUiAtlas("Art/UI/dw_hud_structure_atlas", new[]
            {
                "upgrade_panel_structure", "power_lane_structure", "upgrade_row_structure",
                "mission_panel_structure", "bottom_nav_structure", "scroll_rail_structure"
            }, new[]
            {
                new RectInt(28, 270, 662, 712),
                new RectInt(735, 765, 774, 215),
                new RectInt(718, 577, 792, 165),
                new RectInt(715, 246, 798, 287),
                new RectInt(28, 44, 1484, 164),
                new RectInt(650, 270, 40, 500)
            });
        }

        private void SliceUiAtlas(string resourcePath, string[] names, RectInt[] trimmedRects)
        {
            Texture2D atlas = Resources.Load<Texture2D>(resourcePath);
            if (atlas == null) return;
            int columns = 3;
            int rows = 2;
            int cellWidth = atlas.width / columns;
            int cellHeight = atlas.height / rows;
            for (int i = 0; i < names.Length; i++)
            {
                int column = i % columns;
                int rowFromTop = i / columns;
                int y = atlas.height - ((rowFromTop + 1) * cellHeight);
                RectInt sourceRect = trimmedRects != null && i < trimmedRects.Length
                    ? trimmedRects[i]
                    : new RectInt(column * cellWidth, y, cellWidth, cellHeight);
                Sprite sprite = Sprite.Create(atlas, new Rect(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height),
                    new Vector2(0.5f, 0.5f), 512f);
                sprite.name = names[i];
                uiAssetSprites[names[i]] = sprite;
            }
        }

        private void BuildNavIconAtlas()
        {
            Texture2D atlas = Resources.Load<Texture2D>("Art/UI/dw_nav_icons_v2");
            if (atlas == null) return;

            string[] names = { "nav_mine", "nav_modes", "nav_workshop", "nav_pets", "nav_chests", "nav_shop" };
            const int columns = 3;
            const int rows = 2;
            int cellWidth = atlas.width / columns;
            int cellHeight = atlas.height / rows;
            for (int i = 0; i < names.Length; i++)
            {
                int column = i % columns;
                int rowFromTop = i / columns;
                int y = atlas.height - ((rowFromTop + 1) * cellHeight);
                Sprite sprite = Sprite.Create(atlas, new Rect(column * cellWidth, y, cellWidth, cellHeight),
                    new Vector2(0.5f, 0.5f), 512f);
                sprite.name = names[i];
                navIconSprites[names[i]] = sprite;
            }
        }

        private Sprite NavIconFor(string name)
        {
            if (navIconSprites.TryGetValue(name, out Sprite sprite)) return sprite;
            return Resources.Load<Sprite>("Art/Workshop/gear");
        }

        private RectTransform Region(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var item = new GameObject(name, typeof(RectTransform));
            item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform;
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
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
                caveMotionTweens[i] = caveSeams[i].DOAnchorPosX(-110f, 5.5f + i * 0.18f)
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

        private RectTransform MakeArtActionButton(string name, Transform parent, string text, int size, float left, float bottom,
            float right, float top, UnityEngine.Events.UnityAction action, string art)
        {
            RectTransform rect = MakeArtPanel(name, parent, Color.white, art, left, bottom, right, top);
            Button button = rect.gameObject.AddComponent<Button>();
            Image image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            TextMeshProUGUI label = MakeLabel("Label", rect, text, size, Cream, Vector2.zero, new Vector2(1, 1), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            label.outlineWidth = 0.18f;
            label.outlineColor = Ink;
            return rect;
        }

        private RectTransform MakeOverlayButton(string name, Transform parent, string text, int size, float left, float bottom,
            float right, float top, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = MakePanel(name, parent, new Color(1f, 1f, 1f, 0f),
                new Vector2(left, bottom), new Vector2(right, top), Vector2.zero, Vector2.zero);
            Button button = rect.gameObject.AddComponent<Button>();
            Image image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            TextMeshProUGUI label = MakeLabel("Label", rect, text, size, Cream, Vector2.zero, new Vector2(1, 1), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            label.outlineWidth = 0.18f;
            label.outlineColor = Ink;
            return rect;
        }
    }
}
