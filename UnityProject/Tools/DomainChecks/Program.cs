using DigAndWonder;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

var checks = new (string Name, Action Test)[]
{
    ("abertura: recursos carregáveis existem sem caminhos ambíguos", StartupAssetsExist),
    ("estatísticas: tiros, mortes e retomada", RunStatisticsPersist),
    ("economia insuficiente não compra upgrade", InsufficientGoldDoesNotPurchase),
    ("preço e aumento de dano do upgrade", AttackUpgradeUsesCatalogFormula),
    ("upgrade de vida aplica valor e custo do catálogo", HealthUpgradeUsesCatalogFormula),
    ("oficina compra melhoria entre tentativas", BaseUpgradeUsesSameEconomy),
    ("ouro por inimigo só uma vez", EnemyGoldIsAwardedOnce),
    ("projétil respeita viagem e alvo removido", ProjectilesRespectTravelAndTargetIdentity),
    ("especial preserva quatro golpes e dano total", PioneerSpecialKeepsFourStrikes),
    ("boss libera fase e baú com chaves", BossAloneClearsStage),
    ("retomada restaura combate e projéteis", ResumeRestoresRun),
    ("save usa backup validado após corrupção", SaveRecoversValidatedBackup),
    ("vitória interrompe dano pendente no mesmo passo", VictoryStopsPendingLethalDamage),
    ("especial encerra ações ao derrotar o boss", SpecialVictoryStopsFurtherActions),
    ("derrota limpa ações e reinício preserva perfil", DefeatClearsActionsBeforeRestart),
    ("evento de boss entrega estado completo de recompensa", BossRewardEventIsConsistent),
    ("save null válido por checksum recupera backup", NullPayloadRecoversBackup),
    ("primeira gravação interrompida recupera temporário", InterruptedFirstSaveRecoversTemporary),
    ("temporário inválido impede reset silencioso", InvalidTemporaryCountsAsExistingSave),
    ("snapshot inválido é rejeitado antes da escrita", InvalidSnapshotIsRejected),
    ("save principal válido prevalece sobre temporário", ConfirmedPrimaryWinsOverTemporary),
    ("arquivos inválidos são preservados sem reset", InvalidCopiesRemainUntouched),
    ("personagem inicial começa no nível zero", CharacterStartsAtZero),
    ("brocas têm XP independente e duplicatas não reiniciam", CharactersKeepIndependentExperience),
    ("troca exige posse e aguarda fim da tentativa", CharacterEquipRespectsRun),
    ("trocar e voltar preserva nível de cada broca", SwitchingCharactersPreservesProgress),
    ("XP pertence à broca registrada na tentativa", ExperienceBelongsToRunCharacter),
    ("inimigo paga XP uma vez e dano não paga XP", EnemyExperienceIsAwardedOnce),
    ("chefe paga XP e níveis antes dos eventos de recompensa", BossExperienceIsAtomic),
    ("subida de nível conserva excedente e emite evento", LevelUpKeepsOverflow),
    ("limite de XP não transborda nem reduz progresso", ExperienceCapDoesNotOverflow),
    ("derrota e reinício preservam XP", DefeatKeepsCharacterExperience),
    ("save retomado conserva XP e autoria de projéteis", ResumedRunKeepsExperienceOwnership),
    ("arquivo atômico retoma XP e projéteis sem duplicar recompensa", AtomicFileResumesCharacterExperience),
    ("save 1 migra sem perder economia ou tentativa", LegacySaveMigratesCharacterProgress),
    ("backup recupera XP de múltiplas personagens", CharacterExperienceRecoversFromBackup),
    ("save rejeita XP negativo, duplicatas e personagem inválida", InvalidCharacterProgressIsRejected),
    ("schema 2 incompleto não reinicia XP silenciosamente", MissingCharacterExperienceIsRejected),
    ("versão malformada recupera backup sem exceção", InvalidSchemaValueRecoversBackup),
    ("versão futura de save permanece intacta", FutureCharacterSaveIsPreserved),
    ("mapeamento preserva os números dos catálogos de conteúdo", CatalogNumbersMatchMappedData)
};

checks = checks.Concat(EconomyChecks.All()).ToArray();
checks = checks.Concat(CharacterCampaignChecks.All()).ToArray();
checks = checks.Concat(GameplayHudChecks.All()).ToArray();
checks = checks.Concat(DiscoveryCombatChecks.All()).ToArray();
if (args.Contains("--economy-report")) return EconomyChecks.WriteSimulationReports();

int failed = 0;
foreach ((string name, Action test) in checks)
{
    try
    {
        test();
        Console.WriteLine("PASS " + name);
    }
    catch (Exception exception)
    {
        failed++;
        Console.WriteLine("FAIL " + name + ": " + exception.Message);
    }
}

return failed == 0 ? 0 : 1;

static void StartupAssetsExist()
{
    string root = Path.Combine(EconomyChecks.Root, "UnityProject/Assets/_Game/Resources");
    string[] paths = StartupContent.ResourcePaths.ToArray();
    Assert(paths.Distinct(StringComparer.Ordinal).Count() == paths.Length, "recurso duplicado na abertura");
    foreach (string path in paths)
    {
        string[] matches = new[] { ".asset", ".png", ".jpeg", ".shader", ".ttf" }
            .Select(extension => Path.Combine(root, path + extension)).Where(File.Exists).ToArray();
        Assert(matches.Length == 1, "Resources ausente ou ambíguo: " + path);
        Assert(File.Exists(matches[0] + ".meta"), "metadado de importação ausente: " + path);
    }
}

static void InsufficientGoldDoesNotPurchase()
{
    GameSaveData save = new GameSaveData();
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 100);
    Assert(!simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "compra sem saldo foi aceita");
    Assert(save.Gold == 0 && save.AttackLevel == 0, "saldo ou nível mudou sem compra");
}

