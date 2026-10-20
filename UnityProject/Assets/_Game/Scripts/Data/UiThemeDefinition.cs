using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/UI Theme", fileName = "UiTheme")]
    public sealed class UiThemeDefinition : ScriptableObject
    {
        public int baseFontSize = 16;
        public Color buttonColor;
        public Color pressedColor;
        public Color disabledColor;
        public Color focusColor;
        public Color panelColor;
        public Color outlineColor;
        public float cornerRadius = 22f;
        public float borderThickness = 3f;
    }
}
