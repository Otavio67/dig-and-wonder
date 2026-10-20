using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Character", fileName = "Character")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public bool implemented;
        public CharacterExperienceDefinition experience;
    }
}
