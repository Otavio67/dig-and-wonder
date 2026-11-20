using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>
    /// Peças do HUD aprovado v8 (Art/UI/hud-approved-v8.png, recortadas em Art/UI/Extracted).
    /// Só apresentação: nenhuma regra de combate, economia ou save passa por aqui.
    /// </summary>
    public sealed partial class PrototypeGameUI
    {
        private static readonly Color PlateNavy = new Color(.075f, .125f, .185f);
        private static readonly Color PlateSteel = new Color(.30f, .42f, .54f);
        private static readonly Color RivetSilver = new Color(.72f, .77f, .82f);

        /// <summary>Peça recortada do HUD aprovado; cai para o ícone/atlas de mesmo nome se faltar.</summary>
        private Sprite ExtractedSprite(string name)
        {
            string key = "v8:" + name;
            if (uiSprites.TryGetValue(key, out Sprite cached) && cached != null) return cached;
            Sprite sprite = Resources.Load<Sprite>("Art/UI/Extracted/" + name);
            if (sprite == null) sprite = SpriteFor(name);
            uiSprites[key] = sprite;
            return sprite;
        }

        private static void AddEdge(RectTransform rect, Color color, float distance)
        {
            Outline edge = rect.gameObject.AddComponent<Outline>();
            edge.effectColor = color;
            edge.effectDistance = new Vector2(distance, -distance);
        }

        /// <summary>Chapa azul-marinho única e rebitada atrás de missão, poderes e melhorias (como no v8).</summary>
        private void BuildHudBackplate(RectTransform parent)
        {
            RectTransform plate = MakeArtPanel("HUD backplate", parent, PlateNavy, "hud_panel", .004f, .088f, .996f, .481f);
            plate.GetComponent<Image>().raycastTarget = false;
            AddEdge(plate, PlateSteel, 3f);
            // Divisórias finas entre as três faixas, com rebites nas pontas.
            float[] seams = { .4245f, .3525f };
            foreach (float y in seams)
            {
                float local = (y - .088f) / (.481f - .088f);
                MakePanel("HUD seam", plate, new Color(.03f, .06f, .09f), new Vector2(.012f, local - .004f), new Vector2(.988f, local + .004f), Vector2.zero, Vector2.zero)
                    .GetComponent<Image>().raycastTarget = false;
                AddRivet(plate, .012f, local);
                AddRivet(plate, .988f, local);
            }
            AddRivet(plate, .012f, .985f);
            AddRivet(plate, .988f, .985f);
        }

        private void AddRivet(RectTransform parent, float x, float y)
        {
            Image rivet = MakeIconSprite(parent, SpriteFor("power_round"), x - .009f, y - .009f, x + .009f, y + .009f, RivetSilver);
            rivet.preserveAspect = true;
            rivet.raycastTarget = false;
        }
    }
}
