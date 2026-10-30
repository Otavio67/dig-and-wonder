using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace DigAndWonder
{
    /// <summary>
    /// Apresentação da personagem na oficina. Consulta dados e encaminha compras ao driver;
    /// não controla o combate nem cria atributos, XP ou equipamentos no perfil.
    /// </summary>
    public sealed partial class CharacterWorkshopView : MonoBehaviour
    {
        private static readonly Color Petroleum = new Color(27f / 255, 38f / 255, 59f / 255);
        private static readonly Color Teal = new Color(30f / 255, 111f / 255, 92f / 255);
        private static readonly Color Gold = new Color(1f, 183f / 255, 3f / 255);
        private static readonly Color Cyan = new Color(0f, 245f / 255, 212f / 255);
        private static readonly Color Coral = new Color(1f, 89f / 255, 94f / 255);
        private static readonly Color Cream = new Color(1f, 248f / 255, 231f / 255);
        private static readonly Color Steel = new Color(0.33f, 0.44f, 0.51f);
        private static readonly Color Dim = new Color(0.66f, 0.74f, 0.76f);
        private static readonly CultureInfo Portuguese = CultureInfo.GetCultureInfo("pt-BR");

        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private readonly List<TMP_FontAsset> ownedFonts = new List<TMP_FontAsset>();
        private readonly RectTransform[] panes = new RectTransform[5];
        private readonly Image[] tabFaces = new Image[5];
        private readonly RectTransform[] tabVisuals = new RectTransform[5];
        private AutoBattleDriver driver;
        private CombatBalanceDefinition balance;
        private RectTransform root;
        private TMP_FontAsset titleFont;
        private TMP_FontAsset bodyFont;
        private TextMeshProUGUI wallet;
        private TextMeshProUGUI experienceLabel;
        private TextMeshProUGUI characterName;
        private TextMeshProUGUI attackValue;
        private TextMeshProUGUI healthValue;
        private TextMeshProUGUI attackPrice;
        private TextMeshProUGUI healthPrice;
        private TextMeshProUGUI status;
        private TextMeshProUGUI lockSummary;
        private Image experienceFill;
        private Image attackFace;
        private Image healthFace;
        private Image attackIcon, healthIcon;
        private TextMeshProUGUI sectionTitle;
        private readonly Image[] treasureFaces = new Image[3];
        private Button attackButton;
        private Button healthButton;
        private int selectedTab;
        private bool initialized;
        private bool dirty;
        private bool lastReady;
        private int upgradePage;
        private int powerPage;
        private readonly TextMeshProUGUI[] powerValues = new TextMeshProUGUI[2];
        private readonly TextMeshProUGUI[] powerPrices = new TextMeshProUGUI[2];
        private readonly Button[] powerButtons = new Button[2];
        private readonly Image[] powerFaces = new Image[2];
        private readonly Button[] treasureButtons = new Button[3];
        private readonly TextMeshProUGUI[] treasurePrices = new TextMeshProUGUI[3];
        private TextMeshProUGUI upgradePageLabel, powerPageLabel, treasureSummary;
        private TextMeshProUGUI moduleValue;
        private readonly Image[] passiveFaces = new Image[3];
        private readonly Image[] passiveLocks = new Image[3];
        private Image showcaseImage;
        private TextMeshProUGUI showcaseFallback;
        private DrillSpriteSheetPlayer2D showcaseFrames;
        private Image garageImage;
        private int garageIndex;
        private TextMeshProUGUI garagePage, garageDetails, garagePrice;
        private Button garageEquip;

        public void Initialize(RectTransform parent, AutoBattleDriver battleDriver, Action returnToHub)
        {
            if (initialized) return;
            initialized = true;
            root = parent;
            driver = battleDriver;
            balance = Resources.Load<CombatBalanceDefinition>("Data/CombatBalance");
            titleFont = LoadFont("TitanOne-Regular", 0.17f);
            bodyFont = LoadFont("DWRounded-ExtraBold", 0.055f);
            root.GetComponent<Image>().color = Petroleum;
            Build(returnToHub);
            driver.CombatEventRaised += OnCombatEvent;
            Refresh();
        }

        private TMP_FontAsset LoadFont(string name, float outline)
        {
            Font source = Resources.Load<Font>("Fonts/Workshop/" + name);
            if (source == null) return null; // TMP conserva sua fonte padrão se a importação estiver pendente.
            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source, 64, 8, GlyphRenderMode.SDFAA, 1024, 1024);
            if (font == null) return null;
            font.material.SetFloat("_OutlineWidth", outline);
            font.material.SetColor("_OutlineColor", new Color(0.035f, 0.06f, 0.085f));
            font.material.EnableKeyword("OUTLINE_ON");
            if (outline > 0.1f)
            {
                font.material.EnableKeyword("UNDERLAY_ON");
                font.material.SetColor("_UnderlayColor", new Color(0.025f, 0.04f, 0.06f));
                font.material.SetFloat("_UnderlayOffsetY", -0.3f);
                font.material.SetFloat("_UnderlaySoftness", 0f);
            }
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ÁÀÂÃÉÊÍÓÔÕÚÜÇáàâãéêíóôõúüç.,:/+%!?—•-()");
            ownedFonts.Add(font);
            return font;
        }

        private void Build(Action returnToHub)
        {
            // Todas as regiões usam a área segura já calculada pelo bootstrap (1080 x 1920 de referência).
            RectTransform header = Plate("Workshop sign", root, Steel, 0.04f, 0.925f, 0.82f, 0.986f);
            sectionTitle = Label(header, "OFICINA", 64, Gold, 0.08f, 0.12f, 0.92f, 0.92f, true);
            ButtonAt(root, "Close workshop", "", Coral, 0.84f, 0.91f, 0.98f, 0.99f, returnToHub,
                out TextMeshProUGUI ignoredCloseLabel, out Image closeFace);
            Icon(closeFace.transform, "close", 0.2f, 0.15f, 0.8f, 0.85f);
            characterName = Label(root, "BROCA PIONEIRA", 32, Cream, 0.045f, 0.877f, 0.63f, 0.910f, true, TextAlignmentOptions.Left);
            wallet = Label(root, "0", 23, Gold, 0.76f, 0.877f, 0.96f, 0.915f, true, TextAlignmentOptions.Right);

            BuildShowcase();
            RectTransform experienceTrack = Plate("Character XP track", root, Teal, 0.04f, 0.561f, 0.96f, 0.602f);
            RectTransform track = Plate("Energy tube", experienceTrack, Petroleum, 0.04f, 0.25f, 0.96f, 0.78f);
            experienceFill = Plate("Experience fill", track, Cyan, 0f, 0f, 0f, 1f).GetComponent<Image>();
            experienceLabel = Label(experienceTrack, "NÍVEL 0", 30, Cream, 0.07f, 0.03f, 0.93f, 0.98f, true);

            BuildModules();
            for (int i = 0; i < panes.Length; i++)
                panes[i] = Region("Workshop tab " + i, root, 0.04f, 0.218f, 0.96f, 0.464f);
            BuildUpgradeRows();
            BuildPowerRows();
            BuildTreasury();
            BuildGarage();
            BuildArsenalGrid();
            lockSummary = Label(root, "Novas melhorias em preparação", 22, Dim, 0.05f, 0.193f, 0.95f, 0.217f, false);
            status = Label(root, "Escolha uma melhoria para turbinar a broca.", 26, Cream,
                0.055f, 0.12f, 0.61f, 0.19f, false, TextAlignmentOptions.Left);
            ButtonAt(root, "Return from workshop", "VOLTAR", Cyan, 0.64f, 0.116f, 0.96f, 0.191f, returnToHub,
                out TextMeshProUGUI ignoredBackLabel, out Image ignoredBackFace);
            BuildNavigation();
        }

        private void BuildShowcase()
        {
            RectTransform chamber = Plate("Character showcase", root, new Color(.10f, .21f, .27f), 0.035f, 0.607f, 0.965f, 0.875f);
            Label(chamber, "SISTEMAS DA BROCA", 21, Dim, .25f, .88f, .75f, .98f, false);
            // O pedestal e a broca são independentes; não se anima a posição lógica da unidade.
            Icon(chamber, "pedestal", 0.195f, 0.025f, 0.805f, 0.36f);
            TextMeshProUGUI fallback = Label(chamber, "PIONEIRA", 22, Cyan, 0.29f, 0.28f, 0.71f, 0.79f, true);
            showcaseFallback = fallback;
            RectTransform drill = Region("Drill Visual", fallback.transform, -0.19f, -0.2f, 1.19f, 1.2f);
            showcaseImage = drill.gameObject.AddComponent<Image>();
            showcaseFrames = drill.gameObject.AddComponent<DrillSpriteSheetPlayer2D>();
            // Idle discreto. O pedestal permanece fixo para transmitir contato com o chão.
            drill.gameObject.AddComponent<ProceduralAnimator2D>();
            Slot(chamber, "BROCA", "hud_drill", 0.015f, 0.53f, 0.22f, 0.97f,
                "Ponta da Pioneira. A troca de peças estará disponível em uma próxima atualização.");
            Slot(chamber, "CHASSI", "chassis", 0.015f, 0.06f, 0.22f, 0.50f,
                "Chassi da Pioneira. A vida máxima já pode ser melhorada na aba Oficina.");
            Slot(chamber, "MOTOR", "motor", 0.78f, 0.53f, 0.985f, 0.97f,
                "Motor da Pioneira. Novos motores ainda estão em preparação.");
            Slot(chamber, "NÚCLEO", "core", 0.78f, 0.06f, 0.985f, 0.50f,
                "Núcleo de cristal. A troca de núcleos ainda está em preparação.");
        }

        private void Slot(RectTransform parent, string title, string icon, float x0, float y0, float x1, float y1, string message)
        {
            RectTransform region = Region(title + " slot", parent, x0, y0, x1, y1);
            Label(region, title, 26, Cream, 0f, 0.78f, 1f, 1f, true);
            Image frame = Icon(region, "slot", 0.04f, 0.04f, 0.96f, 0.79f);
            frame.color = new Color(.44f, .64f, .65f);
            Icon(region, icon, 0.09f, 0.11f, 0.91f, 0.74f);
            Icon(region, "lock", 0.75f, 0.05f, 0.94f, 0.24f);
            frame.raycastTarget = true;
            Button button = region.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;
            button.onClick.AddListener(() => status.text = message);
        }

        private void BuildModules()
        {
            RectTransform cartridge = Plate("Combat cartridge", root, Steel, 0.04f, 0.473f, 0.96f, 0.553f);
            Label(cartridge, "MÓDULO DE COMBATE", 22, Cream, 0.035f, 0.7f, 0.56f, 0.96f, true, TextAlignmentOptions.Left);
            Label(cartridge, "PASSIVOS", 22, Cream, 0.62f, 0.7f, 0.965f, 0.96f, true);
            RectTransform module = Plate("Pioneer module", cartridge, Teal, 0.025f, 0.10f, 0.16f, 0.68f);
            Icon(module, "special", 0.12f, 0.07f, 0.88f, 0.93f);
            string specialText = balance != null && balance.specialAbility != null
                ? balance.specialAbility.hitCount + " impactos\n" + Number(balance.specialAbility.damage) + " de dano base"
                : "Pioneira\nDados indisponíveis";
            moduleValue = Label(cartridge, specialText, 28, Cream, 0.18f, 0.12f, 0.58f, 0.69f, false, TextAlignmentOptions.Left);
            string[] passiveIcons = { "shield", "repair", "bolt" };
            for (int i = 0; i < passiveIcons.Length; i++)
            {
                float left = 0.63f + i * 0.112f;
                RectTransform passive = Plate("Locked passive " + i, cartridge, Petroleum, left, 0.1f, left + 0.1f, 0.68f);
                Image art = Icon(passive, passiveIcons[i], 0.05f, 0.05f, 0.95f, 0.95f);
                art.color = Dim;
                passiveFaces[i] = art;
                passiveLocks[i] = Icon(passive, "lock", 0.53f, -0.02f, 1.02f, 0.47f);
            }
        }

        private void BuildUpgradeRows()
        {
            BuildPager(panes[4], () => { upgradePage = (upgradePage + 3) % 4; Refresh(); },
                () => { upgradePage = (upgradePage + 1) % 4; Refresh(); }, out upgradePageLabel);
            RectTransform attack = Plate("Attack row", panes[4], Steel, 0f, 0.37f, 1f, 0.73f);
            attackIcon = Icon(attack, "attack", 0.025f, 0.13f, 0.17f, 0.87f);
            attackValue = Label(attack, "ATQ", 27, Cream, 0.19f, 0.04f, 0.57f, 0.96f, false, TextAlignmentOptions.Left);
            attackButton = ButtonAt(attack, "Upgrade attack", "TURBINAR", Gold, 0.59f, 0.16f, 0.97f, 0.87f,
                () => Purchase(VisibleUpgrade(0).Kind), out attackPrice, out attackFace);
            RectTransform health = Plate("Health row", panes[4], Steel, 0f, 0.005f, 1f, 0.365f);
            healthIcon = Icon(health, "health", 0.025f, 0.13f, 0.17f, 0.87f);
            healthValue = Label(health, "VIDA", 27, Cream, 0.19f, 0.04f, 0.57f, 0.96f, false, TextAlignmentOptions.Left);
            healthButton = ButtonAt(health, "Upgrade health", "TURBINAR", Gold, 0.59f, 0.16f, 0.97f, 0.87f,
                () => { if (VisibleUpgrade(1) != null) Purchase(VisibleUpgrade(1).Kind); }, out healthPrice, out healthFace);
            attackPrice.fontSize = healthPrice.fontSize = 27;
        }

        private void BuildPager(RectTransform parent, Action previous, Action next, out TextMeshProUGUI label)
        {
            ButtonAt(parent, "Previous page", "<", Teal, 0, 0.74f, 0.18f, 0.995f, previous, out TextMeshProUGUI ignored, out Image face);
            label = Label(parent, "1 / 4", 28, Cream, 0.19f, 0.74f, 0.81f, 1f, true);
            ButtonAt(parent, "Next page", ">", Teal, 0.82f, 0.74f, 1, 0.995f, next, out ignored, out face);
        }

        private EconomyUpgradeRule VisibleUpgrade(int row)
        {
            int index = upgradePage * 2 + row;
            return driver.Economy != null && index < driver.Economy.Upgrades.Length ? driver.Economy.Upgrades[index] : null;
        }

        private void BuildPowerRows()
        {
            BuildPager(panes[1], () => { powerPage = 1 - powerPage; Refresh(); },
                () => { powerPage = 1 - powerPage; Refresh(); }, out powerPageLabel);
            for (int row = 0; row < 2; row++)
            {
                int index = row;
                float bottom = row == 0 ? 0.37f : 0.005f;
                RectTransform panel = Plate("Power row " + row, panes[1], Steel, 0, bottom, 1, bottom + 0.36f);
                powerValues[row] = Label(panel, "PODER", 27, Cream, 0.035f, 0.04f, 0.60f, 0.96f, false, TextAlignmentOptions.Left);
                powerButtons[row] = ButtonAt(panel, "Buy power " + row, "APRENDER", Gold, 0.63f, 0.12f, 0.97f, 0.88f,
                    () => BuyPower(index), out powerPrices[row], out powerFaces[row]);
                powerPrices[row].fontSize = 27;
            }
        }

        private void BuyPower(int row)
        {
            bool bought = driver.TryPurchasePower(driver.Economy.Powers[powerPage * 2 + row].Id);
            Refresh();
            status.text = driver.IsSaveBlocked ? driver.LastSaveError : bought ? "Poder da broca melhorado e salvo!" : "Requer nível, ouro ou rank disponível.";
        }

        private void BuildTreasury()
        {
            RectTransform plate = Plate("Treasury", panes[2], Steel, 0, 0.01f, 1, 1);
            treasureSummary = Label(plate, "TESOURO", 27, Cream, 0.035f, 0.51f, 0.965f, 0.98f, false);
            Action[] actions = { OpenChest, BuyKey, BuyGold };
            for (int i = 0; i < 3; i++)
            {
                float left = 0.025f + i * 0.325f;
                treasureButtons[i] = ButtonAt(plate, "Treasury action " + i, "", i == 0 ? Gold : Teal,
                    left, 0.05f, left + 0.3f, 0.46f, actions[i], out treasurePrices[i], out Image face);
                treasurePrices[i].fontSize = 25;
                treasureFaces[i] = face;
            }
        }

        private void OpenChest()
        {
            bool opened = driver.TryOpenChest(out ChestOpening reward);
            Refresh();
            status.text = driver.IsSaveBlocked ? driver.LastSaveError : opened ? reward.Rarity + ": +" + reward.Gold + " ouro, +" + reward.Crystals + " cristais." : "Você precisa de um baú e uma chave.";
        }

        private void BuyKey()
        {
            bool bought = driver.TryPurchaseKey(); Refresh();
            status.text = driver.IsSaveBlocked ? driver.LastSaveError : bought ? "Chave comprada e salva." : "Cristais insuficientes.";
        }

        private void BuyGold()
        {
            bool bought = driver.TryPurchaseGoldPack(); Refresh();
            status.text = driver.IsSaveBlocked ? driver.LastSaveError : bought ? "Reserva de ouro recebida e salva." : "Cristais insuficientes ou carteira cheia.";
        }

        private void BuildNavigation()
        {
            RectTransform tread = ArtRegion("Navigation tread", root, "tread", Petroleum, 0.015f, 0.018f, 0.985f, 0.112f);
            string[] names = { "OFICINA", "PODERES", "MELHORIAS", "BROCAS" };
            string[] icons = { "repair", "bolt", "attack", "hud_drill" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i == 2 ? 4 : i;
                float left = 0.025f + i * 0.245f;
                Button button = ButtonAt(tread, "Tab " + names[i], "", Teal, left, 0.10f, left + 0.235f, 0.96f,
                    () => SelectTab(index), out TextMeshProUGUI ignoredLabel, out Image face);
                tabFaces[index] = face;
                tabVisuals[index] = (RectTransform)button.transform;
                Icon(face.transform, icons[i], 0.25f, 0.38f, 0.75f, 0.96f);
                Label(face.transform, names[i], 24, Cream, 0.04f, 0.06f, 0.96f, 0.43f, true);
            }
            SelectTab(0);
        }

        private void BuildGarage()
        {
            RectTransform pane = panes[3];
            BuildPager(pane, () => BrowseGarage(-1), () => BrowseGarage(1), out garagePage);
            RectTransform body = Plate("Drill catalog", pane, Steel, 0, 0.60f, 1, 0.73f);
            RectTransform art = Region("Catalog Visual", body, 0.015f, 0.46f, 0.29f, 0.94f);
            garageImage = art.gameObject.AddComponent<Image>();
            art.gameObject.AddComponent<DrillSpriteSheetPlayer2D>();
            garageDetails = Label(body, "BROCAS", 32, Cream, 0.31f, 0.43f, 0.96f, 0.98f, false, TextAlignmentOptions.Left);
            garageEquip = ButtonAt(body, "Equip character", "EQUIPAR", Cyan, 0.03f, 0.03f, 0.97f, 0.41f,
                EquipGarageCharacter, out garagePrice, out Image face);
            garagePrice.fontSize = 34;
        }

        private void BrowseGarage(int offset)
        {
            if (driver.CharacterCatalog.Length < 1) return;
            garageIndex = (garageIndex + offset + driver.CharacterCatalog.Length) % driver.CharacterCatalog.Length;
            Refresh();
        }

        private void EquipGarageCharacter()
        {
            CharacterCombatData definition = driver.CharacterCatalog[garageIndex];
            bool equipped = driver.TryEquipCharacter(definition.Id);
            Refresh();
            status.text = driver.IsSaveBlocked ? driver.LastSaveError : equipped
                ? definition.DisplayName + " equipada. XP e poderes anteriores preservados."
                : "Vença a fase indicada e termine a tentativa atual para trocar.";
        }

        private void SelectTab(int index)
        {
            selectedTab = index;
            ApplyPageLayout(index);
            sectionTitle.text = new[] { "OFICINA", "PODERES", "TESOURO", "BROCAS", "MELHORIAS" }[index];
            for (int i = 0; i < panes.Length; i++)
            {
                panes[i].gameObject.SetActive(i == index);
                if (tabFaces[i] != null) tabFaces[i].color = i == index ? Cyan : Teal;
                if (tabVisuals[i] != null) tabVisuals[i].anchoredPosition = new Vector2(0f, i == index ? 5f : 0f);
            }
            if (lockSummary != null)
                lockSummary.gameObject.SetActive(index == 0);
        }

        private void Purchase(UpgradeKind kind)
        {
            bool purchased = driver.TryPurchaseUpgradeAtBase(kind);
            Refresh();
            if (driver.IsSaveBlocked) status.text = driver.LastSaveError;
            else status.text = purchased ? "Broca turbinada! Progresso salvo." : "Ouro insuficiente ou melhoria no máximo.";
        }

        public void ShowPower(int index)
        {
            powerPage = Math.Max(0, Math.Min(1, index / 2));
            Show(1);
        }

        public void Show(int tab = -1)
        {
            root.gameObject.SetActive(true);
            status.text = "Melhorias são da oficina; XP e poderes pertencem a cada broca.";
            if (tab >= 0 && tab < panes.Length) selectedTab = tab;
            if (tab == 3)
                for (int i = 0; i < driver.CharacterCatalog.Length; i++)
                    if (driver.Profile != null && driver.Profile.EquippedCharacterId == driver.CharacterCatalog[i].Id) { garageIndex = i; break; }
            SelectTab(selectedTab);
            Refresh();
        }

        private void OnCombatEvent(CombatEvent combatEvent) => dirty = true;
        private void Update()
        {
            // O evento pode ocorrer no meio da simulação; consulta só depois que o passo terminou.
            if (!dirty && lastReady == driver.IsReady) return;
            dirty = false;
            Refresh();
        }

        private void Refresh()
        {
            GameSaveData profile = driver.Profile;
            if (profile == null)
            {
                attackButton.interactable = false;
                healthButton.interactable = false;
                foreach (Button button in powerButtons) if (button != null) button.interactable = false;
                foreach (Button button in treasureButtons) if (button != null) button.interactable = false;
                if (garageEquip != null) garageEquip.interactable = false;
                status.text = "Não foi possível carregar o progresso. Dados preservados.";
                return;
            }
            lastReady = driver.IsReady;
            wallet.text = profile.Gold.ToString("N0", Portuguese) + " ouro\n" + profile.Crystals + " cristais";
            CharacterLevelState level = driver.CurrentCharacterLevel;
            CharacterDefinition definition = driver.EquippedCharacterDefinition;
            characterName.text = definition != null ? definition.displayName.ToUpperInvariant() : "PERSONAGEM INDISPONÍVEL";
            experienceLabel.text = "NÍVEL " + level.Level + (level.IsMaximum ? "  •  MÁXIMO" :
                "  •  XP " + level.ExperienceInLevel + " / " + level.ExperienceToNextLevel);
            experienceFill.rectTransform.anchorMax = new Vector2(level.IsMaximum ? 1f : level.ExperienceToNextLevel > 0
                ? (float)level.ExperienceInLevel / level.ExperienceToNextLevel : 0f, 1f);
            if (driver.Economy == null) return;
            CharacterCombatData equippedCharacter = driver.EquippedCharacter;
            if (equippedCharacter != null)
            {
                showcaseImage.color = CharacterColor(equippedCharacter);
                showcaseFallback.text = showcaseFrames.IsReady ? string.Empty : equippedCharacter.DisplayName;
            }
            if (driver.CharacterCatalog.Length > 0)
            {
                CharacterCombatData option = driver.CharacterCatalog[garageIndex % driver.CharacterCatalog.Length];
                CharacterProgressData progress = CharacterProgression.Find(profile, option.Id);
                bool owns = progress != null;
                bool selected = profile.EquippedCharacterId == option.Id;
                bool active = driver.Simulation != null && driver.Simulation.IsActive;
                garagePage.text = "BROCAS " + (garageIndex + 1) + " / " + driver.CharacterCatalog.Length;
                garageImage.color = CharacterColor(option);
                garageDetails.text = option.DisplayName + "\n" + (owns ? "NÍVEL " + CharacterProgression.GetLevel(progress, option.Experience).Level : "VENÇA A FASE " + option.UnlockStage) +
                    " • ATQ BASE x" + Number(option.AttackMultiplier) + "\n" + option.Description;
                garagePrice.text = !owns ? "BLOQUEADA • FASE " + option.UnlockStage : selected ? "EQUIPADA" : active ? "TROCA ENTRE TENTATIVAS" : "EQUIPAR";
                garageEquip.interactable = driver.IsReady && owns && !selected && !active;
            }
            float specialDamage = balance.specialAbility.damage * driver.CurrentStats.AttackDamage / balance.drillAttackDamage *
                driver.CurrentStats.SpecialMultiplier * (1f + EconomyProgression.PowerBonus(profile, driver.Economy, profile.EquippedCharacterId, AttackPowerKind.Resonance));
            moduleValue.text = balance.specialAbility.hitCount + " impactos\n" + Number(specialDamage) + " por alvo" +
                (equippedCharacter != null && equippedCharacter.SpecialTargetCount > 1 ? "\nAté " + equippedCharacter.SpecialTargetCount + " alvos" : "");
            UpgradeKind[] passiveKinds = { UpgradeKind.Health, UpgradeKind.Repair, UpgradeKind.CriticalChance };
            for (int i = 0; i < passiveKinds.Length; i++)
            {
                bool unlocked = EconomyProgression.WorkshopLevel(profile, driver.Economy.Experience) >= driver.Economy.Upgrade(passiveKinds[i]).UnlockLevel;
                passiveLocks[i].gameObject.SetActive(!unlocked);
                passiveFaces[i].color = unlocked && EconomyProgression.Level(profile, passiveKinds[i]) > 0 ? Cyan : Dim;
            }
            ConfigureEconomyRow(0, attackButton, attackFace, attackPrice, attackValue);
            ConfigureEconomyRow(1, healthButton, healthFace, healthPrice, healthValue);
            upgradePageLabel.text = "MELHORIAS " + (upgradePage + 1) + " / 4";
            powerPageLabel.text = "PODERES " + (powerPage + 1) + " / 2";
            for (int i = 0; i < 2; i++)
            {
                AttackPowerRule power = driver.Economy.Powers[powerPage * 2 + i];
                int rank = EconomyProgression.PowerRank(profile, profile.EquippedCharacterId, power.Id);
                bool locked = level.Level < power.UnlockLevel;
                bool maximum = rank >= power.MaximumRank;
                powerValues[i].text = power.Name + "  " + rank + "/" + power.MaximumRank + "\n" + power.Description;
                powerPrices[i].text = locked ? "REQUER\nNV. " + power.UnlockLevel : maximum ? "MÁXIMO" : "MELHORAR\n" + power.Cost(rank) + " OURO";
                powerButtons[i].interactable = driver.IsReady && !locked && !maximum && profile.Gold >= power.Cost(rank);
                powerFaces[i].color = powerButtons[i].interactable ? Teal : Steel;
            }
            lockSummary.text = "Fase " + profile.CampaignStage + "/" + driver.Economy.MaximumStage + " • ATQ " + Number(driver.CurrentStats.AttackDamage) + " • PV " + Number(driver.CurrentStats.MaximumHealth);
            treasureSummary.text = "BAÚS " + profile.Chests + "  •  CHAVES " + profile.Keys + "\n" +
                "1 chave por abertura. Ouro e cristais.\nComum 70% • Raro 23% • Épico 6% • Lendário 1%\n" +
                "Cristais por primeira vitória; +20 a cada 5 fases.";
            treasurePrices[0].text = "ABRIR BAÚ\n1 CHAVE";
            treasurePrices[1].text = "1 CHAVE\n" + driver.Economy.KeyCrystalCost + " CRISTAIS";
            treasurePrices[2].text = "+" + driver.Economy.NormalGold(Math.Max(1, profile.HighestClearedStage)) * driver.Economy.GoldPackMultiplier + " OURO\n" + driver.Economy.GoldPackCrystalCost + " CRISTAIS";
            treasureButtons[0].interactable = driver.IsReady && profile.Chests > 0 && profile.Keys > 0;
            treasureButtons[1].interactable = driver.IsReady && profile.Crystals >= driver.Economy.KeyCrystalCost;
            treasureButtons[2].interactable = driver.IsReady && profile.Crystals >= driver.Economy.GoldPackCrystalCost;
            for (int i = 0; i < treasureFaces.Length; i++)
                treasureFaces[i].color = treasureButtons[i].interactable ? Teal : Steel;
            if (driver.IsSaveBlocked) status.text = driver.LastSaveError;
        }

        private void ConfigureEconomyRow(int row, Button button, Image face, TextMeshProUGUI price, TextMeshProUGUI value)
        {
            EconomyUpgradeRule rule = VisibleUpgrade(row);
            value.transform.parent.gameObject.SetActive(rule != null);
            if (rule == null) return;
            (row == 0 ? attackIcon : healthIcon).sprite = SpriteFor(GameplayHudModel.UpgradeIcon(rule.Kind));
            int level = EconomyProgression.Level(driver.Profile, rule.Kind);
            bool locked = EconomyProgression.WorkshopLevel(driver.Profile, driver.Economy.Experience) < rule.UnlockLevel;
            bool maximum = level >= rule.MaximumLevel;
            value.text = "<b>" + rule.Name + "  " + level + "/" + rule.MaximumLevel + "</b>\n" + rule.Description.Replace(" (multiplicativo)", "");
            price.text = locked ? "REQUER\nNV. " + rule.UnlockLevel : maximum ? "MÁXIMO" : "TURBINAR\n" + rule.Cost(level) + " OURO";
            button.interactable = driver.IsReady && !locked && !maximum && driver.Profile.Gold >= rule.Cost(level);
            face.color = button.interactable ? Teal : Steel;
        }

        private static string Number(float value) => value.ToString("0.##", Portuguese);
        private static Color CharacterColor(CharacterCombatData character) => new Color(character.TintR, character.TintG, character.TintB);

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

        private Sprite SpriteFor(string name)
        {
            if (!sprites.TryGetValue(name, out Sprite sprite))
            {
                string icon = name == "hud_drill" ? "drill" : name;
                sprite = Resources.Load<Sprite>("Art/UI/Icons/" + icon);
                if (sprite == null) sprite = Resources.Load<Sprite>("Art/Workshop/" + name);
                sprites.Add(name, sprite);
            }
            return sprite;
        }

        private RectTransform ArtRegion(string name, Transform parent, string art, Color color, float x0, float y0, float x1, float y1)
        {
            RectTransform rect = Region(name, parent, x0, y0, x1, y1);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = SpriteFor(art);
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private RectTransform Plate(string name, Transform parent, Color color, float x0, float y0, float x1, float y1)
            => ArtRegion(name, parent, "plate", color, x0, y0, x1, y1);

        private Image Icon(Transform parent, string art, float x0, float y0, float x1, float y1)
        {
            RectTransform rect = Region(art, parent, x0, y0, x1, y1);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = SpriteFor(art);
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private TextMeshProUGUI Label(Transform parent, string text, int size, Color color, float x0, float y0, float x1, float y1,
            bool title, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = Region("Text", parent, x0, y0, x1, y1);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = title ? titleFont : bodyFont;
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.enableAutoSizing = true;
            label.fontSizeMin = size * 0.6f;
            label.fontSizeMax = size;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        private Button ButtonAt(Transform parent, string name, string text, Color color, float x0, float y0, float x1, float y1,
            Action action, out TextMeshProUGUI label, out Image face)
        {
            RectTransform rect = ArtRegion(name, parent, "button", Petroleum, x0, y0, x1, y1);
            Image target = rect.GetComponent<Image>();
            target.raycastTarget = true;
            RectTransform front = ArtRegion("Button face", rect, "button", color, 0.02f, 0.06f, 0.98f, 0.96f);
            face = front.GetComponent<Image>();
            label = Label(front, text, 32, Cream, 0.06f, 0.12f, 0.94f, 0.89f, false);
            label.fontStyle = FontStyles.Bold;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            button.onClick.AddListener(() => action());
            rect.gameObject.AddComponent<WorkshopButtonFeedback>().Initialize(front, button);
            return button;
        }

        private void OnDestroy()
        {
            if (driver != null) driver.CombatEventRaised -= OnCombatEvent;
            // Fontes e atlas são compartilhados pela tela, nunca criados por ataque ou por frame.
            foreach (TMP_FontAsset font in ownedFonts)
            {
                foreach (Texture2D atlas in font.atlasTextures) if (atlas != null) Destroy(atlas);
                if (font.material != null) Destroy(font.material);
                Destroy(font);
            }
        }
    }
}
