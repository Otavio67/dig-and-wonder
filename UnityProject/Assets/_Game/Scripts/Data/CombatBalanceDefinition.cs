using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Combat Balance", fileName = "CombatBalance")]
    public sealed class CombatBalanceDefinition : ScriptableObject
    {
        public float playerBaseHealth;
        public float drillAttackDamage;
        public float drillAttackInterval;
        public int drillAttackHitCount;
        public float drillAttackHitInterval;
        public float drillAttackTravelTime;
        public AbilityDefinition specialAbility;
        public EnemyDefinition regularEnemy;
        public ProgressionDefinition progression;
        public int maximumEnemies;
        public int maximumPendingSpawns;
        public int maximumProjectiles;
        public float simulationStep;
        public float maximumFrameCatchUp;
        [TextArea] public string sourceNote;
    }
}
