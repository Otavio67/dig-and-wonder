using DigAndWonder;
using Newtonsoft.Json.Linq;

var checks = new (string Name, Action Test)[]
{
    ("economia insuficiente não compra upgrade", InsufficientGoldDoesNotPurchase),
    ("preço e aumento de dano do upgrade", AttackUpgradeUsesGodotFormula),
    ("upgrade de vida aplica valor e custo Godot", HealthUpgradeUsesGodotFormula),
    ("ouro por inimigo só uma vez", EnemyGoldIsAwardedOnce),
    ("projétil respeita viagem e alvo removido", ProjectilesRespectTravelAndTargetIdentity),
    ("especial preserva quatro golpes e dano total", PioneerSpecialKeepsFourStrikes),
    ("boss libera fase e baú com chaves", BossAloneClearsStage),
    ("retomada restaura combate e projéteis", ResumeRestoresRun),
    ("save usa backup validado após corrupção", SaveRecoversValidatedBackup),
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
