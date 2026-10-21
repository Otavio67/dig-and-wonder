using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>Reproduz a pequena folha de disparo da broca sem acoplar animação à simulação.</summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileFramePlayer2D : MonoBehaviour
    {
        private Image image;
        private Sprite[] frames;
        private int frameIndex;
        private float frameTimer;
        private bool playing;

        public void Configure(Image target, Sprite[] configuredFrames)
        {
            image = target;
            frames = configuredFrames;
            frameIndex = 0;
            frameTimer = 0f;
            playing = frames != null && frames.Length > 0;
            if (image != null && playing) image.sprite = frames[0];
        }

        public void Play()
        {
            frameIndex = 0;
            frameTimer = 0f;
            playing = frames != null && frames.Length > 0;
            if (image != null && playing) image.sprite = frames[0];
        }

        public void Stop()
        {
            playing = false;
            frameIndex = 0;
            frameTimer = 0f;
            if (image != null && frames != null && frames.Length > 0) image.sprite = frames[0];
        }

        private void Update()
        {
            if (!playing || image == null || frames == null || frames.Length < 2) return;
            frameTimer += Time.unscaledDeltaTime;
            if (frameTimer < 0.075f) return;
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % frames.Length;
            image.sprite = frames[frameIndex];
        }
    }
}
