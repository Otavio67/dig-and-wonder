using DG.Tweening;
using UnityEngine;

namespace DigAndWonder
{
    /// <summary>
    /// Movimento cosmético da broca sobre o chão.
    /// A posição lógica, o colisor e o combate continuam pertencendo à raiz da unidade.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GroundDrillMotion2D : MonoBehaviour
    {
        [SerializeField] private float stridePixels = 7f;
        [SerializeField] private float groundBobPixels = 2f;
        [SerializeField] private float walkCycleSeconds = 0.72f;

        private RectTransform visualRect;
        private Transform visualTransform;
        private Vector2 baseAnchoredPosition;
        private Vector3 baseLocalPosition;
        private Tween walkX;
        private Tween walkY;
        private Tween actionTween;
        private bool defeated;

        private void Awake()
        {
            visualTransform = transform;
            visualRect = transform as RectTransform;
            if (visualRect != null)
                baseAnchoredPosition = visualRect.anchoredPosition;
            else
                baseLocalPosition = visualTransform.localPosition;
        }

        /// <summary>Inicia o avanço curto e contínuo usado enquanto a caverna rola.</summary>
        public void BeginWalk()
        {
            if (!isActiveAndEnabled || defeated) return;
            StopWalking();
            actionTween?.Kill(false);
            actionTween = null;
            RestoreBasePosition();

            float cycle = walkCycleSeconds > 0.1f ? walkCycleSeconds : 0.1f;
            walkX = MoveX(BaseX - stridePixels, cycle)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
            walkY = MoveY(BaseY + groundBobPixels, cycle * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        /// <summary>Pausa apenas a locomoção; deixa uma reação de ataque/derrota terminar.</summary>
        public void StopWalking()
        {
            walkX?.Kill(false);
            walkY?.Kill(false);
            walkX = null;
            walkY = null;
        }

        /// <summary>Prepara, lança e recupera a ponta sem deslocar a raiz funcional.</summary>
        public void PlayAttack(bool heavy)
        {
            if (defeated) return;
            StopWalking();
            actionTween?.Kill(false);
            RestoreBasePosition();

            float anticipation = heavy ? 7f : 4f;
            float lunge = heavy ? 18f : 12f;
            float recovery = heavy ? 0.22f : 0.16f;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(MoveX(BaseX - anticipation, 0.08f).SetEase(Ease.InBack));
            sequence.Append(MoveX(BaseX + lunge, heavy ? 0.14f : 0.11f).SetEase(Ease.OutCubic));
            sequence.Append(MoveX(BaseX, recovery).SetEase(Ease.OutBack));
            actionTween = sequence.SetUpdate(true);
            actionTween.OnComplete(() =>
            {
                actionTween = null;
                BeginWalk();
            });
        }

        /// <summary>Reação curta ao dano real, interrompendo a caminhada visual anterior.</summary>
        public void PlayImpact()
        {
            if (defeated) return;
            StopWalking();
            actionTween?.Kill(false);
            RestoreBasePosition();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(MoveX(BaseX - 6f, 0.07f).SetEase(Ease.OutCubic));
            sequence.Append(MoveX(BaseX, 0.16f).SetEase(Ease.OutBack));
            actionTween = sequence.SetUpdate(true);
            actionTween.OnComplete(() =>
            {
                actionTween = null;
                BeginWalk();
            });
        }

        /// <summary>Enterra a apresentação da broca e cancela a locomoção pendente.</summary>
        public void PlayDeath()
        {
            StopWalking();
            actionTween?.Kill(false);
            defeated = true;
            RestoreBasePosition();

            actionTween = DOTween.Sequence()
                .Append(MoveY(BaseY - 8f, 0.28f).SetEase(Ease.InBack))
                .SetUpdate(true);
        }

        /// <summary>Limpa tweens da tentativa anterior e devolve a broca ao chão.</summary>
        public void ResetForRun()
        {
            defeated = false;
            CancelAllTweens();
            RestoreBasePosition();
            BeginWalk();
        }

        private float BaseX => visualRect != null ? baseAnchoredPosition.x : baseLocalPosition.x;
        private float BaseY => visualRect != null ? baseAnchoredPosition.y : baseLocalPosition.y;

        private Tween MoveX(float x, float duration)
        {
            return visualRect != null
                ? visualRect.DOAnchorPosX(x, duration)
                : visualTransform.DOLocalMoveX(x, duration);
        }

        private Tween MoveY(float y, float duration)
        {
            return visualRect != null
                ? visualRect.DOAnchorPosY(y, duration)
                : visualTransform.DOLocalMoveY(y, duration);
        }

        private void RestoreBasePosition()
        {
            if (visualRect != null)
            {
                visualRect.anchoredPosition = baseAnchoredPosition;
                return;
            }

            visualTransform.localPosition = baseLocalPosition;
        }

        private void CancelAllTweens()
        {
            StopWalking();
            actionTween?.Kill(false);
            actionTween = null;
        }

        private void OnDisable() => CancelAllTweens();
        private void OnDestroy() => CancelAllTweens();
    }
}
