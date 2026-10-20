using DG.Tweening;
using UnityEngine;

namespace DigAndWonder
{
    /// <summary>Squash/stretch cosmético com uma única animação ativa sobre a escala visual.</summary>
    [DisallowMultipleComponent]
    public sealed class ProceduralAnimator2D : MonoBehaviour
    {
        private enum MotionState { Idle, Action, Death }
        private MotionState state;
        private Tween currentTween;
        private Transform visual;

        private void Awake() => visual = transform;
        private void OnEnable() => BeginIdle();

        public void PlayAttack(bool heavy)
        {
            if (state == MotionState.Death) return;
            Interrupt();
            state = MotionState.Action;
            Vector3 punch = heavy ? new Vector3(0.2f, -0.12f, 0f) : new Vector3(0.11f, -0.07f, 0f);
            currentTween = visual.DOPunchScale(punch, heavy ? 0.34f : 0.22f, heavy ? 6 : 4, 0.65f).SetUpdate(true);
            currentTween.OnComplete(BeginIdle);
        }

        public void PlayImpact()
        {
            if (state == MotionState.Death) return;
            Interrupt();
            state = MotionState.Action;
            currentTween = visual.DOPunchScale(new Vector3(-0.09f, 0.06f, 0f), 0.2f, 4, 0.72f).SetUpdate(true);
            currentTween.OnComplete(BeginIdle);
        }

        public void PlayDeath()
        {
            Interrupt();
            state = MotionState.Death;
            currentTween = visual.DOScale(new Vector3(0.72f, 0.72f, 1f), 0.28f).SetEase(Ease.InBack).SetUpdate(true);
        }

        public void ResetForRun()
        {
            Interrupt();
            state = MotionState.Idle;
            visual.localScale = Vector3.one;
            BeginIdle();
        }

        private void BeginIdle()
        {
            if (!isActiveAndEnabled || state == MotionState.Death) return;
            Interrupt();
            state = MotionState.Idle;
            currentTween = visual.DOScale(new Vector3(1.025f, 0.985f, 1f), 0.9f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }

        private void Interrupt()
        {
            if (currentTween != null) currentTween.Kill(false);
            visual.localScale = Vector3.one;
            currentTween = null;
        }

        private void OnDisable() => Interrupt();
        private void OnDestroy() => Interrupt();
    }
}
