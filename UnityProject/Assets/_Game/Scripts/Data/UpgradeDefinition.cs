using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Upgrade", fileName = "Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        public UpgradeKind kind;
        public string displayName;
        public int baseCost;
        public int costStep;
        public int maxLevel;
        public float additiveBonus;
        public bool implemented;
        [TextArea] public string sourceNote;

        public int GetCost(int currentLevel)
        {
            return baseCost + (costStep * currentLevel);
        }
    }
}