static void AttackUpgradeUsesCatalogFormula()
{
    GameSaveData save = new GameSaveData { Gold = 50 };
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 100);
    Assert(simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "compra válida falhou");
    Assert(save.Gold == 32 && save.AttackLevel == 1, "custo inicial não foi 18");
    Assert(simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "segunda compra válida falhou");
    Assert(save.Gold == 0 && save.AttackLevel == 2, "custo do segundo nível não foi 32");
}

static void HealthUpgradeUsesCatalogFormula()
{
    GameSaveData save = new GameSaveData { Gold = 20 };
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 100);
    Assert(simulation.TryPurchaseUpgrade(UpgradeKind.Health), "compra de vida válida falhou");
    Assert(save.Gold == 0 && save.HealthLevel == 1, "custo inicial de vida deveria ser 20");
    Assert(simulation.PlayerMaximumHealth == 155f && simulation.PlayerHealth == 155f, "+35 PV não foi aplicado");
}

static void BaseUpgradeUsesSameEconomy()
{
    GameSaveData save = TerminalFixture();
    IdleCombatSimulation simulation = NewSimulation(save, 100);
    simulation.Step(0.05f);
    Assert(!simulation.IsActive, "fixture não terminou a tentativa");
    save.Gold = 18;
    Assert(simulation.TryPurchaseUpgradeAtBase(UpgradeKind.Attack), "oficina recusou compra após derrota");
    Assert(save.Gold == 0 && save.AttackLevel == 1, "oficina usou custo ou saldo diferente do combate");
}

static void EnemyGoldIsAwardedOnce()
{
    GameSaveData save = new GameSaveData();
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 2, waveInterval: 50f);
    int defeated = 0;
    simulation.EventRaised += combatEvent =>
    {
        if (combatEvent.Type == CombatEventType.EnemyDefeated) defeated++;
    };
    StepFor(simulation, 5f);
    Assert(defeated == 1, "primeiro inimigo não foi derrotado uma única vez");
    Assert(save.Gold == 7, "recompensa do Torrão deveria ser 7 de ouro");
    StepFor(simulation, 5f);
    Assert(defeated == 1 && save.Gold == 7, "inimigo derrotado concedeu ouro repetido");
}

static void ProjectilesRespectTravelAndTargetIdentity()
{
    GameSaveData save = new GameSaveData();
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 100, regularEnemyHealth: 1f);
    int dissipated = 0;
    simulation.EventRaised += combatEvent =>
    {
        if (combatEvent.Type == CombatEventType.ProjectileDissipated) dissipated++;
    };
    StepFor(simulation, 0.8f);
    Assert(save.Gold == 7, "inimigo de 1 PV não foi recompensado uma vez");
    Assert(dissipated >= 1, "projétil não se dissipou quando o alvo morreu antes do segundo impacto");
}

static void PioneerSpecialKeepsFourStrikes()
{
    GameSaveData save = new GameSaveData();
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 1, bossHealth: 120f);
    int strikes = 0;
    float totalDamage = 0f;
    simulation.EventRaised += combatEvent =>
    {
        if (combatEvent.Type == CombatEventType.SpecialStrike)
        {
            strikes++;
            totalDamage += combatEvent.Amount;
        }
    };
    StepFor(simulation, 6.5f);
    Assert(strikes == 4, "Pioneira não manteve quatro impactos");
    Assert(Math.Abs(totalDamage - 32f) < 0.001f, "dano total do especial não permaneceu em 32");
}

static void BossAloneClearsStage()
{
    GameSaveData save = new GameSaveData();
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 1);
    int regularDefeated = 0;
    int bossDefeated = 0;
    int chests = 0;
    simulation.EventRaised += combatEvent =>
    {
        if (combatEvent.Type == CombatEventType.EnemyDefeated && combatEvent.Text.Contains("Torrão")) regularDefeated++;
        if (combatEvent.Type == CombatEventType.BossDefeated) bossDefeated++;
        if (combatEvent.Type == CombatEventType.ChestAwarded) chests += (int)combatEvent.Amount;
    };

    simulation.Step(0.05f);
    Assert(!simulation.Run.IsStageCleared, "a fase terminou apenas por iniciar");
    StepFor(simulation, 8f);
    Assert(bossDefeated == 1 && simulation.Run.IsStageCleared, "derrotar boss não concluiu fase");
    Assert(regularDefeated == 0, "teste deveria deixar inimigo comum vivo ao derrotar boss");
    Assert(chests == 2 && save.Chests == 2 && save.Keys == 2, "recompensas da fase não seguiram o catálogo");
    Assert(save.UnlockedStageId == "gruta_basaltica", "fase seguinte não foi desbloqueada");
}

static void ResumeRestoresRun()
{
    GameSaveData firstSave = new GameSaveData();
    IdleCombatSimulation first = NewSimulation(firstSave, waveCount: 10);
    StepFor(first, 0.5f);
    firstSave.ActiveRun = first.CaptureRun();
    ActiveRunSnapshot expected = first.CaptureRun();

    GameSaveData reopened = new GameSaveData
    {
        Gold = firstSave.Gold,
        Keys = firstSave.Keys,
        AttackLevel = firstSave.AttackLevel,
        HealthLevel = firstSave.HealthLevel,
        UnlockedStageId = firstSave.UnlockedStageId,
        ActiveRun = expected
    };
    IdleCombatSimulation resumed = NewSimulation(reopened, waveCount: 10);
    Assert(resumed.CurrentStageId == first.CurrentStageId, "fase ativa não foi retomada");
    Assert(Math.Abs(resumed.PlayerHealth - first.PlayerHealth) < 0.001f, "PV não foram retomados");
    Assert(resumed.Enemies.Count == first.Enemies.Count, "inimigos não foram retomados");
    Assert(resumed.Run.ElapsedSeconds == first.Run.ElapsedSeconds, "tempo de fase não foi retomado");
    Assert(expected.PendingHits.Length > 0, "fixture não capturou projétil em voo");
    Assert(JsonConvert.SerializeObject(resumed.CaptureRun()) == JsonConvert.SerializeObject(expected), "timers, alvos ou projéteis mudaram na retomada");
    StepFor(first, 5f);
    StepFor(resumed, 5f);
    Assert(JsonConvert.SerializeObject(first.CaptureRun()) == JsonConvert.SerializeObject(resumed.CaptureRun()), "combate retomado divergiu da execução contínua");
    Assert(first.Profile.Gold == resumed.Profile.Gold, "retomada alterou recompensas");
}

