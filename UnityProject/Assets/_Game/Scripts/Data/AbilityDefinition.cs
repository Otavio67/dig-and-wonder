using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Ability", fileName = "Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public bool implemented;
        public int unlockPhase;
        public int goldCost;
        public float damage;
        public float cooldown;
        public float travelTime;
        public int hitCount = 1;
        public float hitInterval;
        public float duration;
        public int maxSimultaneous = 1;
        [TextArea] public string targeting;
        [TextArea] public string role;
    }
}
