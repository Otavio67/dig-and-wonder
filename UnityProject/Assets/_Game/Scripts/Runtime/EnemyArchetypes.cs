namespace DigAndWonder
{
    public enum EnemyArchetype { Regular, Swift, Armored, Charger }

    /// <summary>Variações sobre a mesma curva de campanha; IDs e regras estáveis no save.</summary>
    public static class EnemyArchetypes
    {
        public static EnemyArchetype ForSpawn(int stage, int entityId)
        {
            // Introduz um comportamento por fase antes do primeiro mini-chefe.
            int available = stage < 2 ? 1 : stage < 3 ? 2 : stage < 4 ? 3 : 4;
            return (EnemyArchetype)((entityId - 1) % available);
        }
        public static string Visual(EnemyArchetype kind, string fallback)
        {
            switch (kind)
            {
                case EnemyArchetype.Swift: return "morcego_cascalho_ii";
                case EnemyArchetype.Armored: return "besouro_pedra";
                case EnemyArchetype.Charger: return "toupeira_pavio";
                default: return "slime_terra";
            }
        }
        public static string Label(EnemyArchetype kind)
        {
            switch (kind)
            {
                case EnemyArchetype.Swift: return "ÁGIL";
                case EnemyArchetype.Armored: return "BLINDADO";
                case EnemyArchetype.Charger: return "CARREGADOR";
                default: return "COMUM";
            }
        }
        public static float Health(EnemyArchetype kind) => kind == EnemyArchetype.Swift ? .7f : kind == EnemyArchetype.Armored ? 1.15f : 1f;
        public static float Damage(EnemyArchetype kind) => kind == EnemyArchetype.Swift ? .65f : kind == EnemyArchetype.Charger ? 1.45f : 1f;
        public static float Interval(EnemyArchetype kind) => kind == EnemyArchetype.Swift ? .7f : kind == EnemyArchetype.Armored ? 1.3f : kind == EnemyArchetype.Charger ? 1.5f : 1f;
        public static float BasicDamageTaken(EnemyArchetype kind) => kind == EnemyArchetype.Armored ? .75f : 1f;
    }
}