static void SaveRecoversValidatedBackup()
{
    string directory = Path.Combine(Path.GetTempPath(), "dw-save-check-" + Guid.NewGuid().ToString("N"));
    try
    {
        var repository = new AtomicSaveRepository(directory);
        repository.Save(new GameSaveData { Gold = 7 });
        repository.Save(new GameSaveData { Gold = 14 });
        string primary = Path.Combine(directory, "dig-and-wonder.save.json");
        File.WriteAllText(primary, "{corrupto");
        Assert(repository.TryLoad(out GameSaveData recovered, out bool fromBackup), "backup válido não foi recuperado");
        Assert(fromBackup && recovered.Gold == 7, "recuperação não selecionou a cópia anterior validada");
    }
    finally
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}

static GameSaveData TerminalFixture(bool special = false)
{
    return new GameSaveData
    {
        ActiveRun = new ActiveRunSnapshot
        {
            StageId = "prado_cristalino", PlayerHealth = 1f, BossSpawned = true,
            NextEntityId = 3, AttackTimer = special ? 0f : 100f, SpecialCooldown = 100f,
            SpecialHitsRemaining = special ? 1 : 0, SpecialTargetId = special ? 1 : 0,
            Enemies = new[]
            {
                new EnemySnapshot { Id = 1, Health = 1f, IsBoss = true, GoldReward = 60, AttackTimer = 100f },
                new EnemySnapshot { Id = 2, Health = 48f, GoldReward = 7, AttackTimer = 100f }
            },
            PendingHits = special ? Array.Empty<PendingHitSnapshot>() : new[]
            {
                new PendingHitSnapshot { TargetEntityId = 1, Damage = 1f, RemainingTravel = 0.01f },
                new PendingHitSnapshot { SourceEntityId = 2, TargetsPlayer = true, Damage = 999f, RemainingTravel = 0.01f }
            }
        }
    };
}

static void VictoryStopsPendingLethalDamage()
{
    GameSaveData save = TerminalFixture();
    IdleCombatSimulation simulation = NewSimulation(save, 100);
    simulation.Step(0.05f);
    Assert(simulation.Run.IsStageCleared && !simulation.Run.IsPlayerDefeated, "o dano pendente transformou vitória em derrota");
    Assert(simulation.Enemies.Count == 0 && simulation.CaptureRun().PendingHits.Length == 0, "ações ficaram ativas após a vitória");
    int gold = save.Gold;
    StepFor(simulation, 1f);
    Assert(save.Gold == gold && save.Keys == 2 && save.Chests == 2, "recompensas repetidas após vitória");
}

static void SpecialVictoryStopsFurtherActions()
{
    IdleCombatSimulation simulation = NewSimulation(TerminalFixture(true), 100);
    bool finished = false;
    int actionsAfterFinish = 0;
    simulation.EventRaised += e =>
    {
        if (e.Type == CombatEventType.StageCleared) finished = true;
        else if (finished && (e.Type == CombatEventType.DrillAttack || e.Type == CombatEventType.ProjectileLaunched || e.Type == CombatEventType.Hit))
            actionsAfterFinish++;
    };
    simulation.Step(0.05f);
    Assert(finished && actionsAfterFinish == 0, "ataque básico continuou após vitória pelo especial");
    Assert(simulation.Run.SpecialHitsRemaining == 0, "contador do especial ficou inválido após limpeza terminal");
}

static void DefeatClearsActionsBeforeRestart()
{
    GameSaveData save = TerminalFixture();
    save.Gold = 18;
    save.ActiveRun.PendingHits = new[] { new PendingHitSnapshot { SourceEntityId = 2, TargetsPlayer = true, Damage = 999f, RemainingTravel = 0.01f } };
    IdleCombatSimulation simulation = NewSimulation(save, 100);
    simulation.Step(0.05f);
    Assert(simulation.Run.IsPlayerDefeated && !simulation.IsActive && save.ActiveRun == null, "derrota não ficou terminal");
    Assert(simulation.Enemies.Count == 0 && simulation.CaptureRun().PendingHits.Length == 0, "derrota deixou combate pendente");
    Assert(!simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "simulação encerrada aceitou compra");
    IdleCombatSimulation restarted = NewSimulation(save, 100);
    Assert(restarted.PlayerHealth == 120f && restarted.Profile.Gold == 18, "reinício não restaurou vida ou perdeu ouro");
    Assert(restarted.Run.SpecialCooldown == 3f && restarted.Enemies.Count == 0, "reinício herdou ações da derrota");
}

static void BossRewardEventIsConsistent()
{
    GameSaveData save = TerminalFixture();
    IdleCombatSimulation simulation = NewSimulation(save, 100);
    bool checkedReward = false;
    simulation.EventRaised += e =>
    {
        if (e.Type != CombatEventType.EnemyDefeated) return;
        checkedReward = true;
        Assert(save.Gold == 60 && save.Keys == 2 && save.Chests == 2 && save.ActiveRun == null,
            "evento observou recompensa de boss parcialmente aplicada");
    };
    simulation.Step(0.05f);
    Assert(checkedReward, "evento de recompensa não ocorreu");
}

static void NullPayloadRecoversBackup() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    repository.Save(new GameSaveData { Gold = 7 });
    repository.Save(new GameSaveData { Gold = 14 });
    WriteEnvelope(Path.Combine(directory, "dig-and-wonder.save.json"), "null");
    Assert(repository.TryLoad(out GameSaveData data, out bool recovered) && recovered && data.Gold == 7,
        "payload null interrompeu recuperação do backup");
});

