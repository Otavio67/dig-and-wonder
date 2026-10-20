using DigAndWonder;
using Newtonsoft.Json;

internal static class CharacterCampaignChecks
{
    internal static IEnumerable<(string Name, Action Test)> All()
    {
        yield return ("campanha: catálogo cobre recompensas das fases 10 a 100", CatalogCoversMilestones);
        yield return ("campanha: nível alto da broca não libera recompensa de fase", ExperienceDoesNotUnlockCharacters);
        yield return ("campanha: broca nova vem zerada e duplicata conserva XP e poderes", MilestonesKeepIndividualProgress);
        yield return ("campanha: chefe concede broca e recompensa antes dos eventos/save", BossUnlockIsAtomic);
        yield return ("campanha: entrar ou perder na fase 10 não concede broca", DefeatDoesNotUnlock);
        yield return ("campanha: perfis anteriores recebem marcos vencidos sem reset", ReconcileExistingCampaign);
        yield return ("campanha: nova broca zero tem bases melhores que a anterior nível 10", NewDrillsHaveBetterBases);
        yield return ("campanha: trocas de marco melhoram vida e cadência efetiva modeladas", MilestoneSwapsAreWorthwhile);
        yield return ("campanha: Centelha dispara três projéteis na rajada", SparkUsesThreeShots);
        yield return ("campanha: Prisma possui estilhaço sem comprar poder", PrismHasInnateRicochet);
        yield return ("campanha: Sísmica aplica especial em três alvos", SeismicHasAreaSpecial);
        yield return ("campanha: Singular encerra o catálogo com padrão próprio e nível zero", FinalDrillHasEndgamePattern);
        yield return ("campanha: dificuldade aumenta nas fases 5/10 e XP cresce por fase", DifficultyAndExperienceGrow);
        yield return ("campanha: save retoma outra broca com projéteis e XP independente", CharacterRunResumes);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Tick(IdleCombatSimulation simulation, float duration)
    {
        for (int i = 0; i < (int)Math.Round(duration / .05f) && simulation.IsActive; i++) simulation.Step(.05f);
    }

    private static void CatalogCoversMilestones()
    {
        EconomyRules rules = EconomyChecks.Rules();
        Check(rules.CharacterRoster.Length == 11, "catálogo incompleto");
        Check(rules.CharacterRoster.Where(x => x.UnlockStage > 0).Select(x => x.UnlockStage).OrderBy(x => x).SequenceEqual(new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }), "marcos incorretos");
        string directory = Path.Combine(EconomyChecks.Root, "UnityProject/Assets/_Game/Resources/Data/Characters");
        foreach (CharacterCombatData character in rules.CharacterRoster)
        {
            Check(Directory.GetFiles(directory, "*.asset").Any(path => File.ReadAllLines(path).Contains("  id: " + character.Id)), "ScriptableObject ausente: " + character.Id);
        }
    }

    private static void ExperienceDoesNotUnlockCharacters()
    {
        EconomyRules rules = EconomyChecks.Rules(); var save = new GameSaveData();
        CharacterProgression.Award(save, "pioneer", rules.Experience, int.MaxValue);
        Check(CharacterProgression.GrantStageRewards(save, rules.CharacterRoster).Length == 0 && save.Characters.Count == 1, "XP liberou broca sem vencer fase");
    }

    private static void MilestonesKeepIndividualProgress()
    {
        EconomyRules rules = EconomyChecks.Rules(); var save = new GameSaveData { HighestClearedStage = 9 };
        Check(CharacterProgression.GrantStageRewards(save, rules.CharacterRoster).Length == 0, "prêmio antecipado");
        save.HighestClearedStage = 10;
        Check(CharacterProgression.GrantStageRewards(save, rules.CharacterRoster).SequenceEqual(new[] { "rivet" }), "fase 10 não concedeu Rebite");
        CharacterProgressData rivet = CharacterProgression.Find(save, "rivet");
        Check(rivet.TotalExperience == 0 && rivet.Powers.Count == 0 && save.EquippedCharacterId == "pioneer", "nova broca herdou progresso ou equipou sozinha");
        CharacterProgression.Award(save, "rivet", rules.Experience, 500);
        rivet.Powers.Add(new PowerProgressData { PowerId = "serration", Rank = 2 });
        Check(CharacterProgression.GrantStageRewards(save, rules.CharacterRoster).Length == 0 && rivet.TotalExperience == 500 && rivet.Powers[0].Rank == 2, "repetição resetou progresso");
    }

