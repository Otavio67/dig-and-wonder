using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Enemy", fileName = "Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float maxHealth;
        public float damage;
        public float firstAttackDelay;
        public float attackInterval;
        public float telegraphDuration;
        public float projectileFlight;
        public int goldReward;
        public bool isBoss;
    }
}
