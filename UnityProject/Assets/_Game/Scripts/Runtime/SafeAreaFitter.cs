using UnityEngine;

namespace DigAndWonder
{
    /// <summary>Encaixa o conteúdo na área segura para notch, gestos e barras do Android.</summary>
    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private bool isApplying;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            Apply();
        }

        private void OnRectTransformDimensionsChange() => Apply();

        private void Apply()
        {
            if (isApplying || rectTransform == null || Screen.width <= 0 || Screen.height <= 0) return;
            isApplying = true;
            try
            {
                Rect safe = Screen.safeArea;
                rectTransform.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
                rectTransform.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            finally
            {
                isApplying = false;
            }
        }
    }
}
