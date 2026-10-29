using DigAndWonder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

internal static class DiscoveryCombatChecks
{
    internal static IEnumerable<(string Name, Action Test)> All()
    {
        yield return ("descobertas: catálogo cobre todos os chefes e mini-chefes", Catalog);
        yield return ("descobertas: vitória concede antes do evento e repetição não duplica", Rewards);
        yield return ("descobertas: mini-chefe exige também derrotar a escolta", Escorts);
        yield return ("save 4: coleção retoma e rejeita duplicata, desconhecido e campo ausente", CollectionSave);
        yield return ("save 4: snapshot malformado é preservado sem exceção", InvalidSnapshotShapes);
        yield return ("save 3: migra sem apagar moedas, poderes, campanha ou tentativa", Migration);
        yield return ("treino: escolha respeita desbloqueio e retorno mantém fronteira e bioma", Training);
        yield return ("inimigos: introdução gradual e vida específica por tipo", SpawnTypes);
        yield return ("inimigos: blindagem reduz tiro e especial a atravessa", Armor);
        yield return ("inimigos: ágil ataca mais vezes e carregador anuncia golpe forte", AttackPatterns);
        yield return ("inimigos: morte durante preparação interrompe ataque, disparo lançado continua", Interrupt);
        yield return ("combate: rajada e especial trocam de alvo após uma morte", Retarget);
        yield return ("retomada: novos tipos e preparação preservam resultado determinístico", Resume);
        yield return ("compatibilidade: encontro 1 retoma sem receber novos inimigos", PreviousEncounter);
        yield return ("HUD: vitória comum anuncia ouro sem baú e derrota sugere compra possível", Feedback);
    }

