namespace DigAndWonder
{
    public enum CombatEventType
    {
        RunStarted,
        EnemySpawned,
        BossSpawned,
        DrillAttack,
        ProjectileLaunched,
        ProjectileImpact,
        ProjectileDissipated,
        EnemyTelegraph,
        SpecialStarted,
        SpecialStrike,
        Hit,
        EnemyDefeated,
        GoldAwarded,
        PlayerDamaged,
        UpgradePurchased,
        BossDefeated,
        ChestAwarded,
        KeyAwarded,
        StageCleared,
        PlayerDefeated,
        Checkpoint,
        CharacterExperienceAwarded,
        CharacterLeveledUp,
        CriticalHit,
        EconomyChanged,
        CharacterUnlocked,
        CharacterEquipped,
        DiscoveryAwarded
    }

    public readonly struct CombatEvent
    {
        public readonly CombatEventType Type;
        public readonly int EntityId;
        public readonly float Amount;
        public readonly string Text;

        public CombatEvent(CombatEventType type, int entityId = 0, float amount = 0f, string text = "")
        {
            Type = type;
            EntityId = entityId;
            Amount = amount;
            Text = text;
        }
    }
}
