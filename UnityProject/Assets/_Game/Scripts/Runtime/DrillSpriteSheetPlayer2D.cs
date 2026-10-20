using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>
    /// Player leve de frames para a folha da Pioneira.
    /// É apresentação: eventos do combate escolhem o clipe, mas nunca causam dano.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DrillSpriteSheetPlayer2D : MonoBehaviour
    {
        private const string ResourcePath = "Art/Drill/drill_pioneira";
        private static readonly string[] IdleNames = { "idle_00", "idle_01", "idle_02", "idle_03" };
        private static readonly string[] WalkNames = { "walk_00", "walk_01", "walk_02", "walk_03" };
        private static readonly string[] ActionNames = { "action_00", "action_01", "action_02", "action_03" };

        private Image image;
        private Sprite[] idleFrames;
        private Sprite[] walkFrames;
        private Sprite[] actionFrames;
        private Sprite[] impactFrames;
        private Sprite[] currentFrames;
        private float frameTimer;
        private int frameIndex;
        private bool oneShot;
        private float frameDuration = 0.14f;

        public bool IsReady { get; private set; }

        private void Awake()
        {
            image = GetComponent<Image>();
            if (image == null) return;

            Dictionary<string, Sprite> byName = new Dictionary<string, Sprite>();
            Sprite[] imported = Resources.LoadAll<Sprite>(ResourcePath);
            foreach (Sprite sprite in imported)
                if (sprite != null && !byName.ContainsKey(sprite.name)) byName.Add(sprite.name, sprite);

            idleFrames = ResolveFrames(byName, IdleNames);
            walkFrames = ResolveFrames(byName, WalkNames);
            actionFrames = ResolveFrames(byName, ActionNames);
            impactFrames = new Sprite[2];
            if (actionFrames.Length >= 4)
            {
                impactFrames[0] = actionFrames[2];
                impactFrames[1] = actionFrames[3];
            }
            IsReady = HasAllFrames(idleFrames) && HasAllFrames(walkFrames) && HasAllFrames(actionFrames);
            if (!IsReady) return;

            image.raycastTarget = false;
            image.preserveAspect = true;
            SetClip(idleFrames, 0.16f, false);
        }

        private void Start()
        {
            if (!IsReady) return;
            TextMeshProUGUI fallbackIcon = GetComponentInParent<TextMeshProUGUI>();
            if (fallbackIcon != null) fallbackIcon.text = string.Empty;
        }

        public void ResetForRun()
        {
            if (!IsReady) return;
            SetClip(idleFrames, 0.16f, false);
        }

        public void BeginWalk()
        {
            if (!IsReady) return;
            SetClip(walkFrames, 0.11f, false);
        }

        public void PlayAttack(bool heavy)
        {
            if (!IsReady) return;
            SetClip(actionFrames, heavy ? 0.09f : 0.075f, true);
        }

        public void PlayImpact()
        {
            if (!IsReady) return;
            SetClip(impactFrames, 0.09f, true);
        }

        public void PlayDeath()
        {
            if (!IsReady) return;
            currentFrames = new[] { actionFrames[3] };
            frameIndex = 0;
            frameTimer = 0f;
            oneShot = false;
            image.sprite = currentFrames[0];
        }

        private void Update()
        {
            if (!IsReady || currentFrames == null || currentFrames.Length == 0) return;
            frameTimer += Time.unscaledDeltaTime;
            if (frameTimer < frameDuration) return;

            frameTimer -= frameDuration;
            frameIndex++;
            if (frameIndex >= currentFrames.Length)
            {
                if (oneShot)
                {
                    SetClip(idleFrames, 0.16f, false);
                    return;
                }

                frameIndex = 0;
            }

            image.sprite = currentFrames[frameIndex];
        }

        private void SetClip(Sprite[] frames, float duration, bool playOnce)
        {
            currentFrames = frames;
            frameDuration = duration;
            frameTimer = 0f;
            frameIndex = 0;
            oneShot = playOnce;
            image.sprite = frames[0];
        }

        private static Sprite[] ResolveFrames(Dictionary<string, Sprite> byName, string[] names)
        {
            Sprite[] result = new Sprite[names.Length];
            for (int i = 0; i < names.Length; i++)
                byName.TryGetValue(names[i], out result[i]);
            return result;
        }

        private static bool HasAllFrames(Sprite[] frames)
        {
            if (frames == null) return false;
            for (int i = 0; i < frames.Length; i++)
                if (frames[i] == null) return false;
            return true;
        }
    }
}
