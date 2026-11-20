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
    public sealed partial class PrototypeGameUI : MonoBehaviour
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
        private TextMeshProUGUI helpTitle, helpText;
        private TextMeshProUGUI statusLabel;
        private TextMeshProUGUI healthLabel;
        private RectTransform helpPanel;
        private TextMeshProUGUI combatLog;
        private TextMeshProUGUI menuTitle;
        private TextMeshProUGUI menuBody;
        private RectTransform restartButton;
        private RectTransform continueButton;
        private RectTransform trainingButton;
        private RectTransform trainingPicker, albumPanel;
        private TextMeshProUGUI trainingSelection, albumTitle, albumCount;
        private readonly TextMeshProUGUI[] discoveryCards = new TextMeshProUGUI[2];
        private readonly RectTransform[] helpTabs = new RectTransform[2];
        private readonly Image[] drillModules = new Image[3];
        private int trainingStage, albumChapter = 1, pendingDiscovery;

        private sealed class UpgradeView
        {
            public TextMeshProUGUI Value, Level, Price;
            public Button Button;
            public Image Lock, Face, Coin;
        }
        private readonly Dictionary<UpgradeKind, UpgradeView> upgradeViews = new Dictionary<UpgradeKind, UpgradeView>();
        private readonly Image[] powerFaces = new Image[5];
        private readonly Image[] powerIcons = new Image[5];
        private static readonly string[] PowerIconNames = { "attack", "core", "shield", "bolt" };
        private readonly TextMeshProUGUI[] powerLabels = new TextMeshProUGUI[5];
        private TextMeshProUGUI forceValue, missionReward, stageProgress;
        private RectTransform scrollThumb;
        private TMP_FontAsset hudFont;
        private TMP_FontAsset readingFont;
        private readonly Dictionary<string, GameObject> navHighlights = new Dictionary<string, GameObject>();
        private RectTransform sceneryMotion;
        private float sceneryTime;
        private readonly Image[] sceneryTiles = new Image[3];
        private Button autoToggleButton;
        private TextMeshProUGUI autoStateLabel;
        private GameObject statusPlate;
        private PooledCombatFeedback feedback;
        private Image healthFill;
        private Image playerHealthTrack;
        private RectTransform bossBar;
        private Image bossBarFill;
        private TextMeshProUGUI bossBarText;
        private Image playerExperienceTrack, playerExperienceFill;
        private TextMeshProUGUI avatarLevelLabel, missionNumber;
        private Image missionRewardIcon, autoSwitchFace;
        private readonly Image[] powerFrames = new Image[5];
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
        private Image[] enemyHealthTracks;
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
        private TMP_FontAsset labelFont;
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
            MakePanel("Screen matte", root.parent, Ink, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).SetAsFirstSibling();
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
                feedback.Initialize(worldPanel, ResolvePlayerPoint, ResolveEnemyPoint, SpriteFor("gold_coin"), hudFont);
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
            if (roundPowerTexture != null) Destroy(roundPowerTexture);
            if (startPanel != null) Destroy(startPanel.gameObject);
            if (startFont != null)
            {
                foreach (Texture2D atlas in startFont.atlasTextures) if (atlas != null) Destroy(atlas);
                if (startFont.material != null) Destroy(startFont.material);
                Destroy(startFont);
            }
            foreach (Sprite sprite in uiAssetSprites.Values)
                if (sprite != null) Destroy(sprite);
            if (hudFont != null)
            {
                foreach (Texture2D atlas in hudFont.atlasTextures) if (atlas != null) Destroy(atlas);
                if (hudFont.material != null) Destroy(hudFont.material);
                Destroy(hudFont);
            }
            if (readingFont != null)
            {
                foreach (Texture2D atlas in readingFont.atlasTextures) if (atlas != null) Destroy(atlas);
                if (readingFont.material != null) Destroy(readingFont.material);
                Destroy(readingFont);
            }
            if (labelFont != null)
            {
                foreach (Texture2D atlas in labelFont.atlasTextures) if (atlas != null) Destroy(atlas);
                if (labelFont.material != null) Destroy(labelFont.material);
                Destroy(labelFont);
            }
            foreach (Sprite sprite in navIconSprites.Values)
                if (sprite != null) Destroy(sprite);
        }

        private void BuildInterface()
        {
            BuildUiAssetAtlases();
            BuildRoundPowerSkin();
            BuildIllustratedMenuArt();
            Font body = Resources.Load<Font>("Fonts/Workshop/DWRounded-ExtraBold");
            if (body != null)
            {
                hudFont = TMP_FontAsset.CreateFontAsset(body, 64, 8, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
                hudFont?.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ÁÀÂÃÉÊÍÓÔÕÚÜÇáàâãéêíóôõúüç.,:%+/!?—•-()×");
            }
            Font reading = Resources.Load<Font>("Fonts/Workshop/Nunito-Reading");
            if (reading != null)
                readingFont = TMP_FontAsset.CreateFontAsset(reading, 64, 8, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            Font labelSource = Resources.Load<Font>("Fonts/Workshop/Nunito-Bold");
            if (labelSource != null)
                labelFont = TMP_FontAsset.CreateFontAsset(labelSource, 64, 8, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            UiTypography.DisableSyntheticBold(hudFont);
            UiTypography.DisableSyntheticBold(readingFont);
            UiTypography.DisableSyntheticBold(labelFont);
            UiTypography.DisableSyntheticBold(startFont);
            var background = MakePanel("Background", root, Ink, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            progressionPanel = MakePanel("Progression screen", background, new Color(0.12f, 0.19f, 0.24f), new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            BuildProgressionInterface(progressionPanel);

            combatRoot = MakePanel("Combat screen", background, Color.clear, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            combatRoot.GetComponent<Image>().raycastTarget = false;


            RectTransform avatar = MakeArtPanel("Pilot avatar", combatRoot, Steel, "hud_panel", 0.025f, 0.90f, 0.175f, 0.978f);
            RectTransform avatarCrop = Region("Portrait crop", avatar, .07f, .12f, .93f, .94f);
            avatarCrop.gameObject.AddComponent<RectMask2D>();
            MakeIconSprite(avatarCrop, FindResourceSprite("Art/Drill/drill_pioneira", "idle_00"), -.10f, -.02f, 1.10f, 1.12f, Color.white);
            MakeArtPanel("Gold wallet", combatRoot, Color.white, "wallet_panel", 0.225f, 0.92f, 0.505f, 0.978f);
            walletLabel = MakeLabel("Wallet", combatRoot, "0", 36, White, new Vector2(0.315f, 0.925f), new Vector2(0.49f, 0.97f), TextAlignmentOptions.Center);
            MakeArtPanel("Crystal wallet", combatRoot, Color.white, "crystal_wallet", 0.52f, 0.92f, 0.82f, 0.978f);
            crystalLabel = MakeLabel("Crystals", combatRoot, "0", 36, White, new Vector2(0.60f, 0.925f), new Vector2(0.72f, 0.97f), TextAlignmentOptions.Center);
            RectTransform options = MakeArtPanel("Options", combatRoot, Color.white, "menu_button", 0.85f, 0.92f, 0.975f, 0.978f);
            MakeOverlayButton("Open settings", options, "", 20, 0, 0, 1, 1, () => OpenTopMenu("configuracoes"));
            MakeOverlayButton("Open crystal store", combatRoot, "", 20, 0.73f, 0.92f, 0.82f, 0.978f, () => OpenTopMenu("loja-cristais"));

            RectTransform stagePlate = MakeArtPanel("Stage plate", combatRoot, Color.white, "stage_plate", 0.21f, 0.858f, 0.82f, 0.918f);
            stageLabel = MakeLabel("Stage", stagePlate, "PRADO CRISTALINO  •  1-1", 32, Cream,
                new Vector2(0.12f, 0.43f), new Vector2(0.88f, 0.82f), TextAlignmentOptions.Center);
            stageProgress = MakeLabel("Stage progress", stagePlate, "", 20, Cyan,
                new Vector2(0.15f, 0.16f), new Vector2(0.85f, 0.40f), TextAlignmentOptions.Center);

            // Missão em placa ilustrada (mesma família dos campos do perfil), não texto solto sobre o fundo.
            // HUD inferior aprovado (hud-approved-v8): uma chapa única rebitada reúne missão, poderes, FORÇA e melhorias.
            BuildHudBackplate(combatRoot);
            var missionPanel = MakePanel("Mission banner", combatRoot, Color.clear, new Vector2(.025f, .428f), new Vector2(.975f, .472f), Vector2.zero, Vector2.zero);
            missionPanel.GetComponent<Image>().raycastTarget = false;
            // Etiqueta âmbar com prancheta (peça extraída v8) + campo escuro com o objetivo e a recompensa.
            RectTransform missionField = MakeArtPanel("Mission field", missionPanel, new Color(.04f, .08f, .12f, .97f), "hud_panel", .255f, .04f, 1f, .96f);
            missionField.GetComponent<Image>().raycastTarget = false;
            AddEdge(missionField, new Color(.30f, .45f, .58f), 2f);
            Image missionTag = MakeIconSprite(missionPanel, ExtractedSprite("mission_banner"), 0f, 0f, .27f, 1f, Color.white);
            missionTag.preserveAspect = false;
            missionNumber = MakeLabel("Mission number", missionTag.rectTransform, "MISSÃO 01", 24, Ink, new Vector2(.33f, .14f), new Vector2(.95f, .86f), TextAlignmentOptions.Center);
            if (startFont != null) missionNumber.font = startFont;
            missionNumber.outlineWidth = 0f;
            missionLabel = MakeLabel("Mission", missionPanel, "Derrote 10 inimigos", 24, Cream,
                new Vector2(0.29f, 0.10f), new Vector2(0.80f, 0.90f), TextAlignmentOptions.Left);
            missionLabel.fontStyle = FontStyles.Bold;
            missionRewardIcon = MakeIcon(missionPanel, "crystal", .855f, .14f, .915f, .86f, Color.white);
            missionReward = MakeLabel("Mission reward", missionPanel, "", 24, Cream,
                new Vector2(0.80f, 0.10f), new Vector2(0.975f, 0.90f), TextAlignmentOptions.Right);
            missionReward.outlineWidth = .15f;
            missionReward.outlineColor = Ink;

            worldPanel = MakePanel("Cave view", combatRoot, Color.clear, new Vector2(0, 0.48f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            worldPanel.SetAsFirstSibling();
            worldPanel.GetComponent<Image>().raycastTarget = false;
            worldPanel.gameObject.AddComponent<RectMask2D>();
            RectTransform backdrop = Region("Stage backdrop", worldPanel, 0, 0, 1, 1);
            sceneryMotion = backdrop;
            sceneryMotion.localScale = new Vector3(1.06f, 1f, 1f);
            stageBackdrop = backdrop.gameObject.AddComponent<Image>();
            sceneryTiles[0] = stageBackdrop;
            stageBackdrop.raycastTarget = false;
            stageBackdrop.preserveAspect = false;
            backdropAspect = backdrop.gameObject.AddComponent<AspectRatioFitter>();
            backdropAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            for (int i = 1; i < sceneryTiles.Length; i++)
            {
                RectTransform tile = Region("Scenery continuation " + i, worldPanel, 0, 0, 1, 1);
                tile.localScale = new Vector3(i == 1 ? -1.06f : 1.06f, 1f, 1);
                sceneryTiles[i] = tile.gameObject.AddComponent<Image>();
                sceneryTiles[i].raycastTarget = false;
                tile.gameObject.SetActive(false);
                tile.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            }
            MakePanel("Ground shade", worldPanel, new Color(0.02f, 0.06f, 0.08f, 0.16f), new Vector2(0, 0), new Vector2(1, 0.08f), Vector2.zero, Vector2.zero);
            // Construções e cristais pertencem à ilustração; movimento lateral dá profundidade.

            caveSeams = new RectTransform[6];
            caveMotionTweens = new Tween[caveSeams.Length];
            for (int i = 0; i < caveSeams.Length; i++)
            {
                float x = 0.02f + i * 0.18f;
                float y = 0.38f + (i % 3) * 0.12f;
                caveSeams[i] = MakePanel("World wind " + i, worldPanel, new Color(0.62f, 0.96f, 0.88f, 0.08f),
                    new Vector2(x, y), new Vector2(x + 0.075f, y + 0.0015f), Vector2.zero, Vector2.zero);
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
            drillVisualRect.anchorMin = Vector2.zero;
            drillVisualRect.anchorMax = new Vector2(1, 0);
            drillVisualRect.pivot = new Vector2(0.5f, 0);
            Sprite initialDrill = drillVisualObject.GetComponent<Image>().sprite;
            AspectRatioFitter drillAspect = drillVisualObject.AddComponent<AspectRatioFitter>();
            drillAspect.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            drillAspect.aspectRatio = initialDrill == null ? 1.35f : initialDrill.rect.width / initialDrill.rect.height;
            drillAnimator = drillVisualObject.AddComponent<ProceduralAnimator2D>();
            drillMotion = drillVisualObject.AddComponent<GroundDrillMotion2D>();
            MakeOverlayButton("Manual drill attack", drillIcon.transform, "", 15, 0, 0, 1, 1, () =>
            {
                if (driver != null && driver.Simulation != null && !driver.Simulation.AutoAttackEnabled &&
                    !driver.TryManualBasicAttack() && statusLabel != null)
                    statusLabel.text = "AGUARDE A CADÊNCIA OU UM ALVO";
            });

            // Quatro criaturas + o guardião, que pode surgir antes de terminar a onda anterior.
            enemyVisualRoots = new RectTransform[5];
            enemyImages = new Image[5];
            enemySprites = new EnemySpritePresenter[5];
            enemyAnimators = new ProceduralAnimator2D[5];
            enemyHealth = new TextMeshProUGUI[5];
            enemyHealthFills = new Image[5];
            enemyHealthTracks = new Image[5];
            enemyIds = new int[5];
            enemyMaximumHealth = new float[5];
            for (int i = 0; i < enemyVisualRoots.Length; i++)
            {
                float left = 0.49f + i * 0.098f;
                var visualRootObject = new GameObject("Enemy " + i, typeof(RectTransform));
                visualRootObject.transform.SetParent(worldPanel, false);
                var visualRoot = (RectTransform)visualRootObject.transform;
                visualRoot.pivot = new Vector2(.5f, 0);
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
                enemyHealth[i].outlineWidth = .18f;
                enemyHealth[i].outlineColor = Ink;
                enemyHealthTracks[i] = MakePanel("Enemy HP track " + i, worldPanel, new Color(0.03f, 0.06f, 0.08f, 0.92f),
                    new Vector2(left - 0.005f, 0.535f), new Vector2(left + 0.10f, 0.55f), Vector2.zero, Vector2.zero).GetComponent<Image>();
                enemyHealthFills[i] = MakePanel("Enemy HP fill " + i, worldPanel, Coral,
                    new Vector2(left - 0.005f, 0.535f), new Vector2(left + 0.10f, 0.55f), Vector2.zero, Vector2.zero).GetComponent<Image>();
                visualRoot.gameObject.SetActive(false);
                enemyHealth[i].gameObject.SetActive(false);
                enemyHealthFills[i].gameObject.SetActive(false);
                enemyHealthTracks[i].gameObject.SetActive(false);
                // Contorno escuro separa a barra do chão claro do cenário.
                Outline trackEdge = enemyHealthTracks[i].gameObject.AddComponent<Outline>();
                trackEdge.effectColor = Ink;
                trackEdge.effectDistance = new Vector2(3f, -3f);
            }
            healthLabel = MakeLabel("Health", worldPanel, "", 24, Cream,
                new Vector2(0.04f, 0.462f), new Vector2(0.38f, 0.493f), TextAlignmentOptions.Left);
            healthLabel.outlineWidth = .2f;
            healthLabel.outlineColor = Ink;
            // Ícone dedicado de vida ao lado do número (guia, seção 12: sem glifos no lugar de ícones).
            MakeIcon(healthLabel.transform, "health", 0f, 0f, .17f, 1f, Color.white);
            healthLabel.margin = new Vector4(52f, 0f, 0f, 0f);
            playerHealthTrack = MakePanel("Health track", worldPanel, new Color(.03f, .06f, .08f, .92f), new Vector2(0.04f, 0.448f), new Vector2(0.38f, 0.46f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            // Barra emoldurada: contorno escuro + friso de latão, reflexo no preenchimento (lê sobre o chão claro).
            Outline hpEdge = playerHealthTrack.gameObject.AddComponent<Outline>();
            hpEdge.effectColor = new Color(.85f, .58f, .16f, .9f);
            hpEdge.effectDistance = new Vector2(3f, -3f);
            healthFill = MakePanel("Health fill", worldPanel, Mint, new Vector2(0.04f, 0.448f), new Vector2(0.38f, 0.46f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            AddBarShine(healthFill.rectTransform);
            for (int i = 0; i < enemyHealthFills.Length; i++) AddBarShine(enemyHealthFills[i].rectTransform);
            // Barra do chefe: chapa escura com contorno coral, emblema de perigo e barra grossa com reflexo.
            bossBar = MakeArtPanel("Boss health", worldPanel, new Color(.03f, .07f, .10f, .92f), "hud_panel", .06f, .595f, .94f, .685f);
            bossBar.GetComponent<Image>().raycastTarget = false;
            Outline bossEdge = bossBar.gameObject.AddComponent<Outline>();
            bossEdge.effectColor = Coral;
            bossEdge.effectDistance = new Vector2(3f, -3f);
            MakeIcon(bossBar, "critical_damage", .01f, .06f, .12f, .94f, Color.white);
            MakePanel("Boss health track", bossBar, Ink, new Vector2(BossBarStart, .12f), new Vector2(BossBarStart + BossBarWidth, .44f), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
            bossBarFill = MakePanel("Boss health fill", bossBar, Coral, new Vector2(BossBarStart, .12f), new Vector2(BossBarStart + BossBarWidth, .44f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            bossBarFill.raycastTarget = false;
            MakePanel("Boss health shine", bossBarFill.transform, new Color(1f, 1f, 1f, .22f), new Vector2(0, .62f), new Vector2(1, .86f), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
            bossBarText = MakeLabel("Boss health text", bossBar, "GUARDIÃO", 26, Cream, new Vector2(BossBarStart, .48f), new Vector2(.97f, .96f), TextAlignmentOptions.Left);
            bossBarText.outlineWidth = .15f;
            bossBarText.outlineColor = Ink;
            bossBarText.raycastTarget = false;
            bossBar.gameObject.SetActive(false);
            playerExperienceTrack = MakePanel("Experience track", avatar, Ink, new Vector2(.12f, .06f), new Vector2(.88f, .10f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            playerExperienceFill = MakePanel("Experience fill", avatar, Cyan, new Vector2(.12f, .06f), new Vector2(.88f, .10f), Vector2.zero, Vector2.zero).GetComponent<Image>();
            // Selo de nível da broca no canto do retrato (padrão do gênero: nível sempre visível no topo).
            RectTransform levelBadge = MakeArtPanel("Avatar level badge", avatar, new Color(.05f, .10f, .14f, .96f), "hud_panel", .52f, -.16f, 1.10f, .20f);
            Outline badgeEdge = levelBadge.gameObject.AddComponent<Outline>();
            badgeEdge.effectColor = Gold;
            badgeEdge.effectDistance = new Vector2(2f, -2f);
            avatarLevelLabel = MakeLabel("Avatar level", levelBadge, "NV.1", 20, Gold, new Vector2(.06f, .04f), new Vector2(.94f, .96f), TextAlignmentOptions.Center);
            avatarLevelLabel.outlineWidth = .18f;
            avatarLevelLabel.outlineColor = Ink;
            MakeOverlayButton("Open profile", avatar, "", 20, 0, 0, 1, 1, () => OpenTopMenu("perfil-info"));
            statusLabel = MakeLabel("Status", combatRoot, "", 25, Cream,
                new Vector2(0.12f, 0.512f), new Vector2(0.88f, 0.545f), TextAlignmentOptions.Center);
            combatLog = MakeLabel("Combat log", combatRoot, "", 18, Cream,
                new Vector2(0.12f, 0.495f), new Vector2(0.88f, 0.512f), TextAlignmentOptions.Center);
            statusLabel.outlineWidth = combatLog.outlineWidth = .2f;
            statusLabel.outlineColor = combatLog.outlineColor = Ink;
            // Faixa translúcida atrás das mensagens: leitura fixa sem depender do chão do cenário.
            RectTransform statusBand = MakeArtPanel("Status plate", combatRoot, new Color(.02f, .06f, .08f, .62f), "hud_panel", .10f, .492f, .90f, .548f);
            statusBand.SetSiblingIndex(statusLabel.transform.GetSiblingIndex());
            statusPlate = statusBand.gameObject;
            BuildPowerBar(combatRoot);
            BuildNavIconAtlas();

            // Painel de melhorias em chapa ilustrada. Ocupa o espaço que antes sobrava sob os poderes,
            // mostrando mais linhas sem comprimir o cenário; FORÇA passou para o cabeçalho.
            // Peças de nove partes esticavam as tampas douradas nesta altura; a chapa usa friso de latão como o dock.
            // v8: as linhas de melhoria ficam direto na chapa rebitada, sem título próprio; o título só aparece em Modos.
            menuPanel = MakePanel("Upgrade panel", combatRoot, Color.clear, new Vector2(0.012f, 0.094f), new Vector2(0.988f, 0.350f), Vector2.zero, Vector2.zero);
            menuTitle = MakeLabel("Tab title", menuPanel, "MELHORIAS DA BROCA", 26, Gold, new Vector2(0.05f, 0.88f), new Vector2(0.58f, 0.975f), TextAlignmentOptions.Left);
            if (startFont != null) menuTitle.font = startFont;
            menuTitle.outlineWidth = .2f;
            menuTitle.outlineColor = Ink;
            menuBody = MakeLabel("Tab info", menuPanel, "", 18, Cream, new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.82f), TextAlignmentOptions.Center);
            menuBody.gameObject.SetActive(false);
            upgradeRowsRoot = BuildUpgradeScroll(menuPanel);
            foreach (UpgradeKind kind in GameplayHudModel.UpgradeOrder) BuildGameplayUpgrade(upgradeRowsRoot, kind);
            restartButton = MakeArtActionButton("Restart run", missionPanel, "RECOMEÇAR", 24, 0.70f, 0.13f, 0.97f, 0.87f,
                () => { driver.StartNewGame(); EnterGame(false); }, "hud_panel");
            restartButton.GetComponent<Image>().color = new Color(.13f, .49f, .42f);
            restartButton.gameObject.SetActive(false);
            continueButton = MakeArtActionButton("Continue stage", missionPanel, "TREINAR", 24, 0.70f, 0.13f, 0.97f, 0.87f,
                () => { if (driver.TryReturnToCampaign() || driver.TryStartTraining()) { currentTab = "MINA"; EnterGame(false); } }, "hud_panel");
            continueButton.GetComponent<Image>().color = new Color(.13f, .49f, .42f);
            continueButton.gameObject.SetActive(false);
            trainingButton = MakeButton("Training mode", menuPanel, "INICIAR TREINO", 22, 0.16f, 0.04f, 0.84f, 0.20f,
                () => { if (driver.TryStartTraining(trainingStage)) { currentTab = "MINA"; EnterGame(false); } }, Mint);
            trainingButton.gameObject.SetActive(false);

            trainingPicker = Region("Training selection", menuPanel, 0.06f, 0.25f, 0.94f, 0.47f);
            RectTransform previousTraining = MakeButton("Previous training", trainingPicker, "<", 28, 0, 0, .22f, 1, () => ChangeTraining(-1), Steel);
            RectTransform nextTraining = MakeButton("Next training", trainingPicker, ">", 28, .78f, 0, 1, 1, () => ChangeTraining(1), Steel);
            if (uiAssetSprites.TryGetValue("skin_tab", out Sprite pagerSkin)) { previousTraining.GetComponent<Image>().sprite = nextTraining.GetComponent<Image>().sprite = pagerSkin; previousTraining.GetComponent<Image>().color = nextTraining.GetComponent<Image>().color = Color.white; }
            else previousTraining.GetComponent<Image>().sprite = nextTraining.GetComponent<Image>().sprite = SpriteFor("slot");
            previousTraining.GetComponentInChildren<TextMeshProUGUI>().color = nextTraining.GetComponentInChildren<TextMeshProUGUI>().color = Cream;
            if (uiAssetSprites.TryGetValue("skin_action", out Sprite trainSkin)) { trainingButton.GetComponent<Image>().sprite = trainSkin; trainingButton.GetComponent<Image>().color = Color.white; }
            else trainingButton.GetComponent<Image>().sprite = SpriteFor("plain_action");
            trainingButton.GetComponentInChildren<TextMeshProUGUI>().color = Cream;
            trainingSelection = MakeLabel("Selected training stage", trainingPicker, "", 20, Cream, new Vector2(.23f, 0), new Vector2(.77f, 1), TextAlignmentOptions.Center);
            trainingPicker.gameObject.SetActive(false);

            bottomBar = MakePanel("Bottom navigation", combatRoot, Deep, Vector2.zero, new Vector2(1,.088f),Vector2.zero,Vector2.zero);
            // Friso de latão contínuo até as bordas: separa o dock do painel sem moldura flutuante.
            MakePanel("Dock brass edge", bottomBar, new Color(.30f, .42f, .54f), new Vector2(0, .955f), new Vector2(1, 1), Vector2.zero, Vector2.zero)
                .GetComponent<Image>().raycastTarget = false;
            MakePanel("Dock brass shadow", bottomBar, new Color(.03f, .06f, .09f), new Vector2(0, .935f), new Vector2(1, .955f), Vector2.zero, Vector2.zero)
                .GetComponent<Image>().raycastTarget = false;
            string[] tabs = { "MINA", "MODOS", "OFICINA", "EQUIP.", "LOJA" };
            string[] icons = { "nav_mine", "nav_modes", "nav_workshop", "shield", "nav_shop" };
            // Inner cells in bottom_nav_structure (1484 px), excluding side endcaps and separators.
            float[] cellLeft = { 80f, 318f, 539f, 751f, 966f, 1179f };
            float[] cellRight = { 300f, 518f, 731f, 944f, 1156f, 1402f };
            for (int i = 0; i < tabs.Length; i++)
            {
                string tab = tabs[i];
                RectTransform tabButton = MakeNavButton("Tab " + tab, bottomBar, tab, icons[i], (.008f + i * .20f), .08f, (.192f + i * .20f), .94f,
                    () => { if (tab == "OFICINA") OpenProgression(); else if (tab == "EQUIP.") OpenEquipmentPage(); else if (tab == "LOJA") OpenTopMenu("loja-suprimentos"); else if (tab == "ÁLBUM") OpenAlbum(); else SelectTab(tab); }, tab == currentTab ? Cyan : Steel);
                tabButtonImages.Add(tab, tabButton.GetComponent<Image>());
            }

            BuildAlbum();
            BuildHelp();
            BuildTopMenus();
            menuPanel.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
            worldPanel.gameObject.SetActive(false);
            stageLabel.gameObject.SetActive(false);
            walletLabel.gameObject.SetActive(false);
            crystalLabel.gameObject.SetActive(false);
            missionLabel.gameObject.SetActive(false);
            progressionPanel.gameObject.SetActive(false);
            combatRoot.gameObject.SetActive(false);
            BuildMomentBanner();
            UiTypography.Normalize(root, hudFont, labelFont);
            UiPressFeedback.AttachAll(root);
        }

        private void ChangeTraining(int direction)
        {
            trainingStage = Math.Max(1, Math.Min(driver.Profile.HighestClearedStage, trainingStage + direction));
            UpdateTab();
        }

        private void BuildHelp()
        {
            // Mesma linguagem dos menus ilustrados: fundo da oficina, placa de título, painel escuro de leitura e botões com arte.
            helpPanel = MakePanel("Combat help", combatRoot, new Color(.02f, .05f, .08f, .88f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform card = MakeArtPanel("Help card", helpPanel, Color.white, "menu_background", .04f, .12f, .96f, .88f);
            MakeArtPanel("Help plaque", card, Color.white, "stage_plate", .08f, .87f, .92f, .97f);
            helpTitle = MakeLabel("Help title", card, "COMO JOGAR", 40, Gold, new Vector2(.16f, .885f), new Vector2(.84f, .955f), TextAlignmentOptions.Center);
            if (startFont != null) helpTitle.font = startFont;
            helpTitle.outlineWidth = .12f; helpTitle.outlineColor = Ink;
            MakeArtPanel("Help reading plate", card, new Color(.03f, .08f, .11f, .88f), "hud_panel", .07f, .33f, .93f, .84f);
            helpText = MakeLabel("Help text", card, "", 28, Cream, new Vector2(.12f, .36f), new Vector2(.88f, .81f), TextAlignmentOptions.Center);
            if (readingFont != null) helpText.font = readingFont;
            helpTabs[0] = MakeArtActionButton("Guide basics", card, "COMBATE", 24, .07f, .215f, .49f, .305f, () => ShowHelpPage(false), "skin_tab");
            helpTabs[1] = MakeArtActionButton("Guide progress", card, "PROGRESSO", 24, .51f, .215f, .93f, .305f, () => ShowHelpPage(true), "skin_tab");
            MakeArtActionButton("Close help", card, "VOLTAR AO JOGO", 28, .17f, .06f, .83f, .18f, () => {
                helpPanel.gameObject.SetActive(false);
                bool resumeCombat = currentTab == "MINA" || currentTab == "MELHORIAS";
                driver.SetMenuPaused(!resumeCombat);
                if (resumeCombat && driver.Simulation != null && driver.Simulation.IsActive && !driver.IsSaveBlocked) StartCaveMotion();
            }, "hud_panel");
            helpPanel.gameObject.SetActive(false);
        }

        private void ShowHelpPage(bool progress)
        {
            for (int i = 0; i < helpTabs.Length; i++)
                if (helpTabs[i] != null && uiAssetSprites.TryGetValue((i == 1) == progress ? "skin_tab_active" : "skin_tab", out Sprite tabSkin))
                    helpTabs[i].GetComponent<Image>().sprite = tabSkin;
            helpTitle.text = progress ? "DIÁRIO DE EXPEDIÇÃO" : "COMO JOGAR";
            if (!progress)
            {
                helpText.text = "Use o ouro para melhorar a broca.\n\nAUTO controla os disparos básicos. Com AUTO desligado, toque na broca para atacar.\n\nO especial atravessa armadura. Interrompa ataques derrotando inimigos durante a preparação.";
                return;
            }
            var sim = driver.Simulation;
            if (sim == null) { helpText.text = "Comece uma expedição para acompanhar seu progresso."; return; }
            CharacterLevelState level = driver.CurrentCharacterLevel;
            string xp = level.IsMaximum ? "NÍVEL MÁXIMO" : "XP " + level.ExperienceInLevel + " / " + level.ExperienceToNextLevel;
            helpText.text = GameplayHudModel.CampaignPosition(sim.Run.CampaignStage) + "\nBROCA NV. " + level.Level + " • " + xp +
                "\n\nÁLBUM: " + sim.Profile.DiscoveryStages.Count + " / " + DiscoveryCatalog.Entries.Count + " descobertas" +
                "\n\nFases vencidas podem ser revisitadas em MODOS. Ouro e XP são preservados.";
        }

        private void OpenHelp()
        {
            ShowHelpPage(false);
            driver.SetMenuPaused(true);
            StopCaveMotion();
            helpPanel.gameObject.SetActive(true);
            OpenWithPop(helpPanel);
        }

        private void BuildAlbum()
        {
            albumPanel = MakeArtPanel("Discovery album", combatRoot, Color.white, "menu_background", 0, 0, 1, 1);
            albumPanel.GetComponent<Image>().raycastTarget = true;
            MakeArtPanel("Album plaque", albumPanel, Color.white, "stage_plate", .06f, .875f, .94f, .965f);
            albumTitle = MakeLabel("Album title", albumPanel, "ÁLBUM DE DESCOBERTAS", 38, Gold, new Vector2(.15f, .888f), new Vector2(.85f, .952f), TextAlignmentOptions.Center);
            if (startFont != null) albumTitle.font = startFont;
            albumTitle.outlineWidth = .12f; albumTitle.outlineColor = Ink;
            MakeArtPanel("Album progress plate", albumPanel, new Color(.03f, .08f, .11f, .85f), "hud_panel", .20f, .80f, .80f, .855f);
            albumCount = MakeLabel("Album progress", albumPanel, "", 26, Cyan, new Vector2(.22f, .805f), new Vector2(.78f, .85f), TextAlignmentOptions.Center);
            for (int i = 0; i < 2; i++)
            {
                float bottom = i == 0 ? .50f : .24f;
                // Cartão de descoberta: chapa escura com contorno de latão e ícone do álbum.
                RectTransform card = MakeArtPanel("Discovery card " + i, albumPanel, new Color(.06f, .13f, .18f, .95f), "hud_panel", .07f, bottom, .93f, bottom + .23f);
                Outline edge = card.gameObject.AddComponent<Outline>();
                edge.effectColor = new Color(.85f, .58f, .16f, .9f);
                edge.effectDistance = new Vector2(3f, -3f);
                MakeArtPanel("Discovery badge", card, new Color(.03f, .08f, .11f), "hud_panel", .04f, .25f, .22f, .75f);
                MakeIcon(card, i == 0 ? "nav_album" : "crystal", .06f, .30f, .20f, .70f, Color.white);
                discoveryCards[i] = MakeLabel("Discovery description", card, "", 28, Cream, new Vector2(.26f, .10f), new Vector2(.95f, .90f), TextAlignmentOptions.Left);
                if (readingFont != null) discoveryCards[i].font = readingFont;
            }
            MakeArtActionButton("Previous chapter", albumPanel, "<", 32, .07f, .14f, .27f, .21f, () => { albumChapter = Math.Max(1, albumChapter - 1); RefreshAlbum(); }, "skin_tab");
            MakeArtActionButton("Next chapter", albumPanel, ">", 32, .73f, .14f, .93f, .21f, () => { albumChapter = Math.Min(10, albumChapter + 1); RefreshAlbum(); }, "skin_tab");
            MakeArtActionButton("Close album", albumPanel, "CONTINUAR", 28, .25f, .04f, .75f, .13f, () => {
                albumPanel.gameObject.SetActive(false);
                SelectTab("MINA");
            }, "hud_panel");
            albumPanel.gameObject.SetActive(false);
        }

        private void OpenAlbum(bool newDiscovery = false)
        {
            currentTab = "ÁLBUM";
            driver.SetMenuPaused(true);
            feedback?.Clear();
            albumTitle.text = newDiscovery ? "NOVA DESCOBERTA!" : "ÁLBUM DE DESCOBERTAS";
            albumPanel.gameObject.SetActive(true);
            OpenWithPop(albumPanel);
            RefreshAlbum();
        }

        private void RefreshAlbum()
        {
            albumCount.text = driver.Profile.DiscoveryStages.Count + " / " + DiscoveryCatalog.Entries.Count + " ACHADOS  •  CAPÍTULO " + albumChapter;
            for (int i = 0; i < 2; i++)
            {
                int stage = (albumChapter - 1) * 10 + (i == 0 ? 5 : 10);
                DiscoveryDefinition item = DiscoveryCatalog.Find(stage);
                bool owned = driver.Profile.DiscoveryStages.Contains(stage);
                discoveryCards[i].color = owned ? Cream : new Color(.83f, .88f, .87f);
                discoveryCards[i].text = owned ? (item.IsRelic ? "RELÍQUIA" : "FÓSSIL") + " • " + item.Name.ToUpperInvariant() + "\n\n" + item.Description
                    : "DESCOBERTA DESCONHECIDA\n\nVença " + (i == 0 ? "o mini-chefe e a escolta" : "o guardião") + " na fase " + (i == 0 ? "5" : "10") + " deste capítulo.";
            }
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
            feedback?.Clear();
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
                // Remove somente o excesso de subsolo da ilustração. A trilha fica perto do HUD,
                // mantendo o mesmo chão para cenário, unidades e projéteis.
                float originalGround = id == "prado_cristalino" ? 0.414f : id == "jardim_fossil" ? 0.337f : 0.38f;
                float cropBottom = (originalGround - 0.12f) / 0.88f * texture.height;
                sprite = Sprite.Create(texture, new Rect(0, cropBottom, texture.width, texture.height - cropBottom), new Vector2(0.5f, 0.5f), 100f);
                mapBackgroundSprites.Add(resource, sprite);
            }
            stageBackdrop.sprite = sprite;
            stageBackdrop.color = id == "fenda_aurora" ? new Color(0.78f, 0.65f, 1f) : Color.white;
            backdropAspect.aspectRatio = sprite.rect.width / sprite.rect.height;
            for (int i = 1; i < sceneryTiles.Length; i++)
            {
                sceneryTiles[i].sprite = sprite;
                sceneryTiles[i].color = stageBackdrop.color;
                sceneryTiles[i].GetComponent<AspectRatioFitter>().aspectRatio = backdropAspect.aspectRatio;
            }
            sceneryTime = 0;
            UpdateScenery();
            ApplyGroundLayout(0.12f);
        }

        private void EnterGame(bool resumed)
        {
            if (driver.Simulation == null) return;
            currentTab = "MINA";
            albumPanel.gameObject.SetActive(false);
            helpPanel.gameObject.SetActive(false);
            feedback?.Clear();
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
            if (resumed) statusLabel.text = "PARTIDA RETOMADA";
            else statusLabel.text = "A broca começou a avançar!";
            int hintPhase = driver.Simulation.Run.CampaignStage;
            combatLog.text = hintPhase == 1 ? "AUTO ataca. Use ouro para melhorar a broca." :
                hintPhase == 2 ? "Ágeis têm pouca vida e atacam mais rápido." :
                hintPhase == 3 ? "Blindados resistem aos tiros; o especial atravessa a armadura." :
                hintPhase == 4 ? "PREPARANDO: derrote o inimigo para interromper o ataque." :
                hintPhase % 5 == 0 ? "Vença o encontro para revelar uma descoberta no álbum." : "";
            RefreshAutoToggle();
            drillAnimator.ResetForRun();
            drillFrames.ResetForRun();
            drillMotion.ResetForRun();
            ClearEnemyVisuals();
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
            if (statusPlate != null)
                statusPlate.SetActive(!string.IsNullOrEmpty(statusLabel.text) || !string.IsNullOrEmpty(combatLog.text));
            UpdateDefeatPopup();
            // Avanço fora do evento de vitória: o driver já terminou o passo e salvou as recompensas.
            if (pendingDiscovery > 0)
            {
                albumChapter = (pendingDiscovery - 1) / 10 + 1;
                pendingDiscovery = 0;
                OpenAlbum(true);
            }
            if (driver.CanContinueCampaign && !driver.IsSaveBlocked && currentTab == "MINA" && !helpPanel.gameObject.activeSelf)
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
            if (driver.Simulation.IsActive && !driver.IsSaveBlocked && currentTab == "MINA" && !helpPanel.gameObject.activeSelf && sceneryMotion != null)
            {
                sceneryTime += Time.unscaledDeltaTime;
                UpdateScenery();
            }
            UpdateEnemyEntrance();
            UpdateScrollThumb();
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
            stageLabel.text = GameplayHudModel.CampaignPosition(sim.Run.CampaignStage);
            int stageNumber = sim.Run.CampaignStage;
            int chapterPhase = (stageNumber - 1) % 10 + 1;
            bool chapterEncounter = sim.Run.EncounterVersion >= 1;
            bool specialEncounter = !chapterEncounter || chapterPhase == 5 || chapterPhase == 10;
            string enemyTitle = chapterEncounter && chapterPhase == 5 ? "MINI-CHEFE" : "GUARDIÃO";
            missionLabel.text = sim.Run.IsPlayerDefeated ? "Tente novamente" : sim.Run.IsStageCleared ? "Missão concluída" : specialEncounter ? (chapterPhase == 5 ? "Vença o mini-chefe e a escolta" : "Derrote o guardião") : "Elimine as ondas";
            stageProgress.text = sim.Run.IsStageCleared ? "FASE CONCLUÍDA" : chapterEncounter && chapterPhase == 10 && sim.Profile.HighestClearedStage >= stageNumber ? "TREINO DE CHEFE • 15% XP" : sim.Run.BossSpawned ? enemyTitle + " À FRENTE" : "ONDA " + Math.Max(1, sim.Run.WavesSpawned) + " / " + sim.StageWaveCount;
            bool firstClear = sim.Profile.HighestClearedStage < stageNumber;
            int crystals = firstClear ? driver.Economy.FirstClearCrystals +
                (stageNumber % driver.Economy.MilestoneInterval == 0 ? driver.Economy.MilestoneCrystals : 0) : 0;
            bool newDiscovery = sim.Run.EncounterVersion >= 2 && DiscoveryCatalog.Find(stageNumber) != null && !sim.Profile.DiscoveryStages.Contains(stageNumber);
            // Recompensa em cristais aparece como ícone + número (v8); as demais continuam por extenso.
            bool crystalReward = !sim.Run.IsStageCleared && !newDiscovery && crystals > 0;
            missionReward.text = sim.Run.IsStageCleared ? "CONCLUÍDA" : newDiscovery ? "BAÚ + CHAVE" : crystalReward ? GameplayHudModel.Number(crystals) : specialEncounter ? "BAÚ + CHAVE" : "+ OURO";
            missionReward.color = crystalReward ? Cream : Gold;
            missionReward.rectTransform.anchorMin = new Vector2(crystalReward ? .92f : .78f, .10f);
            missionReward.alignment = crystalReward ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            if (missionRewardIcon != null) missionRewardIcon.gameObject.SetActive(crystalReward);
            if (missionNumber != null) missionNumber.text = specialEncounter ? (chapterPhase == 5 ? "MINI-CHEFE" : "CHEFE") : "MISSÃO " + chapterPhase.ToString("00");
            forceValue.text = GameplayHudModel.Number(GameplayHudModel.PowerRating(sim, driver.EquippedCharacter));
            RefreshPowerBar();
            if (displayedGold != sim.Gold || displayedCrystals != sim.Profile.Crystals)
            {
                displayedGold = sim.Gold;
                displayedCrystals = sim.Profile.Crystals;
                walletLabel.text = GameplayHudModel.Number(sim.Gold);
                crystalLabel.text = GameplayHudModel.Number(sim.Profile.Crystals);
                walletLabel.transform.DOKill(false);
                walletLabel.transform.localScale = Vector3.one;
                walletLabel.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0), 0.22f, 5, 0.7f);
            }

            healthLabel.text = GameplayHudModel.Number(Math.Ceiling(sim.PlayerHealth)) + " / " + GameplayHudModel.Number(Math.Ceiling(sim.PlayerMaximumHealth));
            UpdatePlayerHealthLayout();
            float healthRatio = sim.PlayerMaximumHealth <= 0 ? 0 : sim.PlayerHealth / sim.PlayerMaximumHealth;
            RectTransform hpTrack = playerHealthTrack.rectTransform;
            healthFill.rectTransform.anchorMax = new Vector2(hpTrack.anchorMin.x + (hpTrack.anchorMax.x - hpTrack.anchorMin.x) * healthRatio, hpTrack.anchorMax.y);
            CharacterLevelState characterLevel = driver.CurrentCharacterLevel;
            float experienceRatio = characterLevel.IsMaximum ? 1f : characterLevel.ExperienceToNextLevel > 0
                ? Mathf.Clamp01((float)characterLevel.ExperienceInLevel / characterLevel.ExperienceToNextLevel) : 0f;
            if (avatarLevelLabel != null) avatarLevelLabel.text = "NV." + characterLevel.Level;
            RectTransform xpTrack = playerExperienceTrack.rectTransform;
            playerExperienceFill.rectTransform.anchorMax = new Vector2(xpTrack.anchorMin.x + (xpTrack.anchorMax.x - xpTrack.anchorMin.x) * experienceRatio, xpTrack.anchorMax.y);
            SyncEnemyVisuals();
            UpdateTab();
            if (sim.Run.IsPlayerDefeated && currentTab == "MINA")
            {
                statusLabel.text = GameplayHudModel.DefeatAdvice(sim);
                menuTitle.text = "TENTATIVA ENCERRADA";
                menuBody.text = GameplayHudModel.DefeatAdvice(sim);
            }
            else if (sim.Run.IsStageCleared)
            {
                statusLabel.text = driver.CanContinueCampaign
                    ? (string.IsNullOrEmpty(lastCharacterReward) ? "Fase concluída!" : lastCharacterReward + " LIBERADA!") + " Próxima fase..."
                    : sim.Run.IsTraining ? "TREINO CONCLUÍDO! Progresso da campanha preservado." : "CAMPANHA CONCLUÍDA! Confira seu álbum e suas brocas.";
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
            driver.SetMenuPaused(tab != "MINA" && tab != "MELHORIAS");
            UpdateTab();
        }

        private void UpdateScenery()
        {
            // Overscan permits a small continuous camera drift without mirrored seams.
            sceneryTiles[0].rectTransform.anchoredPosition = new Vector2(Mathf.Sin(sceneryTime * .16f) * worldPanel.rect.width * .012f, 0);
        }

        private void UpdatePlayerHealthLayout()
        {
            RectTransform unit = drillIcon.rectTransform;
            Sprite sprite = drillFrames.GetComponent<Image>().sprite;
            if (sprite == null || worldPanel.rect.height <= 0) return;
            float height = (unit.anchorMax.x - unit.anchorMin.x) * worldPanel.rect.width * sprite.rect.height / sprite.rect.width;
            float top = unit.anchorMin.y + height / worldPanel.rect.height;
            playerHealthTrack.rectTransform.anchorMin = new Vector2(.06f, top + .008f);
            playerHealthTrack.rectTransform.anchorMax = new Vector2(.27f, top + .025f);
            healthFill.rectTransform.anchorMin = playerHealthTrack.rectTransform.anchorMin;
            healthLabel.rectTransform.anchorMin = new Vector2(.045f, top + .027f);
            healthLabel.rectTransform.anchorMax = new Vector2(.30f, top + .058f);
        }

        private void ApplyGroundLayout(float ground)
        {
            drillIcon.rectTransform.anchorMin = new Vector2(0.055f, ground - 0.012f);
            drillIcon.rectTransform.anchorMax = new Vector2(0.255f, ground + 0.20f);
            drillIcon.rectTransform.offsetMin = drillIcon.rectTransform.offsetMax = Vector2.zero;
            healthLabel.rectTransform.offsetMin = healthLabel.rectTransform.offsetMax = Vector2.zero;
            playerHealthTrack.rectTransform.offsetMin = playerHealthTrack.rectTransform.offsetMax = Vector2.zero;
            healthFill.rectTransform.offsetMin = healthFill.rectTransform.offsetMax = Vector2.zero;
            for (int i = 0; i < enemyVisualRoots.Length; i++)
            {
                float left = 0.49f + i * 0.098f;
                enemyVisualRoots[i].anchorMin = new Vector2(left, ground - 0.012f);
                enemyVisualRoots[i].anchorMax = new Vector2(left + 0.095f, ground + 0.18f);
                enemyHealth[i].rectTransform.anchorMin = new Vector2(left - 0.01f, ground + 0.145f);
                enemyHealth[i].rectTransform.anchorMax = new Vector2(left + 0.10f, ground + 0.178f);
                enemyHealthTracks[i].rectTransform.anchorMin = new Vector2(left, ground + 0.13f);
                enemyHealthTracks[i].rectTransform.anchorMax = new Vector2(left + 0.09f, ground + 0.14f);
                enemyHealthFills[i].rectTransform.anchorMin = enemyHealthTracks[i].rectTransform.anchorMin;
                enemyHealthFills[i].rectTransform.anchorMax = enemyHealthTracks[i].rectTransform.anchorMax;
                enemyVisualRoots[i].offsetMin = enemyVisualRoots[i].offsetMax = Vector2.zero;
                enemyHealth[i].rectTransform.offsetMin = enemyHealth[i].rectTransform.offsetMax = Vector2.zero;
                enemyHealthTracks[i].rectTransform.offsetMin = enemyHealthTracks[i].rectTransform.offsetMax = Vector2.zero;
                enemyHealthFills[i].rectTransform.offsetMin = enemyHealthFills[i].rectTransform.offsetMax = Vector2.zero;
            }
        }

        private int EnemySlot(int id)
        {
            for (int i = 0; i < enemyIds.Length; i++) if (enemyIds[i] == id) return i;
            return -1;
        }

        private void ClearEnemyVisuals()
        {
            for (int i = 0; i < enemyIds.Length; i++)
            {
                enemyVisualRoots[i].DOKill(false);
                enemyVisualRoots[i].anchoredPosition = Vector2.zero;
                enemyIds[i] = 0;
                enemyMaximumHealth[i] = 0;
                SetEnemyVisible(i, false, false);
            }
        }

        private void SetEnemyVisible(int slot, bool visual, bool health)
        {
            enemyVisualRoots[slot].gameObject.SetActive(visual);
            enemyHealth[slot].gameObject.SetActive(health);
            enemyHealthTracks[slot].gameObject.SetActive(health);
            enemyHealthFills[slot].gameObject.SetActive(health);
        }

        private void SyncEnemyVisuals()
        {
            IdleCombatSimulation sim = driver.Simulation;
            EnemySnapshot boss = null;
            foreach (EnemySnapshot candidate in sim.Enemies)
                if (candidate.IsBoss && candidate.Health > 0f) { boss = candidate; break; }
            bool showBossBar = boss != null && (sim.Run.EncounterVersion == 0 || sim.Run.CampaignStage % 10 == 0);
            bossBar.gameObject.SetActive(showBossBar);
            if (showBossBar)
            {
                float maximum = sim.EnemyMaximumHealth(true);
                float bossRatio = maximum > 0f ? Mathf.Clamp01(boss.Health / maximum) : 0f;
                bossBarFill.rectTransform.anchorMax = new Vector2(BossBarStart + BossBarWidth * bossRatio, .44f);
                // Preparação de ataque do chefe pisca a barra em creme: aviso legível sem depender só da cor.
                bossBarFill.color = sim.IsWindingUp(boss.Id) && Mathf.Repeat(Time.unscaledTime, .4f) < .2f ? Cream : Coral;
                string bossTitle = sim.Run.EncounterVersion >= 1 && sim.Run.CampaignStage % 10 == 5 ? "MINI-CHEFE" : "GUARDIÃO";
                bossBarText.text = (sim.IsWindingUp(boss.Id) ? "ATAQUE!  •  " : "") + bossTitle + "  " + GameplayHudModel.Number(Math.Ceiling(boss.Health)) + " / " + GameplayHudModel.Number(Math.Ceiling(maximum));
            }
            for (int i = 0; i < enemyIds.Length; i++)
            {
                bool present = false;
                foreach (EnemySnapshot enemy in sim.Enemies) if (enemy.Id == enemyIds[i]) { present = true; break; }
                if (!present && enemyIds[i] != 0)
                {
                    bool dying = enemySprites[i].IsDying && !enemySprites[i].DeathFinished;
                    SetEnemyVisible(i, dying, false);
                    if (!dying) enemyIds[i] = 0;
                }
            }
            int regularIndex = 0;
            foreach (EnemySnapshot enemy in sim.Enemies)
            {
                int slot = EnemySlot(enemy.Id);
                if (slot < 0)
                {
                    slot = Array.FindIndex(enemyIds, id => id == 0);
                    if (slot < 0)
                    {
                        for (int i = 0; i < enemyIds.Length; i++) if (enemySprites[i].IsDying) { slot = i; break; }
                    }
                    if (slot < 0) continue;
                    enemyIds[slot] = enemy.Id;
                    enemyMaximumHealth[slot] = Mathf.Max(sim.EnemyMaximumHealth(enemy), enemy.Health);
                    SetEnemyVisible(slot, true, true);
                    enemySprites[slot].Configure(string.IsNullOrEmpty(enemy.VisualId) ? "torrao" : enemy.VisualId, enemy.IsBoss, sim.Run.EncounterVersion >= 1 && sim.Run.CampaignStage % 10 == 5 ? 1.05f : 1.3f);
                    enemySprites[slot].ResetForSpawn();
                    enemyAnimators[slot].ResetForRun();
                    RectTransform entering = enemyVisualRoots[slot];
                    entering.DOKill(false);
                    entering.anchoredPosition = new Vector2(worldPanel.rect.width * (1.08f - entering.anchorMin.x), 0);
                    entering.DOAnchorPosX(0, 1.4f).SetEase(Ease.OutCubic);
                }
                SetEnemyVisible(slot, true, true);
                enemyMaximumHealth[slot] = Mathf.Max(enemyMaximumHealth[slot], enemy.Health);
                bool windingUp = sim.IsWindingUp(enemy.Id);
                // Health is conveyed by the bar. Reserve text for roles and attack warnings.
                enemyHealth[slot].text = windingUp ? "ATAQUE!" : enemy.Archetype == EnemyArchetype.Charger ? "CARGA" :
                    enemy.Archetype == EnemyArchetype.Armored ? "BLINDADO" : enemy.IsBoss ? sim.EnemyLabel(enemy) : "";
                enemyHealth[slot].color = windingUp ? Coral : Cream;
                enemyHealthFills[slot].color = enemy.Archetype == EnemyArchetype.Armored ? Sand : Coral;
                float ground = drillIcon.rectTransform.anchorMin.y + .012f;
                bool mini = sim.Run.EncounterVersion >= 1 && sim.Run.CampaignStage % 10 == 5;
                float left = enemy.IsBoss ? (mini ? .39f : .58f) : mini && sim.Run.BossSpawned ? .54f + regularIndex * .105f : .49f + regularIndex * .098f;
                if (!enemy.IsBoss) regularIndex++;
                float unitWidth = enemy.IsBoss ? (mini ? .14f : .23f) : .095f;
                enemyVisualRoots[slot].anchorMin = new Vector2(left, ground - .012f);
                enemyVisualRoots[slot].anchorMax = new Vector2(left + unitWidth, ground + .18f);
                Sprite visual = enemyImages[slot].sprite;
                if (visual != null && worldPanel.rect.height > 0)
                {
                    RectTransform unit = enemyVisualRoots[slot];
                    float barY = unit.anchorMin.y + enemySprites[slot].VisualHeight / worldPanel.rect.height + 0.008f;
                    // Recuo lateral evita que barras de inimigos vizinhos virem uma faixa única.
                    float inset = enemy.IsBoss ? 0f : (unit.anchorMax.x - unit.anchorMin.x) * .14f;
                    enemyHealthTracks[slot].rectTransform.anchorMin = new Vector2(unit.anchorMin.x + inset, barY);
                    enemyHealthTracks[slot].rectTransform.anchorMax = new Vector2(unit.anchorMax.x - inset, barY + 0.014f);
                    enemyHealth[slot].rectTransform.anchorMin = new Vector2(unit.anchorMin.x - .02f, barY + 0.018f);
                    enemyHealth[slot].rectTransform.anchorMax = new Vector2(unit.anchorMax.x + .02f, barY + 0.047f);
                    enemyHealthTracks[slot].rectTransform.sizeDelta = Vector2.zero;
                    enemyHealth[slot].rectTransform.sizeDelta = Vector2.zero;
                }
                float ratio = Mathf.Clamp01(enemy.Health / enemyMaximumHealth[slot]);
                RectTransform track = enemyHealthTracks[slot].rectTransform;
                enemyHealthFills[slot].rectTransform.anchorMin = track.anchorMin;
                enemyHealthFills[slot].rectTransform.anchorMax = new Vector2(track.anchorMin.x + (track.anchorMax.x - track.anchorMin.x) * ratio, track.anchorMax.y);
                enemyHealthFills[slot].rectTransform.sizeDelta = Vector2.zero;
                if (enemy.IsBoss && showBossBar)
                {
                    enemyHealth[slot].gameObject.SetActive(false);
                    enemyHealthTracks[slot].gameObject.SetActive(false);
                    enemyHealthFills[slot].gameObject.SetActive(false);
                }
            }
        }

        // Mantém as barras junto das criaturas durante a entrada.
        private void UpdateEnemyEntrance()
        {
            if (enemyIds == null) return;
            for (int i = 0; i < enemyIds.Length; i++)
            {
                if (enemyIds[i] == 0) continue;
                float x = enemyVisualRoots[i].anchoredPosition.x;
                enemySprites[i].SetApproaching(x > 1f && currentTab == "MINA" && !helpPanel.gameObject.activeSelf);
                enemyHealth[i].rectTransform.anchoredPosition = new Vector2(x, 0);
                enemyHealthTracks[i].rectTransform.anchoredPosition = new Vector2(x, 0);
                enemyHealthFills[i].rectTransform.anchoredPosition = new Vector2(x, 0);
            }
        }

        private Vector2 ResolvePlayerPoint()
        {
            RectTransform unit = drillIcon.rectTransform;
            return new Vector2((unit.anchorMax.x - 0.5f) * worldPanel.rect.width,
                (unit.anchorMin.y + 0.065f - 0.5f) * worldPanel.rect.height);
        }

        private Vector2 ResolveEnemyPoint(int id)
        {
            int slot = EnemySlot(id);
            if (slot < 0) return ResolvePlayerPoint();
            RectTransform unit = enemyVisualRoots[slot];
            return new Vector2(((unit.anchorMin.x + unit.anchorMax.x) * 0.5f - 0.5f) * worldPanel.rect.width + unit.anchoredPosition.x,
                (unit.anchorMin.y + 0.05f - 0.5f) * worldPanel.rect.height + unit.anchoredPosition.y);
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
                    menuBody.text = "TREINO: escolha uma fase vencida.\n" +
                        (sim.IsActive ? "Termine a tentativa atual para treinar." : "Ouro e XP persistem. Cristais de estreia não se repetem.");
                    break;
                case "PETS":
                    menuTitle.text = "PETS DE APOIO";
                    menuBody.text = "Companheiros mecânicos em preparação.\n\nOs baús atuais concedem ouro e cristais.\nPets serão adicionados em uma próxima etapa.";
                    break;
                case "LOJA":
                    menuTitle.text = "LOJA DE SUPRIMENTOS";
                    menuBody.text = "Suprimentos da oficina\n\nOuro e cristais serão usados para reforços, visuais e itens especiais.\nCatálogo em preparação.";
                    break;
                case "MENU":
                    menuTitle.text = "MELHORIAS DA BROCA";
                    menuBody.text = "Progresso salvo automaticamente.\n\nAUTO liga e desliga os disparos básicos.\nÁgeis atacam rápido; blindados resistem aos tiros.\nO especial atravessa armadura.\nPREPARANDO: derrote o inimigo antes do disparo.\nUse MINA para voltar às melhorias.";
                    break;
                default:
                    menuTitle.text = "PERFIL DA EXPEDIÇÃO";
                    menuBody.text = "FASE DESBLOQUEADA  " + sim.Profile.UnlockedStageId.Replace('_', ' ') + "\nATQ  Nv." + sim.Profile.AttackLevel + "   •   VIDA  Nv." + sim.Profile.HealthLevel + "\n\nPROGRESSO SALVO AUTOMATICAMENTE\nNovos módulos aparecem conforme a expedição avança.";
                    break;
            }
            bool defeated = sim.Run.IsPlayerDefeated;
            bool cleared = sim.Run.IsStageCleared;
            bool upgradesVisible = currentTab == "MINA" || currentTab == "MELHORIAS";
            if (upgradeRowsRoot != null) upgradeRowsRoot.gameObject.SetActive(upgradesVisible);
            if (menuBody != null) menuBody.gameObject.SetActive(!upgradesVisible);
            if (menuTitle != null) menuTitle.gameObject.SetActive(!upgradesVisible);
            foreach (KeyValuePair<UpgradeKind, UpgradeView> entry in upgradeViews)
            {
                EconomyUpgradeRule rule = driver.Economy.Upgrade(entry.Key);
                UpgradeView view = entry.Value;
                int level = EconomyProgression.Level(sim.Profile, entry.Key);
                bool unlocked = GameplayHudModel.UpgradeUnlocked(sim.Profile, driver.Economy, entry.Key);
                bool maxed = level >= rule.MaximumLevel;
                view.Level.text = "NV. " + level;
                view.Value.text = GameplayHudModel.UpgradeValue(entry.Key, sim, driver.EquippedCharacter);
                view.Lock.gameObject.SetActive(!unlocked);
                view.Coin.gameObject.SetActive(false);
                bool purchasable = unlocked && !maxed;
                view.Price.text = !unlocked ? "NV. " + rule.UnlockLevel : maxed ? "" : GameplayHudModel.Number(rule.Cost(level));
                TextMeshProUGUI actionTitle = view.Button.transform.Find("Upgrade action title").GetComponent<TextMeshProUGUI>();
                actionTitle.text = !unlocked ? "Requer" : maxed ? "Máximo" : "Melhorar";
                // Máximo ocupa o botão inteiro; os demais estados mantêm título em cima e valor embaixo.
                actionTitle.rectTransform.anchorMin = new Vector2(.10f, maxed ? .14f : .52f);
                view.Price.rectTransform.anchorMin = new Vector2(purchasable ? .41f : .14f, .14f);
                view.Price.alignment = purchasable ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
                view.Button.interactable = driver.IsReady && GameplayHudModel.CanBuyUpgrade(sim.Profile, driver.Economy, entry.Key);
                SetReadyPulse(actionTitle.transform, view.Button.interactable);
                // Guia, seção 9: ouro insuficiente dessatura a peça e mantém o preço legível; coral fica para perigo.
                bool affordable = purchasable && sim.Profile.Gold >= rule.Cost(level);
                view.Price.color = affordable ? Cream : new Color(.86f, .89f, .92f);
                view.Value.color = unlocked ? new Color(.66f, .76f, .95f) : new Color(.48f, .55f, .64f);
                Image actionFace = view.Button.GetComponent<Image>();
                // v8: ciano para comprar (desbloqueado), ciano escurecido sem ouro e chapa escura "em preparação" quando bloqueado.
                Sprite actionSprite = purchasable ? ExtractedSprite("upgrade_button") : null;
                if (actionFace.sprite != actionSprite || actionSprite == null)
                {
                    actionFace.sprite = actionSprite;
                    actionFace.type = Image.Type.Simple;
                    actionFace.preserveAspect = false;
                }
                actionFace.color = purchasable ? (affordable ? Color.white : new Color(.58f, .64f, .70f)) : maxed ? new Color(.62f, .48f, .14f) : new Color(.07f, .12f, .17f);
            }
            if (scrollThumb != null) scrollThumb.parent.gameObject.SetActive(upgradesVisible);
            restartButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive((cleared && !driver.CanContinueCampaign || defeated && sim.Run.IsTraining) && !driver.IsSaveBlocked);
            continueButton.anchorMin = new Vector2(defeated && sim.Run.IsTraining ? .40f : .70f, .13f);
            continueButton.anchorMax = new Vector2(defeated && sim.Run.IsTraining ? .68f : .97f, .87f);
            missionReward.gameObject.SetActive(!restartButton.gameObject.activeSelf && !continueButton.gameObject.activeSelf);
            if (missionRewardIcon != null && !missionReward.gameObject.activeSelf) missionRewardIcon.gameObject.SetActive(false);
            missionLabel.rectTransform.anchorMax = new Vector2(defeated && sim.Run.IsTraining ? .39f : .67f, .78f);
            trainingButton.gameObject.SetActive(currentTab == "MODOS");
            trainingPicker.gameObject.SetActive(currentTab == "MODOS");
            menuBody.rectTransform.anchorMin = new Vector2(.08f, currentTab == "MODOS" ? .50f : .16f);
            if (trainingStage < 1 || trainingStage > sim.Profile.HighestClearedStage) trainingStage = Math.Max(1, sim.Profile.HighestClearedStage);
            trainingSelection.text = sim.Profile.HighestClearedStage < 1 ? "VENÇA A FASE 1" : GameplayHudModel.CampaignPosition(trainingStage);
            continueButton.GetComponentInChildren<TextMeshProUGUI>().text = sim.Profile.HighestClearedStage < driver.Economy.MaximumStage ? "CAMPANHA" : "TREINAR";
            trainingButton.GetComponent<Button>().interactable = driver.IsReady && !sim.IsActive && sim.Profile.HighestClearedStage > 0;
            if (cleared && currentTab == "MINA")
                menuBody.text = driver.IsSaveBlocked ? "Aguardando salvar para continuar. Seu progresso foi preservado."
                    : sim.LastClearSummary + (driver.CanContinueCampaign ? "\nPróxima fase..." : "");
            foreach (KeyValuePair<string, Image> button in tabButtonImages)
                button.Value.color = button.Key == currentTab ? new Color(.15f, .29f, .38f) : new Color(.09f, .15f, .22f);
            foreach (KeyValuePair<string, GameObject> item in navHighlights)
            {
                item.Value.SetActive(item.Key == currentTab);
                Transform navLabel = item.Value.transform.parent.Find("Tab label");
                if (navLabel != null) navLabel.GetComponent<TextMeshProUGUI>().color = item.Key == currentTab ? White : new Color(.80f, .85f, .90f);
            }
            menuTitle.rectTransform.anchorMax = new Vector2(.965f, .99f);
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
                "\nÁLBUM DE DESCOBERTAS  " + sim.Profile.DiscoveryStages.Count + "/" + DiscoveryCatalog.Entries.Count;
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
            HandleMomentEvent(e);
            if (e.Type == CombatEventType.EnemySpawned || e.Type == CombatEventType.BossSpawned) SyncEnemyVisuals();
            feedback.Handle(e);
            int enemySlot = EnemySlot(e.EntityId);
            if (enemySlot >= 0)
            {
                if (e.Type == CombatEventType.EnemyTelegraph) { enemySprites[enemySlot].PlayAttack(); enemyAnimators[enemySlot].PlayAttack(false); }
                else if (e.Type == CombatEventType.Hit) { enemySprites[enemySlot].PlayImpact(); enemyAnimators[enemySlot].PlayImpact(); }
                else if (e.Type == CombatEventType.EnemyDefeated) { enemySprites[enemySlot].PlayDeath(); enemyAnimators[enemySlot].PlayDeath(); }
            }
            switch (e.Type)
            {
                case CombatEventType.EnemySpawned: statusLabel.text = "UMA CRIATURA SURGIU!"; break;
                case CombatEventType.DiscoveryAwarded: pendingDiscovery = (int)e.Amount; break;
                case CombatEventType.BossSpawned: statusLabel.text = driver.Simulation.Run.CampaignStage % 10 == 5 ? "MINI-CHEFE E ESCOLTA À FRENTE!" : "GUARDIÃO À FRENTE!"; statusLabel.transform.DOPunchScale(new Vector3(0.18f, 0.18f, 0), 0.35f, 4, 0.65f); break;
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
                case CombatEventType.PlayerDefeated: currentTab = "MINA"; statusLabel.text = ""; OpenDefeatPopup(); break;
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
        }

        private void ApplyCharacterColors()
        {
            CharacterCombatData character = driver.EquippedCharacter;
            if (character == null) return;
            var tint = new Color(character.TintR, character.TintG, character.TintB);
            if (drillFrames != null)
            {
                drillFrames.GetComponent<Image>().color = tint;
                if (drillModules[0] == null)
                {
                    drillModules[0] = MakeIcon(drillFrames.transform, "shield", .16f, .28f, .39f, .62f, tint);
                    drillModules[1] = MakeIcon(drillFrames.transform, "bolt", .35f, .55f, .54f, .88f, tint);
                    drillModules[2] = MakeIcon(drillFrames.transform, "core", .02f, .38f, .23f, .72f, tint);
                }
                drillModules[0].gameObject.SetActive(character.UnlockStage > 0);
                drillModules[1].gameObject.SetActive(character.BasicHitCount >= 3);
                drillModules[2].gameObject.SetActive(character.SpecialTargetCount > 1 || character.InnateRicochet > 0);
                foreach (Image module in drillModules) { module.color = tint; module.raycastTarget = false; }
            }
        }

        private void BuildPowerBar(RectTransform parent)
        {
            // Linha de poderes do HUD aprovado v8: AUTO com chave liga/desliga, cinco slots quadrados
            // rebitados (ativo em ciano, bloqueado em ferro com cadeado) e o selo de FORÇA à direita.
            const float rowBottom = 0.356f, rowTop = 0.422f;
            Image autoFace = MakeIconSprite(parent, ExtractedSprite("auto_control"), .022f, rowBottom, .142f, rowTop, Color.white);
            autoFace.raycastTarget = true;
            RectTransform auto = autoFace.rectTransform;
            autoToggleButton = auto.gameObject.AddComponent<Button>();
            autoToggleButton.targetGraphic = autoFace;
            autoToggleButton.onClick.AddListener(ToggleAutoAttack);
            TextMeshProUGUI autoTitle = MakeLabel("Auto label", auto, "AUTO", 22, Ink, new Vector2(0.14f, 0.60f), new Vector2(0.86f, 0.92f), TextAlignmentOptions.Center);
            if (startFont != null) autoTitle.font = startFont;
            autoTitle.outlineWidth = 0f;
            // Capa cinza sobre a chave quando o ataque automático está desligado.
            autoSwitchFace = MakeArtPanel("Auto switch off", auto, new Color(.30f, .35f, .40f), "hud_panel", .07f, .10f, .93f, .48f).GetComponent<Image>();
            autoSwitchFace.raycastTarget = false;
            autoStateLabel = MakeLabel("Auto state", auto, "ON", 20, Cream, new Vector2(0.10f, 0.12f), new Vector2(0.56f, 0.46f), TextAlignmentOptions.Center);
            autoStateLabel.outlineWidth = .2f;
            autoStateLabel.outlineColor = Ink;

            string[] powers = { "special", "attack", "core", "shield", "bolt" };
            for (int i = 0; i < powers.Length; i++)
            {
                float left = 0.150f + i * 0.111f;
                Image frame = MakeIconSprite(parent, ExtractedSprite("power_active"), left, .3615f, left + 0.104f, .4165f, Color.white);
                RectTransform slot = frame.rectTransform;
                powerFaces[i] = frame;
                powerIcons[i] = MakeIcon(slot, powers[i], 0.22f, 0.24f, 0.78f, 0.80f, Color.white);
                MakePanel("Power state pill", slot, new Color(.02f, .05f, .08f, .88f), new Vector2(.16f, .02f), new Vector2(.84f, .26f), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
                powerLabels[i] = MakeLabel("Power state", slot, "", 16, Cream, new Vector2(0.16f, 0.02f), new Vector2(0.84f, 0.26f), TextAlignmentOptions.Center);
                powerLabels[i].outlineWidth = .25f;
                powerLabels[i].outlineColor = Ink;
                int powerIndex = i;
                MakeOverlayButton("Inspect power " + i, slot, "", 15, 0, 0, 1, 1, () => OpenPower(powerIndex));
            }

            Image force = MakeIconSprite(parent, ExtractedSprite("force_badge"), .712f, .363f, .978f, .415f, Color.white);
            TextMeshProUGUI forceTitle = MakeLabel("Force title", force.rectTransform, "FORÇA", 18, Cream, new Vector2(.43f, .52f), new Vector2(.94f, .88f), TextAlignmentOptions.Center);
            forceTitle.outlineWidth = .2f;
            forceTitle.outlineColor = Ink;
            forceValue = MakeLabel("Force value", force.rectTransform, "", 30, Gold, new Vector2(.43f, .10f), new Vector2(.94f, .56f), TextAlignmentOptions.Center);
            if (startFont != null) forceValue.font = startFont;
            forceValue.outlineWidth = .2f;
            forceValue.outlineColor = Ink;
            RefreshAutoToggle();
        }

        private void ToggleAutoAttack()
        {
            bool enabled = driver == null || driver.Simulation == null || driver.Simulation.AutoAttackEnabled;
            driver?.SetAutoAttackEnabled(!enabled);
            if (statusLabel != null && enabled) statusLabel.text = "MODO MANUAL: TOQUE NA BROCA PARA ATACAR";
            RefreshAutoToggle();
        }

        private void RefreshAutoToggle()
        {
            if (autoStateLabel == null || autoToggleButton == null) return;
            bool enabled = driver == null || driver.Simulation == null || driver.Simulation.AutoAttackEnabled;
            autoStateLabel.text = enabled ? "ON" : "OFF";
            autoStateLabel.color = Cream;
            // ON: chave verde da arte com o texto à esquerda do botão branco; OFF: capa cinza com o texto à direita.
            autoStateLabel.rectTransform.anchorMin = new Vector2(enabled ? .10f : .44f, .12f);
            autoStateLabel.rectTransform.anchorMax = new Vector2(enabled ? .56f : .90f, .46f);
            if (autoSwitchFace != null) autoSwitchFace.gameObject.SetActive(!enabled);
        }

        private RectTransform BuildUpgradeScroll(RectTransform parent)
        {
            RectTransform viewport = Region("Upgrade viewport", parent, 0.0f, 0.02f, 0.962f, 0.985f);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0.08f);
            viewportImage.raycastTarget = true;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            RectTransform content = Region("Upgrade content", viewport, 0, 1, 1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
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
                "scroll_rail_structure", 0.968f, 0.03f, 0.995f, 0.975f);
            rail.GetComponent<Image>().raycastTarget = false;
            scrollThumb = MakePanel("Scroll thumb", rail, new Color(.95f, .68f, .20f), new Vector2(0.24f, 0.70f), new Vector2(0.76f, 0.96f), Vector2.zero, Vector2.zero);
            scrollThumb.GetComponent<Image>().raycastTarget = false;

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

        private void UpdateScrollThumb()
        {
            if (scrollThumb == null || upgradeScroll == null || upgradeScroll.content == null) return;
            float height = upgradeScroll.content.rect.height;
            float ratio = height <= 0 ? 1 : Mathf.Clamp(upgradeScroll.viewport.rect.height / height, 0.12f, 1f);
            float bottom = (1f - ratio) * Mathf.Clamp01(upgradeScroll.verticalNormalizedPosition);
            scrollThumb.anchorMin = new Vector2(0.24f, bottom);
            scrollThumb.anchorMax = new Vector2(0.76f, bottom + ratio);
        }

        private void OpenPower(int index)
        {
            if (index == 0)
            {
                statusLabel.text = driver.TryActivateSpecial() ? "ESPECIAL ATIVADO" : "ESPECIAL AUTOMÁTICO — AGUARDE A RECARGA E UM ALVO";
                return;
            }
            OpenProgression();
            workshop.ShowPower(index - 1);
        }

        private void RefreshPowerBar()
        {
            IdleCombatSimulation sim = driver.Simulation;
            powerLabels[0].text = sim.Run.SpecialHitsRemaining > 0 ? "ATIVO" : sim.Run.SpecialCooldown > 0 ? GameplayHudModel.Number(Math.Ceiling(sim.Run.SpecialCooldown)) + "s" : "PRONTO";
            bool specialReady = sim.Run.SpecialCooldown <= 0 || sim.Run.SpecialHitsRemaining > 0;
            powerFaces[0].sprite = ExtractedSprite("power_active");
            powerFaces[0].color = specialReady ? Color.white : new Color(.62f, .70f, .76f);
            powerIcons[0].color = specialReady ? Color.white : new Color(.70f, .75f, .80f);
            for (int i = 1; i < powerFaces.Length; i++)
            {
                AttackPowerRule rule = driver.Economy.Powers[i - 1];
                int rank = EconomyProgression.PowerRank(sim.Profile, sim.ActiveCharacterId, rule.Id);
                bool unlocked = driver.CurrentCharacterLevel.Level >= rule.UnlockLevel;
                // Bloqueado usa a peça de ferro com cadeado já desenhado; desbloqueado usa a moldura ciano.
                Sprite face = ExtractedSprite(unlocked ? "power_active" : "power_locked");
                if (powerFaces[i].sprite != face) powerFaces[i].sprite = face;
                powerIcons[i].gameObject.SetActive(unlocked);
                powerIcons[i].sprite = SpriteFor(PowerIconNames[i - 1]);
                powerIcons[i].color = rank > 0 ? Color.white : new Color(.70f, .75f, .80f);
                powerFaces[i].color = !unlocked || rank > 0 ? Color.white : new Color(.62f, .70f, .76f);
                powerLabels[i].text = !unlocked ? "NV." + rule.UnlockLevel : rank + "/" + rule.MaximumRank;
            }
            RefreshAutoToggle();
        }

        private void BuildGameplayUpgrade(RectTransform content, UpgradeKind kind)
        {
            string title = GameplayHudModel.UpgradeTitle(kind);
            RectTransform rowForFeedback = null;
            RectTransform action = BuildUpgradeRow(content, title, GameplayHudModel.UpgradeIcon(kind), "", 0,
                () => { bool bought = driver.Simulation.IsActive ? driver.TryPurchaseUpgrade(kind) : driver.TryPurchaseUpgradeAtBase(kind);
                    statusLabel.text = bought ? title + " MELHORADO" : "REQUISITO, SALDO OU LIMITE NÃO ATENDIDO"; Refresh();
                    if (bought) PlayPurchaseFeedback(rowForFeedback, upgradeViews[kind].Level); },
                out Button button, out TextMeshProUGUI price);
            RectTransform row = (RectTransform)action.parent;
            rowForFeedback = row;
            TextMeshProUGUI value = row.Find("Upgrade value").GetComponent<TextMeshProUGUI>();
            Image face = row.GetComponent<Image>();
            Image lockIcon = MakeIcon(action, "lock", 0.02f, 0.25f, 0.16f, 0.75f, Cream);
            upgradeViews.Add(kind, new UpgradeView { Value = value, Level = upgradeLevelLabels[title], Price = price, Button = button, Lock = lockIcon, Face = face,
                Coin = action.Find("Icon gold_coin").GetComponent<Image>() });
        }

        private RectTransform BuildUpgradeRow(RectTransform content, string title, string icon, string value, int row,
            UnityEngine.Events.UnityAction action, out Button buttonComponent, out TextMeshProUGUI buttonLabel)
        {
            var rowObject = new GameObject("Upgrade row " + title, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rowObject.transform.SetParent(content, false);
            RectTransform rowRect = (RectTransform)rowObject.transform;
            var element = rowObject.GetComponent<LayoutElement>();
            // Proporção da peça aprovada upgrade_row (1457 × 226) na largura útil do painel.
            element.preferredHeight = 160f;
            element.minHeight = 160f;
            var rowImage = rowObject.GetComponent<Image>();
            rowImage.sprite = ExtractedSprite("upgrade_row");
            rowImage.type = Image.Type.Simple;
            rowImage.preserveAspect = false;
            rowImage.color = Color.white;
            rowImage.raycastTarget = false;
            // Ícone e nível dentro da moldura âmbar desenhada na peça (x 4–18%).
            MakeIcon(rowRect, icon, 0.060f, 0.38f, 0.162f, 0.88f, Color.white);
            TextMeshProUGUI levelLabel = MakeLabel("Upgrade level", rowRect, "NV. 0", 20, Cream,
                new Vector2(0.048f, 0.13f), new Vector2(0.174f, 0.40f), TextAlignmentOptions.Center);
            levelLabel.outlineWidth = 0.2f;
            levelLabel.outlineColor = Ink;
            upgradeLevelLabels[title] = levelLabel;
            TextMeshProUGUI titleLabel = MakeLabel("Upgrade title", rowRect, title, 32, Cream, new Vector2(0.205f, 0.48f), new Vector2(0.655f, 0.90f), TextAlignmentOptions.Left);
            if (startFont != null) titleLabel.font = startFont;
            titleLabel.outlineWidth = .15f;
            titleLabel.outlineColor = Ink;
            TextMeshProUGUI valueLabel = MakeLabel("Upgrade value", rowRect, value, 25, new Color(.66f, .76f, .95f), new Vector2(0.205f, 0.12f), new Vector2(0.655f, 0.50f), TextAlignmentOptions.Left);
            if (readingFont != null) valueLabel.font = readingFont;
            // Botão sobre a área ciano desenhada na peça; o estado troca a arte em RefreshUpgradeViews.
            RectTransform button = MakeArtActionButton("Upgrade action", rowRect, "Melhorar", 24, 0.672f, 0.15f, 0.970f, 0.86f, action, "hud_panel");
            button.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
            Image buttonFace = button.GetComponent<Image>();
            buttonFace.sprite = ExtractedSprite("upgrade_button");
            buttonFace.type = Image.Type.Simple;
            buttonFace.color = Color.white;
            // Estado desabilitado opaco: sem isso o Unity desenha a 50% e a área ciano da linha aparece por baixo.
            Button upgradeButton = button.GetComponent<Button>();
            ColorBlock upgradeColors = upgradeButton.colors;
            upgradeColors.disabledColor = Color.white;
            upgradeButton.colors = upgradeColors;
            AddEdge(button, new Color(.30f, .42f, .54f), 2f);
            TextMeshProUGUI actionTitle = MakeLabel("Upgrade action title", button, "Melhorar", 25, Cream, new Vector2(.10f, .52f), new Vector2(.90f, .90f), TextAlignmentOptions.Center);
            if (startFont != null) actionTitle.font = startFont;
            actionTitle.outlineWidth = .2f;
            actionTitle.outlineColor = Ink;
            // A moeda já vem desenhada na peça upgrade_button; o ícone separado fica só como reserva.
            Image coin = MakeIcon(button, "gold_coin", .21f, .22f, .38f, .56f, Color.white);
            coin.gameObject.SetActive(false);
            TextMeshProUGUI costLabel = MakeLabel("Upgrade cost", button, "", 28, Cream, new Vector2(.41f, .14f), new Vector2(.88f, .54f), TextAlignmentOptions.Left);
            if (startFont != null) costLabel.font = startFont;
            costLabel.outlineWidth = .2f;
            costLabel.outlineColor = Ink;
            buttonComponent = button.GetComponent<Button>();
            buttonLabel = costLabel;
            return button;
        }

        private void BuildLockedUpgradeRow(RectTransform content, string title, string icon, string value, string requirement)
        {
            var rowObject = new GameObject("Locked upgrade row " + title, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rowObject.transform.SetParent(content, false);
            RectTransform rowRect = (RectTransform)rowObject.transform;
            var element = rowObject.GetComponent<LayoutElement>();
            element.preferredHeight = 156f;
            element.minHeight = 156f;
            var rowImage = rowObject.GetComponent<Image>();
            rowImage.sprite = SpriteFor("upgrade_row");
            rowImage.type = Image.Type.Simple;
            rowImage.color = Color.white;
            rowImage.raycastTarget = false;
            RectTransform iconFrame = Region("Locked icon", rowRect, 0.012f, 0.08f, 0.17f, 0.92f);
            MakeIcon(iconFrame, icon, 0.16f, 0.35f, 0.84f, 0.88f, Dim);
            TextMeshProUGUI levelLabel = MakeLabel("Locked level", iconFrame, "NV. 0", 13, Dim,
                new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.32f), TextAlignmentOptions.Center);
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
            if (art=="hud_panel" && color==Color.white) image.color=new Color(.10f,.18f,.27f);
            if (art=="power_round") image.preserveAspect=true;
            image.type = art.StartsWith("skin_") && art!="skin_close" && art!="skin_avatar" ? Image.Type.Sliced : uiAssetSprites.ContainsKey(art) ? Image.Type.Simple : Image.Type.Sliced;
            if(art=="skin_close" || art=="skin_avatar" || art=="skin_tab" || art=="skin_tab_active") { image.preserveAspect=true; image.type=Image.Type.Simple; }
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
            // v8: cada aba é uma célula escura própria; a ativa clareia e ganha barra ciano embaixo e em cima.
            RectTransform rect = MakeArtPanel(name,parent,new Color(.09f,.15f,.22f),"hud_panel",left,bottom,right,top);
            Button button = rect.gameObject.AddComponent<Button>();
            Image image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            AddEdge(rect, new Color(.02f, .04f, .06f), 2f);
            RectTransform highlight = Region("Active nav glow", rect, 0f, 0f, 1f, 1f);
            MakePanel("Active nav bar", highlight, Cyan, new Vector2(.06f, .0f), new Vector2(.94f, .045f), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
            MakePanel("Active nav top", highlight, new Color(0f, .96f, .83f, .55f), new Vector2(.0f, .975f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
            navHighlights[text] = highlight.gameObject;
            highlight.gameObject.SetActive(stateColor == Cyan);
            // Picareta ilustrada do v8; as demais peças nav_* extraídas são de outra família, então seguem os ícones do atlas.
            Sprite illustrated = icon == "nav_mine" ? Resources.Load<Sprite>("Art/UI/Extracted/nav_mine") : null;
            Image navIcon = illustrated != null ? MakeIconSprite(rect, illustrated, .17f, .34f, .83f, .97f, Color.white) : MakeIcon(rect, icon, .22f, .38f, .78f, .93f, Color.white);
            TextMeshProUGUI label = MakeLabel("Tab label", rect, text, 22, Cream,
                new Vector2(.03f, .06f), new Vector2(.97f, .34f), TextAlignmentOptions.Center);
            label.outlineWidth = .18f;
            label.outlineColor = Ink;
            return rect;
        }

        private Sprite SpriteFor(string name)
        {
            if (uiSprites.TryGetValue(name, out Sprite cachedIcon)) return cachedIcon;
            Sprite dedicatedIcon = Resources.Load<Sprite>("Art/UI/Icons/" + name);
            if (dedicatedIcon != null)
            {
                uiSprites[name] = dedicatedIcon;
                return dedicatedIcon;
            }
            if (name == "gold_coin")
            {
                Sprite coin = Resources.Load<Sprite>("Art/UI/Extracted/gold_coin");
                if (coin != null) return coin;
            }
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
                "wallet_panel", "crystal_wallet", "menu_button", "stage_plate", "mission_banner", "upgrade_button", "gold_coin"
            }, new[]
            {
                new RectInt(12, 579, 558, 185),
                new RectInt(610, 573, 530, 195),
                new RectInt(1187, 557, 315, 226),
                new RectInt(8, 232, 624, 185),
                new RectInt(644, 239, 494, 156),
                new RectInt(1165, 223, 358, 197),
                new RectInt(18, 579, 165, 175)
            });
            SliceUiAtlas("Art/UI/dw_hud_combat_atlas", new[]
            {
                "mission_banner_combat", "auto_control", "power_active", "power_locked", "force_badge", "upgrade_row"
            }, new[]
            {
                new RectInt(36, 709, 698, 151),
                new RectInt(762, 627, 333, 324),
                new RectInt(1125, 608, 375, 352),
                new RectInt(178, 324, 330, 307),
                new RectInt(613, 349, 590, 206),
                new RectInt(40, 58, 1457, 226)
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
            Texture2D structure = Resources.Load<Texture2D>("Art/UI/dw_hud_structure_atlas");
            if (structure != null)
                uiAssetSprites["plain_action"] = Sprite.Create(structure, new Rect(1280, 604, 210, 110), new Vector2(.5f, .5f), 100);
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
            if (name == "nav_album")
            {
                sprite = Resources.Load<Sprite>("Art/UI/Icons/nav_album");
                if (sprite != null) navIconSprites[name] = sprite;
                return sprite;
            }
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
            drillFrames?.StopWalk();
        }

        private TextMeshProUGUI MakeLabel(string name, Transform parent, string text, int size, Color color, Vector2 min, Vector2 max, TextAlignmentOptions alignment)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var label = rect.gameObject.GetComponent<TextMeshProUGUI>();
            label.text = text; label.fontSize = size; label.color = color; label.alignment = alignment;
            label.enableAutoSizing = true; label.fontSizeMin = size * 0.65f; label.fontSizeMax = size;
            if (hudFont != null) label.font = hudFont;
            label.textWrappingMode = TextWrappingModes.Normal; label.raycastTarget = false;
            return label;
        }

        private RectTransform MakeButton(string name, Transform parent, string text, int size, float left, float bottom, float right, float top, UnityEngine.Events.UnityAction action, Color color)
        {
            var rect = MakePanel(name, parent, color, new Vector2(left, bottom), new Vector2(right, top), Vector2.zero, Vector2.zero);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.targetGraphic.raycastTarget = true;
            button.onClick.AddListener(action);
            var label = MakeLabel("Label", rect, text, size, Ink, Vector2.zero, new Vector2(1, 1), TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            return rect;
        }

        private RectTransform MakeArtActionButton(string name, Transform parent, string text, int size, float left, float bottom,
            float right, float top, UnityEngine.Events.UnityAction action, string art)
        {
            if (art=="slot" || art=="hud_panel") art="skin_action";
            if(name=="Close top menu")art="skin_close";
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
