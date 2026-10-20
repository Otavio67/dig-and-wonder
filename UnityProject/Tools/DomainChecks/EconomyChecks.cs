using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DigAndWonder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal static class EconomyChecks
{
    internal static string Root
    {
        get
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null && !File.Exists(Path.Combine(current.FullName, "project.godot"))) current = current.Parent;
            return current?.FullName ?? throw new InvalidOperationException("Execute a partir do repositório.");
        }
    }

    internal static EconomyRules Rules()
    {
        EconomyRules rules = JsonConvert.DeserializeObject<EconomyRules>(File.ReadAllText(Path.Combine(Root,
            "UnityProject/Assets/_Game/Resources/Data/Economy/midgame.json")));
        rules.Validate(); return rules;
    }

    // O ensaio lê os mesmos assets numéricos que o bridge Unity, sem depender do Editor.
    internal static CombatConfiguration Configuration(EconomyRules rules = null)
    {
        rules ??= Rules();
        string data = Path.Combine(Root, "UnityProject/Assets/_Game/Resources/Data");
        float Value(string path, string field)
        {
            Match match = Regex.Match(File.ReadAllText(Path.Combine(data, path)), @"^  " + Regex.Escape(field) + @": ([0-9.]+)$", RegexOptions.Multiline);
            return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
        EnemyCombatData Enemy(string path, string id, bool boss)
        {
            return new EnemyCombatData
            {
                Id = id, DisplayName = id, IsBoss = boss, MaxHealth = Value(path, "maxHealth"), Damage = Value(path, "damage"),
                GoldReward = (int)Value(path, "goldReward"), FirstAttackDelay = Value(path, "firstAttackDelay"),
                AttackInterval = Value(path, "attackInterval"), TelegraphDuration = Value(path, "telegraphDuration"),
                ProjectileFlight = Value(path, "projectileFlight")
            };
        }
        StageCombatData Stage(string id)
        {
            string path = "Stages/" + id + ".asset";
            return new StageCombatData
            {
                Id = id, DisplayName = id, LengthSegments = (int)Value(path, "lengthSegments"),
                WaveCount = (int)Value(path, "timedWaveCount"), WaveInterval = Value(path, "timedWaveInterval"),
                LateWaveAfter = (int)Value(path, "lateWaveAfter"), LateWaveSize = (int)Value(path, "lateWaveSize"),
                KeyCount = (int)Value(path, "keyCount"), ChestCount = (int)Value(path, "chestCount"),
                Boss = Enemy("Enemies/" + id + "Boss.asset", id + "_boss", true)
            };
        }
        var stages = new[] { Stage("prado_cristalino"), Stage("gruta_basaltica"), Stage("jardim_fossil") };
        return new CombatConfiguration
        {
            Economy = rules, PlayerBaseHealth = Value("CombatBalance.asset", "playerBaseHealth"),
            BasicAttackDamage = Value("CombatBalance.asset", "drillAttackDamage"), BasicAttackInterval = Value("CombatBalance.asset", "drillAttackInterval"),
            BasicAttackHitCount = (int)Value("CombatBalance.asset", "drillAttackHitCount"), BasicAttackHitInterval = Value("CombatBalance.asset", "drillAttackHitInterval"),
            BasicAttackTravelTime = Value("CombatBalance.asset", "drillAttackTravelTime"), SimulationStep = Value("CombatBalance.asset", "simulationStep"),
            MaximumFrameCatchUp = Value("CombatBalance.asset", "maximumFrameCatchUp"), MaximumEnemies = (int)Value("CombatBalance.asset", "maximumEnemies"),
            MaximumPendingSpawns = (int)Value("CombatBalance.asset", "maximumPendingSpawns"), MaximumProjectiles = (int)Value("CombatBalance.asset", "maximumProjectiles"),
            SurpriseChancePercent = 22, SegmentsPerStage = 24, Stages = stages, SurpriseStage = Stage("fenda_aurora"),
            RegularEnemy = Enemy("Enemies/Torrão.asset", "torrao", false),
            Special = new SpecialCombatData
            {
                Damage = Value("Abilities/Pioneira.asset", "damage"), Cooldown = Value("Abilities/Pioneira.asset", "cooldown"),
                TravelTime = Value("Abilities/Pioneira.asset", "travelTime"), HitCount = (int)Value("Abilities/Pioneira.asset", "hitCount"),
                HitInterval = Value("Abilities/Pioneira.asset", "hitInterval")
            },
            AttackUpgrade = new UpgradeCombatData { MaximumLevel = 100 }, HealthUpgrade = new UpgradeCombatData { MaximumLevel = 100 },
            Characters = rules.CharacterRoster.Select(x => x.WithExperience(rules.Experience)).Concat(new[] {
                new CharacterCombatData { Id = "test_drill", DisplayName = "Somente fixture", Experience = rules.Experience }
            }).ToArray()
        };
    }

    internal static IEnumerable<(string Name, Action Test)> All()
    {
        yield return ("economia: tabela e probabilidades válidas", RulesAreValid);
        yield return ("economia: todos os custos crescem até os limites", CostsAreMonotonic);
        yield return ("economia: curva de XP completa, excedente e nível 60", FullExperienceCurve);
        yield return ("economia: nível 60 exige ao menos 60 horas mesmo no limite ideal de farm", LongTermExperienceBudget);
        yield return ("economia: sete melhorias respeitam saldo, nível e teto", UpgradeTransactions);
        yield return ("economia: poderes são individuais e respeitam cinco ranks", CharacterPowers);
        yield return ("economia: efeitos de stats e percentuais corretos", StatFormulas);
        yield return ("economia: os quatro poderes alteram o combate", PowersAffectCombat);
        yield return ("economia: críticos usam sorteio determinístico", CriticalIsDeterministic);
        yield return ("economia: cadência aumenta ataques sem aumentar dano unitário", SpeedAffectsAttacks);
        yield return ("economia: autorreparo depende do tempo e respeita vida máxima", RepairUsesTime);
        yield return ("economia: chefe entrega recompensa e primeira vitória atomicamente", FirstClearIsAtomic);
        yield return ("economia: repetir fase não duplica cristais de primeira vitória", RepeatClearDoesNotPayFirstClear);
        yield return ("economia: treino repete fase vencida e não interrompe tentativa", TrainingRespectsProgress);
        yield return ("economia: fase 100 encerra avanço sem ultrapassar o marco", FinalStageRemainsBounded);
        yield return ("economia: baú exige chave e persiste sorteio", ChestTransactions);
        yield return ("economia: guardar baú não aumenta valor com a fase", ChestValueDoesNotInflate);
        yield return ("economia: distribuição de 100 mil baús respeita percentuais", ChestDistribution);
        yield return ("economia: loja rejeita saldo insuficiente e overflow", ShopTransactions);
        yield return ("economia: save 2 preserva XP, moedas e backup na migração", SchemaTwoMigration);
        yield return ("economia: save 3 rejeita poderes inválidos e campos ausentes", SchemaThreeValidation);
        yield return ("economia: retomada mantém críticos, poderes, XP e carteira", MidgameResume);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void RulesAreValid()
    {
        EconomyRules rules = Rules(); Check(rules.MaximumStage == 100 && rules.Experience.MaximumLevel == 60, "marco incompleto");
        rules.ChestRewards[0].Weight--;
        bool rejected = false; try { rules.Validate(); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "probabilidade inválida aceita");
    }

    private static void CostsAreMonotonic()
    {
        EconomyRules rules = Rules();
        foreach (EconomyUpgradeRule upgrade in rules.Upgrades)
            for (int i = 1; i < upgrade.MaximumLevel; i++) Check(upgrade.Cost(i) > upgrade.Cost(i - 1), upgrade.Name);
        foreach (AttackPowerRule power in rules.Powers)
            for (int i = 1; i < power.MaximumRank; i++) Check(power.Cost(i) > power.Cost(i - 1), power.Name);
        Check(rules.Upgrade(UpgradeKind.Attack).Cost(0) == 18 && rules.Upgrade(UpgradeKind.Attack).Cost(1) == 20, "arredondamento de preço incorreto");
    }

    private static void FullExperienceCurve()
    {
        EconomyRules rules = Rules(); var save = new GameSaveData(); long sum = 0;
        for (int level = 0; level < 60; level++)
        {
            long required = (long)Math.Ceiling((20 + 12d * level) * Math.Pow(1.125, level));
            Check(rules.Experience.RequiredForNextLevel(level) == required, "curva divergente");
            CharacterProgression.Award(save, "pioneer", rules.Experience, (int)required); sum += required;
            Check(CharacterProgression.GetLevel(save.Characters[0], rules.Experience).Level == level + 1, "nível divergente");
        }
        Check(sum == rules.Experience.MaximumExperience && CharacterProgression.Award(save, "pioneer", rules.Experience, int.MaxValue) == 0, "teto falhou");
    }

    private static GameSaveData Experienced(EconomyRules rules, int level = 20)
    {
        var save = new GameSaveData { Gold = 1000000 };
        for (int i = 0; i < level; i++) CharacterProgression.Award(save, "pioneer", rules.Experience, (int)rules.Experience.RequiredForNextLevel(i));
        return save;
    }

    private static void LongTermExperienceBudget()
    {
        EconomyRules rules = Rules();
        int enemies = 0;
        for (int wave = 1; wave <= rules.WaveCount; wave++) enemies += wave > rules.LateWaveAfter ? rules.LateWaveSize : 1;
        // A referência de longo prazo continua sendo a faixa 60. As fases 61-100 são
        // conteúdo de end game e oferecem catch-up para brocas novas sem encurtar a
        // meta de desenvolvimento individual medida no mid-game.
        int referenceStage = Math.Min(60, rules.MaximumStage);
        int xp = enemies * rules.ExperienceReward(referenceStage, false) + rules.ExperienceReward(referenceStage, true);
        double secondsPerCycle = (rules.WaveCount - 1) * rules.WaveInterval;
        double hours = rules.Experience.MaximumExperience * secondsPerCycle / xp / 3600;
        Check(hours >= 60 && hours <= 100, "curva fugiu da referência de longo prazo: " + hours);
    }

    private static void UpgradeTransactions()
    {
        EconomyRules rules = Rules();
        foreach (EconomyUpgradeRule upgrade in rules.Upgrades)
        {
            var novice = new GameSaveData { Gold = upgrade.BaseCost };
            string before = JsonConvert.SerializeObject(novice);
            if (upgrade.UnlockLevel > 0)
                Check(!EconomyProgression.TryBuyUpgrade(novice, rules, rules.Experience, upgrade.Kind) && JsonConvert.SerializeObject(novice) == before, "bloqueio não atômico");
            GameSaveData save = Experienced(rules); int gold = save.Gold;
            Check(EconomyProgression.TryBuyUpgrade(save, rules, rules.Experience, upgrade.Kind), "compra recusada");
            Check(save.Gold == gold - upgrade.BaseCost && EconomyProgression.Level(save, upgrade.Kind) == 1, "débito errado");
            save.Gold = 0; before = JsonConvert.SerializeObject(save);
            Check(!EconomyProgression.TryBuyUpgrade(save, rules, rules.Experience, upgrade.Kind) && JsonConvert.SerializeObject(save) == before, "compra sem saldo mudou perfil");
            save.Gold = int.MaxValue;
            for (int i = 1; i < upgrade.MaximumLevel; i++) Check(EconomyProgression.TryBuyUpgrade(save, rules, rules.Experience, upgrade.Kind), "teto prematuro");
            gold = save.Gold;
            Check(!EconomyProgression.TryBuyUpgrade(save, rules, rules.Experience, upgrade.Kind) && save.Gold == gold, "compra acima do teto");
        }
    }

    private static void CharacterPowers()
    {
        EconomyRules rules = Rules(); GameSaveData save = Experienced(rules);
        foreach (AttackPowerRule power in rules.Powers)
        {
            for (int rank = 0; rank < 5; rank++)
            {
                int before = save.Gold;
                Check(EconomyProgression.TryBuyPower(save, rules, rules.Experience, power.Id) && save.Gold == before - power.Cost(rank), "rank/custo incorreto");
            }
            Check(!EconomyProgression.TryBuyPower(save, rules, rules.Experience, power.Id), "rank acima do limite");
        }
        CharacterProgression.TryUnlock(save, "test_drill"); CharacterProgression.TryEquip(save, "test_drill");
        foreach (AttackPowerRule power in rules.Powers)
        {
            Check(EconomyProgression.PowerRank(save, "test_drill", power.Id) == 0, "poder vazou para nova broca");
            Check(!EconomyProgression.TryBuyPower(save, rules, rules.Experience, power.Id), "nível 0 comprou poder bloqueado");
        }
        Check(EconomyProgression.PowerRank(save, "pioneer", "serration") == 5, "troca apagou poderes");
        Check(EconomyProgression.TryBuyUpgrade(save, rules, rules.Experience, UpgradeKind.Special), "troca bloqueou melhoria global já liberada");
    }

    private static void StatFormulas()
    {
        EconomyRules rules = Rules(); GameSaveData save = Experienced(rules, 10);
        save.AttackLevel = 10; save.HealthLevel = 10; save.SpecialLevel = 3; save.AttackSpeedLevel = 5;
        save.CriticalChanceLevel = 10; save.CriticalDamageLevel = 10; save.RepairLevel = 2;
        DrillStats stats = EconomyProgression.Stats(save, rules, "pioneer", rules.Experience);
        Check(Math.Abs(stats.AttackDamage - 6 * Math.Pow(1.055, 10) * 1.2) < .001, "dano");
        Check(Math.Abs(stats.MaximumHealth - 120 * Math.Pow(1.055, 10) * 1.3) < .001, "vida");
        Check(Math.Abs(stats.AttackSpeed - 1.1) < .001 && Math.Abs(stats.SpecialMultiplier - 1.3) < .001, "cadência/especial");
        Check(Math.Abs(stats.CriticalChance - .15) < .0001 && Math.Abs(stats.CriticalMultiplier - 2) < .0001 && Math.Abs(stats.RepairPerSecond - .002) < .00001, "crítico/reparo");
    }

    private static ActiveRunSnapshot CombatFixture(int stage = 1, bool boss = false)
    {
        return new ActiveRunSnapshot
        {
            StageId = "prado_cristalino", CharacterId = "pioneer", CampaignStage = stage, PlayerHealth = 100,
            BossSpawned = true, WavesSpawned = 8, NextEntityId = 3, RandomState = 12345, AttackTimer = 0,
            SpecialCooldown = 100,
            Enemies = new[] {
                new EnemySnapshot { Id = 1, Health = 10000, AttackTimer = 100, IsBoss = boss, GoldReward = 8 },
                new EnemySnapshot { Id = 2, Health = 10000, AttackTimer = 100, GoldReward = 8 }
            }
        };
    }

    private static void Tick(IdleCombatSimulation sim, float duration)
    {
        for (int i = 0; i < (int)Math.Round(duration / .05f) && sim.IsActive; i++) sim.Step(.05f);
    }

    private static void PowersAffectCombat()
    {
        EconomyRules rules = Rules(); CombatConfiguration config = Configuration(rules);
        foreach (AttackPowerRule power in rules.Powers)
        {
            GameSaveData save = Experienced(rules); save.ActiveRun = CombatFixture(boss: power.Kind == AttackPowerKind.BossBreaker);
            if (power.Kind == AttackPowerKind.Resonance) { save.ActiveRun.SpecialCooldown = 0; save.ActiveRun.AttackTimer = 100; }
            var baseline = JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(save));
            Check(EconomyProgression.TryBuyPower(save, rules, rules.Experience, power.Id), "compra falhou");
            var powered = new IdleCombatSimulation(config, save, "prado_cristalino", 12345);
            var normal = new IdleCombatSimulation(config, baseline, "prado_cristalino", 12345);
            Tick(powered, 1); Tick(normal, 1);
            float poweredLoss = 20000 - powered.Enemies.Sum(x => x.Health);
            float normalLoss = 20000 - normal.Enemies.Sum(x => x.Health);
            Check(poweredLoss > normalLoss + .01f, power.Name + " não mudou dano efetivo");
            if (power.Kind == AttackPowerKind.Ricochet) Check(powered.Enemies[1].Health < normal.Enemies[1].Health, "estilhaço não atingiu outro alvo");
        }
    }

    private static void CriticalIsDeterministic()
    {
        EconomyRules rules = Rules(); GameSaveData save = Experienced(rules); save.CriticalChanceLevel = 30;
        save.ActiveRun = CombatFixture(); save.ActiveRun.Enemies[0].Health = save.ActiveRun.Enemies[1].Health = 1000000;
        var sim = new IdleCombatSimulation(Configuration(rules), save, "prado_cristalino", 12345);
        int crits = 0; sim.EventRaised += e => { if (e.Type == CombatEventType.CriticalHit) crits++; };
        Tick(sim, 60); Check(crits > 0 && crits < 80, "crítico ausente ou aplicado sempre");
    }

    private static void SpeedAffectsAttacks()
    {
        EconomyRules rules = Rules();
        int Attacks(int level)
        {
            GameSaveData save = Experienced(rules); save.AttackSpeedLevel = level; save.ActiveRun = CombatFixture();
            var sim = new IdleCombatSimulation(Configuration(rules), save, "prado_cristalino", 1); int count = 0;
            sim.EventRaised += e => { if (e.Type == CombatEventType.DrillAttack) count++; }; Tick(sim, 20); return count;
        }
        int normal = Attacks(0), fast = Attacks(25); Check(fast > normal * 1.35 && fast < normal * 1.65, "cadência fora da apresentação prometida");
    }

    private static void RepairUsesTime()
    {
        EconomyRules rules = Rules(); GameSaveData save = Experienced(rules); save.RepairLevel = 2; save.ActiveRun = CombatFixture();
        save.ActiveRun.AttackTimer = 100;
        var sim = new IdleCombatSimulation(Configuration(rules), save, "prado_cristalino", 1);
        float start = sim.PlayerHealth; Tick(sim, 3);
        Check(Math.Abs(sim.PlayerHealth - start - sim.PlayerMaximumHealth * .002f * 3) < .002f, "reparo por segundo incorreto");
    }

    private static GameSaveData WinningSave(int stage, int highest = 0)
    {
        var save = new GameSaveData { CampaignStage = stage, HighestClearedStage = highest, ActiveRun = CombatFixture(stage, true) };
        save.ActiveRun.Enemies = new[] { new EnemySnapshot { Id = 1, Health = 1, IsBoss = true, AttackTimer = 100, GoldReward = Rules().NormalGold(stage) * 10 } };
        save.ActiveRun.PendingHits = new[] { new PendingHitSnapshot { TargetEntityId = 1, Damage = 100, RemainingTravel = 0 } };
        return save;
    }

    private static void FirstClearIsAtomic()
    {
        EconomyRules rules = Rules(); GameSaveData save = WinningSave(5); bool observed = false;
        var sim = new IdleCombatSimulation(Configuration(rules), save, "prado_cristalino", 1);
        sim.EventRaised += e => {
            if (e.Type != CombatEventType.EnemyDefeated) return;
            observed = true; Check(save.Crystals == 25 && save.CampaignStage == 6 && save.HighestClearedStage == 5 &&
                save.Keys == 1 && save.Chests == 1 && save.ActiveRun == null && save.Gold == rules.NormalGold(5) * 10, "evento recebeu recompensa parcial");
        };
        Tick(sim, 1); Check(observed, "boss não morreu");
    }

    private static void RepeatClearDoesNotPayFirstClear()
    {
        GameSaveData save = WinningSave(5, 5);
        var sim = new IdleCombatSimulation(Configuration(), save, "prado_cristalino", 1); Tick(sim, 1); Tick(sim, 1);
        Check(save.Crystals == 0 && save.Keys == 1 && save.Chests == 1, "repetição duplicou primeira vitória");
    }

    private static void FinalStageRemainsBounded()
    {
        GameSaveData save = WinningSave(100, 99);
        var sim = new IdleCombatSimulation(Configuration(), save, "prado_cristalino", 1); Tick(sim, 1);
        Check(save.CampaignStage == 100 && save.HighestClearedStage == 100, "avançou além do marco");
    }

    private static void TrainingRespectsProgress()
    {
        var save = new GameSaveData(); Check(!EconomyProgression.TrySelectTrainingStage(save), "treino sem vitória");
        save.HighestClearedStage = 10; save.CampaignStage = 11; save.ActiveRun = CombatFixture(11);
        Check(!EconomyProgression.TrySelectTrainingStage(save) && save.CampaignStage == 11, "treino interrompeu combate");
        save.ActiveRun = null;
        Check(EconomyProgression.TrySelectTrainingStage(save) && save.CampaignStage == 10 && save.HighestClearedStage == 10, "treino perdeu avanço");
    }

    private static void ChestTransactions()
    {
        EconomyRules rules = Rules(); var save = new GameSaveData { Chests = 2 };
        string before = JsonConvert.SerializeObject(save);
        Check(!EconomyProgression.TryOpenChest(save, rules, out _) && JsonConvert.SerializeObject(save) == before, "baú sem chave alterou estado");
        save.Keys = 2;
        var copy = JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(save));
        Check(EconomyProgression.TryOpenChest(save, rules, out ChestOpening a), "baú válido recusado");
        Check(EconomyProgression.TryOpenChest(copy, rules, out ChestOpening b), "cópia do baú recusada");
        Check(a.Rarity == b.Rarity && a.Gold == b.Gold && save.Keys == 1 && save.Chests == 1 && save.Gold > 0 && save.LootRandomState == copy.LootRandomState, "sorteio/débito divergente");
        save.Gold = int.MaxValue; before = JsonConvert.SerializeObject(save);
        Check(!EconomyProgression.TryOpenChest(save, rules, out _) && JsonConvert.SerializeObject(save) == before, "overflow consumiu chave ou sorteio");
    }

    private static void ChestDistribution()
    {
        EconomyRules rules = Rules(); var save = new GameSaveData { Keys = 100000, Chests = 100000 };
        var counts = rules.ChestRewards.ToDictionary(x => x.Name, _ => 0);
        for (int i = 0; i < 100000; i++)
        {
            Check(EconomyProgression.TryOpenChest(save, rules, out ChestOpening opening), "sorteio recusado"); counts[opening.Rarity]++;
        }
        foreach (ChestRewardRule reward in rules.ChestRewards) Check(Math.Abs(counts[reward.Name] / 100000d - reward.Weight / 10000d) < .005, "distribuição fora de 0,5 pp");
    }

    private static void ChestValueDoesNotInflate()
    {
        EconomyRules rules = Rules();
        var original = new GameSaveData { Keys = 1, Chests = 1, HighestClearedStage = 10 };
        EconomyProgression.RecordChestStage(original, 10, 1);
        var hoarded = JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(original)); hoarded.HighestClearedStage = 60;
        Check(EconomyProgression.TryOpenChest(original, rules, out ChestOpening a), "baú de origem recusado");
        Check(EconomyProgression.TryOpenChest(hoarded, rules, out ChestOpening b), "baú guardado recusado");
        Check(a.Gold == b.Gold && original.ChestStacks.Count == 0 && hoarded.ChestStacks.Count == 0, "esperar inflou baú antigo");
    }

    private static void ShopTransactions()
    {
        EconomyRules rules = Rules(); var save = new GameSaveData();
        Check(!EconomyProgression.TryBuyKey(save, rules) && !EconomyProgression.TryBuyGoldPack(save, rules), "loja permitiu saldo negativo");
        save.Crystals = 30;
        Check(EconomyProgression.TryBuyKey(save, rules) && save.Crystals == 20 && save.Keys == 1, "preço de chave");
        Check(EconomyProgression.TryBuyGoldPack(save, rules) && save.Crystals == 0 && save.Gold == 160, "pacote de ouro");
        save.Crystals = 20; save.Gold = int.MaxValue;
        Check(!EconomyProgression.TryBuyGoldPack(save, rules) && save.Crystals == 20, "overflow gastou cristais");
    }

    private static void InDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dw-economy-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory); try { action(directory); } finally { Directory.Delete(directory, true); }
    }

    private static void Envelope(string path, JObject payload, int version)
    {
        string text = payload.ToString(Formatting.None);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
        File.WriteAllText(path, JsonConvert.SerializeObject(new { SchemaVersion = version, Payload = text, Sha256 = hash }));
    }

    private static void SchemaTwoMigration() => InDirectory(directory =>
    {
        GameSaveData original = Experienced(Rules()); original.Gold = 1000; original.Keys = 3; original.Chests = 4; original.AttackLevel = 10;
        JObject payload = JObject.FromObject(original); payload["SchemaVersion"] = 2;
        foreach (string field in new[] { "Crystals", "CampaignStage", "HighestClearedStage", "LootRandomState", "SpecialLevel", "AttackSpeedLevel", "RepairLevel" }) payload.Remove(field);
        ((JObject)payload["Characters"][0]).Remove("Powers");
        string path = Path.Combine(directory, "dig-and-wonder.save.json"); Envelope(path, payload, 2); string bytes = File.ReadAllText(path);
        var repository = new AtomicSaveRepository(directory);
        Check(repository.TryLoad(out GameSaveData loaded, out _) && loaded.SchemaVersion == 3 && loaded.Gold == 1000 && loaded.Keys == 3 && loaded.Chests == 4 && loaded.AttackLevel == 10 &&
            loaded.Characters[0].TotalExperience == original.Characters[0].TotalExperience && loaded.CampaignStage == 1 && loaded.Crystals == 0, "migração perdeu dados");
        Check(File.ReadAllText(path) == bytes, "leitura escreveu sobre legado"); repository.Save(loaded);
        Check(File.ReadAllText(path + ".bak") == bytes, "backup perdeu original");
    });

    private static void SchemaThreeValidation() => InDirectory(directory =>
    {
        var repository = new AtomicSaveRepository(directory); GameSaveData save = Experienced(Rules());
        save.Characters[0].Powers.Add(new PowerProgressData { PowerId = "serration", Rank = 1 });
        save.Chests = 2; EconomyProgression.RecordChestStage(save, 10, 2);
        repository.Save(save); repository.Save(save);
        JObject broken = JObject.FromObject(save); broken.Remove("Crystals");
        Envelope(Path.Combine(directory, "dig-and-wonder.save.json"), broken, 3);
        Check(repository.TryLoad(out GameSaveData loaded, out bool recovered) && recovered && loaded.Characters[0].Powers.Count == 1 &&
            loaded.ChestStacks.Count == 1 && loaded.ChestStacks[0].Stage == 10 && loaded.ChestStacks[0].Count == 2, "recuperação perdeu carteira ou procedência dos baús");
        save.Characters[0].Powers.Add(new PowerProgressData { PowerId = "serration", Rank = 1 });
        bool rejected = false; try { repository.Save(save); } catch (InvalidDataException) { rejected = true; }
        Check(rejected, "poder duplicado aceito");
    });

    private static void MidgameResume() => InDirectory(directory =>
    {
        EconomyRules rules = Rules(); GameSaveData original = Experienced(rules); original.CampaignStage = 20; original.AttackLevel = 20; original.HealthLevel = 20;
        original.CriticalChanceLevel = 10; original.AttackSpeedLevel = 10; original.RepairLevel = 2;
        foreach (AttackPowerRule power in rules.Powers) EconomyProgression.TryBuyPower(original, rules, rules.Experience, power.Id);
        CombatConfiguration config = Configuration(rules);
        var continuous = new IdleCombatSimulation(config, original, "prado_cristalino", 54321); Tick(continuous, 9.1f);
        original.ActiveRun = continuous.CaptureRun(); var repository = new AtomicSaveRepository(directory); repository.Save(original);
        Check(repository.TryLoad(out GameSaveData loaded, out _), "save não reabriu");
        var resumed = new IdleCombatSimulation(config, loaded, "prado_cristalino", 1);
        Tick(continuous, 30); Tick(resumed, 30);
        original.ActiveRun = continuous.IsActive ? continuous.CaptureRun() : null;
        loaded.ActiveRun = resumed.IsActive ? resumed.CaptureRun() : null;
        Check(JsonConvert.SerializeObject(original) == JsonConvert.SerializeObject(loaded), "retomada divergiu de combate contínuo");
    });

    internal static int WriteSimulationReports()
    {
        EconomyRules rules = Rules(); CombatConfiguration config = Configuration(rules);
        string output = Path.Combine(Root, "docs/balance"); Directory.CreateDirectory(output);
        var curves = new List<string> { "phase,normal_hp,boss_hp,normal_damage,normal_gold,boss_gold,normal_xp,boss_xp,first_clear_crystals" };
        for (int phase = 1; phase <= rules.MaximumStage; phase++)
            curves.Add(FormattableString.Invariant($"{phase},{rules.EnemyHealth(phase):F2},{rules.EnemyHealth(phase) * rules.BossHealthMultiplier:F2},{rules.EnemyDamage(phase):F2},{rules.NormalGold(phase)},{rules.NormalGold(phase) * rules.BossGoldMultiplier},{rules.ExperienceReward(phase, false)},{rules.ExperienceReward(phase, true)},{rules.FirstClearCrystals + (phase % rules.MilestoneInterval == 0 ? rules.MilestoneCrystals : 0)}"));
        File.WriteAllLines(Path.Combine(output, "campaign-0.0.20.csv"), curves);
        var xp = new List<string> { "level,xp_to_next,total_xp" }; long total = 0;
        for (int level = 0; level <= 60; level++)
        {
            xp.Add($"{level},{(level < 60 ? rules.Experience.RequiredForNextLevel(level) : 0)},{total}");
            if (level < 60) total += rules.Experience.RequiredForNextLevel(level);
        }
        File.WriteAllLines(Path.Combine(output, "character-xp-0.0.20.csv"), xp);
        var upgrades = new List<string> { "kind,level,cost,cumulative_cost,unlock_character_level" };
        foreach (EconomyUpgradeRule upgrade in rules.Upgrades)
        {
            long cumulative = 0;
            for (int level = 0; level < upgrade.MaximumLevel; level++)
            {
                upgrades.Add($"{upgrade.Kind},{level},{upgrade.Cost(level)},{cumulative},{upgrade.UnlockLevel}"); cumulative += upgrade.Cost(level);
            }
        }
        File.WriteAllLines(Path.Combine(output, "upgrade-costs-0.0.20.csv"), upgrades);
        var results = new List<string> { "strategy,seed,phase,result,simulated_minutes,attempts,character_level,attack_level,health_level,gold,crystals,gold_earned,gold_spent,powers_bought,active_character,next_character" };
        foreach (string strategy in new[] { "balanced", "attack", "survival" })
            foreach (uint seed in new uint[] { 12345, 54321, 98765 }) Simulate(config, strategy, seed, results);
        File.WriteAllLines(Path.Combine(output, "simulation-0.0.20.csv"), results);
        WriteLongTermExperienceReport(config, output);
        Console.WriteLine("Relatórios gerados em docs/balance; tempos simulados, sem anúncios, sem dinheiro real e sem usar cristais.");
        foreach (string strategy in new[] { "balanced", "attack", "survival" })
            Console.WriteLine(results.Last(x => x.StartsWith(strategy + ",")));
        bool incomplete = new[] { "balanced", "attack", "survival" }.Any(strategy => new uint[] { 12345, 54321, 98765 }.Any(seed =>
            !results.Any(x => x.StartsWith(strategy + "," + seed + "," + rules.MaximumStage + ",clear,"))));
        return incomplete || results.Any(x => x.Contains(",blocked,")) ? 1 : 0;
    }

    private static void WriteLongTermExperienceReport(CombatConfiguration config, string output)
    {
        EconomyRules rules = config.Economy;
        // A projeção de XP continua comparável ao mid-game na fase 60. O relatório
        // de campanha separado percorre o novo fim em 100.
        int farmStage = Math.Min(60, rules.MaximumStage);
        var save = new GameSaveData { CampaignStage = farmStage, HighestClearedStage = farmStage, LootRandomState = 54321 };
        CharacterProgression.GrantStageRewards(save, config.Characters);
        CharacterProgression.TryEquip(save, rules.CharacterRoster.Where(x => x.UnlockStage <= farmStage).OrderBy(x => x.UnlockStage).Last().Id);
        CharacterProgressData characterProgress = CharacterProgression.Find(save, save.EquippedCharacterId);
        save.AttackLevel = rules.Upgrade(UpgradeKind.Attack).MaximumLevel;
        save.HealthLevel = rules.Upgrade(UpgradeKind.Health).MaximumLevel;
        save.SpecialLevel = rules.Upgrade(UpgradeKind.Special).MaximumLevel;
        save.CriticalChanceLevel = rules.Upgrade(UpgradeKind.CriticalChance).MaximumLevel;
        save.CriticalDamageLevel = rules.Upgrade(UpgradeKind.CriticalDamage).MaximumLevel;
        save.AttackSpeedLevel = rules.Upgrade(UpgradeKind.AttackSpeed).MaximumLevel;
        save.RepairLevel = rules.Upgrade(UpgradeKind.Repair).MaximumLevel;
        foreach (AttackPowerRule power in rules.Powers) characterProgress.Powers.Add(new PowerProgressData { PowerId = power.Id, Rank = power.MaximumRank });
        var rows = new List<string> { "level,total_xp,projected_active_hours,measured_cycle_seconds,measured_xp_per_cycle,cycles_projected" };
        double elapsed = 0; int cycles = 0;
        rows.Add("0,0,0,0,0,0");
        for (int level = 0; level < rules.Experience.MaximumLevel; level++)
        {
            long target = 0;
            for (int i = 0; i <= level; i++) target += rules.Experience.RequiredForNextLevel(i);
            long before = characterProgress.TotalExperience;
            var sim = new IdleCombatSimulation(config, save, "prado_cristalino", 54321u + (uint)level);
            int ticks = 0;
            while (sim.IsActive && ticks < 7200) { sim.Step(.05f); ticks++; }
            if (!sim.Run.IsStageCleared) throw new InvalidOperationException("Cenário de farm favorável não concluiu a fase " + farmStage + ".");
            long gained = characterProgress.TotalExperience - before;
            if (gained < 1) throw new InvalidOperationException("Ciclo sem XP.");
            int extraCycles = (int)Math.Ceiling(Math.Max(0, target - characterProgress.TotalExperience) / (double)gained);
            if (extraCycles > 0) CharacterProgression.Award(save, save.EquippedCharacterId, rules.Experience, checked((int)(gained * extraCycles)));
            int currentCycles = extraCycles + 1;
            elapsed += sim.Run.ElapsedSeconds * currentCycles; cycles += currentCycles;
            int measuredLevel = CharacterProgression.GetLevel(characterProgress, rules.Experience).Level;
            rows.Add(FormattableString.Invariant($"{measuredLevel},{characterProgress.TotalExperience},{elapsed / 3600:F3},{sim.Run.ElapsedSeconds:F2},{gained},{cycles}"));
            // Farma sempre a mesma fase de referência; a vitória normalmente aponta
            // para a fase seguinte e isso não deve alterar a projeção.
            save.CampaignStage = farmStage;
            save.HighestClearedStage = farmStage;
            // Um único ciclo pode pular níveis iniciais; segue do nível realmente alcançado.
            level = measuredLevel - 1;
        }
        File.WriteAllLines(Path.Combine(output, "long-term-xp-0.0.20.csv"), rows);
        Console.WriteLine("XP longo prazo: " + rows.Last() + " (projeção por ciclos medidos, oficina/poderes no máximo desde o início; não é partida humana)");
    }

    private static void Simulate(CombatConfiguration config, string strategy, uint seed, List<string> report)
    {
        EconomyRules rules = config.Economy; var save = new GameSaveData { LootRandomState = seed };
        int attempts = 0, powersBought = 0; double time = 0; long goldEarned = 0, goldSpent = 0;
        var stageFailures = new Dictionary<int, int>();
        while (save.HighestClearedStage < rules.MaximumStage && time < 21600)
        {
            int phase = save.CampaignStage; attempts++;
            bool training = phase <= save.HighestClearedStage;
            var sim = new IdleCombatSimulation(config, save, save.UnlockedStageId, seed + (uint)attempts);
            sim.EventRaised += e => { if (e.Type == CombatEventType.GoldAwarded) goldEarned += (int)e.Amount; };
            int ticks = 0;
            while (sim.IsActive && ticks < 7200)
            {
                sim.Step(.05f); ticks++; time += .05;
                if (ticks % 40 == 0)
                {
                    int before = save.Gold;
                    Spend(sim, rules, strategy, ref powersBought);
                    goldSpent += before - save.Gold;
                }
            }
            int beforeChest = save.Gold;
            while (EconomyProgression.TryOpenChest(save, rules, out _)) { }
            goldEarned += save.Gold - beforeChest;
            string foughtCharacter = sim.ActiveCharacterId;
            int level = CharacterProgression.GetLevel(CharacterProgression.Find(save, foughtCharacter), rules.Experience).Level;
            if (!sim.IsActive)
            {
                CharacterCombatData next = config.Characters.Where(x => x.Id != "test_drill" && CharacterProgression.Find(save, x.Id) != null).OrderBy(x => x.UnlockStage).Last();
                CharacterProgression.TryEquip(save, next.Id);
            }
            int beforePurchase = save.Gold; Spend(sim, rules, strategy, ref powersBought); goldSpent += beforePurchase - save.Gold;
            if (sim.Run.IsPlayerDefeated) stageFailures[phase] = stageFailures.GetValueOrDefault(phase) + 1;
            bool blocked = stageFailures.GetValueOrDefault(phase) >= 20 || sim.IsActive;
            report.Add(FormattableString.Invariant($"{strategy},{seed},{phase},{(blocked ? "blocked" : sim.Run.IsStageCleared ? training ? "training" : "clear" : "defeat")},{time / 60:F2},{attempts},{level},{save.AttackLevel},{save.HealthLevel},{save.Gold},{save.Crystals},{goldEarned},{goldSpent},{powersBought},{foughtCharacter},{save.EquippedCharacterId}"));
            if (blocked) break;
            if (sim.Run.IsPlayerDefeated) EconomyProgression.TrySelectTrainingStage(save);
        }
    }

    private static void Spend(IdleCombatSimulation sim, EconomyRules rules, string strategy, ref int powersBought)
    {
        GameSaveData save = sim.Profile;
        int level = CharacterProgression.GetLevel(CharacterProgression.Find(save, save.EquippedCharacterId), rules.Experience).Level;
        int workshopLevel = EconomyProgression.WorkshopLevel(save, rules.Experience);
        for (int count = 0; count < 5; count++)
        {
            // Estratégias explícitas e repetíveis. Não representam comportamento humano medido.
            double best = 0; EconomyUpgradeRule selected = null; AttackPowerRule selectedPower = null;
            foreach (EconomyUpgradeRule upgrade in rules.Upgrades)
            {
                int rank = EconomyProgression.Level(save, upgrade.Kind);
                if (workshopLevel < upgrade.UnlockLevel || rank >= upgrade.MaximumLevel) continue;
                double gain = upgrade.Kind switch
                {
                    UpgradeKind.Attack => .055,
                    UpgradeKind.Health => .055 * (strategy == "survival" ? 1.6 : strategy == "attack" ? .3 : .8),
                    UpgradeKind.Special => .04 / (1 + .1 * rank),
                    UpgradeKind.CriticalChance => .01 * (sim.CurrentStats.CriticalMultiplier - 1) / (1 + sim.CurrentStats.CriticalChance * (sim.CurrentStats.CriticalMultiplier - 1)),
                    UpgradeKind.CriticalDamage => .05 * sim.CurrentStats.CriticalChance / (1 + sim.CurrentStats.CriticalChance * (sim.CurrentStats.CriticalMultiplier - 1)),
                    UpgradeKind.AttackSpeed => .012 / (1 + .02 * rank),
                    UpgradeKind.Repair => .03 * (strategy == "survival" ? 1.6 : .8),
                    _ => 0
                };
                if (upgrade.Kind == UpgradeKind.Health && sim.IsActive && sim.PlayerHealth < sim.PlayerMaximumHealth * .6) gain *= 4;
                double score = gain / upgrade.Cost(rank);
                if (score > best) { best = score; selected = upgrade; selectedPower = null; }
            }
            foreach (AttackPowerRule power in rules.Powers)
            {
                int rank = EconomyProgression.PowerRank(save, save.EquippedCharacterId, power.Id);
                if (level < power.UnlockLevel || rank >= power.MaximumRank) continue;
                double weight = power.Kind == AttackPowerKind.Resonance ? .4 : power.Kind == AttackPowerKind.Serration ? .6 : .3;
                double score = power.Bonus * weight / (1 + power.Bonus * rank) / power.Cost(rank);
                if (score > best) { best = score; selectedPower = power; selected = null; }
            }
            if (selected != null)
            {
                if (!sim.TryPurchaseUpgradeAtBase(selected.Kind)) break;
            }
            else if (selectedPower != null)
            {
                if (!EconomyProgression.TryBuyPower(save, rules, rules.Experience, selectedPower.Id)) break;
                powersBought++;
            }
            else break;
        }
    }
}