    private static void Check(bool value, string reason) { if (!value) throw new Exception(reason); }
    private static void Tick(IdleCombatSimulation sim, float seconds)
    { for (int i = 0; i < (int)Math.Ceiling(seconds / .05f) && sim.IsActive; i++) sim.Step(.05f); }
    private static IdleCombatSimulation New(int stage, int highest, out GameSaveData save)
    {
        save = new GameSaveData { CampaignStage = stage, HighestClearedStage = highest, AttackLevel = 80, HealthLevel = 80 };
        return new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 44);
    }
    private static IdleCombatSimulation Fixture(EnemyArchetype kind, bool special = false, float firstHealth = 10000, bool attack = false)
    {
        var config = EconomyChecks.Configuration();
        config.BasicAttackInterval = 5; config.BasicAttackHitInterval = .2f; config.BasicAttackTravelTime = .05f;
        config.RegularEnemy.AttackInterval = 2; config.RegularEnemy.TelegraphDuration = .2f;
        var save = new GameSaveData { CampaignStage = 4, HighestClearedStage = 3 };
        save.ActiveRun = new ActiveRunSnapshot {
            StageId = "prado_cristalino", CharacterId = "pioneer", CampaignStage = 4, EncounterVersion = 2,
            WavesSpawned = 3, BossSpawned = true, PlayerHealth = 120, NextEntityId = 3, RandomState = 12345,
            AttackTimer = special || attack ? 100 : 0, SpecialCooldown = special ? 0 : 100,
            Enemies = new[] {
                new EnemySnapshot { Id = 1, Archetype = kind, Health = firstHealth, AttackTimer = attack ? 0 : 100, GoldReward = 8 },
                new EnemySnapshot { Id = 2, Health = 10000, AttackTimer = 100, GoldReward = 8 }
            }
        };
        return new IdleCombatSimulation(config, save, "prado_cristalino", 12345);
    }
    private static void Catalog()
    {
        Check(DiscoveryCatalog.Entries.Count == 20, "quantidade incorreta");
        Check(DiscoveryCatalog.Entries.Select(x => x.Stage).Distinct().Count() == 20, "ID duplicado");
        for (int stage = 1; stage <= 100; stage++)
            Check((DiscoveryCatalog.Find(stage) != null) == (stage % 5 == 0), "marco incorreto");
    }
    private static void Rewards()
    {
        foreach (int stage in new[] { 1, 5, 10 })
        {
            var sim = New(stage, stage - 1, out var save); int events = 0;
            sim.EventRaised += e => {
                if (e.Type != CombatEventType.DiscoveryAwarded) return;
                events++;
                Check(save.DiscoveryStages.Contains(stage) && save.HighestClearedStage == stage && save.ActiveRun == null && sim.Run.IsStageCleared, "evento antes da transação");
            };
            Tick(sim, 180); Check(sim.Run.IsStageCleared, "fixture não concluiu");
            Check(events == (stage % 5 == 0 ? 1 : 0), "recompensa incorreta");
            int crystals = save.Crystals;
            Check(EconomyProgression.TrySelectTrainingStage(save, stage), "treino bloqueado");
            var replay = new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 44);
            Tick(replay, 180);
            Check(replay.Run.IsStageCleared && save.DiscoveryStages.Count == events && save.Crystals == crystals, "replay duplicou recompensa");
        }
    }
    private static void Escorts()
    {
        var sim = New(5, 4, out var save);
        for (int i = 0; i < 3000 && !sim.Run.BossSpawned; i++) sim.Step(.05f);
        Check(sim.Run.BossSpawned, "mini-chefe ausente");
        // Deixa o chefe vulnerável e a escolta resistente para validar a condição de vitória.
        foreach (var enemy in sim.Enemies) { enemy.Health = enemy.IsBoss ? 1 : 100000; enemy.AttackTimer = 100; }
        Tick(sim, 1);
        Check(!sim.Enemies.Any(x => x.IsBoss) && sim.Enemies.Count >= 3, "fixture não matou só o chefe");
        Check(save.DiscoveryStages.Count == 0 && !sim.Run.IsStageCleared, "descoberta antes de vencer escolta");
    }
    private static void InDirectory(Action<AtomicSaveRepository, string> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dw-discoveries-" + Guid.NewGuid().ToString("N"));
        try { test(new AtomicSaveRepository(directory), directory); }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
    private static void Envelope(string directory, JObject payload, int version)
    {
        string text = payload.ToString(Formatting.None);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        File.WriteAllText(Path.Combine(directory, "dig-and-wonder.save.json"), JsonConvert.SerializeObject(new { SchemaVersion = version, Payload = text, Sha256 = hash }));
    }
    private static void CollectionSave()
    {
        InDirectory((repo, dir) => {
            var sim = New(10, 9, out var save); Tick(sim, 180); repo.Save(save);
            Check(repo.TryLoad(out var loaded, out _) && loaded.DiscoveryStages.SequenceEqual(new[] { 10 }), "coleção não retomou");
            foreach (int invalid in new[] { 10, 11, 100 })
            {
                loaded.DiscoveryStages.Add(invalid);
                bool rejected = false; try { repo.Save(loaded); } catch (InvalidDataException) { rejected = true; }
                Check(rejected, "aceitou descoberta inválida"); loaded.DiscoveryStages.RemoveAt(loaded.DiscoveryStages.Count - 1);
            }
        });
        InDirectory((repo, dir) => {
            JObject payload = JObject.FromObject(new GameSaveData()); payload.Remove("DiscoveryStages"); Envelope(dir, payload, 4);
            Check(!repo.TryLoad(out _, out _) && repo.HasAnySaveFile, "campo ausente resetou coleção");
        });
    }
    private static void InvalidSnapshotShapes()
    {
        foreach (string field in new[] { "Enemies", "PendingHits" })
            InDirectory((repo, dir) => {
                var sim = New(4, 3, out var save); sim.Step(.05f); save.ActiveRun = sim.CaptureRun();
                JObject payload = JObject.FromObject(save); payload["ActiveRun"][field] = new JArray(42);
                Envelope(dir, payload, 4);
                Check(!repo.TryLoad(out _, out _) && repo.HasAnySaveFile, "snapshot malformado aceito");
            });
    }
    private static void Migration()
    {
        InDirectory((repo, dir) => {
            var sim = New(35, 34, out var save); Tick(sim, .1f);
            save.ActiveRun = sim.CaptureRun(); save.ActiveRun.EncounterVersion = 1;
            save.Gold = 321; save.Keys = 2; save.Chests = 2; save.Crystals = 77;
            save.Characters[0].TotalExperience = 1234; save.Characters[0].Powers.Add(new PowerProgressData { PowerId = "serration", Rank = 2 });
            JObject payload = JObject.FromObject(save); payload["SchemaVersion"] = 3; payload.Remove("DiscoveryStages");
            Envelope(dir, payload, 3);
            Check(repo.TryLoad(out var loaded, out _), "schema 3 não migrou");
            Check(loaded.SchemaVersion == 4 && loaded.CampaignStage == 35 && loaded.HighestClearedStage == 34 && loaded.Gold == 321 && loaded.Crystals == 77 && loaded.Keys == 2 && loaded.Chests == 2 &&
                loaded.Characters[0].TotalExperience == 1234 && loaded.Characters[0].Powers[0].Rank == 2 && loaded.ActiveRun.EncounterVersion == 1 && loaded.DiscoveryStages.Count == 0,
                "migração perdeu progresso");
            repo.Save(loaded); Check(repo.TryLoad(out var again, out _) && again.CampaignStage == 35, "migração não persistiu");
        });
    }
    private static void Training()
    {
        var save = new GameSaveData { HighestClearedStage = 34, CampaignStage = 35, AttackLevel = 80, HealthLevel = 80, UnlockedStageId = "jardim_fossil" };
        Check(!EconomyProgression.TrySelectTrainingStage(save, 35) && !EconomyProgression.TrySelectTrainingStage(save, -1) && save.CampaignStage == 35, "treino aceitou fase bloqueada");
        Check(EconomyProgression.TrySelectTrainingStage(save, 10), "não selecionou fase anterior");
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 44);
        // BeginStage deve preservar a fronteira de bioma enquanto treina em outro cenário.
        Check(!EconomyProgression.TrySelectTrainingStage(save, 5), "interrompeu tentativa");
        Tick(sim, 180);
        Check(sim.Run.IsTraining && sim.Run.IsStageCleared && save.CampaignStage == 35 && save.HighestClearedStage == 34, "treino regrediu fronteira");
        Check(!EconomyProgression.CanContinueCampaign(save, sim.Run, sim.Economy), "treino avançou automaticamente");
        Check(save.DiscoveryStages.Contains(10), "perfil antigo não recuperou descoberta no treino");
        Check(save.Crystals == 0 && save.UnlockedStageId == "jardim_fossil", "treino pagou cristais ou alterou bioma da campanha");
    }
    private static void SpawnTypes()
    {
        for (int stage = 1; stage <= 4; stage++)
        {
            var sim = New(stage, stage - 1, out _); sim.AutoAttackEnabled = false; sim.Run.SpecialCooldown = 100; sim.Step(.05f);
            Check(sim.Enemies.All(x => (int)x.Archetype < stage), "tipo apareceu antes da introdução");
            foreach (var enemy in sim.Enemies) Check(Math.Abs(sim.EnemyMaximumHealth(enemy) - enemy.Health) < .001, "barra começa incompleta");
            if (stage == 2) Check(sim.Enemies.Any(x => x.Archetype == EnemyArchetype.Swift), "ágil ausente");
            if (stage == 3) Check(sim.Enemies.Any(x => x.Archetype == EnemyArchetype.Armored), "blindado ausente");
        }
        var later = New(7, 6, out _); later.Step(.05f);
        Check(later.Enemies.Select(x => x.VisualId).Distinct().Count() == later.Enemies.Count, "silhuetas dos primeiros tipos não são distintas");
    }
    private static void Armor()
    {
        foreach (bool special in new[] { false, true })
        {
            var normal = Fixture(EnemyArchetype.Regular, special); var armor = Fixture(EnemyArchetype.Armored, special);
            Tick(normal, 1); Tick(armor, 1);
            float n = 10000 - normal.Enemies[0].Health, a = 10000 - armor.Enemies[0].Health;
            Check(n > 0 && (special ? Math.Abs(n - a) < .01 : a < n * .8f), "blindagem/especial incorreto");
        }
    }
    private static void AttackPatterns()
    {
        int Count(EnemyArchetype kind)
        {
            var sim = Fixture(kind, attack: true); int count = 0;
            sim.EventRaised += e => { if (e.Type == CombatEventType.EnemyTelegraph && e.EntityId == 1) count++; };
            Tick(sim, 5); return count;
        }
        Check(Count(EnemyArchetype.Swift) > Count(EnemyArchetype.Regular), "ágil sem cadência");
        var charged = Fixture(EnemyArchetype.Charger, attack: true); int launched = 0;
        charged.EventRaised += e => { if (e.Type == CombatEventType.ProjectileLaunched && e.EntityId == 1) launched++; };
        charged.Step(.05f);
        Check(charged.IsWindingUp(1) && charged.PlayerHealth == 120 && launched == 0, "golpe não avisado ou projétil antecipado");
        Tick(charged, 2); Check(charged.PlayerHealth < 120 && launched == 1, "golpe nunca atingiu ou lançamento duplicado");
    }
    private static void Interrupt()
    {
        var sim = Fixture(EnemyArchetype.Charger, firstHealth: 1, attack: true); sim.Run.AttackTimer = 0;
        Tick(sim, 2);
        Check(sim.Enemies.All(x => x.Id != 1) && sim.PlayerHealth == 120, "ataque carregado atingiu após interrupção");
        var live = Fixture(EnemyArchetype.Regular); var save = live.Profile;
        save.ActiveRun = live.CaptureRun(); save.ActiveRun.AttackTimer = 100;
        save.ActiveRun.PendingHits = new[] { new PendingHitSnapshot { SourceEntityId = 99, TargetsPlayer = true, Damage = 12, RemainingTravel = .5f } };
        var resumed = new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 9); Tick(resumed, 1);
        Check(resumed.PlayerHealth == 108, "projétil já lançado foi apagado");
    }
    private static void Retarget()
    {
        foreach (bool special in new[] { false, true })
        {
            var sim = Fixture(EnemyArchetype.Regular, special, 1); Tick(sim, 1);
            Check(sim.Enemies.Count == 1 && sim.Enemies[0].Id == 2 && sim.Enemies[0].Health < 10000, "rajada desperdiçou ataques após morte");
        }
    }
    private static void Resume()
    {
        InDirectory((repo, dir) => {
            var sim = New(4, 3, out var save); sim.AutoAttackEnabled = false;
            for (int i = 0; i < 200 && !sim.Enemies.Any(x => sim.IsWindingUp(x.Id)); i++) sim.Step(.05f);
            save.ActiveRun = sim.CaptureRun(); Check(save.ActiveRun.PendingHits.Any(x => x.TelegraphRemaining > 0), "fixture sem preparação");
            repo.Save(save); Check(repo.TryLoad(out var loaded, out _), "snapshot não carregou");
            var resumed = new IdleCombatSimulation(EconomyChecks.Configuration(), loaded, "prado_cristalino", 7) { AutoAttackEnabled = false };
            Tick(sim, 5); Tick(resumed, 5);
            Check(JsonConvert.SerializeObject(sim.CaptureRun()) == JsonConvert.SerializeObject(resumed.CaptureRun()), "retomada mudou combate");
        });
    }
    private static void PreviousEncounter()
    {
        var sim = New(4, 3, out var save); save.ActiveRun = sim.CaptureRun(); save.ActiveRun.EncounterVersion = 1;
        var resumed = new IdleCombatSimulation(EconomyChecks.Configuration(), save, "prado_cristalino", 9); resumed.Step(.05f);
        Check(resumed.Run.EncounterVersion == 1 && resumed.Enemies.All(x => x.Archetype == EnemyArchetype.Regular && x.VisualId == "torrao"), "encontro 1 mudou de tipos");
    }
    private static void Feedback()
    {
        var sim = New(1, 0, out var save); Tick(sim, 180);
        Check(sim.LastClearSummary.Contains("OURO") && !sim.LastClearSummary.Contains("BAÚ") && sim.LastClearSummary.Contains("CRISTAIS"), "resumo anuncia recompensa falsa");
        save.Gold = 100000; Check(GameplayHudModel.DefeatAdvice(sim).Contains("melhorar"), "sem dica de compra acessível");
    }
}
