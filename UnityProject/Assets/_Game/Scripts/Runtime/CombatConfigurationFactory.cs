using System.Linq;

namespace DigAndWonder
{
    public static class CombatConfigurationFactory
    {
        public static CombatConfiguration Create(CombatBalanceDefinition balance)
        {
            ProgressionDefinition progression = balance.progression;
            return new CombatConfiguration
            {
                PlayerBaseHealth = balance.playerBaseHealth,
                BasicAttackDamage = balance.drillAttackDamage,
                BasicAttackInterval = balance.drillAttackInterval,
                BasicAttackHitCount = balance.drillAttackHitCount,
                BasicAttackHitInterval = balance.drillAttackHitInterval,
                BasicAttackTravelTime = balance.drillAttackTravelTime,
                SimulationStep = balance.simulationStep,
                MaximumFrameCatchUp = balance.maximumFrameCatchUp,
                MaximumEnemies = balance.maximumEnemies,
                MaximumPendingSpawns = balance.maximumPendingSpawns,
                MaximumProjectiles = balance.maximumProjectiles,
                SurpriseChancePercent = progression.surpriseChancePercent,
                SegmentsPerStage = progression.segmentsPerStage,
                RegularEnemy = ToRuntime(balance.regularEnemy),
                Special = ToRuntime(balance.specialAbility),
                AttackUpgrade = ToRuntime(progression.attackUpgrade),
                HealthUpgrade = ToRuntime(progression.healthUpgrade),
                CriticalChanceUpgrade = ToRuntime(progression.criticalChanceUpgrade),
                CriticalDamageUpgrade = ToRuntime(progression.criticalDamageUpgrade),
                Stages = progression.normalCycle.Select(ToRuntime).ToArray(),
                SurpriseStage = progression.surpriseStage == null ? null : ToRuntime(progression.surpriseStage)
            };
        }

        private static EnemyCombatData ToRuntime(EnemyDefinition source)
        {
            if (source == null) return null;
            return new EnemyCombatData
            {
                Id = source.id,
                DisplayName = source.displayName,
                MaxHealth = source.maxHealth,
                Damage = source.damage,
                FirstAttackDelay = source.firstAttackDelay,
                AttackInterval = source.attackInterval,
                TelegraphDuration = source.telegraphDuration,
                ProjectileFlight = source.projectileFlight,
                GoldReward = source.goldReward,
                IsBoss = source.isBoss
            };
        }

        private static SpecialCombatData ToRuntime(AbilityDefinition source)
        {
            if (source == null) return null;
            return new SpecialCombatData
            {
                Damage = source.damage,
                Cooldown = source.cooldown,
                TravelTime = source.travelTime,
                HitCount = source.hitCount,
                HitInterval = source.hitInterval
            };
        }

        private static UpgradeCombatData ToRuntime(UpgradeDefinition source)
        {
            if (source == null) return null;
            return new UpgradeCombatData
            {
                Kind = source.kind,
                BaseCost = source.baseCost,
                CostStep = source.costStep,
                MaximumLevel = source.maxLevel,
                Bonus = source.additiveBonus,
                Implemented = source.implemented
            };
        }

        private static StageCombatData ToRuntime(StageDefinition source)
        {
            return new StageCombatData
            {
                Id = source.id,
                DisplayName = source.displayName,
                LengthSegments = source.lengthSegments,
                ChestCount = source.chestCount,
                KeyCount = source.keyCount,
                WaveInterval = source.timedWaveInterval,
                WaveCount = source.timedWaveCount,
                LateWaveAfter = source.lateWaveAfter,
                LateWaveSize = source.lateWaveSize,
                Boss = ToRuntime(source.boss)
            };
        }
    }
}
