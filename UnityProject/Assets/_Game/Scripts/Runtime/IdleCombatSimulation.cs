using System;
using System.Collections.Generic;
using System.Linq;

namespace DigAndWonder
{
    public sealed class IdleCombatSimulation
    {
        private readonly CombatConfiguration config;
        private readonly GameSaveData profile;
        private readonly List<EnemySnapshot> enemies = new List<EnemySnapshot>();
        private readonly List<PendingHitSnapshot> pendingHits = new List<PendingHitSnapshot>();
        private ActiveRunSnapshot run;
        private StageCombatData currentStage;
        private CharacterCombatData currentCharacter;
        private EnemyCombatData regularDefinition;
        private EnemyCombatData bossDefinition;

        public event Action<CombatEvent> EventRaised;

        public GameSaveData Profile => profile;
        public ActiveRunSnapshot Run => run;
        public IReadOnlyList<EnemySnapshot> Enemies => enemies;
        public float PlayerMaximumHealth => config.Economy == null
            ? config.PlayerBaseHealth + profile.HealthLevel * config.HealthUpgrade.Bonus : CurrentStats.MaximumHealth;
        public DrillStats CurrentStats => EconomyProgression.Stats(profile, config.Economy,
            currentCharacter.Id, currentCharacter.Experience, config.BasicAttackDamage, config.PlayerBaseHealth, currentCharacter);
        public EconomyRules Economy => config.Economy;
        public float PlayerHealth => run.PlayerHealth;
        public int Gold => profile.Gold;
        public string LastClearSummary { get; private set; } = "";
        public float EnemyMaximumHealth(EnemySnapshot enemy) => EnemyMaximumHealth(enemy.IsBoss) * EnemyArchetypes.Health(enemy.Archetype);
        public bool IsWindingUp(int enemyId) => pendingHits.Any(hit => hit.SourceEntityId == enemyId && hit.TelegraphRemaining > 0);
        public string EnemyLabel(EnemySnapshot enemy) => enemy.IsBoss
            ? (run.EncounterVersion >= 1 && run.CampaignStage % 10 == 5 ? "MINI-CHEFE" : "GUARDIÃO")
            : EnemyArchetypes.Label(enemy.Archetype);
        public int Keys => profile.Keys;
        public bool IsActive => run != null && !run.IsStageCleared && !run.IsPlayerDefeated;
        public bool AutoAttackEnabled { get; set; } = true;
        public string CurrentStageId => run?.StageId;
        public string CurrentStageDisplayName => currentStage?.DisplayName;
        public string ActiveCharacterId => run?.CharacterId;
        public int StageWaveCount => run.EncounterVersion >= 1 ? (run.CampaignStage % 10 == 0 ? 0 : 3) : currentStage.WaveCount;
        public float SpecialCooldownSeconds => config.Special.Cooldown;
        public float EnemyMaximumHealth(bool boss) => boss ? bossDefinition.MaxHealth : regularDefinition.MaxHealth;
        public float BasicHitsPerSecond => (currentCharacter.BasicHitCount > 0 ? currentCharacter.BasicHitCount : config.BasicAttackHitCount)
            * AttackSpeed / config.BasicAttackInterval;

        public bool TryActivateSpecial()
        {
            if (!IsActive || run.SpecialHitsRemaining > 0 || run.SpecialCooldown > 0f) return false;
            EnemySnapshot target = SelectTarget();
            if (target == null) return false;
            run.SpecialTargetId = target.Id;
            run.SpecialHitsRemaining = config.Special.HitCount;
            run.SpecialTravelTimer = config.Special.TravelTime;
            run.SpecialHitTimer = 0f;
            run.SpecialCooldown = config.Special.Cooldown;
            Emit(CombatEventType.SpecialStarted, target.Id, config.Special.Damage, target.IsBoss ? bossDefinition.DisplayName : regularDefinition.DisplayName);
            return true;
        }

        public IdleCombatSimulation(CombatConfiguration configuration, GameSaveData saveData, string newStageId, uint randomSeed)
        {
            config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            profile = saveData ?? throw new ArgumentNullException(nameof(saveData));
            config.Economy?.Validate();
            NormalizeProfile();

            if (profile.ActiveRun != null && !profile.ActiveRun.IsPlayerDefeated && !profile.ActiveRun.IsStageCleared)
            {
                RestoreRun(profile.ActiveRun);
                Emit(CombatEventType.RunStarted, text: currentStage.DisplayName + " (retomada)");
            }
            else
            {
                BeginStage(FindStage(newStageId) ?? FirstStage(), randomSeed);
            }
        }

