using System;

namespace DigAndWonder
{
    public enum UpgradeKind
    {
        Attack,
        Health,
        Special,
        CriticalChance,
        CriticalDamage,
        AttackSpeed
    }

    [Serializable]
    public sealed class EnemyCombatData
    {
        public string Id;
        public string DisplayName;
        public float MaxHealth;
        public float Damage;
        public float FirstAttackDelay;
        public float AttackInterval;
        public float TelegraphDuration;
        public float ProjectileFlight;
        public int GoldReward;
        public bool IsBoss;
    }

    [Serializable]
    public sealed class SpecialCombatData
    {
        public float Damage;
        public float Cooldown;
        public float TravelTime;
        public int HitCount;
        public float HitInterval;
    }

    [Serializable]
    public sealed class UpgradeCombatData
    {
        public UpgradeKind Kind;
        public int BaseCost;
        public int CostStep;
        public int MaximumLevel;
        public float Bonus;
        public bool Implemented;

        public int GetCost(int level) => BaseCost + CostStep * level;
    }

    [Serializable]
    public sealed class StageCombatData
    {
        public string Id;
        public string DisplayName;
        public int LengthSegments;
        public int ChestCount;
        public int KeyCount;
        public float WaveInterval;
        public int WaveCount;
        public int LateWaveAfter;
        public int LateWaveSize;
        public EnemyCombatData Boss;
    }

    [Serializable]
    public sealed class CombatConfiguration
    {
        public float PlayerBaseHealth;
        public float BasicAttackDamage;
        public float BasicAttackInterval;
        public int BasicAttackHitCount;
        public float BasicAttackHitInterval;
        public float BasicAttackTravelTime;
        public float SimulationStep;
        public float MaximumFrameCatchUp;
        public int MaximumEnemies;
        public int MaximumPendingSpawns;
        public int MaximumProjectiles;
        public int SurpriseChancePercent;
        public int SegmentsPerStage;
        public EnemyCombatData RegularEnemy;
        public SpecialCombatData Special;
        public UpgradeCombatData AttackUpgrade;
        public UpgradeCombatData HealthUpgrade;
        public UpgradeCombatData CriticalChanceUpgrade;
        public UpgradeCombatData CriticalDamageUpgrade;
        public StageCombatData[] Stages;
        public StageCombatData SurpriseStage;
    }
}
