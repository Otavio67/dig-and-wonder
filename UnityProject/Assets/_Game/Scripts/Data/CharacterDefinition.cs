using UnityEngine;
using System.Linq;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Character", fileName = "Character")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public bool implemented;
        public CharacterExperienceDefinition experience;

        public CharacterCombatData ToRuntime(EconomyRules economy)
        {
            CharacterCombatData source = economy.CharacterRoster.FirstOrDefault(x => x.Id == id);
            if (source == null || experience == null) throw new System.InvalidOperationException("Dados da broca ausentes: " + id);
            return source.WithExperience(experience.ToRuntime());
        }
    }
}