    private static GameSaveData BossFixture(int stage)
    {
        return new GameSaveData
        {
            CampaignStage = stage, HighestClearedStage = stage - 1,
            ActiveRun = new ActiveRunSnapshot
            {
                StageId = "prado_cristalino", CampaignStage = stage, PlayerHealth = 100,
                AttackTimer = 100, SpecialCooldown = 100, BossSpawned = true, NextEntityId = 2, RandomState = 12345,
                Enemies = new[] { new EnemySnapshot { Id = 1, IsBoss = true, Health = 1, AttackTimer = 100, GoldReward = 150 } },
                PendingHits = new[] { new PendingHitSnapshot { TargetEntityId = 1, Damage = 100, RemainingTravel = 0 } }
            }
        };
    }

    private static void BossUnlockIsAtomic()
    {
        string directory = Path.Combine(Path.GetTempPath(), "dw-milestone-" + Guid.NewGuid().ToString("N"));
        try
        {
            var save = BossFixture(10); CombatConfiguration config = EconomyChecks.Configuration();
            var repository = new AtomicSaveRepository(directory);
            var sim = new IdleCombatSimulation(config, save, "prado_cristalino", 1); int unlocks = 0;
            sim.EventRaised += e => {
                if (e.Type != CombatEventType.CharacterUnlocked) return;
                unlocks++;
                Check(e.Text == "rivet" && save.ActiveRun == null && save.HighestClearedStage == 10 && save.CampaignStage == 11 &&
                    save.Gold == 150 && save.Keys == 1 && save.Chests == 1 && save.Crystals == 25 &&
                    CharacterProgression.Find(save, "pioneer").TotalExperience > 0 && CharacterProgression.Find(save, "rivet").TotalExperience == 0,
                    "evento recebeu transação parcial");
                repository.Save(save);
            };
            Tick(sim, 1); Check(unlocks == 1, "evento duplicado/ausente");
            Check(repository.TryLoad(out GameSaveData loaded, out _) && CharacterProgression.Find(loaded, "rivet") != null, "save perdeu nova broca");
            Check(CharacterProgression.TryEquip(loaded, "rivet"), "troca na base falhou");
            var next = new IdleCombatSimulation(config, loaded, loaded.UnlockedStageId, 2);
            Check(next.ActiveCharacterId == "rivet", "nova tentativa usou broca anterior");
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    private static void DefeatDoesNotUnlock()
    {
        GameSaveData save = BossFixture(10);
        save.ActiveRun.PendingHits = new[] { new PendingHitSnapshot { SourceEntityId = 1, Damage = 1000, TargetsPlayer = true } };
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 1); Tick(sim, 1);
        Check(sim.Run.IsPlayerDefeated && CharacterProgression.Find(save, "rivet") == null && save.HighestClearedStage == 9, "derrota concedeu recompensa");
    }

    private static void ReconcileExistingCampaign()
    {
        var save = new GameSaveData { HighestClearedStage = 40, CampaignStage = 41 };
        save.Characters[0].TotalExperience = 1000;
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 1);
        Check(save.Characters.Count == 5 && save.Characters[0].TotalExperience == 1000 && save.Characters.Skip(1).All(x => x.TotalExperience == 0), "reconciliação errada");
        Check(CharacterProgression.GrantStageRewards(save, sim.Economy.CharacterRoster).Length == 0, "reconciliação duplicou prêmio");
    }

