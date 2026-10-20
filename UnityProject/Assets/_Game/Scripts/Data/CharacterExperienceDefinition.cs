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
        [TextArea] public string designNote;

        public CharacterExperienceRules ToRuntime()
        {
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
