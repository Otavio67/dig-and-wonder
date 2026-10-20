using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>Entry point autônomo para cenas vazias; a cena de produto virá depois da revisão visual.</summary>
    public static class DigAndWonderBootstrap
    {
        private static GameObject root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (root != null) return;
            root = new GameObject("Dig & Wonder — Protótipo");
            UnityEngine.Object.DontDestroyOnLoad(root);

            Screen.orientation = ScreenOrientation.Portrait;
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.transform.SetParent(root.transform, false);
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.backgroundColor = new Color(0.035f, 0.09f, 0.12f);
            }

            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem");
                events.transform.SetParent(root.transform, false);
                events.AddComponent<EventSystem>();
                events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var safeArea = new GameObject("Safe Area", typeof(RectTransform));
            safeArea.transform.SetParent(canvas.transform, false);
            safeArea.AddComponent<SafeAreaFitter>();

            var driverObject = new GameObject("Combat Driver");
            driverObject.transform.SetParent(root.transform, false);
            var driver = driverObject.AddComponent<AutoBattleDriver>();
            canvasObject.AddComponent<PrototypeGameUI>().Initialize(safeArea.transform, driver);
        }
    }
}