static void InterruptedFirstSaveRecoversTemporary() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    repository.Save(new GameSaveData { Gold = 21 });
    string primary = Path.Combine(directory, "dig-and-wonder.save.json");
    File.Move(primary, primary + ".tmp");
    Assert(repository.HasAnySaveFile, "temporário válido foi ignorado na detecção de progresso");
    Assert(repository.TryLoad(out GameSaveData data, out _) && data.Gold == 21, "primeiro save interrompido não foi recuperado");
    Assert(File.Exists(primary), "temporário recuperado não foi promovido a principal");
});

static void InvalidTemporaryCountsAsExistingSave() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    File.WriteAllText(Path.Combine(directory, "dig-and-wonder.save.json.tmp"), "{interrompido");
    Assert(repository.HasAnySaveFile && !repository.TryLoad(out _, out _), "temporário inválido permitiria criar perfil por cima do progresso");
});

static void InvalidSnapshotIsRejected() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    repository.Save(new GameSaveData { Gold = 7 });
    foreach (GameSaveData invalid in new[]
    {
        new GameSaveData { ActiveRun = new ActiveRunSnapshot { StageId = "prado_cristalino", PlayerHealth = float.NaN, NextEntityId = 1 } },
        new GameSaveData { ActiveRun = new ActiveRunSnapshot { StageId = "prado_cristalino", PlayerHealth = 120f, NextEntityId = 1, Enemies = new EnemySnapshot[] { null } } },
        new GameSaveData { CriticalChanceLevel = -1 }
    })
    {
        bool rejected = false;
        try { repository.Save(invalid); }
        catch (InvalidDataException) { rejected = true; }
        Assert(rejected, "snapshot inválido foi aceito");
    }
    Assert(repository.TryLoad(out GameSaveData preserved, out _) && preserved.Gold == 7, "validação alterou o último save válido");
});

static void WriteEnvelope(string path, string payload, int schemaVersion = GameSaveData.CurrentSchemaVersion)
{
    string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    File.WriteAllText(path, JsonConvert.SerializeObject(new { SchemaVersion = schemaVersion, Payload = payload, Sha256 = hash }));
}

static void ConfirmedPrimaryWinsOverTemporary() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    repository.Save(new GameSaveData { Gold = 7 });
    WriteEnvelope(Path.Combine(directory, "dig-and-wonder.save.json.tmp"), JsonConvert.SerializeObject(new GameSaveData { Gold = 999 }));
    Assert(repository.TryLoad(out GameSaveData data, out bool recovered) && !recovered && data.Gold == 7,
        "temporário substituiu uma geração já confirmada válida");
});

static void InvalidCopiesRemainUntouched() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    string primary = Path.Combine(directory, "dig-and-wonder.save.json");
    string[] paths = { primary, primary + ".bak", primary + ".tmp" };
    foreach (string path in paths) File.WriteAllText(path, "{inválido:" + Path.GetExtension(path));
    string[] original = paths.Select(File.ReadAllText).ToArray();
    Assert(!repository.TryLoad(out _, out _) && repository.HasAnySaveFile, "arquivos inválidos foram tratados como perfil novo");
    for (int i = 0; i < paths.Length; i++) Assert(File.ReadAllText(paths[i]) == original[i], "leitura alterou arquivo inválido");
});

static void WithSaveDirectory(Action<string> test)
{
    string directory = Path.Combine(Path.GetTempPath(), "dw-regression-" + Guid.NewGuid().ToString("N"));
    try { Directory.CreateDirectory(directory); test(directory); }
    finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
}

static void CatalogNumbersMatchMappedData()
{
    string root = EconomyChecks.Root;
    JObject prototype = JObject.Parse(File.ReadAllText(Path.Combine(root, "content/defense/prototype.json")));
    JObject abilities = JObject.Parse(File.ReadAllText(Path.Combine(root, "content/defense/abilities.json")));
    JObject phases = JObject.Parse(File.ReadAllText(Path.Combine(root, "content/phases/catalog.json")));
    JObject cart = (JObject)prototype["cart"];
    JObject pioneer = (JObject)prototype["pioneer"];
    JObject mole = (JObject)prototype["mole"];
    JObject attackUpgrade = (JObject)((JObject)prototype["upgrades"])["damage"];
    JObject healthUpgrade = (JObject)((JObject)prototype["upgrades"])["hull"];
    JObject prado = (JObject)((JObject)phases["phases"])["prado_cristalino"];
    JObject boss = (JObject)prado["boss"];

    Assert(cart.Value<int>("hp") == 120, "PV base do carrinho mudaram");
    Assert(cart.Value<int>("cannon_damage") == 6 && Math.Abs(cart.Value<float>("cannon_interval") - 1.25f) < 0.001f, "dano/cadência do canhão mudaram");
    Assert(pioneer.Value<int>("damage") == 32 && pioneer.Value<int>("hits") == 4, "dados da Pioneira não foram lidos");
    Assert(Math.Abs(pioneer.Value<float>("hit_interval") - 0.7f) < 0.001f, "intervalo dos golpes da Pioneira mudou");
    Assert(mole.Value<int>("hp") == 48 && mole.Value<int>("gold") == 7, "PV/ouro do Torrão mudaram");
    Assert(attackUpgrade.Value<int>("base_cost") == 18 && attackUpgrade.Value<int>("cost_step") == 14, "fórmula de custo de Ataque mudou");
    Assert(healthUpgrade.Value<int>("base_cost") == 20 && healthUpgrade.Value<int>("cost_step") == 16, "fórmula de custo de Vida mudou");
    Assert(phases.Value<int>("surprise_chance_percent") == 22 && boss.Value<int>("health") == 12, "dados de fase surpresa ou boss foram alterados");
}

