using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>Move somente a face do botão; a área de toque conserva tamanho e posição.</summary>
    public sealed class WorkshopButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private RectTransform face;
        private Button button;
        private Tween movement;

        public void Initialize(RectTransform visualFace, Button owner)
        {
            face = visualFace;
            button = owner;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && button.interactable) MoveFace(-5f);
        }

        public void OnPointerUp(PointerEventData eventData) => MoveFace(0f);
        public void OnPointerExit(PointerEventData eventData) => MoveFace(0f);

        private void MoveFace(float y)
        {
            if (face == null) return;
            movement?.Kill(false);
            movement = face.DOAnchorPosY(y, 0.09f).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private void OnDisable()
        {
            movement?.Kill(false);
            movement = null;
            if (face != null) face.anchoredPosition = Vector2.zero;
        }

        private void OnDestroy() => movement?.Kill(false);
    }
}
