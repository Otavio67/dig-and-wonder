using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>Feedback visual separado da simulação; todos os objetos são aquecidos antes do primeiro combate.</summary>
    public sealed class PooledCombatFeedback : MonoBehaviour
    {
        private readonly List<GameObject> allObjects = new List<GameObject>();
        private readonly Dictionary<GameObject, Tween> activeTweens = new Dictionary<GameObject, Tween>();
        private readonly List<GameObject> activeProjectiles = new List<GameObject>();
        private readonly List<GameObject> activeDamage = new List<GameObject>();
        private readonly List<GameObject> activeCoins = new List<GameObject>();
        private readonly List<GameObject> activeParticles = new List<GameObject>();
        private ObjectPool<GameObject> projectiles;
        private ObjectPool<GameObject> damageNumbers;
        private ObjectPool<GameObject> coins;
        private ObjectPool<GameObject> particles;
        private RectTransform cave;
        private Sprite[] projectileFrames;
        private Material projectileMaterial;
        private System.Func<Vector2> playerPoint;
        private System.Func<int, Vector2> enemyPoint;
        private readonly Dictionary<GameObject, int> projectileTargets = new Dictionary<GameObject, int>();
        private Sprite goldSprite;
        private TMP_FontAsset feedbackFont;

        public void Initialize(RectTransform caveView, System.Func<Vector2> resolvePlayer = null, System.Func<int, Vector2> resolveEnemy = null,
            Sprite coinSprite = null, TMP_FontAsset font = null)
        {
            cave = caveView;
            playerPoint = resolvePlayer ?? (() => Vector2.zero);
            enemyPoint = resolveEnemy ?? (id => Vector2.zero);
            goldSprite = coinSprite;
            feedbackFont = font;
            projectileFrames = BuildProjectileFrames();
            projectiles = CreatePool("projectile", 24);
            damageNumbers = CreatePool("damage", 32);
            coins = CreatePool("coin", 8);
            particles = CreatePool("particle", 32);
        }

        private ObjectPool<GameObject> CreatePool(string kind, int capacity)
        {
            var pool = new ObjectPool<GameObject>(() => CreateElement(kind),
                OnGet, OnRelease, null, false, capacity, capacity);
            var warm = new GameObject[capacity];
            for (int i = 0; i < capacity; i++) warm[i] = pool.Get();
            foreach (GameObject element in warm) pool.Release(element);
            return pool;
        }

        private GameObject CreateElement(string kind)
        {
            var gameObject = new GameObject("Pooled " + kind, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            gameObject.transform.SetParent(cave, false);
            var rect = (RectTransform)gameObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = kind == "damage" ? new Vector2(150, 64) : kind == "projectile" ? new Vector2(76, 52) : new Vector2(44, 44);
            var image = gameObject.GetComponent<Image>();
            image.raycastTarget = false;
            if (kind == "projectile")
            {
                image.preserveAspect = true;
                image.material = projectileMaterial;
                var framePlayer = gameObject.AddComponent<ProjectileFramePlayer2D>();
                framePlayer.Configure(image, projectileFrames);
            }
            else if (kind == "coin") image.sprite = goldSprite;
            else if (kind == "particle") image.sprite = Resources.Load<Sprite>("Art/Workshop/core");
            gameObject.GetComponent<CanvasGroup>().interactable = false;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
            if (kind == "damage")
            {
                var labelObject = new GameObject("Amount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(gameObject.transform, false);
                var labelRect = (RectTransform)labelObject.transform;
                labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
                var label = labelObject.GetComponent<TextMeshProUGUI>();
                if (feedbackFont != null) label.font = feedbackFont;
                label.outlineWidth = 0.15f;
                label.outlineColor = new Color(0.03f, 0.06f, 0.10f);
                label.fontSize = 26; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            }
            gameObject.SetActive(false);
            allObjects.Add(gameObject);
            return gameObject;
        }

        private void OnGet(GameObject item)
        {
            item.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.GetComponent<CanvasGroup>().alpha = 1f;
            DOTween.Kill(item.GetComponent<RectTransform>());
            DOTween.Kill(item.GetComponent<CanvasGroup>());
            item.GetComponent<ProjectileFramePlayer2D>()?.Play();
        }

        private void OnRelease(GameObject item)
        {
            projectileTargets.Remove(item);
            if (activeTweens.TryGetValue(item, out Tween tween))
            {
                activeTweens.Remove(item);
                tween.Kill(false);
            }
            DOTween.Kill(item.GetComponent<RectTransform>());
            DOTween.Kill(item.GetComponent<CanvasGroup>());
            item.GetComponent<ProjectileFramePlayer2D>()?.Stop();
            item.SetActive(false);
        }

        private Sprite[] BuildProjectileFrames()
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/Projectiles/drill_projectile");
            if (texture == null)
            {
                Sprite fallback = Resources.Load<Sprite>("Art/Workshop/drill");
                projectileMaterial = null;
                return fallback == null ? System.Array.Empty<Sprite>() : new[] { fallback };
            }

            const int columns = 4;
            int cellWidth = texture.width / columns;
            int cellHeight = texture.height / 3;
            var frames = new Sprite[columns];
            for (int i = 0; i < columns; i++)
            {
                frames[i] = Sprite.Create(texture,
                    new Rect(i * cellWidth, texture.height - cellHeight, cellWidth, cellHeight),
                    new Vector2(0.5f, 0.5f), 100f);
                frames[i].name = "drill_projectile_" + i;
            }

            Shader shader = Shader.Find("DigAndWonder/EnemyWhiteCutout");
            if (shader != null)
            {
                projectileMaterial = new Material(shader) { name = "Drill Projectile Cutout (Runtime)" };
                projectileMaterial.SetFloat("_WhiteThreshold", 0.94f);
            }
            return frames;
        }

        public void Handle(CombatEvent combatEvent)
        {
            if (combatEvent.Type == CombatEventType.PlayerDefeated ||
                combatEvent.Type == CombatEventType.StageCleared ||
                combatEvent.Type == CombatEventType.RunStarted)
                CancelAll();

            switch (combatEvent.Type)
            {
                case CombatEventType.ProjectileLaunched:
                    AnimateProjectile(combatEvent);
                    break;
                case CombatEventType.Hit:
                    ShowDamage(combatEvent.Amount, combatEvent.EntityId, combatEvent.Text == "CRÍTICO!");
                    break;
                case CombatEventType.GoldAwarded:
                    ShowCoin(combatEvent.Amount, combatEvent.EntityId);
                    break;
                case CombatEventType.ProjectileImpact:
                    ShowImpact(enemyPoint(combatEvent.EntityId));
                    break;
                case CombatEventType.PlayerDamaged:
                    ShowImpact(playerPoint());
                    ShowDamage(combatEvent.Amount, 0, false);
                    break;
                case CombatEventType.EnemyDefeated:
                    ShowImpact(enemyPoint(combatEvent.EntityId));
                    CancelTarget(combatEvent.EntityId);
                    break;
                case CombatEventType.ProjectileDissipated:
                    CancelTarget(combatEvent.EntityId);
                    break;
            }
        }

        private void AnimateProjectile(CombatEvent combatEvent)
        {
            GameObject item = projectiles.Get();
            activeProjectiles.Add(item);
            bool enemyShot = combatEvent.Text.Contains("inimigo");
            var rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = enemyShot ? enemyPoint(combatEvent.EntityId) : playerPoint();
            item.GetComponent<Image>().color = enemyShot ? new Color(1f, 0.55f, 0.45f) : Color.white;
            rect.localScale = new Vector3(enemyShot ? -1 : 1, 1, 1);
            rect.localRotation = Quaternion.identity;
            Vector2 target = enemyShot ? playerPoint() : enemyPoint(combatEvent.EntityId);
            if (!enemyShot) projectileTargets[item] = combatEvent.EntityId;
            float duration = Mathf.Max(0.01f, combatEvent.Amount);
            var tween = rect.DOAnchorPos(target, duration).SetEase(Ease.Linear);
            activeTweens[item] = tween;
            tween.OnComplete(() => { activeTweens.Remove(item); activeProjectiles.Remove(item); projectiles.Release(item); });
        }

        private void ShowDamage(float amount, int entityId, bool critical)
        {
            GameObject item = damageNumbers.Get();
            activeDamage.Add(item);
            item.GetComponent<Image>().color = Color.clear;
            item.GetComponentInChildren<TextMeshProUGUI>().text = "-" + Mathf.CeilToInt(amount) + (critical ? "!" : "");
            item.GetComponentInChildren<TextMeshProUGUI>().color = critical ? new Color(1f, 0.4f, 0.37f) : new Color(1f, 0.88f, 0.59f);
            Vector2 point = entityId == 0 ? playerPoint() : enemyPoint(entityId);
            point.y += 45;
            AnimatePopup(item, point, 0.72f, damageNumbers, activeDamage);
        }

        private void ShowCoin(float amount, int entityId)
        {
            GameObject item = coins.Get();
            activeCoins.Add(item);
            item.GetComponent<Image>().color = new Color(1f, 0.77f, 0.24f);
            AnimatePopup(item, enemyPoint(entityId), 0.58f, coins, activeCoins);
        }

        private void ShowImpact(Vector2 point)
        {
            GameObject item = particles.Get();
            activeParticles.Add(item);
            item.GetComponent<Image>().color = new Color(0.62f, 0.94f, 0.8f, 0.94f);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = point;
            var sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(1.9f, 0.24f).SetEase(Ease.OutBack));
            sequence.Join(item.GetComponent<CanvasGroup>().DOFade(0f, 0.24f));
            activeTweens[item] = sequence;
            sequence.OnComplete(() => { activeTweens.Remove(item); activeParticles.Remove(item); particles.Release(item); });
        }

        private void AnimatePopup(GameObject item, Vector2 position, float duration, ObjectPool<GameObject> pool, List<GameObject> active)
        {
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            var sequence = DOTween.Sequence();
            sequence.Append(rect.DOAnchorPosY(position.y + 76, duration).SetEase(Ease.OutCubic));
            sequence.Join(rect.DOScale(1.12f, duration * 0.32f).SetEase(Ease.OutBack));
            sequence.Join(item.GetComponent<CanvasGroup>().DOFade(0f, duration * 0.48f).SetDelay(duration * 0.52f));
            activeTweens[item] = sequence;
            sequence.OnComplete(() =>
            {
                activeTweens.Remove(item);
                active.Remove(item);
                pool.Release(item);
            });
        }

        private void CancelAll()
        {
            Cancel(activeProjectiles, projectiles);
            Cancel(activeDamage, damageNumbers);
            Cancel(activeCoins, coins);
            Cancel(activeParticles, particles);
        }

        private void CancelTarget(int id)
        {
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                GameObject item = activeProjectiles[i];
                if (!projectileTargets.TryGetValue(item, out int target) || target != id) continue;
                activeProjectiles.RemoveAt(i);
                projectiles.Release(item);
            }
        }

        public void Clear() => CancelAll();

        private void OnDisable() => CancelAll();

        private void Cancel(List<GameObject> active, ObjectPool<GameObject> pool)
        {
            if (pool == null) return;
            foreach (GameObject item in active.ToArray())
            {
                if (activeTweens.TryGetValue(item, out Tween tween))
                {
                    activeTweens.Remove(item);
                    tween.Kill(false);
                }
                active.Remove(item);
                pool.Release(item);
            }
        }

        private void OnDestroy()
        {
            foreach (GameObject item in allObjects)
            {
                if (item == null) continue;
                if (activeTweens.TryGetValue(item, out Tween tween)) tween.Kill(false);
                DOTween.Kill(item.GetComponent<RectTransform>());
                DOTween.Kill(item.GetComponent<CanvasGroup>());
            }
            if (projectileMaterial != null) Destroy(projectileMaterial);
            if (projectileFrames != null)
                foreach (Sprite frame in projectileFrames)
                    if (frame != null) Destroy(frame);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) CancelAll();
        }
    }
}
