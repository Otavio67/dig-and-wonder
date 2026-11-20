using TMPro;
using UnityEngine;

namespace DigAndWonder
{
    /// <summary>
    /// Regras únicas de tipografia para HUD, menus e Oficina (guia Toy-Mecha, seção 4).
    /// Os pesos vêm de arquivos estáticos reais; nada de negrito sintético nem contorno grosso em texto pequeno.
    /// </summary>
    public static class UiTypography
    {
        /// <summary>Desliga o negrito sintético do TMP: ele dilata o SDF de fontes já pesadas e deforma as letras.</summary>
        public static void DisableSyntheticBold(TMP_FontAsset font)
        {
            if (font == null) return;
            font.boldStyle = 0f;
            font.boldSpacing = 0f;
        }

        /// <summary>
        /// Normaliza todos os textos sob <paramref name="scope"/>.
        /// Rótulos pequenos (até 20 pt) trocam a fonte de impacto pela Nunito Bold, mais legível em tamanho reduzido.
        /// </summary>
        public static void Normalize(Transform scope, TMP_FontAsset heavy, TMP_FontAsset smallLabel)
        {
            if (scope == null) return;
            foreach (TextMeshProUGUI label in scope.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (label == null) continue;
                label.fontStyle &= ~FontStyles.Bold;
                float size = label.enableAutoSizing ? label.fontSizeMax : label.fontSize;
                // Contorno serve de apoio ao contraste; em texto pequeno ele fecha os vãos das letras.
                if (label.outlineWidth > 0f)
                    label.outlineWidth = Mathf.Min(label.outlineWidth, size < 24f ? 0.12f : 0.18f);
                // Encolher demais vira texto ilegível: limite de 78% do tamanho de projeto.
                if (label.enableAutoSizing)
                    label.fontSizeMin = Mathf.Max(label.fontSizeMin, size * 0.78f);
                if (smallLabel != null && heavy != null && label.font == heavy && size <= 20f)
                    label.font = smallLabel;
            }
        }
    }
}