        private void NormalizeProfile()
        {
            if (profile.SchemaVersion != GameSaveData.CurrentSchemaVersion)
                throw new InvalidOperationException("O perfil deve ser migrado antes de iniciar a simulação.");
            if (config.Economy != null)
            {
                if (profile.CampaignStage < 1 || profile.CampaignStage > config.Economy.MaximumStage ||
                    profile.HighestClearedStage < 0 || profile.HighestClearedStage > config.Economy.MaximumStage ||
                    profile.ActiveRun != null && (profile.ActiveRun.CampaignStage < 1 || profile.ActiveRun.CampaignStage > config.Economy.MaximumStage))
                    throw new InvalidOperationException("A campanha salva não está disponível nesta tabela de conteúdo.");
                foreach (EconomyUpgradeRule upgrade in config.Economy.Upgrades)
                    if (EconomyProgression.Level(profile, upgrade.Kind) < 0 || EconomyProgression.Level(profile, upgrade.Kind) > upgrade.MaximumLevel)
                        throw new InvalidOperationException("Melhoria salva fora da tabela atual. Progresso preservado.");
            }
            if (profile.Gold < 0) profile.Gold = 0;
            if (profile.Keys < 0) profile.Keys = 0;
            if (profile.Chests < 0) profile.Chests = 0;
            if (config.Economy == null)
            {
                profile.AttackLevel = ClampLevel(profile.AttackLevel, config.AttackUpgrade.MaximumLevel);
                profile.HealthLevel = ClampLevel(profile.HealthLevel, config.HealthUpgrade.MaximumLevel);
            }
            profile.CriticalChanceLevel = Math.Max(0, profile.CriticalChanceLevel);
            profile.CriticalDamageLevel = Math.Max(0, profile.CriticalDamageLevel);
            CharacterProgression.GrantStageRewards(profile, config.Characters);
        }

        public void BeginStage(StageCombatData stage, uint randomSeed)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            currentCharacter = ResolveCharacter(profile.EquippedCharacterId);
            ConfigureStage(stage, profile.CampaignStage, config.Economy == null ? 0 : 2);
            enemies.Clear();
            pendingHits.Clear();
            LastClearSummary = "";
            run = new ActiveRunSnapshot
            {
                StageId = stage.Id,
                CharacterId = currentCharacter.Id,
                CampaignStage = profile.CampaignStage,
                EncounterVersion = config.Economy == null ? 0 : 2,
                IsTraining = profile.CampaignStage <= profile.HighestClearedStage,
                PlayerHealth = PlayerMaximumHealth,
                AttackTimer = 0f,
                AttackHitTimer = 0f,
                AttackHitsRemaining = 0,
                AttackTargetId = 0,
                SpecialCooldown = config.Special.Cooldown,
                NextWaveTimer = 0f,
                NextEntityId = 1,
                RandomState = randomSeed == 0 ? 0x9E3779B9u : randomSeed
            };
            profile.ActiveRun = run;
            if (!run.IsTraining || run.EncounterVersion < 2) profile.UnlockedStageId = stage.Id;
            Emit(CombatEventType.RunStarted, text: stage.DisplayName);
        }

