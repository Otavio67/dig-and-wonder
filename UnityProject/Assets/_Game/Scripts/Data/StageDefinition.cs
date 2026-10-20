using System;
using UnityEngine;

namespace DigAndWonder
{
    [Serializable]
    public struct TerrainLayerDefinition
    {
        public string displayName;
        public Color color;
        public int hardness;
        public int goldReward;
        public float appearanceChance;
    }

    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Stage", fileName = "Stage")]
    public sealed class StageDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public Color caveColor;
        public Color groundColor;
        public Color rockColor;
        public int lengthSegments;
        public int chestCount;
        public int keyCount;
        public float timedWaveInterval;
        public int timedWaveCount;
        public int lateWaveAfter;
        public int lateWaveSize;
        public int optionalFinalEnemies;
        public TerrainLayerDefinition[] terrainLayers = Array.Empty<TerrainLayerDefinition>();
        public EnemyDefinition regularEnemy;
        public EnemyDefinition boss;
        [TextArea] public string sourceNote;
    }
}
