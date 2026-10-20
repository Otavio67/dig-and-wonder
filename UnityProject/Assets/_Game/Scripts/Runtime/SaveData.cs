using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DigAndWonder
{
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentSchemaVersion = 2;
        public int SchemaVersion = CurrentSchemaVersion;
        public int Gold;
        public int Keys;
        public int Chests;
        public int AttackLevel;
        public int HealthLevel;
        public int CriticalChanceLevel;
        public int CriticalDamageLevel;
        public string UnlockedStageId = "prado_cristalino";
        public string EquippedCharacterId = CharacterProgression.StarterCharacterId;
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<CharacterProgressData> Characters = new List<CharacterProgressData>
        {
            new CharacterProgressData { CharacterId = CharacterProgression.StarterCharacterId }
        };
        public ActiveRunSnapshot ActiveRun;
    }

    [Serializable]
    public sealed class CharacterProgressData
    {
        public string CharacterId;
        public long TotalExperience;
    }

    [Serializable]
    public sealed class ActiveRunSnapshot
    {
        public string StageId;
        public string CharacterId = CharacterProgression.StarterCharacterId;
        public float ElapsedSeconds;
        public float PlayerHealth;
        public float AttackTimer;
        public float AttackHitTimer;
        public int AttackHitsRemaining;
        public int AttackTargetId;
        public float SpecialCooldown;
        public float SpecialTravelTimer;
        public float SpecialHitTimer;
        public int SpecialHitsRemaining;
        public int SpecialTargetId;
        public int WavesSpawned;
        public float NextWaveTimer;
        public int PendingSpawns;
        public int NextEntityId;
        public bool BossSpawned;
        public bool IsStageCleared;
        public bool IsPlayerDefeated;
        public uint RandomState;
        public EnemySnapshot[] Enemies = Array.Empty<EnemySnapshot>();
        public PendingHitSnapshot[] PendingHits = Array.Empty<PendingHitSnapshot>();
    }

    [Serializable]
    public sealed class EnemySnapshot
    {
        public int Id;
        public float Health;
        public float AttackTimer;
        public bool IsBoss;
        public int GoldReward;
    }

    [Serializable]
    public sealed class PendingHitSnapshot
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float Damage;
        public float RemainingTravel;
        public bool TargetsPlayer;
        public bool IsSpecial;
    }
}
