using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>
    /// Apresenta uma pose inicial das folhas de inimigos recebidas. A regra de combate continua no simulador;
    /// este componente só faz a ponte entre ID visual, recorte da folha e Image do HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemySpritePresenter : MonoBehaviour
    {
        private const int Columns = 5;
        private const int Rows = 3;
        private const string EnemyRoot = "Art/Enemies/";
        private static readonly Dictionary<string, string> Visuals = new Dictionary<string, string>
        {
            { "torrao", "toupeira" }, // visual provisório mais próximo; Torrão final ainda usa a folha própria com pá.
            { "slime_terra", "lama" },
            { "morcego_cascalho_ii", "morcego" },
            { "toupeira_pavio", "toupeira" },
            { "caramujo_miudo", "lesma" },
            { "cogumelo_po", "cogumelo" },
            { "aranha_argila", "aranha" },
            { "goblin_picareta", "goblin" },
            { "besouro_pedra", "aranha-pedra" },
            { "xilo_rocha", "tronco" },
            { "lagarto_basaltico", "lagarto" },
            { "aranha_ferro", "aranha-robo" },
            { "golem_cascalho", "golem" },
            { "mineiro_sonolento", "mineiro" },
            { "eco_azul", "eco" },
            { "tatu_fossil", "tatu" },
            { "abelha_ambar", "abelha" }
        };

        private Image image;
        private Material cutoutMaterial;
        private Sprite currentSprite;
        private string configuredEnemyId;
        private bool configuredBoss;

        public void Configure(string enemyId, bool boss)
        {
            image = image != null ? image : GetComponentInChildren<Image>();
            if (image == null) return;

            if (currentSprite != null && configuredEnemyId == enemyId && configuredBoss == boss)
                return;

            configuredEnemyId = enemyId;
            configuredBoss = boss;

            string visualName = boss ? "golem" : ResolveVisual(enemyId);
            Texture2D texture = Resources.Load<Texture2D>(EnemyRoot + visualName);
            if (texture == null)
            {
                image.enabled = false;
                return;
            }

            ReleaseSprite();
            int cellWidth = texture.width / Columns;
            int cellHeight = texture.height / Rows;
            Rect cell = new Rect(0, texture.height - cellHeight, cellWidth, cellHeight);
            currentSprite = Sprite.Create(texture, cell, new Vector2(0.5f, 0.12f), 100f);
            currentSprite.name = visualName + "_idle_00_runtime";
            image.sprite = currentSprite;
            image.preserveAspect = true;
            image.enabled = true;
            image.raycastTarget = false;
            image.material = CreateCutoutMaterial();

            // As unidades chegam pela direita e enfrentam a broca que avança para a direita.
            image.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        }

        private static string ResolveVisual(string enemyId)
        {
            if (!string.IsNullOrEmpty(enemyId) && Visuals.TryGetValue(enemyId, out string path)) return path;
            return "toupeira";
        }

        private Material CreateCutoutMaterial()
        {
            if (cutoutMaterial != null) return cutoutMaterial;
            Shader shader = Shader.Find("DigAndWonder/EnemyWhiteCutout");
            if (shader == null) return null;
            cutoutMaterial = new Material(shader) { name = "Enemy White Cutout (Runtime)" };
            cutoutMaterial.SetFloat("_WhiteThreshold", 0.96f);
            return cutoutMaterial;
        }

        private void ReleaseSprite()
        {
            if (currentSprite == null) return;
            Destroy(currentSprite);
            currentSprite = null;
        }

        private void OnDestroy()
        {
            ReleaseSprite();
            if (cutoutMaterial != null) Destroy(cutoutMaterial);
        }
    }
}