    private static void NewDrillsHaveBetterBases()
    {
        EconomyRules rules = EconomyChecks.Rules(); CharacterCombatData[] catalog = rules.CharacterRoster.OrderBy(x => x.UnlockStage).ToArray();
        for (int i = 1; i < catalog.Length; i++)
        {
            Check(catalog[i].AttackMultiplier > catalog[i - 1].AttackMultiplier * 1.2f &&
                catalog[i].HealthMultiplier >= catalog[i - 1].HealthMultiplier * 1.3f - .001f, "troca sem ganho de base: " + catalog[i].Id);
        }
    }

    private static IdleCombatSimulation PatternSimulation(string id, bool special, int highestClearedStage = 60)
    {
        CombatConfiguration config = EconomyChecks.Configuration();
        var save = new GameSaveData { HighestClearedStage = highestClearedStage };
        CharacterProgression.GrantStageRewards(save, config.Characters); CharacterProgression.TryEquip(save, id);
        save.ActiveRun = new ActiveRunSnapshot
        {
            StageId = "prado_cristalino", CharacterId = id, PlayerHealth = 100,
            AttackTimer = special ? 100 : 0, SpecialCooldown = special ? 0 : 100,
            BossSpawned = true, NextEntityId = 4, RandomState = 12345,
            Enemies = Enumerable.Range(1, 3).Select(n => new EnemySnapshot { Id = n, Health = 10000, GoldReward = 8, AttackTimer = 100 }).ToArray()
        };
        return new IdleCombatSimulation(config, save, "prado_cristalino", 12345);
    }

    private static void MilestoneSwapsAreWorthwhile()
    {
        EconomyRules rules = EconomyChecks.Rules(); CharacterCombatData[] catalog = rules.CharacterRoster.OrderBy(x => x.UnlockStage).ToArray();
        int[] modeledPreviousLevels = { 11, 12, 14, 17, 20, 22, 24, 26, 28, 30 };
        double BasicRate(CharacterCombatData character) => character.BasicHitCount * character.ProjectileDamageMultiplier * character.SpeedMultiplier /
            (1.25 + (character.BasicHitCount - 1) * .28);
        for (int i = 1; i < catalog.Length; i++)
        {
            int level = modeledPreviousLevels[i - 1];
            double previousDamage = catalog[i - 1].AttackMultiplier * (1 + rules.CharacterDamagePerLevel * level) * BasicRate(catalog[i - 1]);
            double previousHealth = catalog[i - 1].HealthMultiplier * (1 + rules.CharacterHealthPerLevel * level);
            Check(catalog[i].AttackMultiplier * BasicRate(catalog[i]) > previousDamage && catalog[i].HealthMultiplier > previousHealth,
                "nova broca sem ganho no caso de troca: " + catalog[i].Id);
        }
    }

    private static void SparkUsesThreeShots()
    {
        var sim = PatternSimulation("spark", false); int shots = 0;
        sim.EventRaised += e => { if (e.Type == CombatEventType.DrillAttack) shots++; };
        Tick(sim, .65f); Check(shots == 3, "rajada não contém três tiros");
    }

    private static void PrismHasInnateRicochet()
    {
        var sim = PatternSimulation("prism", false); Tick(sim, .5f);
        Check(sim.Enemies[1].Health < 10000 && sim.Profile.Characters.All(x => x.Powers.Count == 0), "inato depende de compra ou não atingiu segundo alvo");
    }

    private static void SeismicHasAreaSpecial()
    {
        var sim = PatternSimulation("seismic", true); Tick(sim, .15f);
        Check(sim.Enemies.All(x => x.Health < 10000), "especial não atingiu três alvos");
    }