static IdleCombatSimulation NewSimulation(GameSaveData save, int waveCount, float waveInterval = 10f, float regularEnemyHealth = 48f, float bossHealth = 12f)
{
    var regular = new EnemyCombatData
    {
        Id = "torrao", DisplayName = "Toupeira Torrão", MaxHealth = regularEnemyHealth, Damage = 8f,
        FirstAttackDelay = 1.7f, AttackInterval = 2.8f, TelegraphDuration = 0.65f,
        ProjectileFlight = 0.65f, GoldReward = 7
    };
    var boss = new EnemyCombatData
    {
        Id = "boss", DisplayName = "Tatu de Cristal", MaxHealth = bossHealth, Damage = 8f,
        FirstAttackDelay = 1.7f, AttackInterval = 2.8f, TelegraphDuration = 0.65f,
        ProjectileFlight = 0.65f, GoldReward = 60, IsBoss = true
    };
    var stage = new StageCombatData
    {
        Id = "prado_cristalino", DisplayName = "Prado Cristalino", LengthSegments = 24,
        ChestCount = 2, KeyCount = 2, WaveInterval = waveInterval, WaveCount = waveCount,
        LateWaveAfter = 3, LateWaveSize = 2, Boss = boss
    };
    var next = new StageCombatData
    {
        Id = "gruta_basaltica", DisplayName = "Gruta Basáltica", LengthSegments = 24,
        ChestCount = 2, KeyCount = 2, WaveInterval = waveInterval, WaveCount = waveCount,
        LateWaveAfter = 3, LateWaveSize = 2, Boss = boss
    };
    var config = new CombatConfiguration
    {
        PlayerBaseHealth = 120f, BasicAttackDamage = 6f, BasicAttackInterval = 1.25f,
        BasicAttackHitCount = 2, BasicAttackHitInterval = 0.28f, BasicAttackTravelTime = 0.34f,
        SimulationStep = 0.05f, MaximumFrameCatchUp = 0.25f, MaximumEnemies = 4,
        MaximumPendingSpawns = 8, MaximumProjectiles = 24,
        RegularEnemy = regular,
        Special = new SpecialCombatData { Damage = 32f, Cooldown = 3f, TravelTime = 0.8f, HitCount = 4, HitInterval = 0.7f },
        AttackUpgrade = new UpgradeCombatData { Kind = UpgradeKind.Attack, BaseCost = 18, CostStep = 14, MaximumLevel = 10, Bonus = 0.12f, Implemented = true },
        HealthUpgrade = new UpgradeCombatData { Kind = UpgradeKind.Health, BaseCost = 20, CostStep = 16, MaximumLevel = 10, Bonus = 35f, Implemented = true },
        CriticalChanceUpgrade = new UpgradeCombatData { Kind = UpgradeKind.CriticalChance, Implemented = false },
        CriticalDamageUpgrade = new UpgradeCombatData { Kind = UpgradeKind.CriticalDamage, Implemented = false },
        Stages = new[] { stage, next },
        Characters = new[]
        {
            new CharacterCombatData { Id = CharacterProgression.StarterCharacterId, DisplayName = "Broca Pioneira", Experience = TestExperienceRules() },
            new CharacterCombatData { Id = "test_drill", DisplayName = "Personagem de fixture", Experience = TestExperienceRules() }
        }
    };
    return new IdleCombatSimulation(config, save, "prado_cristalino", 12345u);
}

