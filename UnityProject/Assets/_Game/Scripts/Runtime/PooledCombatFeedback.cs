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

        public void Initialize(RectTransform caveView)
        {
            cave = caveView;
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
            rect.sizeDelta = kind == "damage" ? new Vector2(150, 64) : new Vector2(44, 44);
            var image = gameObject.GetComponent<Image>();
            image.raycastTarget = false;
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
        }

        private void OnRelease(GameObject item)
        {
            if (activeTweens.TryGetValue(item, out Tween tween))
            {
                activeTweens.Remove(item);
                tween.Kill(false);
            }
            DOTween.Kill(item.GetComponent<RectTransform>());
            DOTween.Kill(item.GetComponent<CanvasGroup>());
            item.SetActive(false);
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
                    ShowDamage(combatEvent.Amount, combatEvent.EntityId);
                    break;
                case CombatEventType.GoldAwarded:
                    ShowCoin(combatEvent.Amount);
                    break;
                case CombatEventType.ProjectileImpact:
                case CombatEventType.PlayerDamaged:
                case CombatEventType.EnemyDefeated:
                    ShowImpact(combatEvent.EntityId);
                    break;
            }
        }

        private void AnimateProjectile(CombatEvent combatEvent)
        {
            GameObject item = projectiles.Get();
            activeProjectiles.Add(item);
            bool enemyShot = combatEvent.Text.Contains("inimigo");
            var rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = enemyShot ? new Vector2(280, -20) : new Vector2(-300, -80);
            item.GetComponent<Image>().color = enemyShot ? new Color(1f, 0.39f, 0.29f) : new Color(0.47f, 0.91f, 0.78f);
            float targetX = enemyShot ? -300 : 300;
            float duration = Mathf.Clamp(combatEvent.Amount, 0.12f, 1.25f);
            var tween = rect.DOAnchorPos(new Vector2(targetX, -70), duration).SetEase(Ease.OutCubic).SetUpdate(true);
            activeTweens[item] = tween;
            tween.OnComplete(() => { activeTweens.Remove(item); activeProjectiles.Remove(item); projectiles.Release(item); });
        }

        private void ShowDamage(float amount, int entityId)
        {
            GameObject item = damageNumbers.Get();
            activeDamage.Add(item);
            item.GetComponent<Image>().color = Color.clear;
            item.GetComponentInChildren<TextMeshProUGUI>().text = "-" + Mathf.CeilToInt(amount);
            item.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1f, 0.88f, 0.59f);
            AnimatePopup(item, new Vector2(180 + (entityId % 3) * 55, -10), 0.72f, damageNumbers, activeDamage);
        }

        private void ShowCoin(float amount)
        {
            GameObject item = coins.Get();
            activeCoins.Add(item);
            item.GetComponent<Image>().color = new Color(1f, 0.77f, 0.24f);
            AnimatePopup(item, new Vector2(-240, 170), 0.58f, coins, activeCoins);
        }

        private void ShowImpact(int entityId)
        {
            GameObject item = particles.Get();
            activeParticles.Add(item);
            item.GetComponent<Image>().color = new Color(0.62f, 0.94f, 0.8f, 0.94f);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(200 + (entityId % 4) * 46, -35);
            var sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
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
            sequence.SetUpdate(true);
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
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) CancelAll();
        }
    }
}
