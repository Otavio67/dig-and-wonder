using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>
    /// Resposta imediata ao toque (guia, seção 9/10: pressão em 80–120 ms): o botão afunda levemente e volta.
    /// A área de toque e o layout não mudam; só a escala visual do próprio botão.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Button button;
        private bool pressed;

        private void Awake() => button = GetComponent<Button>();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            pressed = true;
            transform.DOKill(false);
            transform.DOScale(.93f, .08f).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        private void Release()
        {
            if (!pressed) return;
            pressed = false;
            transform.DOKill(false);
            transform.DOScale(1f, .14f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void OnDisable()
        {
            pressed = false;
            transform.DOKill(false);
            transform.localScale = Vector3.one;
        }

        /// <summary>Adiciona o retorno de toque a todos os botões sob <paramref name="scope"/> que ainda não têm um.</summary>
        public static void AttachAll(Transform scope)
        {
            if (scope == null) return;
            foreach (Button target in scope.GetComponentsInChildren<Button>(true))
            {
                if (target == null) continue;
                if (target.GetComponent<UiPressFeedback>() != null || target.GetComponent<WorkshopButtonFeedback>() != null) continue;
                target.gameObject.AddComponent<UiPressFeedback>();
            }
        }
    }
}