static void StepFor(IdleCombatSimulation simulation, float seconds)
{
    int steps = (int)Math.Ceiling(seconds / 0.05f);
    for (int i = 0; i < steps && simulation.IsActive; i++) simulation.Step(0.05f);
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static CharacterExperienceRules TestExperienceRules() => new CharacterExperienceRules
{
    RegularEnemyExperience = 10, BossExperience = 50, FirstLevelExperience = 20, ExperiencePerLevelStep = 10, MaximumLevel = 50
};

static void CharacterStartsAtZero()
{
    var save = new GameSaveData();
    CharacterLevelState state = CharacterProgression.GetLevel(CharacterProgression.Find(save, save.EquippedCharacterId), TestExperienceRules());
    Assert(state.Level == 0 && state.ExperienceInLevel == 0 && state.ExperienceToNextLevel == 20, "personagem não começou no nível 0 com XP zerado");
}

static void CharactersKeepIndependentExperience()
{
    var save = new GameSaveData();
    var rules = TestExperienceRules();
    CharacterProgression.Award(save, "pioneer", rules, 50);
    Assert(CharacterProgression.TryUnlock(save, "test_drill"), "personagem nova não foi concedida");
    Assert(CharacterProgression.GetLevel(CharacterProgression.Find(save, "test_drill"), rules).Level == 0, "nova personagem herdou nível");
    Assert(!CharacterProgression.TryUnlock(save, "pioneer"), "duplicata criou outro registro");
    CharacterProgression.Award(save, "test_drill", rules, 10);
    Assert(save.Characters.Count == 2 && CharacterProgression.Find(save, "pioneer").TotalExperience == 50 &&
        CharacterProgression.Find(save, "test_drill").TotalExperience == 10, "XP foi compartilhado ou reiniciado");
    Assert(CharacterProgression.GetLevel(CharacterProgression.Find(save, "pioneer"), rules).Level == 2, "progresso antigo foi perdido");
}

static void CharacterEquipRespectsRun()
{
    var save = new GameSaveData();
    Assert(!CharacterProgression.TryEquip(save, "test_drill"), "personagem não possuída foi equipada");
    CharacterProgression.TryUnlock(save, "test_drill");
    IdleCombatSimulation sim = NewSimulation(save, 100);
    Assert(!CharacterProgression.TryEquip(save, "test_drill"), "troca foi aceita com projéteis/tentativa ativos");
    Assert(sim.ActiveCharacterId == "pioneer" && save.EquippedCharacterId == "pioneer", "tentativa trocou de personagem");
}

static void SwitchingCharactersPreservesProgress()
{
    var save = new GameSaveData();
    var rules = TestExperienceRules();
    CharacterProgression.Award(save, "pioneer", rules, 35);
    CharacterProgression.TryUnlock(save, "test_drill");
    Assert(CharacterProgression.TryEquip(save, "test_drill"), "troca na base falhou");
    Assert(CharacterProgression.GetLevel(CharacterProgression.Find(save, save.EquippedCharacterId), rules).Level == 0, "nova broca não veio nível zero");
    Assert(CharacterProgression.TryEquip(save, "pioneer"), "retorno à Pioneira falhou");
    CharacterLevelState previous = CharacterProgression.GetLevel(CharacterProgression.Find(save, save.EquippedCharacterId), rules);
    Assert(previous.Level == 1 && previous.ExperienceInLevel == 15, "voltar apagou nível/XP da Pioneira");
}

static void ExperienceBelongsToRunCharacter()
{
    var save = new GameSaveData();
    CharacterProgression.TryUnlock(save, "test_drill");
    IdleCombatSimulation sim = NewSimulation(save, 100);
    // Mesmo uma mutação externa indevida do DTO não pode redirecionar recompensas em voo.
    save.EquippedCharacterId = "test_drill";
    StepFor(sim, 5f);
    Assert(CharacterProgression.Find(save, "pioneer").TotalExperience == 10 &&
        CharacterProgression.Find(save, "test_drill").TotalExperience == 0, "XP migrou para uma broca fora da tentativa");
}

static void EnemyExperienceIsAwardedOnce()
{
    var save = new GameSaveData();
    IdleCombatSimulation sim = NewSimulation(save, 100);
    int events = 0;
    sim.EventRaised += e => { if (e.Type == CombatEventType.CharacterExperienceAwarded) events++; };
    StepFor(sim, 0.5f);
    Assert(CharacterProgression.Find(save, "pioneer").TotalExperience == 0, "disparo ou dano concedeu XP antes da derrota");
    StepFor(sim, 4.5f);
    Assert(CharacterProgression.Find(save, "pioneer").TotalExperience == 10 && events == 1, "morte não pagou 10 XP uma vez");
    StepFor(sim, 3f);
    Assert(CharacterProgression.Find(save, "pioneer").TotalExperience == 10 && events == 1, "XP repetido após inimigo removido");
}

static void BossExperienceIsAtomic()
{
    var save = TerminalFixture();
    IdleCombatSimulation sim = NewSimulation(save, 100);
    bool observed = false;
    sim.EventRaised += e =>
    {
        if (e.Type != CombatEventType.EnemyDefeated) return;
        observed = true;
        Assert(CharacterProgression.Find(save, "pioneer").TotalExperience == 50, "observador recebeu recompensa antes do XP");
        Assert(CharacterProgression.GetLevel(CharacterProgression.Find(save, "pioneer"), TestExperienceRules()).Level == 2,
            "50 XP não aplicou a subida de dois níveis");
        Assert(save.Chests == 2 && save.Keys == 2 && save.ActiveRun == null, "recompensa do chefe ficou parcial");
    };
    sim.Step(0.05f);
    Assert(observed, "chefe não foi derrotado pela fixture");
}

static void LevelUpKeepsOverflow()
{
    var save = new GameSaveData();
    var rules = TestExperienceRules();
    CharacterProgression.Award(save, "pioneer", rules, 35);
    IdleCombatSimulation sim = NewSimulation(save, 100);
    int levels = 0;
    sim.EventRaised += e => { if (e.Type == CombatEventType.CharacterLeveledUp) levels++; };
    StepFor(sim, 5f); // 35 + 10 = 45: nível 1, 25/30 XP.
    CharacterLevelState state = CharacterProgression.GetLevel(CharacterProgression.Find(save, "pioneer"), rules);
    Assert(state.Level == 1 && state.ExperienceInLevel == 25 && levels == 0, "XP parcial foi descartado ou concedeu nível cedo");
    var bossSave = TerminalFixture();
    CharacterProgression.Award(bossSave, "pioneer", rules, 35);
    IdleCombatSimulation boss = NewSimulation(bossSave, 100);
    boss.EventRaised += e => { if (e.Type == CombatEventType.CharacterLeveledUp) levels++; };
    boss.Step(0.05f); // 85 total: nível 2, 35/40 XP.
    state = CharacterProgression.GetLevel(CharacterProgression.Find(bossSave, "pioneer"), rules);
    Assert(state.Level == 2 && state.ExperienceInLevel == 35 && levels == 1, "evento de nível ou excedente está incorreto");
}

static void ExperienceCapDoesNotOverflow()
{
    var save = new GameSaveData();
    var rules = TestExperienceRules();
    int awarded = CharacterProgression.Award(save, "pioneer", rules, int.MaxValue);
    CharacterProgressData character = CharacterProgression.Find(save, "pioneer");
    CharacterLevelState state = CharacterProgression.GetLevel(character, rules);
    Assert(awarded == 13250 && character.TotalExperience == 13250 && state.Level == 50 && state.IsMaximum,
        "limite da curva calculado incorretamente");
    Assert(CharacterProgression.Award(save, "pioneer", rules, 50) == 0, "máximo continuou acumulando XP");
    character.TotalExperience = long.MaxValue;
    Assert(CharacterProgression.Award(save, "pioneer", rules, 50) == 0 && character.TotalExperience == long.MaxValue,
        "curva reduzida destruiu XP ou causou overflow");
    bool rejected = false;
    try { CharacterProgression.Award(save, "pioneer", rules, -1); }
    catch (ArgumentOutOfRangeException) { rejected = true; }
    Assert(rejected && character.TotalExperience == long.MaxValue, "XP negativo foi aceito");
}

static void DefeatKeepsCharacterExperience()
{
    var save = TerminalFixture();
    CharacterProgression.Award(save, "pioneer", TestExperienceRules(), 35);
    save.ActiveRun.PendingHits = new[] { new PendingHitSnapshot { TargetsPlayer = true, Damage = 999, RemainingTravel = 0.01f } };
    IdleCombatSimulation sim = NewSimulation(save, 100);
    sim.Step(0.05f);
    Assert(sim.Run.IsPlayerDefeated && CharacterProgression.Find(save, "pioneer").TotalExperience == 35, "derrota retirou ou concedeu XP");
    IdleCombatSimulation restarted = NewSimulation(save, 100);
    Assert(restarted.ActiveCharacterId == "pioneer" && CharacterProgression.Find(save, "pioneer").TotalExperience == 35, "reinício zerou a broca");
}

static void ResumedRunKeepsExperienceOwnership()
{
    var save = new GameSaveData();
    CharacterProgression.TryUnlock(save, "test_drill");
    CharacterProgression.TryEquip(save, "test_drill");
    IdleCombatSimulation continuous = NewSimulation(save, 100);
    StepFor(continuous, 0.5f);
    save.ActiveRun = continuous.CaptureRun();
    GameSaveData restored = JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(save));
    IdleCombatSimulation resumed = NewSimulation(restored, 100);
    Assert(resumed.ActiveCharacterId == "test_drill" && restored.Characters.Count == 2, "retomada mudou autora ou duplicou a lista");
    StepFor(continuous, 5f);
    StepFor(resumed, 5f);
    Assert(CharacterProgression.Find(restored, "test_drill").TotalExperience == 10 &&
        CharacterProgression.Find(restored, "pioneer").TotalExperience == 0, "retomada dividiu XP ou premiou duas vezes");
    Assert(JsonConvert.SerializeObject(save.Characters) == JsonConvert.SerializeObject(restored.Characters), "XP retomado divergiu da execução contínua");
}

