using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>
    /// Anima as folhas de inimigos recebidas conforme os eventos de combate. A regra continua no simulador;
    /// este componente liga ID visual, recortes e poses à Image do HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemySpritePresenter : MonoBehaviour
    {
        private const string EnemyRoot = "Art/Enemies/";
        private static readonly Dictionary<string, string> Visuals = new Dictionary<string, string>
        {
            { "torrao", "toupeira" }, // Substituto provisório: Torrão ainda precisa de uma folha exclusiva.
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
            { "abelha_ambar", "abelha" },
            // Folhas recebidas usadas como substitutos distintos dos guardiões por bioma.
            { "prado_cristalino_boss", "tatu" }, { "gruta_basaltica_boss", "golem" },
            { "jardim_fossil_boss", "aranha-pedra" }, { "fenda_aurora_boss", "eco" }
        };

        private Image image;
        private Material cutoutMaterial;
        private Sprite currentSprite;
        private string configuredEnemyId;
        private bool configuredBoss;
        private float configuredScale;
        private Sprite[] idle, attack, hit, death, clip;
        private readonly List<Sprite> ownedSprites = new List<Sprite>();
        private float frameTimer, deathElapsed;
        private int frame;
        private bool oneShot;
        public bool IsDying { get; private set; }
        public bool DeathFinished => IsDying && deathElapsed >= 0.5f;

        public void Configure(string enemyId, bool boss, float bossScale = 1.3f)
        {
            image = image != null ? image : GetComponentInChildren<Image>();
            if (image == null) return;

            if (currentSprite != null && configuredEnemyId == enemyId && configuredBoss == boss && configuredScale == bossScale)
                return;

            configuredEnemyId = enemyId;
            configuredBoss = boss;
            configuredScale = bossScale;

            string visualName = ResolveVisual(enemyId);
            Texture2D texture = Resources.Load<Texture2D>(EnemyRoot + visualName);
            if (texture == null)
            {
                image.enabled = false;
                return;
            }

            ReleaseSprite();
            int rows = visualName == "goblin" ? 4 : 3;
            int columns = visualName == "toupeira" ? 5 : visualName == "aranha-pedra" ? 6 : 4;
            idle = Row(texture, rows, 0, columns, 0, columns);
            attack = Row(texture, rows, 1, columns, 0, System.Math.Min(3, columns));
            hit = Row(texture, rows, rows - 1, 4, 2, 1);
            death = Row(texture, rows, rows - 1, 4, 3, 1);
            if (visualName == "lama")
            {
                idle = new[] { Cell(texture, 3, 0, 4, 0), Cell(texture, 3, 1, 4, 0), Cell(texture, 3, 2, 4, 0) };
                attack = new[] { Cell(texture, 3, 0, 4, 1), Cell(texture, 3, 1, 4, 2), Cell(texture, 3, 2, 4, 2) };
                hit = Row(texture, 3, 1, 4, 3, 1);
            }
            else if (visualName == "goblin")
            {
                attack = Row(texture, 4, 2, 4, 0, 3);
                hit = Row(texture, 4, 2, 4, 3, 1);
                death = Row(texture, 4, 3, 4, 0, 2);
            }
            else if (visualName == "lesma")
            {
                attack = Row(texture, 3, 2, 6, 0, 3);
                hit = Row(texture, 3, 2, 6, 3, 1);
                death = Row(texture, 3, 2, 6, 4, 2);
            }
            currentSprite = idle[0];
            image.preserveAspect = false;
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = new Vector2(1, 0);
            image.rectTransform.pivot = new Vector2(0.5f, 0);
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            AspectRatioFitter aspect = image.GetComponent<AspectRatioFitter>();
            if (aspect == null) aspect = image.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            aspect.aspectRatio = idle[0].rect.width / idle[0].rect.height;
            image.enabled = true;
            image.raycastTarget = false;
            image.material = CreateCutoutMaterial();

            // As unidades chegam pela direita e enfrentam a broca que avança para a direita.
            image.rectTransform.localScale = new Vector3(boss ? -bossScale : -1f, boss ? bossScale : 1f, 1f);
            ResetForSpawn();
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
            foreach (Sprite sprite in ownedSprites) if (sprite != null) Destroy(sprite);
            ownedSprites.Clear();
            idle = attack = hit = death = clip = null;
            currentSprite = null;
        }

        private Sprite Cell(Texture2D texture, int rows, int row, int columns, int column)
        {
            float width = (float)texture.width / columns;
            float height = (float)texture.height / rows;
            Sprite sprite = Sprite.Create(texture, new Rect(column * width + 1, texture.height - (row + 1) * height + 1, width - 2, height - 2), new Vector2(0.5f, 0.12f), 100f);
            ownedSprites.Add(sprite);
            return sprite;
        }

        private Sprite[] Row(Texture2D texture, int rows, int row, int columns, int start, int count)
        {
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++) sprites[i] = Cell(texture, rows, row, columns, start + i);
            return sprites;
        }

        public void ResetForSpawn() { IsDying = false; deathElapsed = 0; Play(idle, false); }
        public void PlayAttack() { if (!IsDying) Play(attack, true); }
        public void PlayImpact() { if (!IsDying) Play(hit, true); }
        public void PlayDeath() { IsDying = true; deathElapsed = 0; Play(death, false); }

        private void Play(Sprite[] frames, bool once)
        {
            if (frames == null || frames.Length == 0) return;
            clip = frames; frame = 0; frameTimer = 0; oneShot = once;
            image.sprite = clip[0];
        }

        private void Update()
        {
            if (clip == null || clip.Length == 0) return;
            if (IsDying) deathElapsed += Time.deltaTime;
            frameTimer += Time.deltaTime;
            if (frameTimer < 0.14f) return;
            frameTimer -= 0.14f;
            frame++;
            if (frame >= clip.Length)
            {
                if (oneShot) { Play(idle, false); return; }
                frame = IsDying ? clip.Length - 1 : 0;
            }
            image.sprite = clip[frame];
        }

        private void OnDestroy()
        {
            ReleaseSprite();
            if (cutoutMaterial != null) Destroy(cutoutMaterial);
        }
    }
}