        public void Step(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f) return;
            float remaining = Math.Min(deltaTime, config.MaximumFrameCatchUp);
            while (remaining > 0f && IsActive)
            {
                float step = Math.Min(config.SimulationStep, remaining);
                StepFixed(step);
                remaining -= step;
            }
        }

        private void StepFixed(float step)
        {
            run.ElapsedSeconds += step;
            if (config.Economy != null && profile.RepairLevel > 0)
                run.PlayerHealth = Math.Min(PlayerMaximumHealth, run.PlayerHealth + PlayerMaximumHealth * CurrentStats.RepairPerSecond * step);
            SpawnTimedWaves(step);
            UpdateEnemyAttacks(step);
            UpdateSpecial(step);
            if (!IsActive) return;
            UpdateBasicAttack(step);
            UpdatePendingHits(step);
        }

        private void SpawnTimedWaves(float step)
        {
            if (run.EncounterVersion >= 1)
            {
                SpawnChapterEncounter(step);
                return;
            }
            if (run.BossSpawned) return;
            run.NextWaveTimer -= step;
            while (run.NextWaveTimer <= 0f && run.WavesSpawned < currentStage.WaveCount)
            {
                int waveIndex = run.WavesSpawned + 1;
                int waveSize = waveIndex > currentStage.LateWaveAfter ? currentStage.LateWaveSize : 1;
                for (int i = 0; i < waveSize; i++) QueueOrSpawnRegularEnemy();
                run.WavesSpawned++;
                run.NextWaveTimer += currentStage.WaveInterval;
            }

            // O guardião espera o fim da última onda, inclusive da fila de inimigos.
            // Tentativas antigas que já criaram o chefe continuam como foram salvas.
            if (run.WavesSpawned >= currentStage.WaveCount && !run.BossSpawned &&
                (config.Economy == null || run.PendingSpawns == 0 && ActiveEnemyCount() == 0))
            {
                run.BossSpawned = true;
                SpawnEnemy(bossDefinition, true);
            }
        }

        private void SpawnChapterEncounter(float step)
        {
            int phase = (run.CampaignStage - 1) % 10 + 1;
            if (run.BossSpawned || ActiveEnemyCount() > 0 || run.PendingSpawns > 0) return;
            if (phase == 10)
            {
                run.BossSpawned = true;
                SpawnEnemy(bossDefinition, true);
                return;
            }
            if (run.WavesSpawned >= 3) return;
            run.NextWaveTimer -= step;
            if (run.NextWaveTimer > 0f) return;
            run.WavesSpawned++;
            if (phase == 5 && run.WavesSpawned == 3)
            {
                run.BossSpawned = true;
                SpawnEnemy(bossDefinition, true);
                int escorts = 3 + (run.CampaignStage / 10 % 2);
                for (int i = 0; i < escorts; i++) QueueOrSpawnRegularEnemy();
            }
            else
            {
                int count = 3 + (run.WavesSpawned - 1 + phase - 1) % 3;
                for (int i = 0; i < count; i++) QueueOrSpawnRegularEnemy();
            }
            run.NextWaveTimer = 1f;
        }

        private void QueueOrSpawnRegularEnemy()
        {
            if (ActiveEnemyCount() < (run.EncounterVersion >= 1 ? Math.Max(5, config.MaximumEnemies) : config.MaximumEnemies))
            {
                SpawnEnemy(regularDefinition, false);
                return;
            }

            if (run.PendingSpawns < config.MaximumPendingSpawns)
                run.PendingSpawns++;
        }

        private void SpawnPendingEnemies()
        {
            while (run.PendingSpawns > 0 && ActiveEnemyCount() < (run.EncounterVersion >= 1 ? Math.Max(5, config.MaximumEnemies) : config.MaximumEnemies))
            {
                run.PendingSpawns--;
                SpawnEnemy(regularDefinition, false);
            }
        }

        private void SpawnEnemy(EnemyCombatData definition, bool isBoss)
        {
            EnemyArchetype kind = run.EncounterVersion >= 2 && !isBoss
                ? EnemyArchetypes.ForSpawn(run.CampaignStage, run.NextEntityId) : EnemyArchetype.Regular;
            var enemy = new EnemySnapshot
            {
                Id = run.NextEntityId++,
                VisualId = run.EncounterVersion >= 2 && isBoss && run.CampaignStage % 10 == 5
                    ? "mineiro_sonolento" : run.EncounterVersion >= 2 && !isBoss ? EnemyArchetypes.Visual(kind, definition.Id) : definition.Id,
                Archetype = kind,
                Health = definition.MaxHealth * EnemyArchetypes.Health(kind),
                AttackTimer = definition.FirstAttackDelay + (run.EncounterVersion >= 2 ? (run.NextEntityId % 3) * .25f : 0),
                IsBoss = isBoss,
                GoldReward = definition.GoldReward
            };
            enemies.Add(enemy);
            Emit(isBoss ? CombatEventType.BossSpawned : CombatEventType.EnemySpawned, enemy.Id, enemy.Health, definition.DisplayName);
        }

        private void UpdateEnemyAttacks(float step)
        {
            foreach (EnemySnapshot enemy in enemies.ToArray())
            {
                if (enemy.Health <= 0f) continue;
                EnemyCombatData definition = enemy.IsBoss ? bossDefinition : regularDefinition;
                enemy.AttackTimer -= step;
                if (enemy.AttackTimer > 0f) continue;
                enemy.AttackTimer += definition.AttackInterval * EnemyArchetypes.Interval(enemy.Archetype);
                float telegraph = run.EncounterVersion >= 2 && (enemy.IsBoss || enemy.Archetype == EnemyArchetype.Charger)
                    ? Math.Max(1.2f, definition.TelegraphDuration) : definition.TelegraphDuration;
                Emit(CombatEventType.EnemyTelegraph, enemy.Id, telegraph, EnemyLabel(enemy));
                QueueHit(new PendingHitSnapshot
                {
                    SourceEntityId = enemy.Id,
                    Damage = definition.Damage * EnemyArchetypes.Damage(enemy.Archetype),
                    TelegraphRemaining = run.EncounterVersion >= 2 ? telegraph : 0,
                    RemainingTravel = telegraph + definition.ProjectileFlight,
                    TargetsPlayer = true
                });
            }
        }

        private void UpdateSpecial(float step)
        {
            if (run.SpecialHitsRemaining > 0)
            {
                run.SpecialTravelTimer -= step;
                if (run.SpecialTravelTimer <= 0f)
                {
                    run.SpecialHitTimer -= step;
                    while (run.SpecialHitsRemaining > 0 && run.SpecialHitTimer <= 0f)
                    {
                        ApplySpecialHit();
                        if (!IsActive) return;
                        run.SpecialHitsRemaining--;
                        run.SpecialHitTimer += config.Special.HitInterval;
                    }
                    if (run.SpecialHitsRemaining == 0) run.SpecialTargetId = 0;
                }
                return;
            }

            run.SpecialCooldown = Math.Max(0f, run.SpecialCooldown - step);
            // O especial mantém sua automação; o mesmo contrato também aceita toque quando pronto.
            TryActivateSpecial();
        }

        private void ApplySpecialHit()
        {
            EnemySnapshot target = enemies.FirstOrDefault(e => e.Id == run.SpecialTargetId && e.Health > 0f);
            if (target == null && run.EncounterVersion >= 2)
            {
                target = SelectTarget();
                if (target != null) run.SpecialTargetId = target.Id;
            }
            if (target == null) return;
            // Captura os alvos antes dos impactos: uma morte pode liberar inimigos da fila.
            EnemySnapshot[] secondary = currentCharacter.SpecialTargetCount > 1
                ? enemies.Where(e => e.Id != target.Id && e.Health > 0).Take(currentCharacter.SpecialTargetCount - 1).ToArray()
                : Array.Empty<EnemySnapshot>();
            float damage = config.Special.Damage / config.Special.HitCount * GetDamageMultiplier();
            if (config.Economy != null)
                damage *= CurrentStats.SpecialMultiplier * (1f + PowerBonus(AttackPowerKind.Resonance));
            float primaryDamage = damage * BossMultiplier(target);
            target.Health = Math.Max(0f, target.Health - primaryDamage);
            Emit(CombatEventType.SpecialStrike, target.Id, primaryDamage, "Especial");
            Emit(CombatEventType.Hit, target.Id, primaryDamage, "Especial");
            if (target.Health <= 0f) DefeatEnemy(target);
            foreach (EnemySnapshot other in secondary)
            {
                if (!IsActive) return;
                float areaDamage = damage * BossMultiplier(other);
                other.Health = Math.Max(0, other.Health - areaDamage);
                Emit(CombatEventType.Hit, other.Id, areaDamage, "Especial em área");
                if (other.Health <= 0) DefeatEnemy(other);
            }
        }

        private void UpdateBasicAttack(float step)
        {
            if (run.AttackHitsRemaining > 0)
            {
                run.AttackHitTimer -= step;
                if (run.AttackHitTimer <= 0f)
                {
                    LaunchBasicHit();
                    run.AttackHitsRemaining--;
                    run.AttackHitTimer += config.BasicAttackHitInterval / AttackSpeed;
                    if (run.AttackHitsRemaining == 0) run.AttackTargetId = 0;
                }
                return;
            }

            run.AttackTimer -= step;
            if (!AutoAttackEnabled) run.AttackTimer = Math.Max(0f, run.AttackTimer);
            if (!AutoAttackEnabled || run.AttackTimer > 0f) return;
            StartBasicAttack();
        }

        /// <summary>Dispara uma rajada manual quando AUTO está desligado e a cadência permite.</summary>
        public bool TryManualBasicAttack()
        {
            if (!IsActive || AutoAttackEnabled || run.AttackHitsRemaining > 0 || run.AttackTimer > 0f) return false;
            return StartBasicAttack();
        }

        private bool StartBasicAttack()
        {
            EnemySnapshot target = SelectTarget();
            if (target == null) return false;
            run.AttackTimer = Math.Max(0f, run.AttackTimer) + config.BasicAttackInterval / AttackSpeed;
            run.AttackTargetId = target.Id;
            run.AttackHitsRemaining = currentCharacter.BasicHitCount > 0 ? currentCharacter.BasicHitCount : config.BasicAttackHitCount;
            run.AttackHitTimer = 0f;
            LaunchBasicHit();
            run.AttackHitsRemaining--;
            run.AttackHitTimer = config.BasicAttackHitInterval / AttackSpeed;
            if (run.AttackHitsRemaining == 0) run.AttackTargetId = 0;
            return true;
        }

        private void LaunchBasicHit()
        {
            EnemySnapshot target = enemies.FirstOrDefault(e => e.Id == run.AttackTargetId && e.Health > 0f);
            if (target == null && run.EncounterVersion >= 2)
            {
                target = SelectTarget();
                if (target != null) run.AttackTargetId = target.Id;
            }
            if (target == null || pendingHits.Count >= config.MaximumProjectiles) return;
            float damage = config.BasicAttackDamage * GetDamageMultiplier() * currentCharacter.ProjectileDamageMultiplier;
            bool critical = false;
            if (config.Economy != null)
            {
                damage *= 1f + PowerBonus(AttackPowerKind.Serration);
                run.RandomState = EconomyProgression.NextRandom(run.RandomState);
                critical = run.RandomState % 10000u < (uint)Math.Round(CurrentStats.CriticalChance * 10000);
                if (critical) damage *= CurrentStats.CriticalMultiplier;
            }
            pendingHits.Add(new PendingHitSnapshot
            {
                TargetEntityId = target.Id,
                Damage = damage,
                IsBasic = true,
                IsCritical = critical,
                RemainingTravel = config.BasicAttackTravelTime
            });
            Emit(CombatEventType.DrillAttack, target.Id, damage, "Disparo automático da broca");
            Emit(CombatEventType.ProjectileLaunched, target.Id, config.BasicAttackTravelTime, "Projétil da broca");
        }

        private void QueueHit(PendingHitSnapshot hit)
        {
            if (pendingHits.Count >= config.MaximumProjectiles) return;
            pendingHits.Add(hit);
            if (hit.TelegraphRemaining > 0f) return;
            Emit(CombatEventType.ProjectileLaunched,
                hit.TargetsPlayer ? hit.SourceEntityId : hit.TargetEntityId,
                hit.RemainingTravel,
                hit.TargetsPlayer ? "Projétil inimigo" : "Projétil da broca");
        }

        private void UpdatePendingHits(float step)
        {
            foreach (PendingHitSnapshot hit in pendingHits.ToArray())
            {
                if (hit.TelegraphRemaining > 0f && !enemies.Any(enemy => enemy.Id == hit.SourceEntityId && enemy.Health > 0))
                {
                    pendingHits.Remove(hit);
                    Emit(CombatEventType.ProjectileDissipated, hit.SourceEntityId, 0, "Preparação interrompida");
                    continue;
                }
                bool wasPreparing = hit.TelegraphRemaining > 0f;
                hit.TelegraphRemaining = Math.Max(0, hit.TelegraphRemaining - step);
                hit.RemainingTravel -= step;
                if (wasPreparing && hit.TelegraphRemaining <= 0f)
                    Emit(CombatEventType.ProjectileLaunched, hit.SourceEntityId, Math.Max(.01f, hit.RemainingTravel), "Projétil inimigo");
                if (hit.RemainingTravel > 0f) continue;
                pendingHits.Remove(hit);

                if (hit.TargetsPlayer)
                {
                    run.PlayerHealth = Math.Max(0f, run.PlayerHealth - hit.Damage);
                    Emit(CombatEventType.PlayerDamaged, hit.SourceEntityId, hit.Damage, "Impacto inimigo");
                    if (run.PlayerHealth <= 0f)
                    {
                        run.IsPlayerDefeated = true;
                        profile.ActiveRun = null;
                        ClearCombatActions();
                        Emit(CombatEventType.PlayerDefeated, hit.SourceEntityId, 0f, currentStage.DisplayName);
                        return;
                    }
                    continue;
                }

                EnemySnapshot target = enemies.FirstOrDefault(e => e.Id == hit.TargetEntityId && e.Health > 0f);
                if (target == null)
                {
                    Emit(CombatEventType.ProjectileDissipated, hit.TargetEntityId, 0f, "Alvo removido antes do impacto");
                    continue;
                }

                float damage = hit.Damage * BossMultiplier(target) * (hit.IsBasic ? EnemyArchetypes.BasicDamageTaken(target.Archetype) : 1f);
                target.Health = Math.Max(0f, target.Health - damage);
                Emit(CombatEventType.ProjectileImpact, target.Id, damage,
                    target.IsBoss ? bossDefinition.DisplayName : regularDefinition.DisplayName);
                Emit(CombatEventType.Hit, target.Id, damage,
                    hit.IsCritical ? "CRÍTICO!" : target.IsBoss ? bossDefinition.DisplayName : regularDefinition.DisplayName);
                if (hit.IsCritical) Emit(CombatEventType.CriticalHit, target.Id, damage, "Crítico");
                if (target.Health <= 0f) DefeatEnemy(target);
                if (!IsActive) return;
                if (hit.IsBasic && config.Economy != null) ApplyRicochet(target.Id, hit.Damage);
                if (!IsActive) return;
            }
        }

        private EnemySnapshot SelectTarget()
        {
            EnemySnapshot boss = enemies.FirstOrDefault(e => e.IsBoss && e.Health > 0f);
            return boss ?? enemies.FirstOrDefault(e => e.Health > 0f);
        }

        private void DefeatEnemy(EnemySnapshot enemy)
        {
            if (!enemies.Remove(enemy)) return;
            run.EnemiesDefeated++;
            if (enemies.Count==0 && run.PendingSpawns==0 && run.WavesSpawned>run.WavesCleared) run.WavesCleared=run.WavesSpawned;
            CharacterProgressData character = CharacterProgression.Find(profile, run.CharacterId);
            int previousLevel = CharacterProgression.GetLevel(character, currentCharacter.Experience).Level;
            float previousMaximumHealth = PlayerMaximumHealth;
            int experienceReward = config.Economy != null ? config.Economy.ExperienceReward(run.CampaignStage, enemy.IsBoss, currentCharacter.Experience) :
                enemy.IsBoss ? currentCharacter.Experience.BossExperience : currentCharacter.Experience.RegularEnemyExperience;
            // Repetir um chefe isolado é muito mais rápido que jogar uma fase com ondas.
            if (run.EncounterVersion >= 1 && enemy.IsBoss && run.CampaignStage % 10 == 0 && run.CampaignStage <= profile.HighestClearedStage)
                experienceReward = Math.Max(1, (int)Math.Ceiling(experienceReward * 0.15));
            int awardedExperience = CharacterProgression.Award(profile, run.CharacterId, currentCharacter.Experience, experienceReward);
            int newLevel = CharacterProgression.GetLevel(character, currentCharacter.Experience).Level;
            if (config.Economy != null && newLevel > previousLevel)
                run.PlayerHealth = Math.Min(PlayerMaximumHealth, run.PlayerHealth + PlayerMaximumHealth - previousMaximumHealth);
            profile.Gold = EconomyProgression.Credit(profile.Gold, enemy.GoldReward);
            string[] unlockedCharacters = Array.Empty<string>();
            int phase = (run.CampaignStage - 1) % 10 + 1;
            bool stageComplete = run.EncounterVersion == 0 ? enemy.IsBoss :
                run.PendingSpawns == 0 && ActiveEnemyCount() == 0 &&
                (phase == 10 ? enemy.IsBoss : run.WavesSpawned >= 3 && (phase != 5 || run.BossSpawned));
            bool chestReward = stageComplete && (run.EncounterVersion == 0 || phase == 5 || phase == 10);
            int clearGold = 0;
            int clearCrystals = 0;
            bool discoveryAwarded = false;
            // Observadores recebem a transação completa, inclusive ao salvar no evento.
            if (stageComplete)
            {
                if (chestReward) profile.Keys = EconomyProgression.Credit(profile.Keys, currentStage.KeyCount);
                int previousChests = profile.Chests;
                if (chestReward) profile.Chests = EconomyProgression.Credit(profile.Chests, currentStage.ChestCount);
                if (config.Economy != null)
                {
                    if (run.EncounterVersion >= 1 && !chestReward)
                    {
                        int before = profile.Gold;
                        profile.Gold = EconomyProgression.Credit(profile.Gold, (long)config.Economy.NormalGold(run.CampaignStage) * 10);
                        clearGold = profile.Gold - before;
                    }
                    EconomyProgression.RecordChestStage(profile, run.CampaignStage, profile.Chests - previousChests);
                    if (run.CampaignStage > profile.HighestClearedStage)
                    {
                        int crystals = config.Economy.FirstClearCrystals +
                            (run.CampaignStage % config.Economy.MilestoneInterval == 0 ? config.Economy.MilestoneCrystals : 0);
                        int beforeCrystals = profile.Crystals;
                        profile.Crystals = EconomyProgression.Credit(profile.Crystals, crystals);
                        clearCrystals = profile.Crystals - beforeCrystals;
                        profile.HighestClearedStage = run.CampaignStage;
                    }
                    if (run.EncounterVersion >= 2 && chestReward)
                        discoveryAwarded = DiscoveryCatalog.TryAward(profile, run.CampaignStage);
                    profile.CampaignStage = Math.Min(config.Economy.MaximumStage,
                        run.EncounterVersion >= 2 ? profile.HighestClearedStage + 1 : run.CampaignStage + 1);
                    unlockedCharacters = CharacterProgression.GrantStageRewards(profile, config.Characters);
                }
                if ((run.EncounterVersion == 0 || phase == 10) && !(run.EncounterVersion >= 2 && run.IsTraining))
                    profile.UnlockedStageId = GetNextStageId(currentStage.Id);
                LastClearSummary = (chestReward ? currentStage.ChestCount + " BAÚ + " + currentStage.KeyCount + " CHAVE" : clearGold + " OURO DE BÔNUS") +
                    (clearCrystals > 0 ? " • " + clearCrystals + " CRISTAIS" : "") +
                    (discoveryAwarded ? "\nDESCOBERTA: " + DiscoveryCatalog.Find(run.CampaignStage).Name : "");
                run.IsStageCleared = true;
                profile.ActiveRun = null;
                ClearCombatActions();
            }
            if (discoveryAwarded) Emit(CombatEventType.DiscoveryAwarded, 0, run.CampaignStage, DiscoveryCatalog.Find(run.CampaignStage).Name);
            foreach (string id in unlockedCharacters) Emit(CombatEventType.CharacterUnlocked, 0, run.CampaignStage, id);
            Emit(CombatEventType.EnemyDefeated, enemy.Id, enemy.GoldReward, enemy.IsBoss ? bossDefinition.DisplayName : regularDefinition.DisplayName);
            Emit(CombatEventType.GoldAwarded, enemy.Id, enemy.GoldReward, "Ouro por inimigo derrotado");
            if (clearGold > 0) Emit(CombatEventType.GoldAwarded, enemy.Id, clearGold, "Bônus de fase");
            if (awardedExperience > 0)
                Emit(CombatEventType.CharacterExperienceAwarded, enemy.Id, awardedExperience, run.CharacterId);
            if (newLevel > previousLevel)
                Emit(CombatEventType.CharacterLeveledUp, enemy.Id, newLevel, run.CharacterId);
            if (enemy.IsBoss) Emit(CombatEventType.BossDefeated, enemy.Id, enemy.GoldReward, bossDefinition.DisplayName);
            if (!stageComplete)
            {
                SpawnPendingEnemies();
                return;
            }
            if (chestReward)
            {
                Emit(CombatEventType.ChestAwarded, enemy.Id, currentStage.ChestCount, currentStage.DisplayName);
                Emit(CombatEventType.KeyAwarded, enemy.Id, currentStage.KeyCount, currentStage.DisplayName);
            }
            Emit(CombatEventType.StageCleared, enemy.Id, 0f, profile.UnlockedStageId);
        }

        public bool TryPurchaseUpgrade(UpgradeKind kind)
        {
            return TryPurchaseUpgradeInternal(kind, true);
        }

        /// <summary>Compra uma melhoria na base entre tentativas, sem reabrir um combate encerrado.</summary>
        public bool TryPurchaseUpgradeAtBase(UpgradeKind kind)
        {
            return TryPurchaseUpgradeInternal(kind, false);
        }

        private bool TryPurchaseUpgradeInternal(UpgradeKind kind, bool requireActiveRun)
        {
            if (requireActiveRun && !IsActive) return false;
            if (config.Economy != null)
            {
                float previousHealth = PlayerMaximumHealth;
                EconomyUpgradeRule economyUpgrade = config.Economy.Upgrade(kind);
                if (economyUpgrade == null) return false;
                int economyCost = economyUpgrade.Cost(EconomyProgression.Level(profile, kind));
                if (!EconomyProgression.TryBuyUpgrade(profile, config.Economy, currentCharacter.Experience, kind)) return false;
                if (IsActive && kind == UpgradeKind.Health)
                    run.PlayerHealth = Math.Min(PlayerMaximumHealth, run.PlayerHealth + PlayerMaximumHealth - previousHealth);
                Emit(CombatEventType.UpgradePurchased, 0, economyCost, kind.ToString());
                return true;
            }
            UpgradeCombatData upgrade = kind == UpgradeKind.Attack ? config.AttackUpgrade :
                kind == UpgradeKind.Health ? config.HealthUpgrade :
                kind == UpgradeKind.CriticalChance ? config.CriticalChanceUpgrade :
                kind == UpgradeKind.CriticalDamage ? config.CriticalDamageUpgrade : null;

            if (upgrade == null || !upgrade.Implemented) return false;
            int currentLevel = kind == UpgradeKind.Attack ? profile.AttackLevel :
                kind == UpgradeKind.Health ? profile.HealthLevel :
                kind == UpgradeKind.CriticalChance ? profile.CriticalChanceLevel : profile.CriticalDamageLevel;
            if (currentLevel >= upgrade.MaximumLevel) return false;
            int cost = upgrade.GetCost(currentLevel);
            if (profile.Gold < cost) return false;

            profile.Gold -= cost;
            switch (kind)
            {
                case UpgradeKind.Attack:
                    profile.AttackLevel++;
                    break;
                case UpgradeKind.Health:
                    profile.HealthLevel++;
                    if (run != null && IsActive)
                        run.PlayerHealth = Math.Min(PlayerMaximumHealth, run.PlayerHealth + upgrade.Bonus);
                    break;
                case UpgradeKind.CriticalChance:
                    profile.CriticalChanceLevel++;
                    break;
                case UpgradeKind.CriticalDamage:
                    profile.CriticalDamageLevel++;
                    break;
            }

            Emit(CombatEventType.UpgradePurchased, 0, cost, kind.ToString());
            return true;
        }

        private void ClearCombatActions()
        {
            enemies.Clear();
            pendingHits.Clear();
            run.Enemies = Array.Empty<EnemySnapshot>();
            run.PendingHits = Array.Empty<PendingHitSnapshot>();
            run.PendingSpawns = 0;
            run.AttackHitsRemaining = 0;
            run.AttackTargetId = 0;
            run.AttackHitTimer = 0f;
            run.SpecialHitsRemaining = 0;
            run.SpecialTargetId = 0;
            run.SpecialTravelTimer = 0f;
            run.SpecialHitTimer = 0f;
        }

        public ActiveRunSnapshot CaptureRun()
        {
            if (run == null) return null;
            run.Enemies = enemies.Select(CloneEnemy).ToArray();
            run.PendingHits = pendingHits.Select(ClonePendingHit).ToArray();
            return CloneRun(run);
        }

        private void RestoreRun(ActiveRunSnapshot snapshot)
        {
            if (snapshot.CharacterId != profile.EquippedCharacterId)
                throw new InvalidOperationException("A personagem equipada diverge da tentativa salva.");
            currentCharacter = ResolveCharacter(snapshot.CharacterId);
            ConfigureStage(FindStage(snapshot.StageId) ?? FirstStage(), snapshot.CampaignStage, snapshot.EncounterVersion);
            run = CloneRun(snapshot);
            enemies.Clear();
            if (snapshot.Enemies != null)
                enemies.AddRange(snapshot.Enemies.Where(e => e != null).Select(CloneEnemy));
            pendingHits.Clear();
            if (snapshot.PendingHits != null)
                pendingHits.AddRange(snapshot.PendingHits.Where(hit => hit != null).Select(ClonePendingHit));
            profile.ActiveRun = run;
        }

        private CharacterCombatData ResolveCharacter(string characterId)
        {
            if (CharacterProgression.Find(profile, characterId) == null)
                throw new InvalidOperationException("A personagem da tentativa não pertence ao perfil: " + characterId);
            CharacterCombatData definition = config.Characters?.FirstOrDefault(item => item != null && item.Id == characterId);
            if (definition == null || definition.Experience == null)
                throw new InvalidOperationException("Personagem ou curva de XP não encontrada: " + characterId);
            definition.Experience.Validate();
            return definition;
        }

        private float AttackSpeed => config.Economy == null ? 1f : CurrentStats.AttackSpeed;
        private float GetDamageMultiplier() => config.Economy == null ? 1f + profile.AttackLevel * config.AttackUpgrade.Bonus
            : CurrentStats.AttackDamage / config.BasicAttackDamage;
        private float PowerBonus(AttackPowerKind kind) => EconomyProgression.PowerBonus(profile, config.Economy, run.CharacterId, kind);
        private float BossMultiplier(EnemySnapshot enemy) => config.Economy != null && enemy.IsBoss
            ? 1f + PowerBonus(AttackPowerKind.BossBreaker) : 1f;

        private void ApplyRicochet(int primaryId, float originalDamage)
        {
            // Combinação com limite natural: inato + poder não ultrapassam 100% do disparo.
            float bonus = 1f - (1f - currentCharacter.InnateRicochet) * (1f - PowerBonus(AttackPowerKind.Ricochet));
            if (bonus <= 0f) return;
            EnemySnapshot secondary = enemies.FirstOrDefault(e => e.Id != primaryId && e.Health > 0);
            if (secondary == null) return;
            float damage = originalDamage * bonus * BossMultiplier(secondary) * EnemyArchetypes.BasicDamageTaken(secondary.Archetype);
            secondary.Health = Math.Max(0, secondary.Health - damage);
            Emit(CombatEventType.Hit, secondary.Id, damage, "Estilhaço magnético");
            if (secondary.Health <= 0) DefeatEnemy(secondary);
        }

        private void ConfigureStage(StageCombatData stage, int campaignStage, int encounterVersion)
        {
            currentStage = stage;
            regularDefinition = config.RegularEnemy;
            bossDefinition = stage.Boss;
            if (config.Economy == null) return;
            EconomyRules rules = config.Economy;
            int index = rules.StageIndex(campaignStage);
            regularDefinition = ScaleEnemy(config.RegularEnemy, false, index, rules);
            bossDefinition = ScaleEnemy(stage.Boss, true, index, rules);
            // Ondas de até cinco inimigos substituem os encontros antigos de um ou dois.
            if (encounterVersion >= 1) regularDefinition.Damage *= 0.55f;
            if (encounterVersion >= 1 && campaignStage % 10 == 5)
            {
                bossDefinition.MaxHealth = (float)(rules.EnemyHealth(campaignStage) * 3);
                bossDefinition.Damage = regularDefinition.Damage * 1.2f;
            }
            // Suaviza a barreira dos guardiões; versões antigas mantêm seus atributos salvos.
            if (encounterVersion >= 2 && campaignStage % 10 == 0)
            {
                bossDefinition.MaxHealth *= .9f;
                bossDefinition.Damage *= .85f;
            }
            currentStage = new StageCombatData
            {
                Id = stage.Id, DisplayName = stage.DisplayName, LengthSegments = stage.LengthSegments,
                ChestCount = 1, KeyCount = 1, WaveCount = rules.WaveCount, WaveInterval = rules.WaveInterval,
                LateWaveAfter = rules.LateWaveAfter, LateWaveSize = rules.LateWaveSize, Boss = bossDefinition
            };
        }

        private static EnemyCombatData ScaleEnemy(EnemyCombatData source, bool boss, int index, EconomyRules rules)
        {
            return new EnemyCombatData
            {
                Id = source.Id, DisplayName = source.DisplayName, IsBoss = boss,
                MaxHealth = (float)(rules.EnemyHealth(index + 1) * (boss ? rules.BossHealthMultiplier : 1)),
                Damage = (float)(rules.EnemyDamage(index + 1) * (boss ? rules.BossDamageMultiplier : 1)),
                GoldReward = checked(rules.NormalGold(index + 1) * (boss ? rules.BossGoldMultiplier : 1)),
                FirstAttackDelay = source.FirstAttackDelay, AttackInterval = source.AttackInterval,
                TelegraphDuration = source.TelegraphDuration, ProjectileFlight = source.ProjectileFlight
            };
        }
        private int ActiveEnemyCount() => enemies.Count(e => e.Health > 0f);

        private StageCombatData FindStage(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (config.SurpriseStage != null && config.SurpriseStage.Id == id) return config.SurpriseStage;
            return config.Stages?.FirstOrDefault(stage => stage.Id == id);
        }

        private StageCombatData FirstStage() => config.Stages != null && config.Stages.Length > 0 ? config.Stages[0] : throw new InvalidOperationException("Nenhuma fase foi configurada.");

        private string GetNextStageId(string currentId)
        {
            if (config.Stages == null || config.Stages.Length == 0) return currentId;
            if (config.SurpriseStage != null && currentId != config.SurpriseStage.Id && NextRandomPercent() < config.SurpriseChancePercent)
                return config.SurpriseStage.Id;
            if (config.SurpriseStage != null && currentId == config.SurpriseStage.Id)
                return config.Stages[0].Id;
            int index = Array.FindIndex(config.Stages, stage => stage.Id == currentId);
            return config.Stages[(index + 1 + config.Stages.Length) % config.Stages.Length].Id;
        }

        private int NextRandomPercent()
        {
            uint value = run.RandomState;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            run.RandomState = value == 0 ? 0x9E3779B9u : value;
            return (int)(run.RandomState % 100u);
        }

        private void Emit(CombatEventType type, int entityId = 0, float amount = 0f, string text = "")
        {
            if (type==CombatEventType.ProjectileLaunched && text=="Projétil da broca") run.ShotsFired++;
            EventRaised?.Invoke(new CombatEvent(type, entityId, amount, text));
        }

        private static int ClampLevel(int value, int maximum) => Math.Max(0, Math.Min(value, maximum));

        private static EnemySnapshot CloneEnemy(EnemySnapshot value) => new EnemySnapshot
        {
            Id = value.Id,
            VisualId = value.VisualId,
            Archetype = value.Archetype,
            Health = value.Health,
            AttackTimer = value.AttackTimer,
            IsBoss = value.IsBoss,
            GoldReward = value.GoldReward
        };

        private static ActiveRunSnapshot CloneRun(ActiveRunSnapshot value) => new ActiveRunSnapshot
        {
            StageId = value.StageId,
            CharacterId = value.CharacterId,
            CampaignStage = value.CampaignStage,
            EncounterVersion = value.EncounterVersion,
            IsTraining = value.IsTraining,
            ElapsedSeconds = value.ElapsedSeconds,
            ShotsFired=value.ShotsFired, EnemiesDefeated=value.EnemiesDefeated, WavesCleared=value.WavesCleared,
            PlayerHealth = value.PlayerHealth,
            AttackTimer = value.AttackTimer,
            AttackHitTimer = value.AttackHitTimer,
            AttackHitsRemaining = value.AttackHitsRemaining,
            AttackTargetId = value.AttackTargetId,
            SpecialCooldown = value.SpecialCooldown,
            SpecialTravelTimer = value.SpecialTravelTimer,
            SpecialHitTimer = value.SpecialHitTimer,
            SpecialHitsRemaining = value.SpecialHitsRemaining,
            SpecialTargetId = value.SpecialTargetId,
            WavesSpawned = value.WavesSpawned,
            NextWaveTimer = value.NextWaveTimer,
            PendingSpawns = value.PendingSpawns,
            NextEntityId = value.NextEntityId,
            BossSpawned = value.BossSpawned,
            IsStageCleared = value.IsStageCleared,
            IsPlayerDefeated = value.IsPlayerDefeated,
            RandomState = value.RandomState,
            Enemies = value.Enemies == null ? Array.Empty<EnemySnapshot>() : value.Enemies.Select(CloneEnemy).ToArray(),
            PendingHits = value.PendingHits == null ? Array.Empty<PendingHitSnapshot>() : value.PendingHits.Select(ClonePendingHit).ToArray()
        };

        private static PendingHitSnapshot ClonePendingHit(PendingHitSnapshot value) => new PendingHitSnapshot
        {
            SourceEntityId = value.SourceEntityId,
            TargetEntityId = value.TargetEntityId,
            Damage = value.Damage,
            RemainingTravel = value.RemainingTravel,
            TelegraphRemaining = value.TelegraphRemaining,
            TargetsPlayer = value.TargetsPlayer,
            IsSpecial = value.IsSpecial,
            IsBasic = value.IsBasic,
            IsCritical = value.IsCritical
        };
    }
}