static void LegacySaveMigratesCharacterProgress() => WithSaveDirectory(directory =>
{
    string primary = Path.Combine(directory, "dig-and-wonder.save.json");
    var legacy = new GameSaveData
    {
        Gold = 137, Keys = 2, Chests = 3, AttackLevel = 4, HealthLevel = 2,
        ActiveRun = new ActiveRunSnapshot { StageId = "prado_cristalino", PlayerHealth = 150, ElapsedSeconds = 2f, NextEntityId = 1 }
    };
    JObject payload = JObject.FromObject(legacy);
    payload["SchemaVersion"] = 1;
    payload.Remove("Characters");
    payload.Remove("EquippedCharacterId");
    ((JObject)payload["ActiveRun"]).Remove("CharacterId");
    WriteEnvelope(primary, payload.ToString(Formatting.None), 1);
    byte[] original = File.ReadAllBytes(primary);
    var repository = new AtomicSaveRepository(directory);
    Assert(repository.TryLoad(out GameSaveData migrated, out bool recovered) && !recovered, "save 1 não foi lido");
    Assert(migrated.SchemaVersion == GameSaveData.CurrentSchemaVersion && migrated.Gold == 137 && migrated.Keys == 2 && migrated.Chests == 3 &&
        migrated.AttackLevel == 4 && migrated.HealthLevel == 2, "migração alterou economia ou níveis existentes");
    Assert(migrated.Characters.Count == 1 && migrated.Characters[0].TotalExperience == 0 && migrated.EquippedCharacterId == "pioneer" &&
        migrated.ActiveRun.CharacterId == "pioneer" && migrated.ActiveRun.ElapsedSeconds == 2f, "migração perdeu tentativa ou inventou XP retroativo");
    Assert(File.ReadAllBytes(primary).SequenceEqual(original), "somente ler já sobrescreveu o save antigo");
    repository.Save(migrated);
    Assert(File.ReadAllBytes(primary + ".bak").SequenceEqual(original), "backup não preservou os bytes originais");
    Assert(repository.TryLoad(out GameSaveData reopened, out _) && reopened.Characters.Count == 1 && reopened.SchemaVersion == GameSaveData.CurrentSchemaVersion,
        "save migrado não reabriu sem duplicação");
});

static void AtomicFileResumesCharacterExperience() => WithSaveDirectory(directory =>
{
    var save = new GameSaveData();
    CharacterProgression.Award(save, "pioneer", TestExperienceRules(), 50);
    CharacterProgression.TryUnlock(save, "test_drill");
    CharacterProgression.TryEquip(save, "test_drill");
    IdleCombatSimulation continuous = NewSimulation(save, 100, waveInterval: 1f, regularEnemyHealth: 1f);
    for (int step = 0; step < 200 && continuous.IsActive; step++)
    {
        continuous.Step(0.05f);
        if (CharacterProgression.Find(save, "test_drill").TotalExperience >= 20 &&
            continuous.CaptureRun().PendingHits.Any(hit => !hit.TargetsPlayer)) break;
    }
    save.ActiveRun = continuous.CaptureRun();
    Assert(save.ActiveRun.PendingHits.Any(hit => !hit.TargetsPlayer) && CharacterProgression.Find(save, "test_drill").TotalExperience >= 20,
        "fixture não chegou ao nível 1 com projétil em voo");
    var repository = new AtomicSaveRepository(directory);
    repository.Save(save);
    Assert(repository.TryLoad(out GameSaveData restored, out _) && restored.Characters.Count == 2, "arquivo não restaurou a coleção");
    IdleCombatSimulation resumed = NewSimulation(restored, 100, waveInterval: 1f, regularEnemyHealth: 1f);
    StepFor(continuous, 2f);
    StepFor(resumed, 2f);
    Assert(JsonConvert.SerializeObject(continuous.CaptureRun()) == JsonConvert.SerializeObject(resumed.CaptureRun()) &&
        JsonConvert.SerializeObject(save.Characters) == JsonConvert.SerializeObject(restored.Characters),
        "save em disco mudou projéteis, XP ou contagem de recompensas");
    Assert(CharacterProgression.Find(restored, "pioneer").TotalExperience == 50, "personagem guardada ganhou ou perdeu XP");
});

