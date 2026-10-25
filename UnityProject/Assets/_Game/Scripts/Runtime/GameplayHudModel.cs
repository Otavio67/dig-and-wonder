using System;
using System.Globalization;

namespace DigAndWonder
{
    /// <summary>Leitura do domínio para o HUD; não concede recursos nem altera o balanceamento.</summary>
    public static class GameplayHudModel
    {
        private static readonly CultureInfo Portuguese = CultureInfo.GetCultureInfo("pt-BR");
        public static readonly UpgradeKind[] UpgradeOrder = {
            UpgradeKind.Attack, UpgradeKind.Health, UpgradeKind.Repair, UpgradeKind.AttackSpeed,
            UpgradeKind.CriticalChance, UpgradeKind.CriticalDamage, UpgradeKind.Special
        };

        public static string Number(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "—";
            if (Math.Abs(value) >= 1000000000) return (value / 1000000000).ToString("0.##", Portuguese) + "B";
            if (Math.Abs(value) >= 1000000) return (value / 1000000).ToString("0.##", Portuguese) + "M";
            if (Math.Abs(value) >= 10000) return (value / 1000).ToString("0.##", Portuguese) + "K";
            return value.ToString("0.##", Portuguese);
        }

        public static float ShotDamage(IdleCombatSimulation sim, CharacterCombatData character)
            => sim.CurrentStats.AttackDamage * (character?.ProjectileDamageMultiplier ?? 1f) *
               (1f + EconomyProgression.PowerBonus(sim.Profile, sim.Economy, sim.ActiveCharacterId, AttackPowerKind.Serration));

        public static string UpgradeTitle(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Attack: return "ATQ";
                case UpgradeKind.Health: return "VIDA";
                case UpgradeKind.Repair: return "AUTORREPARO";
                case UpgradeKind.AttackSpeed: return "CADÊNCIA";
                case UpgradeKind.CriticalChance: return "CHANCE CRÍTICA";
                case UpgradeKind.CriticalDamage: return "DANO CRÍTICO";
                default: return "ESPECIAL";
            }
        }

        public static string UpgradeIcon(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Health: return "health";
                case UpgradeKind.Repair: return "repair";
                case UpgradeKind.AttackSpeed: return "motor";
                case UpgradeKind.Special: return "core";
                default: return "attack";
            }
        }

        public static string UpgradeValue(UpgradeKind kind, IdleCombatSimulation sim, CharacterCombatData character)
        {
            DrillStats stats = sim.CurrentStats;
            switch (kind)
            {
                case UpgradeKind.Attack: return Number(ShotDamage(sim, character)) + " de dano";
                case UpgradeKind.Health: return Number(stats.MaximumHealth) + " de vida";
                case UpgradeKind.Repair: return Number(stats.MaximumHealth * stats.RepairPerSecond) + " PV/s";
                case UpgradeKind.AttackSpeed: return Number(stats.AttackSpeed) + "x velocidade";
                case UpgradeKind.CriticalChance: return Number(stats.CriticalChance * 100) + "% de chance";
                case UpgradeKind.CriticalDamage: return Number(stats.CriticalMultiplier) + "x dano";
                default: return Number(stats.SpecialMultiplier) + "x especial";
            }
        }

        public static bool UpgradeUnlocked(GameSaveData profile, EconomyRules rules, UpgradeKind kind)
            => EconomyProgression.WorkshopLevel(profile, rules.Experience) >= rules.Upgrade(kind).UnlockLevel;

        public static bool CanBuyUpgrade(GameSaveData profile, EconomyRules rules, UpgradeKind kind)
        {
            EconomyUpgradeRule rule = rules.Upgrade(kind);
            int level = EconomyProgression.Level(profile, kind);
            return UpgradeUnlocked(profile, rules, kind) && level < rule.MaximumLevel && profile.Gold >= rule.Cost(level);
        }

        // Indicador de comparação do HUD: vida + dez vezes o DPS básico esperado.
        // Não participa dos cálculos de dano, recompensas ou dificuldade.
        public static double PowerRating(IdleCombatSimulation sim, CharacterCombatData character)
        {
            double expectedCritical = 1 + sim.CurrentStats.CriticalChance * (sim.CurrentStats.CriticalMultiplier - 1);
            return Math.Ceiling(sim.PlayerMaximumHealth + 10 * ShotDamage(sim, character) * sim.BasicHitsPerSecond * expectedCritical);
        }
    }
}