    private static void FinalDrillHasEndgamePattern()
    {
        EconomyRules rules = EconomyChecks.Rules();
        var save = new GameSaveData { HighestClearedStage = rules.MaximumStage };
        string[] granted = CharacterProgression.GrantStageRewards(save, rules.CharacterRoster);
        CharacterProgressData finalProgress = CharacterProgression.Find(save, "singularity");
        CharacterCombatData finalCharacter = rules.CharacterRoster.First(x => x.Id == "singularity");
        Check(granted.Contains("singularity") && finalProgress != null && finalProgress.TotalExperience == 0 &&
            finalCharacter.BasicHitCount == 3 && finalCharacter.SpecialTargetCount == 3 &&
            Math.Abs(finalCharacter.InnateRicochet - .3) < .0001, "recompensa final ou padrão incorreto");
        CharacterProgression.TryEquip(save, "singularity");
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(rules), save, "prado_cristalino", 12345);
        int shots = 0; sim.EventRaised += e => { if (e.Type == CombatEventType.DrillAttack) shots++; };
        Tick(sim, 1f);
        Check(sim.ActiveCharacterId == "singularity" && shots > 0, "broca final não entrou no combate: " + sim.ActiveCharacterId + "/" + shots);
    }

    private static void DifficultyAndExperienceGrow()
    {
        EconomyRules rules = EconomyChecks.Rules();
        Check(rules.DifficultyTier(4) == 0 && rules.DifficultyTier(5) == 1 && rules.DifficultyTier(9) == 1 && rules.DifficultyTier(10) == 2, "degrau deslocado");
        Check(Math.Abs(rules.EnemyHealth(5) / rules.EnemyHealth(4) - 1.115 * 1.12) < .00001 &&
            Math.Abs(rules.EnemyDamage(5) / rules.EnemyDamage(4) - 1.10 * 1.08) < .00001, "multiplicador de degrau incorreto");
        Check(rules.EnemyHealth(61) > rules.EnemyHealth(60) && rules.EnemyDamage(61) > rules.EnemyDamage(60) &&
            rules.EnemyHealth(61) / rules.EnemyHealth(60) < 1.10,
            "compressão do end game inválida: " + rules.EnemyHealth(60) + "/" + rules.EnemyHealth(61) + "/" + rules.EnemyDamage(60) + "/" + rules.EnemyDamage(61));
        for (int stage = 2; stage <= 100; stage++)
            Check(rules.ExperienceReward(stage, false) > rules.ExperienceReward(stage - 1, false) &&
                rules.ExperienceReward(stage, true) > rules.ExperienceReward(stage - 1, true), "XP regrediu com a fase");
        Check(rules.Experience.RequiredForNextLevel(30) > rules.Experience.RequiredForNextLevel(20), "requisito de XP não cresce");
    }

    private static void CharacterRunResumes()
    {
        EconomyRules rules = EconomyChecks.Rules(); CombatConfiguration config = EconomyChecks.Configuration(rules);
        var save = new GameSaveData { HighestClearedStage = 20, CampaignStage = 21, HealthLevel = 30 };
        CharacterProgression.GrantStageRewards(save, config.Characters); CharacterProgression.TryEquip(save, "spark");
        var sim = new IdleCombatSimulation(config, save, "prado_cristalino", 12345); Tick(sim, .15f);
        save.ActiveRun = sim.CaptureRun();
        Check(save.ActiveRun.PendingHits.Length > 0, "fixture não capturou projétil");
        GameSaveData loaded = JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(save));
        var resumed = new IdleCombatSimulation(config, loaded, "prado_cristalino", 999);
        Tick(sim, 20); Tick(resumed, 20);
        save.ActiveRun = sim.IsActive ? sim.CaptureRun() : null;
        loaded.ActiveRun = resumed.IsActive ? resumed.CaptureRun() : null;
        Check(JsonConvert.SerializeObject(save) == JsonConvert.SerializeObject(loaded) &&
            CharacterProgression.Find(save, "pioneer").TotalExperience == 0, "retomada divergiu ou XP foi para outra broca");
    }
}