static void CharacterExperienceRecoversFromBackup() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    var save = new GameSaveData { Gold = 51 };
    var rules = TestExperienceRules();
    CharacterProgression.Award(save, "pioneer", rules, 35);
    CharacterProgression.TryUnlock(save, "test_drill");
    CharacterProgression.TryEquip(save, "test_drill");
    CharacterProgression.Award(save, "test_drill", rules, 10);
    repository.Save(save);
    CharacterProgression.Award(save, "test_drill", rules, 20);
    repository.Save(save);
    File.WriteAllText(Path.Combine(directory, "dig-and-wonder.save.json"), "{corrompido");
    Assert(repository.TryLoad(out GameSaveData restored, out bool recovered) && recovered, "backup de personagens não foi recuperado");
    Assert(restored.Characters.Count == 2 && restored.EquippedCharacterId == "test_drill" && restored.Gold == 51 &&
        CharacterProgression.Find(restored, "test_drill").TotalExperience == 10 && CharacterProgression.Find(restored, "pioneer").TotalExperience == 35,
        "backup misturou XP ou mudou a personagem equipada");
});

static void InvalidCharacterProgressIsRejected() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    repository.Save(new GameSaveData { Gold = 17 });
    var duplicate = new GameSaveData();
    duplicate.Characters.Add(new CharacterProgressData { CharacterId = "pioneer" });
    var negative = new GameSaveData();
    negative.Characters[0].TotalExperience = -1;
    foreach (GameSaveData invalid in new[] { duplicate, negative, new GameSaveData { Characters = null },
        new GameSaveData { EquippedCharacterId = "not_owned" },
        new GameSaveData { ActiveRun = new ActiveRunSnapshot { StageId = "prado_cristalino", CharacterId = "other", PlayerHealth = 120, NextEntityId = 1 } } })
    {
        bool rejected = false;
        try { repository.Save(invalid); }
        catch (InvalidDataException) { rejected = true; }
        Assert(rejected, "progresso inválido sobrescreveu save válido");
    }
    Assert(repository.TryLoad(out GameSaveData preserved, out _) && preserved.Gold == 17, "validação destruiu o save anterior");
});

static void MissingCharacterExperienceIsRejected() => WithSaveDirectory(directory =>
{
    string primary = Path.Combine(directory, "dig-and-wonder.save.json");
    JObject payload = JObject.FromObject(new GameSaveData());
    ((JObject)payload["Characters"][0]).Remove("TotalExperience");
    WriteEnvelope(primary, payload.ToString(Formatting.None));
    string original = File.ReadAllText(primary);
    var repository = new AtomicSaveRepository(directory);
    Assert(!repository.TryLoad(out _, out _) && repository.HasAnySaveFile && File.ReadAllText(primary) == original,
        "XP ausente foi aceito como zero ou seu arquivo foi sobrescrito");
});

static void FutureCharacterSaveIsPreserved() => WithSaveDirectory(directory =>
{
    string primary = Path.Combine(directory, "dig-and-wonder.save.json");
    var repository = new AtomicSaveRepository(directory);
    repository.Save(new GameSaveData { Gold = 7 });
    repository.Save(new GameSaveData { Gold = 14 });
    JObject payload = JObject.FromObject(new GameSaveData());
    payload["SchemaVersion"] = GameSaveData.CurrentSchemaVersion + 1;
    WriteEnvelope(primary, payload.ToString(Formatting.None), GameSaveData.CurrentSchemaVersion + 1);
    byte[] original = File.ReadAllBytes(primary);
    Assert(!repository.TryLoad(out _, out _) && repository.HasAnySaveFile && File.ReadAllBytes(primary).SequenceEqual(original),
        "backup antigo substituiu uma versão futura íntegra");
    bool rejected = false;
    try { repository.Save(new GameSaveData()); }
    catch (InvalidDataException) { rejected = true; }
    Assert(rejected && File.ReadAllBytes(primary).SequenceEqual(original), "escrita sobrescreveu uma versão futura");
});

static void InvalidSchemaValueRecoversBackup() => WithSaveDirectory(directory =>
{
    var repository = new AtomicSaveRepository(directory);
    var save = new GameSaveData { Gold = 23 };
    CharacterProgression.Award(save, "pioneer", TestExperienceRules(), 35);
    repository.Save(save);
    repository.Save(save);
    JObject payload = JObject.FromObject(save);
    payload["SchemaVersion"] = "inválido";
    WriteEnvelope(Path.Combine(directory, "dig-and-wonder.save.json"), payload.ToString(Formatting.None));
    Assert(repository.TryLoad(out GameSaveData restored, out bool recovered) && recovered && restored.Gold == 23 &&
        CharacterProgression.Find(restored, "pioneer").TotalExperience == 35, "tipo inválido de versão quebrou recuperação de XP");
});

static void RunStatisticsPersist()
{
    var save=new GameSaveData();
    var sim=NewSimulation(save,100,waveInterval:1f,regularEnemyHealth:1f);
    int shots=0,kills=0;
    sim.EventRaised+=e=>{if(e.Type==CombatEventType.ProjectileLaunched && e.Text=="Projétil da broca")shots++;if(e.Type==CombatEventType.EnemyDefeated)kills++;};
    StepFor(sim,4f);
    Assert(shots>0 && kills>0,"ensaio não produziu combate");
    Assert(sim.Run.ShotsFired==shots && sim.Run.EnemiesDefeated==kills,"resumo não corresponde aos eventos reais");
    save.ActiveRun=sim.CaptureRun();
    var restored=JsonConvert.DeserializeObject<GameSaveData>(JsonConvert.SerializeObject(save));
    var resumed=NewSimulation(restored,100,waveInterval:1f,regularEnemyHealth:1f);
    Assert(resumed.Run.ShotsFired==shots && resumed.Run.EnemiesDefeated==kills,"retomada perdeu estatísticas");
    Assert(resumed.Run.WavesCleared==sim.Run.WavesCleared,"retomada perdeu ondas concluídas");
}