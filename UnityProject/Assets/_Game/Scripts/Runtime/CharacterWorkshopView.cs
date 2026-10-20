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
    public sealed class CharacterWorkshopView : MonoBehaviour
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
        private readonly RectTransform[] panes = new RectTransform[3];
        private readonly Image[] tabFaces = new Image[3];
        private readonly RectTransform[] tabVisuals = new RectTransform[3];
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
        private Button attackButton;
        private Button healthButton;
        private int selectedTab;
        private bool initialized;
        private bool dirty;
        private bool lastReady;

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
            Label(header, "OFICINA", 64, Gold, 0.08f, 0.12f, 0.92f, 0.92f, true);
            ButtonAt(root, "Close workshop", "", Coral, 0.84f, 0.91f, 0.98f, 0.99f, returnToHub,
                out TextMeshProUGUI ignoredCloseLabel, out Image closeFace);
            Icon(closeFace.transform, "close", 0.2f, 0.15f, 0.8f, 0.85f);
            characterName = Label(root, "BROCA PIONEIRA", 32, Cream, 0.045f, 0.877f, 0.63f, 0.910f, true, TextAlignmentOptions.Left);
            Icon(root, "gear", 0.69f, 0.877f, 0.75f, 0.909f);
            wallet = Label(root, "0", 32, Gold, 0.76f, 0.877f, 0.96f, 0.909f, true, TextAlignmentOptions.Right);

            BuildShowcase();
            RectTransform experienceTrack = Plate("Character XP track", root, Teal, 0.04f, 0.561f, 0.96f, 0.602f);
            RectTransform track = Plate("Energy tube", experienceTrack, Petroleum, 0.04f, 0.25f, 0.96f, 0.78f);
            experienceFill = Plate("Experience fill", track, Cyan, 0f, 0f, 0f, 1f).GetComponent<Image>();
            experienceLabel = Label(experienceTrack, "NÍVEL 0", 30, Cream, 0.07f, 0.03f, 0.93f, 0.98f, true);

            BuildModules();
            for (int i = 0; i < panes.Length; i++)
                panes[i] = Region("Workshop tab " + i, root, 0.04f, 0.218f, 0.96f, 0.464f);
            BuildUpgradeRows();
            BuildPartsPane(panes[1]);
            BuildDronePane(panes[2]);
            lockSummary = Label(root, "Novas melhorias em preparação", 22, Dim, 0.05f, 0.193f, 0.95f, 0.217f, false);
            status = Label(root, "Escolha uma melhoria para turbinar a broca.", 26, Cream,
                0.055f, 0.12f, 0.61f, 0.19f, false, TextAlignmentOptions.Left);
            ButtonAt(root, "Return from workshop", "VOLTAR", Cyan, 0.64f, 0.116f, 0.96f, 0.191f, returnToHub,
                out TextMeshProUGUI ignoredBackLabel, out Image ignoredBackFace);
            BuildNavigation();
        }

        private void BuildShowcase()
        {
            RectTransform chamber = Plate("Character showcase", root, Teal, 0.035f, 0.607f, 0.965f, 0.875f);
            // O pedestal e a broca são independentes; não se anima a posição lógica da unidade.
            Icon(chamber, "pedestal", 0.195f, 0.025f, 0.805f, 0.36f);
            TextMeshProUGUI fallback = Label(chamber, "PIONEIRA", 22, Cyan, 0.29f, 0.28f, 0.71f, 0.79f, true);
            RectTransform drill = Region("Drill Visual", fallback.transform, -0.19f, -0.2f, 1.19f, 1.2f);
            drill.gameObject.AddComponent<Image>();
            drill.gameObject.AddComponent<DrillSpriteSheetPlayer2D>();
            // Idle discreto. O pedestal permanece fixo para transmitir contato com o chão.
            drill.gameObject.AddComponent<ProceduralAnimator2D>();
            Slot(chamber, "BROCA", "drill", 0.015f, 0.53f, 0.22f, 0.97f,
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
            Icon(region, icon, 0.16f, 0.13f, 0.84f, 0.69f);
            Icon(region, "lock", 0.66f, 0.04f, 0.98f, 0.3f);
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
            Icon(module, "drill", 0.06f, 0.07f, 0.94f, 0.93f);
            string specialText = balance != null && balance.specialAbility != null
                ? balance.specialAbility.hitCount + " impactos\n" + Number(balance.specialAbility.damage) + " de dano base"
                : "Pioneira\nDados indisponíveis";
            Label(cartridge, specialText, 28, Cream, 0.18f, 0.12f, 0.58f, 0.69f, false, TextAlignmentOptions.Left);
            string[] passiveIcons = { "shield", "repair", "bolt" };
            for (int i = 0; i < passiveIcons.Length; i++)
            {
                float left = 0.63f + i * 0.112f;
                RectTransform passive = Plate("Locked passive " + i, cartridge, Petroleum, left, 0.1f, left + 0.1f, 0.68f);
                Image art = Icon(passive, passiveIcons[i], 0.05f, 0.05f, 0.95f, 0.95f);
                art.color = Dim;
                Icon(passive, "lock", 0.53f, -0.02f, 1.02f, 0.47f);
            }
        }

        private void BuildUpgradeRows()
        {
            RectTransform attack = Plate("Attack row", panes[0], Steel, 0f, 0.53f, 1f, 1f);
            Icon(attack, "attack", 0.015f, 0.1f, 0.17f, 0.9f);
            attackValue = Label(attack, "ATQ", 36, Cream, 0.19f, 0.09f, 0.55f, 0.93f, false, TextAlignmentOptions.Left);
            attackButton = ButtonAt(attack, "Upgrade attack", "TURBINAR", Gold, 0.59f, 0.16f, 0.97f, 0.87f,
                () => Purchase(UpgradeKind.Attack), out attackPrice, out attackFace);
            RectTransform health = Plate("Health row", panes[0], Steel, 0f, 0.02f, 1f, 0.49f);
            Icon(health, "health", 0.015f, 0.1f, 0.17f, 0.9f);
            healthValue = Label(health, "VIDA", 36, Cream, 0.19f, 0.09f, 0.55f, 0.93f, false, TextAlignmentOptions.Left);
            healthButton = ButtonAt(health, "Upgrade health", "TURBINAR", Gold, 0.59f, 0.16f, 0.97f, 0.87f,
                () => Purchase(UpgradeKind.Health), out healthPrice, out healthFace);
        }

        private void BuildPartsPane(RectTransform parent)
        {
            RectTransform plate = Plate("Parts inventory", parent, Steel, 0f, 0.02f, 1f, 1f);
            Label(plate, "PEÇAS DA PIONEIRA", 29, Gold, 0.06f, 0.79f, 0.94f, 0.97f, true);
            string[] names = { "BROCA", "CHASSI", "MOTOR", "NÚCLEO" };
            string[] icons = { "drill", "chassis", "motor", "core" };
            for (int i = 0; i < names.Length; i++)
            {
                float left = 0.025f + i * 0.245f;
                Icon(plate, "slot", left, 0.33f, left + 0.215f, 0.8f);
                Icon(plate, icons[i], left + 0.03f, 0.4f, left + 0.185f, 0.72f);
                Label(plate, names[i], 20, Cream, left, 0.24f, left + 0.215f, 0.34f, true);
            }
            Label(plate, "Peças originais equipadas.\nNovas peças e trocas chegam em uma próxima atualização.", 28, Cream,
                0.06f, 0.025f, 0.94f, 0.23f, false);
        }

        private void BuildDronePane(RectTransform parent)
        {
            RectTransform plate = Plate("Drone hangar", parent, Steel, 0f, 0.02f, 1f, 1f);
            Icon(plate, "drone", 0.34f, 0.40f, 0.66f, 0.95f);
            Icon(plate, "lock", 0.56f, 0.41f, 0.69f, 0.66f);
            AbilityDefinition drone = Resources.Load<AbilityDefinition>("Data/Abilities/drone");
            string requirement = drone != null ? "Projeto previsto para a fase " + drone.unlockPhase : "Projeto em preparação";
            Label(plate, "HANGAR DE DRONES", 27, Gold, 0.05f, 0.26f, 0.95f, 0.43f, true);
            Label(plate, requirement + "\nNovos aliados ainda estão em preparação.", 28, Cream, 0.05f, 0.04f, 0.95f, 0.26f, false);
        }

        private void BuildNavigation()
        {
            RectTransform tread = ArtRegion("Navigation tread", root, "tread", Petroleum, 0.015f, 0.018f, 0.985f, 0.112f);
            string[] names = { "OFICINA", "PEÇAS", "DRONES" };
            string[] icons = { "repair", "gear", "drone" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                float left = 0.075f + i * 0.287f;
                Button button = ButtonAt(tread, "Tab " + names[i], "", Teal, left, 0.10f, left + 0.277f, 0.96f,
                    () => SelectTab(index), out TextMeshProUGUI ignoredLabel, out Image face);
                tabFaces[i] = face;
                tabVisuals[i] = (RectTransform)button.transform;
                Icon(face.transform, icons[i], 0.32f, 0.40f, 0.68f, 0.96f);
                Label(face.transform, names[i], 24, Cream, 0.04f, 0.06f, 0.96f, 0.43f, true);
            }
            SelectTab(0);
        }

        private void SelectTab(int index)
        {
            selectedTab = index;
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

        public void Show()
        {
            root.gameObject.SetActive(true);
            status.text = driver.Simulation == null ? "Inicie uma expedição para liberar as compras." : "Escolha uma melhoria para turbinar a broca.";
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
                status.text = "Não foi possível carregar o progresso. Dados preservados.";
                return;
            }
            lastReady = driver.IsReady;
            wallet.text = profile.Gold.ToString("N0", Portuguese);
            CharacterLevelState level = driver.CurrentCharacterLevel;
            CharacterDefinition definition = driver.EquippedCharacterDefinition;
            characterName.text = definition != null ? definition.displayName.ToUpperInvariant() : "PERSONAGEM INDISPONÍVEL";
            experienceLabel.text = "NÍVEL " + level.Level + (level.IsMaximum ? "  •  MÁXIMO" :
                "  •  XP " + level.ExperienceInLevel + " / " + level.ExperienceToNextLevel);
            experienceFill.rectTransform.anchorMax = new Vector2(level.IsMaximum ? 1f : level.ExperienceToNextLevel > 0
                ? (float)level.ExperienceInLevel / level.ExperienceToNextLevel : 0f, 1f);
            float hp = driver.Simulation != null ? driver.Simulation.PlayerMaximumHealth : balance == null ? 0f : balance.playerBaseHealth;
            attackValue.text = "ATQ  " + Number(driver.CurrentAttackDamage) + "\nNv. " + profile.AttackLevel + " / " + driver.CurrentAttackMaximum;
            healthValue.text = "VIDA  " + Number(hp) + "\nNv. " + profile.HealthLevel + " / " + driver.CurrentHealthMaximum;
            ConfigureUpgrade(attackButton, attackFace, attackPrice, profile.Gold, profile.AttackLevel,
                balance == null || balance.progression == null ? null : balance.progression.attackUpgrade);
            ConfigureUpgrade(healthButton, healthFace, healthPrice, profile.Gold, profile.HealthLevel,
                balance == null || balance.progression == null ? null : balance.progression.healthUpgrade);
            lockSummary.text = "Reparo: VIDA " + driver.CurrentHealthMaximum + " • Crítico: ATQ " + driver.CurrentAttackMaximum + " • Em preparação";
            if (driver.IsSaveBlocked) status.text = driver.LastSaveError;
        }

        private void ConfigureUpgrade(Button button, Image face, TextMeshProUGUI label, int gold, int level, UpgradeDefinition definition)
        {
            bool available = definition != null && definition.implemented;
            bool maximum = available && level >= definition.maxLevel;
            int cost = available ? definition.GetCost(level) : 0;
            label.text = maximum ? "MÁXIMO" : available ? "TURBINAR\n" + cost.ToString("N0", Portuguese) + " OURO" : "EM PREPARAÇÃO";
            button.interactable = available && !maximum && driver.Simulation != null && driver.IsReady && gold >= cost;
            face.color = button.interactable ? Gold : Steel;
        }

        private static string Number(float value) => value.ToString("0.##", Portuguese);

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
                sprite = Resources.Load<Sprite>("Art/Workshop/" + name);
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
            RectTransform front = ArtRegion("Button face", rect, "button", color, 0f, 0.06f, 1f, 1.06f);
            face = front.GetComponent<Image>();
            label = Label(front, text, 32, Cream, 0.04f, 0.10f, 0.96f, 0.91f, true);
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
