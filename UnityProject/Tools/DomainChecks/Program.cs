using DigAndWonder;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

var checks = new (string Name, Action Test)[]
{
    ("economia insuficiente não compra upgrade", InsufficientGoldDoesNotPurchase),
    ("preço e aumento de dano do upgrade", AttackUpgradeUsesGodotFormula),
    ("upgrade de vida aplica valor e custo Godot", HealthUpgradeUsesGodotFormula),
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
    ("ScriptableObject mapping stays tied to Godot source numbers", GodotNumbersMatchMappedData)
};

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

static void InsufficientGoldDoesNotPurchase()
{
    GameSaveData save = new GameSaveData();
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 100);
    Assert(!simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "compra sem saldo foi aceita");
    Assert(save.Gold == 0 && save.AttackLevel == 0, "saldo ou nível mudou sem compra");
}

static void AttackUpgradeUsesGodotFormula()
{
    GameSaveData save = new GameSaveData { Gold = 50 };
    IdleCombatSimulation simulation = NewSimulation(save, waveCount: 100);
    Assert(simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "compra válida falhou");
    Assert(save.Gold == 32 && save.AttackLevel == 1, "custo inicial não foi 18");
    Assert(simulation.TryPurchaseUpgrade(UpgradeKind.Attack), "segunda compra válida falhou");
    Assert(save.Gold == 0 && save.AttackLevel == 2, "custo do segundo nível não foi 32");
}

static void HealthUpgradeUsesGodotFormula()
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

static void WriteEnvelope(string path, string payload)
{
    string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    File.WriteAllText(path, JsonConvert.SerializeObject(new { SchemaVersion = 1, Payload = payload, Sha256 = hash }));
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

static void GodotNumbersMatchMappedData()
{
    string root = Directory.GetCurrentDirectory();
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
        Stages = new[] { stage, next }
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
