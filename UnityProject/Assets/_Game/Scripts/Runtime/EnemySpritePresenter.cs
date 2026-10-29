using System.Collections.Generic;
using Newtonsoft.Json;
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
        private EnemySheetPresentation presentation;
        private RectTransform body, shadow;
        private bool floating;
        private float phase, motionTime, actionTime, impactTime;
        private float visualScale = 1;
        public float VisualHeight => presentation == null ? 0 :
            ((RectTransform)transform).rect.width * presentation.ReferenceHeight / presentation.ReferenceWidth * visualScale + (floating ? 16 : 0);
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
            TextAsset data = Resources.Load<TextAsset>(EnemyRoot + "Presentation/" + visualName);
            presentation = data == null ? null : JsonConvert.DeserializeObject<EnemySheetPresentation>(data.text);
            if (presentation == null) throw new System.InvalidOperationException("Prepare a apresentação dos inimigos: " + visualName);
            floating = visualName == "morcego" || visualName == "abelha" || visualName == "eco";
            visualScale = boss ? bossScale : 1f;
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
                hit = Row(texture, 3, 0, 4, 3, 1);
                death = Row(texture, 3, 2, 4, 3, 1);
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
            else if (visualName == "toupeira")
            {
                hit = Row(texture, 3, 1, 5, 3, 1);
                death = Row(texture, 3, 2, 4, 2, 2);
            }
            else
            {
                // Actual attack poses are on the last row. The middle row is
                // anticipation; it must not loop printed warning rings as idle.
                attack = Row(texture, 3, 2, 4, 0, 2);
                if (visualName == "aranha-pedra") attack = Row(texture, 3, 1, 6, 0, 3);
                if (visualName == "morcego" || visualName == "lagarto" || visualName == "golem")
                    death = Row(texture, 3, 2, 4, 2, 2);
            }
            currentSprite = idle[0];
            image.preserveAspect = false;
            body = image.rectTransform;
            image.rectTransform.anchorMin = new Vector2(.5f, 0);
            image.rectTransform.anchorMax = new Vector2(.5f, 0);
            image.rectTransform.pivot = new Vector2(0.5f, 0);
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            image.enabled = true;
            image.raycastTarget = false;
            image.material = CreateCutoutMaterial();
            cutoutMaterial.SetTexture("_CoverageTex", Resources.Load<Texture2D>(EnemyRoot + "Presentation/" + visualName));

            // As unidades chegam pela direita e enfrentam a broca que avança para a direita.
            image.rectTransform.localScale = new Vector3(boss ? -bossScale : -1f, boss ? bossScale : 1f, 1f);
            EnsureShadow();
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
            return cutoutMaterial;
        }

        private void EnsureShadow()
        {
            if (shadow != null) return;
            var go = new GameObject("Ground contact", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();
            shadow = (RectTransform)go.transform;
            shadow.anchorMin = shadow.anchorMax = new Vector2(.5f, 0);
            var graphic = go.GetComponent<Image>();
            graphic.color = new Color(.08f, .055f, .04f, .35f);
            graphic.raycastTarget = false;
            graphic.material = new Material(Shader.Find("DigAndWonder/GroundContact"));
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
            EnemyFrameBounds bounds = System.Array.Find(presentation.Frames, f => f.Row == row && f.Column == column);
            if (bounds == null) throw new System.InvalidOperationException("Pose ausente na folha: " + row + "/" + column);
            Sprite sprite = Sprite.Create(texture, new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height), new Vector2(0.5f, 0), 100f);
            ownedSprites.Add(sprite);
            return sprite;
        }

        private Sprite[] Row(Texture2D texture, int rows, int row, int columns, int start, int count)
        {
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++) sprites[i] = Cell(texture, rows, row, columns, start + i);
            return sprites;
        }

        public void ResetForSpawn()
        {
            IsDying = false; deathElapsed = actionTime = impactTime = 0;
            phase = ((RectTransform)transform).anchorMin.x * 17;
            Play(idle, false);
            // Offset creatures so a wave does not breathe on the same frame.
            frame = (int)(phase * 3) % idle.Length;
            image.sprite = idle[frame];
            ApplyMotion();
        }
        public void PlayAttack() { if (!IsDying) { actionTime = .38f; Play(attack, true); } }
        public void PlayImpact() { if (!IsDying) { impactTime = .18f; Play(hit, true); } }
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
            motionTime += Time.deltaTime;
            actionTime = Mathf.Max(0, actionTime - Time.deltaTime);
            impactTime = Mathf.Max(0, impactTime - Time.deltaTime);
            if (IsDying) deathElapsed += Time.deltaTime;
            ApplyMotion();
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

        private void ApplyMotion()
        {
            if (body == null || image.sprite == null || presentation == null) return;
            float unit = ((RectTransform)transform).rect.width / presentation.ReferenceWidth;
            body.sizeDelta = new Vector2(image.sprite.rect.width * unit, image.sprite.rect.height * unit);
            float breath = Mathf.Sin(motionTime * (floating ? 5 : 3.3f) + phase);
            float lift = floating ? 13 + breath * 4 : 0;
            float strike = actionTime > 0 ? Mathf.Sin((.38f - actionTime) / .38f * 3.14159f) * -12 : 0;
            float recoil = impactTime / .18f * 5;
            body.anchoredPosition = new Vector2(strike + recoil, lift - (IsDying ? deathElapsed * 10 : 0));
            float squash = IsDying ? 1 - Mathf.Clamp01(deathElapsed / .5f) * .22f : 1 + breath * .015f;
            body.localScale = new Vector3(-visualScale / squash, visualScale * squash, 1);
            image.color = impactTime > .1f ? new Color(1, .76f, .62f) :
                new Color(.96f, .96f, .94f, IsDying ? 1 - Mathf.Clamp01(deathElapsed / .5f) : 1);
            if (shadow != null)
            {
                float spread = floating ? .62f - breath * .025f : .78f;
                shadow.sizeDelta = new Vector2(((RectTransform)transform).rect.width * visualScale * spread, 11 * visualScale);
                shadow.anchoredPosition = new Vector2(0, 1);
                shadow.GetComponent<Image>().color = new Color(.08f, .055f, .04f, (floating ? .23f : .35f) * (IsDying ? 1 - Mathf.Clamp01(deathElapsed / .5f) : 1));
            }
        }

        private void OnDestroy()
        {
            ReleaseSprite();
            if (cutoutMaterial != null) Destroy(cutoutMaterial);
            if (shadow != null) Destroy(shadow.GetComponent<Image>().material);
        }
    }
}
