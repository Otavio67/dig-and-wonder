using System;

namespace DigAndWonder
{
    [Serializable]
    public sealed class CharacterExperienceRules
    {
        public int RegularEnemyExperience;
        public int BossExperience;
        public int FirstLevelExperience;
        public int ExperiencePerLevelStep;
        public int MaximumLevel;
        public double Growth = 1d;

        public void Validate()
        {
            if (RegularEnemyExperience < 0 || BossExperience < 0 || FirstLevelExperience < 1 ||
                ExperiencePerLevelStep < 0 || MaximumLevel < 1 || MaximumLevel > 1000 ||
                double.IsNaN(Growth) || Growth < 1d || Growth > 1.2d)
                throw new ArgumentException("A curva de XP do personagem contém valores inválidos.");
        }

        public long RequiredForNextLevel(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel > MaximumLevel) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            double value = Math.Ceiling((FirstLevelExperience + (double)ExperiencePerLevelStep * currentLevel) * Math.Pow(Growth, currentLevel));
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 1 || value > int.MaxValue)
                throw new ArgumentException("Requisito de XP fora do intervalo suportado.");
            return (long)value;
        }
        public long MaximumExperience
        {
            get
            {
                long total = 0;
                for (int level = 0; level < MaximumLevel; level++) total = checked(total + RequiredForNextLevel(level));
                return total;
            }
        }
    }

    [Serializable]
    public sealed class CharacterCombatData
    {
        public string Id;
        public string DisplayName;
        public CharacterExperienceRules Experience;
    }

    public readonly struct CharacterLevelState
    {
        public readonly int Level;
        public readonly long ExperienceInLevel;
        public readonly long ExperienceToNextLevel;
        public readonly bool IsMaximum;

        public CharacterLevelState(int level, long experience, long required, bool isMaximum)
        {
            Level = level;
            ExperienceInLevel = experience;
            ExperienceToNextLevel = required;
            IsMaximum = isMaximum;
        }
    }

    /// <summary>XP pertence ao ID estável da personagem. A interface nunca concede experiência.</summary>
    public static class CharacterProgression
    {
        public const string StarterCharacterId = "pioneer";

        public static CharacterProgressData Find(GameSaveData profile, string characterId)
        {
            if (profile == null || profile.Characters == null) return null;
            for (int i = 0; i < profile.Characters.Count; i++)
                if (profile.Characters[i] != null && profile.Characters[i].CharacterId == characterId) return profile.Characters[i];
            return null;
        }

        /// <summary>Uma personagem nova começa zerada. Duplicatas nunca reiniciam seu progresso.</summary>
        public static bool TryUnlock(GameSaveData profile, string characterId)
        {
            if (profile == null || profile.Characters == null || string.IsNullOrWhiteSpace(characterId) || Find(profile, characterId) != null)
                return false;
            profile.Characters.Add(new CharacterProgressData { CharacterId = characterId, TotalExperience = 0 });
            return true;
        }

        public static bool TryEquip(GameSaveData profile, string characterId)
        {
            if (Find(profile, characterId) == null) return false;
            if (profile.ActiveRun != null && !profile.ActiveRun.IsPlayerDefeated && !profile.ActiveRun.IsStageCleared) return false;
            profile.EquippedCharacterId = characterId;
            return true;
        }

        public static CharacterLevelState GetLevel(CharacterProgressData character, CharacterExperienceRules rules)
        {
            if (character == null || character.TotalExperience < 0) throw new ArgumentException("Progresso da personagem inválido.");
            rules.Validate();
            long remaining = character.TotalExperience;
            int level = 0;
            while (level < rules.MaximumLevel)
            {
                long required = rules.RequiredForNextLevel(level);
                if (remaining < required) return new CharacterLevelState(level, remaining, required, false);
                remaining -= required;
                level++;
            }
            return new CharacterLevelState(level, 0, 0, true);
        }

        public static int Award(GameSaveData profile, string characterId, CharacterExperienceRules rules, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            rules.Validate();
            CharacterProgressData character = Find(profile, characterId);
            if (character == null) throw new InvalidOperationException("XP direcionado a uma personagem não possuída.");
            if (character.TotalExperience < 0) throw new InvalidOperationException("XP salvo é inválido.");
            // Não reduz XP armazenado acima do limite caso a curva seja ajustada no futuro.
            long room = Math.Max(0L, rules.MaximumExperience - character.TotalExperience);
            int granted = (int)Math.Min((long)amount, room);
            character.TotalExperience += granted;
            return granted;
        }
    }
}
