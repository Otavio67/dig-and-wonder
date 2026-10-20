using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Character Experience", fileName = "CharacterExperience")]
    public sealed class CharacterExperienceDefinition : ScriptableObject
    {
        public int regularEnemyExperience;
        public int bossExperience;
        public int firstLevelExperience;
        public int experiencePerLevelStep;
        public int maximumLevel;
        public EconomyDefinition economy;
        [TextArea] public string designNote;

        public CharacterExperienceRules ToRuntime()
        {
            if (economy != null) return economy.ToRuntime().Experience;
            var rules = new CharacterExperienceRules
            {
                RegularEnemyExperience = regularEnemyExperience,
                BossExperience = bossExperience,
                FirstLevelExperience = firstLevelExperience,
                ExperiencePerLevelStep = experiencePerLevelStep,
                MaximumLevel = maximumLevel
            };
            rules.Validate();
            return rules;
        }
    }
}
