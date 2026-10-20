using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Progression", fileName = "Progression")]
    public sealed class ProgressionDefinition : ScriptableObject
    {
        public int surpriseChancePercent;
        public int segmentsPerStage;
        public StageDefinition[] normalCycle;
        public StageDefinition surpriseStage;
        public UpgradeDefinition attackUpgrade;
        public UpgradeDefinition healthUpgrade;
        public UpgradeDefinition criticalChanceUpgrade;
        public UpgradeDefinition criticalDamageUpgrade;
        public UpgradeDefinition specialUpgrade;
    }
}
