using System;
using System.IO;
using System.Linq;
using DigAndWonder;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace DigAndWonder.Editor
{
    [InitializeOnLoad]
    public static class GodotCatalogImporter
    {
        private const string OutputRoot = "Assets/_Game/Resources/Data";
        private const string BalancePath = OutputRoot + "/CombatBalance.asset";
        private static bool isImporting;

        static GodotCatalogImporter()
        {
            EditorApplication.delayCall += ImportOnFirstOpen;
        }

        [MenuItem("Dig & Wonder/Import Godot data" )]
        public static void ImportAll()
        {
            if (isImporting) return;
            isImporting = true;
            try
            {
                string sourceRoot = GetSourceRoot();
                JObject prototype = ReadObject(Path.Combine(sourceRoot, "defense/prototype.json"));
                JObject abilities = ReadObject(Path.Combine(sourceRoot, "defense/abilities.json"));
                JObject phases = ReadObject(Path.Combine(sourceRoot, "phases/catalog.json"));
                JArray normalCycle = (JArray)phases["normal_cycle"];
                JObject phaseCatalog = (JObject)phases["phases"];
                JObject mole = (JObject)prototype["mole"];

                EnsureFolder("Assets/_Game/Resources");
                EnsureFolder("Assets/_Game/Resources/Data");
                EnemyDefinition regularEnemy = ImportEnemy("Torrão", mole, false, mole.Value<int>("gold"));
                AbilityDefinition pioneer = ImportAbility("Pioneira", "pioneer", (JObject)abilities["pioneer"], (JObject)prototype["pioneer"]);
                ImportInactiveAbilities(abilities);

                JObject phaseConfig = (JObject)prototype["phase"];
                JObject cart = (JObject)prototype["cart"];
                JObject upgrades = (JObject)prototype["upgrades"];
                JObject limits = (JObject)prototype["limits"];
                JObject damageUpgradeJson = (JObject)upgrades["damage"];
                JObject healthUpgradeJson = (JObject)upgrades["hull"];

                UpgradeDefinition attackUpgrade = ImportUpgrade("Ataque", UpgradeKind.Attack, damageUpgradeJson, true,
                    "Valores de damage upgrade; adaptado aos ataques da broca.");
                UpgradeDefinition healthUpgrade = ImportUpgrade("Vida", UpgradeKind.Health, healthUpgradeJson, true,
                    "Valores de hull upgrade; PV do carrinho viram PV base da broca.");
                UpgradeDefinition critChance = ImportUnavailableUpgrade("Crítico", UpgradeKind.CriticalChance,
                    "Não existe número de crítico nos dados Godot; permanece indisponível até balanceamento rastreável.");
                UpgradeDefinition critDamage = ImportUnavailableUpgrade("Dano crítico", UpgradeKind.CriticalDamage,
                    "Não existe número de dano crítico nos dados Godot; permanece indisponível até balanceamento rastreável.");
                UpgradeDefinition specialUpgrade = ImportUnavailableUpgrade("Especial", UpgradeKind.Special,
                    "O catálogo define dano/cooldown da Pioneira, mas não define curva/preço de melhoria do especial no novo loop.");

                StageDefinition[] stageAssets = phaseCatalog.Properties()
                    .Select(property => ImportStage(property.Name, (JObject)property.Value, regularEnemy, mole,
                        phaseConfig.Value<int>("layers"),
                        phaseConfig.Value<float>("wave_interval"),
                        phaseConfig.Value<int>("timed_waves"),
                        phaseConfig.Value<int>("late_wave_after"),
                        phaseConfig.Value<int>("late_wave_size"),
                        phaseConfig.Value<int>("final_moles")))
                    .ToArray();
                StageDefinition[] cycle = normalCycle.Select(token => stageAssets.First(stage => stage.id == token.Value<string>())).ToArray();
                StageDefinition surprise = stageAssets.First(stage => stage.id == "fenda_aurora");

                ProgressionDefinition progression = GetOrCreate<ProgressionDefinition>("Progression.asset");
                progression.surpriseChancePercent = phases.Value<int>("surprise_chance_percent");
                progression.segmentsPerStage = phases.Value<int>("cells_per_phase");
                progression.normalCycle = cycle;
                progression.surpriseStage = surprise;
                progression.attackUpgrade = attackUpgrade;
                progression.healthUpgrade = healthUpgrade;
                progression.criticalChanceUpgrade = critChance;
                progression.criticalDamageUpgrade = critDamage;
                progression.specialUpgrade = specialUpgrade;
                EditorUtility.SetDirty(progression);

                CombatBalanceDefinition balance = GetOrCreate<CombatBalanceDefinition>("CombatBalance.asset");
                balance.playerBaseHealth = cart.Value<float>("hp");
                balance.drillAttackDamage = cart.Value<float>("cannon_damage");
                balance.drillAttackInterval = cart.Value<float>("cannon_interval");
                balance.drillAttackHitCount = 2;
                balance.drillAttackHitInterval = 0.28f;
                balance.drillAttackTravelTime = 0.34f;
                balance.specialAbility = pioneer;
                balance.regularEnemy = regularEnemy;
                balance.progression = progression;
                balance.maximumEnemies = limits.Value<int>("enemies");
                balance.maximumPendingSpawns = limits.Value<int>("pending_spawns");
                balance.maximumProjectiles = limits.Value<int>("projectiles");
                balance.simulationStep = 0.05f;
                balance.maximumFrameCatchUp = 0.25f;
                balance.sourceNote = "Ataque 6 e intervalo 1,25 s vêm de cada canhão; ciclo usa dois disparos, com desfasamento 0,28 s e voo 0,34 s conforme defense_run.gd. PV 120 vêm do carrinho. A Pioneira vira especial. Fases/bosses vêm do catálogo legado; seus valores ainda não foram reequilibrados para o novo loop.";
                EditorUtility.SetDirty(balance);

                ImportUiTheme();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Dados Godot importados em ScriptableObjects. Verifique a tabela de paridade antes de alterar o balanceamento.");
            }
            finally
            {
                isImporting = false;
            }
        }

        private static void ImportOnFirstOpen()
        {
            if (AssetDatabase.LoadAssetAtPath<CombatBalanceDefinition>(BalancePath) == null)
                ImportAll();
        }

        private static EnemyDefinition ImportEnemy(string assetName, JObject source, bool isBoss, int goldReward)
        {
            EnemyDefinition asset = GetOrCreate<EnemyDefinition>("Enemies/" + assetName + ".asset");
            asset.id = isBoss ? "boss_" + assetName.ToLowerInvariant() : "torrao";
            asset.displayName = source.Value<string>("name") ?? assetName;
            asset.maxHealth = source.Value<float>("hp");
            asset.damage = source.Value<float>("damage");
            asset.firstAttackDelay = source.Value<float>("first_attack");
            asset.attackInterval = source.Value<float>("attack_interval");
            asset.telegraphDuration = source.Value<float>("telegraph");
            asset.projectileFlight = source.Value<float?>("projectile_flight") ?? 0.65f;
            asset.goldReward = goldReward;
            asset.isBoss = isBoss;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static AbilityDefinition ImportAbility(string assetName, string id, JObject source, JObject timingSource = null)
        {
            AbilityDefinition asset = GetOrCreate<AbilityDefinition>("Abilities/" + assetName + ".asset");
            asset.id = id;
            asset.displayName = source.Value<string>("name");
            asset.implemented = source.Value<bool>("implemented");
            asset.unlockPhase = source.Value<int>("unlock_phase");
            asset.goldCost = source.Value<int>("cost");
            asset.damage = source.Value<float>("damage");
            asset.cooldown = source.Value<float>("cooldown");
            asset.travelTime = source.Value<float>("flight");
            asset.hitCount = timingSource == null ? 1 : timingSource.Value<int>("hits");
            asset.hitInterval = timingSource == null ? 0f : timingSource.Value<float>("hit_interval");
            asset.duration = source.Value<float>("duration");
            asset.maxSimultaneous = source.Value<int>("limit");
            asset.targeting = source.Value<string>("target");
            asset.role = source.Value<string>("role");
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void ImportInactiveAbilities(JObject abilities)
        {
            foreach (JProperty property in abilities.Properties())
            {
                if (property.Name == "pioneer") continue;
                ImportAbility(property.Name, property.Name, (JObject)property.Value);
            }
        }

        private static UpgradeDefinition ImportUpgrade(string assetName, UpgradeKind kind, JObject source, bool implemented, string note)
        {
            UpgradeDefinition asset = GetOrCreate<UpgradeDefinition>("Upgrades/" + assetName + ".asset");
            asset.kind = kind;
            asset.displayName = assetName;
            asset.baseCost = source.Value<int>("base_cost");
            asset.costStep = source.Value<int>("cost_step");
            asset.maxLevel = source.Value<int>("max_level");
            asset.additiveBonus = source.Value<float>("bonus");
            asset.implemented = implemented;
            asset.sourceNote = note;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static UpgradeDefinition ImportUnavailableUpgrade(string assetName, UpgradeKind kind, string note)
        {
            UpgradeDefinition asset = GetOrCreate<UpgradeDefinition>("Upgrades/" + assetName + ".asset");
            asset.kind = kind;
            asset.displayName = assetName;
            asset.baseCost = 0;
            asset.costStep = 0;
            asset.maxLevel = 0;
            asset.additiveBonus = 0f;
            asset.implemented = false;
            asset.sourceNote = note;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static StageDefinition ImportStage(string id, JObject source, EnemyDefinition regularEnemy, JObject mole,
            int length, float waveInterval, int waveCount, int lateWaveAfter, int lateWaveSize, int finalEnemies)
        {
            JObject bossSource = (JObject)source["boss"];
            EnemyDefinition boss = GetOrCreate<EnemyDefinition>("Enemies/" + id + "Boss.asset");
            boss.id = id + "_boss";
            boss.displayName = bossSource.Value<string>("name");
            boss.maxHealth = bossSource.Value<float>("health");
            boss.damage = mole.Value<float>("damage");
            boss.firstAttackDelay = mole.Value<float>("first_attack");
            boss.attackInterval = mole.Value<float>("attack_interval");
            boss.telegraphDuration = mole.Value<float>("telegraph");
            boss.projectileFlight = mole.Value<float?>("projectile_flight") ?? 0.65f;
            boss.goldReward = bossSource.Value<int>("coins");
            boss.isBoss = true;
            EditorUtility.SetDirty(boss);

            StageDefinition asset = GetOrCreate<StageDefinition>("Stages/" + id + ".asset");
            asset.id = id;
            asset.displayName = source.Value<string>("name");
            asset.caveColor = ParseColor(source.Value<string>("sky"));
            asset.groundColor = ParseColor(source.Value<string>("surface"));
            asset.rockColor = ParseColor(source.Value<string>("rock"));
            asset.lengthSegments = length;
            asset.chestCount = source.Value<int>("chests");
            asset.keyCount = source.Value<int>("keys");
            asset.timedWaveInterval = waveInterval;
            asset.timedWaveCount = waveCount;
            asset.lateWaveAfter = lateWaveAfter;
            asset.lateWaveSize = lateWaveSize;
            asset.optionalFinalEnemies = finalEnemies;
            asset.regularEnemy = regularEnemy;
            asset.boss = boss;
            asset.terrainLayers = source["layers"].Select(token => new TerrainLayerDefinition
            {
                displayName = token.Value<string>("name"),
                color = ParseColor(token.Value<string>("color")),
                hardness = token.Value<int>("hardness"),
                goldReward = token.Value<int>("reward"),
                appearanceChance = token.Value<float>("chance")
            }).ToArray();
            asset.sourceNote = "Fase e boss vieram de content/phases/catalog.json (catálogo antigo de escavação). Os ataques dos bosses reaproveitam os do Torrão porque o catálogo antigo não define ataques; essa equivalência é provisória e documentada.";
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void ImportUiTheme()
        {
            UiThemeDefinition asset = GetOrCreate<UiThemeDefinition>("UiTheme.asset");
            asset.baseFontSize = 16;
            asset.buttonColor = new Color(0.949f, 0.722f, 0.294f, 1f);
            asset.pressedColor = new Color(0.75f, 0.51f, 0.21f, 1f);
            asset.disabledColor = new Color(0.29f, 0.40f, 0.42f, 1f);
            asset.focusColor = new Color(0.718f, 1f, 0.945f, 1f);
            asset.panelColor = new Color(0.09f, 0.188f, 0.239f, 1f);
            asset.outlineColor = new Color(0.09f, 0.188f, 0.239f, 1f);
            asset.cornerRadius = 22f;
            asset.borderThickness = 3f;
            EditorUtility.SetDirty(asset);
        }

        private static T GetOrCreate<T>(string relativePath) where T : ScriptableObject
        {
            string folder = Path.GetDirectoryName(OutputRoot + "/" + relativePath).Replace('\\', '/');
            EnsureFolder(folder);
            string path = OutputRoot + "/" + relativePath;
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static JObject ReadObject(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Catálogo Godot não encontrado.", path);
            return JObject.Parse(File.ReadAllText(path));
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.magenta;
        }

        private static string GetSourceRoot()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../content"));
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("Pasta content do Godot não encontrada: " + root);
            return root;
        }
    }
}
