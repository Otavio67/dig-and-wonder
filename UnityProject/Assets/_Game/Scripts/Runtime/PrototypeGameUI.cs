using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>HUD de protótipo: contraste alto, retrato, toque e mouse, sem assets externos.</summary>
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
        private TextMeshProUGUI statusLabel;
        private TextMeshProUGUI healthLabel;
        private TextMeshProUGUI combatLog;
        private TextMeshProUGUI menuTitle;
        private TextMeshProUGUI menuBody;
        private TextMeshProUGUI startErrorLabel;
        private RectTransform attackUpgradeButton;
        private RectTransform healthUpgradeButton;
        private RectTransform restartButton;
        private RectTransform continueButton;
        private PooledCombatFeedback feedback;
        private Image healthFill;
        private TextMeshProUGUI drillIcon;
        private ProceduralAnimator2D drillAnimator;
        private GroundDrillMotion2D drillMotion;
        private TextMeshProUGUI[] enemyIcons;
        private ProceduralAnimator2D[] enemyAnimators;
        private RectTransform[] caveSeams;
        private Tween[] caveMotionTweens;
        private TextMeshProUGUI[] enemyHealth;
        private RectTransform menuPanel;
        private RectTransform startPanel;
        private RectTransform worldPanel;
        private RectTransform bottomBar;
        private bool inGame;
        private bool saveWasBlocked;
        private string currentTab = "MELHORIAS";
        private float refreshTimer;
        private int displayedGold = -1;
        private readonly Dictionary<string, Image> tabButtonImages = new Dictionary<string, Image>();

        public void Initialize(Transform canvasRoot, AutoBattleDriver battleDriver)
        {
            root = canvasRoot;
            driver = battleDriver;
            driver.CombatEventRaised += OnCombatEvent;
            BuildInterface();
            feedback = gameObject.AddComponent<PooledCombatFeedback>();
            feedback.Initialize(worldPanel);
            if (driver.Simulation != null)
                EnterGame(true);
            else
                ShowStart();
        }

        private void OnDestroy()
        {
            if (driver != null) driver.CombatEventRaised -= OnCombatEvent;
            StopCaveMotion();
        }

        private void BuildInterface()
        {
            var background = MakePanel("Background", root, Ink, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            MakeLabel("Title", background, "DIG & WONDER", 40, Sand, new Vector2(0.04f, 0.91f), new Vector2(0.7f, 0.975f), TextAlignmentOptions.Left);
            stageLabel = MakeLabel("Stage", background, "CAVERNA", 25, White, new Vector2(0.04f, 0.865f), new Vector2(0.68f, 0.91f), TextAlignmentOptions.Left);
            walletLabel = MakeLabel("Wallet", background, "◉ 0", 30, Sand, new Vector2(0.68f, 0.905f), new Vector2(0.96f, 0.97f), TextAlignmentOptions.Right);

            worldPanel = MakePanel("Cave view", background, Deep, new Vector2(0.04f, 0.325f), new Vector2(0.96f, 0.85f), Vector2.zero, Vector2.zero);
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
            drillAnimator = drillIcon.gameObject.AddComponent<ProceduralAnimator2D>();
            drillMotion = drillIcon.gameObject.AddComponent<GroundDrillMotion2D>();
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
            statusLabel = MakeLabel("Status", background, "", 20, Mint,
                new Vector2(0.04f, 0.275f), new Vector2(0.96f, 0.32f), TextAlignmentOptions.Center);
            combatLog = MakeLabel("Combat log", background, "", 18, White,
                new Vector2(0.06f, 0.235f), new Vector2(0.94f, 0.275f), TextAlignmentOptions.Center);

            bottomBar = MakePanel("Bottom navigation", background, new Color(0.04f, 0.13f, 0.16f), new Vector2(0.02f, 0.025f), new Vector2(0.98f, 0.125f), Vector2.zero, Vector2.zero);
            string[] tabs = { "MINA", "MELHORIAS", "BROCAS", "BAÚS", "PERFIL" };
            for (int i = 0; i < tabs.Length; i++)
            {
                string tab = tabs[i];
                float left = i / 5f;
                RectTransform tabButton = MakeButton("Tab " + tab, bottomBar, tab, 18, left + 0.006f, 0.04f, left + 0.194f, 0.96f,
                    () => SelectTab(tab), tab == currentTab ? Sand : White);
                tabButtonImages.Add(tab, tabButton.GetComponent<Image>());
            }

            menuPanel = MakePanel("Tab panel", background, Panel, new Vector2(0.06f, 0.135f), new Vector2(0.94f, 0.285f), Vector2.zero, Vector2.zero);
            menuTitle = MakeLabel("Tab title", menuPanel, "MELHORIAS", 24, Sand, new Vector2(0.03f, 0.72f), new Vector2(0.97f, 0.98f), TextAlignmentOptions.Center);
            menuBody = MakeLabel("Tab info", menuPanel, "", 18, White, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.72f), TextAlignmentOptions.Center);
            attackUpgradeButton = MakeButton("Upgrade attack", menuPanel, "DANO +", 17, 0.04f, 0.03f, 0.47f, 0.28f,
                () => { if (!driver.TryPurchaseUpgrade(UpgradeKind.Attack)) statusLabel.text = "OURO INSUFICIENTE OU NÍVEL MÁXIMO"; Refresh(); }, Mint).GetComponent<RectTransform>();
            healthUpgradeButton = MakeButton("Upgrade health", menuPanel, "VIDA +", 17, 0.53f, 0.03f, 0.96f, 0.28f,
                () => { if (!driver.TryPurchaseUpgrade(UpgradeKind.Health)) statusLabel.text = "OURO INSUFICIENTE OU NÍVEL MÁXIMO"; Refresh(); }, Sand).GetComponent<RectTransform>();
            restartButton = MakeButton("Restart run", menuPanel, "RECOMEÇAR", 17, 0.25f, 0.03f, 0.75f, 0.28f,
                () => { driver.StartNewGame(); EnterGame(false); }, Coral).GetComponent<RectTransform>();
            restartButton.gameObject.SetActive(false);
            continueButton = MakeButton("Continue stage", menuPanel, "AVANÇAR FASE", 17, 0.2f, 0.03f, 0.8f, 0.28f,
                () => { driver.StartNewGame(); EnterGame(false); }, Mint).GetComponent<RectTransform>();
            continueButton.gameObject.SetActive(false);
            startPanel = MakePanel("Start panel", background, Panel, new Vector2(0.1f, 0.37f), new Vector2(0.9f, 0.71f), Vector2.zero, Vector2.zero);
            MakeLabel("Welcome", startPanel, "A CAVERNA ESPERA", 34, Sand, new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.95f), TextAlignmentOptions.Center);
            MakeLabel("Intro", startPanel, "Sua broca avança sozinha.\nDerrote criaturas, ganhe ouro\ne prepare-se para o guardião.", 22, White,
                new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.68f), TextAlignmentOptions.Center);
            startErrorLabel = MakeLabel("Start error", startPanel, "", 16, Coral, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.34f), TextAlignmentOptions.Center);
            MakeButton("Start", startPanel, "INICIAR ESCAVAÇÃO", 26, 0.08f, 0.04f, 0.92f, 0.27f,
                () =>
                {
                    if (!driver.TryStartNewGame())
                    {
                        startErrorLabel.text = "NÃO FOI POSSÍVEL CARREGAR DADOS/SAVE. O ARQUIVO FOI PRESERVADO.";
                        return;
                    }
                    EnterGame(false);
                }, Mint);
            menuPanel.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
            worldPanel.gameObject.SetActive(false);
            stageLabel.gameObject.SetActive(false);
            walletLabel.gameObject.SetActive(false);
        }

        private void ShowStart()
        {
            inGame = false;
            startPanel.gameObject.SetActive(true);
            worldPanel.gameObject.SetActive(false);
            menuPanel.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
            stageLabel.gameObject.SetActive(false);
            walletLabel.gameObject.SetActive(false);
            statusLabel.gameObject.SetActive(false);
            combatLog.gameObject.SetActive(false);
        }

        private void EnterGame(bool resumed)
        {
            if (driver.Simulation == null) return;
            inGame = true;
            startPanel.gameObject.SetActive(false);
            worldPanel.gameObject.SetActive(true);
            menuPanel.gameObject.SetActive(true);
            bottomBar.gameObject.SetActive(true);
            stageLabel.gameObject.SetActive(true);
            walletLabel.gameObject.SetActive(true);
            statusLabel.gameObject.SetActive(true);
            combatLog.gameObject.SetActive(true);
            if (resumed) statusLabel.text = "PROGRESSO RECUPERADO — CONTINUANDO DA ÚLTIMA POSIÇÃO";
            else statusLabel.text = "A broca começou a avançar!";
            drillAnimator.ResetForRun();
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
                    menuBody.text = sim.Run.IsPlayerDefeated ? "A escavação terminou. Toque para recomeçar." : "A broca avança e luta automaticamente.\nO guardião é a única condição de avanço.";
                    break;
                case "MELHORIAS":
                    menuBody.text = "DANO  Nv." + sim.Profile.AttackLevel + "   •   " + (attackCost < 0 ? "MÁX." : attackCost + " ouro") + "\nVIDA  Nv." + sim.Profile.HealthLevel + "   •   " + (healthCost < 0 ? "MÁX." : healthCost + " ouro");
                    break;
                case "BROCAS":
                    menuBody.text = "BROCA PIONEIRA  •  COMUM\nEspecial: investida em quatro impactos.\nBrocas alternativas entram em marcos futuros.";
                    break;
                case "BAÚS":
                    menuBody.text = "Baús guardados: " + sim.Profile.Chests + "   •   Chaves: " + sim.Profile.Keys + "\nO guardião entrega baús e chaves da fase.";
                    break;
                default:
                    menuBody.text = "PERFIL DA EXPEDIÇÃO\nFase: " + sim.Profile.UnlockedStageId.Replace('_', ' ') + "\nO progresso é salvo automaticamente neste aparelho.";
                    break;
            }
            bool defeated = sim.Run.IsPlayerDefeated;
            bool cleared = sim.Run.IsStageCleared;
            bool upgradesVisible = currentTab == "MELHORIAS" && !defeated && !cleared && !driver.IsSaveBlocked;
            attackUpgradeButton.gameObject.SetActive(upgradesVisible);
            healthUpgradeButton.gameObject.SetActive(upgradesVisible);
            restartButton.gameObject.SetActive(defeated && !driver.IsSaveBlocked);
            continueButton.gameObject.SetActive(cleared && !driver.IsSaveBlocked);
            foreach (KeyValuePair<string, Image> button in tabButtonImages)
                button.Value.color = button.Key == currentTab ? Sand : new Color(0.14f, 0.34f, 0.36f);
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
            }
            else if (e.Type == CombatEventType.SpecialStarted)
            {
                drillAnimator.PlayAttack(true);
                drillMotion.PlayAttack(true);
            }
            else if (e.Type == CombatEventType.PlayerDamaged)
            {
                drillAnimator.PlayImpact();
                drillMotion.PlayImpact();
            }
            else if (e.Type == CombatEventType.PlayerDefeated)
            {
                drillAnimator.PlayDeath();
                drillMotion.PlayDeath();
            }
            else if (e.Type == CombatEventType.RunStarted) drillAnimator.ResetForRun();
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
