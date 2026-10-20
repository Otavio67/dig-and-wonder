using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object { public static void DontDestroyOnLoad(Object target) { } public static T FindFirstObjectByType<T>() where T : Object => null; }
    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => (T)Activator.CreateInstance(typeof(T));
    }
    public class MonoBehaviour : Component
    {
        public bool enabled = true;
        public bool isActiveAndEnabled => enabled;
    }
    [AttributeUsage(AttributeTargets.Class)] public sealed class DisallowMultipleComponentAttribute : Attribute { }
    public class Component : Object { public GameObject gameObject; public Transform transform; public T GetComponent<T>() where T : class => null; }
    public class GameObject : Object
    {
        public GameObject(string name, params Type[] components) { }
        public Transform transform;
        public string tag;
        public T AddComponent<T>() where T : Component, new() => new T();
        public T GetComponent<T>() where T : class => null;
        public T GetComponentInChildren<T>() where T : class => null;
        public void SetActive(bool active) { }
    }
    public class Transform : Component { public Vector3 localPosition, localScale; public void SetParent(Transform parent, bool worldPositionStays) { } }
    public class RectTransform : Transform { public Vector2 anchorMin, anchorMax, offsetMin, offsetMax, anchoredPosition, sizeDelta; }
    public class Camera : Component { public static Camera main; public bool orthographic; public Color backgroundColor; }
    public class CanvasRenderer : Component { }
    public enum ScreenOrientation { Portrait }
    public enum UIOrientation { Portrait }
    public struct Rect { public float xMin, yMin, xMax, yMax; }
    public static class Screen { public static ScreenOrientation orientation; public static int width = 1080, height = 1920; public static Rect safeArea; }
    [AttributeUsage(AttributeTargets.Method)] public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { } }
    public enum RuntimeInitializeLoadType { AfterSceneLoad }
    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Class)] public sealed class CreateAssetMenuAttribute : Attribute
    {
        public CreateAssetMenuAttribute() { }
        public string menuName { get; set; }
        public string fileName { get; set; }
    }
    [AttributeUsage(AttributeTargets.Field)] public sealed class TextAreaAttribute : Attribute { }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color magenta => new Color(1f, 0f, 1f, 1f);
        public static Color clear => new Color(0f, 0f, 0f, 0f);
    }
    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; } public static Vector2 zero => new Vector2(); public static Vector2 one => new Vector2(1, 1); }
    public struct Vector3 { public float x, y, z; public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; } public static Vector3 one => new Vector3(1, 1, 1); }
    public static class ColorUtility
    {
        public static bool TryParseHtmlString(string html, out Color color) { color = default; return !string.IsNullOrEmpty(html); }
    }
    public static class Resources
    {
        public static T Load<T>(string path) where T : Object => null;
        public static T[] FindObjectsOfTypeAll<T>() where T : Object => Array.Empty<T>();
    }
    public static class Application
    {
        public static string persistentDataPath => "/tmp";
        public static string dataPath => "/tmp/UnityProject/Assets";
    }
    public static class Time { public static float deltaTime; public static float unscaledDeltaTime; }
    public static class Mathf { public static int CeilToInt(float value) => (int)Math.Ceiling(value); public static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value)); public static int Abs(int value) => Math.Abs(value); }
    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message, Object context = null) { }
        public static void LogError(object message, Object context = null) { }
    }
}

namespace UnityEngine.SceneManagement { public struct Scene { } }

namespace UnityEngine.EventSystems
{
    public class EventSystem : UnityEngine.Component { }
}

namespace UnityEngine.InputSystem.UI
{
    public class InputSystemUIInputModule : UnityEngine.Component { public void AssignDefaultActions() { } }
}

namespace UnityEngine.UI
{
    using UnityEngine.Events;
    public enum RenderMode { ScreenSpaceOverlay }
    public class Canvas : UnityEngine.Component { public RenderMode renderMode; }
    public class CanvasScaler : UnityEngine.Component
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize }
        public ScaleMode uiScaleMode; public UnityEngine.Vector2 referenceResolution; public float matchWidthOrHeight;
    }
    public class GraphicRaycaster : UnityEngine.Component { }
    public class Graphic : UnityEngine.Component { public UnityEngine.Color color; public bool raycastTarget; public UnityEngine.RectTransform rectTransform; }
    public class Image : Graphic { }
    public class CanvasGroup : UnityEngine.Component { public float alpha; public bool interactable, blocksRaycasts; }
    public class Button : UnityEngine.Component { public ButtonClickedEvent onClick = new ButtonClickedEvent(); public sealed class ButtonClickedEvent { public void AddListener(UnityAction action) { } } }
}

namespace UnityEngine.Events { public delegate void UnityAction(); }

namespace TMPro
{
    using UnityEngine.UI;
    public enum TextAlignmentOptions { Left, Right, Center }
    public enum TextWrappingModes { Normal }
    public enum FontStyles { Normal, Bold }
    public class TextMeshProUGUI : Graphic
    {
        public string text; public int fontSize; public TextAlignmentOptions alignment; public TextWrappingModes textWrappingMode; public FontStyles fontStyle;
    }
}

namespace DG.Tweening
{
    public enum Ease { InOutSine, Linear, OutCubic, OutBack, InBack }
    public enum LoopType { Yoyo, Restart }
    public class Tween { public Tween SetEase(Ease ease) => this; public Tween SetLoops(int loops, LoopType type = LoopType.Yoyo) => this; public Tween SetDelay(float delay) => this; public Tween SetUpdate(bool independent) => this; public Tween OnComplete(Action action) => this; public void Kill(bool complete = false) { } }
    public class Sequence : Tween { public Sequence Append(Tween tween) => this; public Sequence Join(Tween tween) => this; }
    public static class DOTween { public static void Kill(UnityEngine.Object target, bool complete = false) { } public static Sequence Sequence() => new Sequence(); }
    public static class TweenExtensions
    {
        public static Tween DOAnchorPosX(this UnityEngine.RectTransform target, float x, float duration) => new Tween();
        public static Tween DOAnchorPos(this UnityEngine.RectTransform target, UnityEngine.Vector2 pos, float duration) => new Tween();
        public static Tween DOAnchorPosY(this UnityEngine.RectTransform target, float y, float duration) => new Tween();
        public static Tween DOScale(this UnityEngine.Transform target, float scale, float duration) => new Tween();
        public static Tween DOScale(this UnityEngine.Transform target, UnityEngine.Vector3 scale, float duration) => new Tween();
        public static Tween DOPunchScale(this UnityEngine.Transform target, UnityEngine.Vector3 punch, float duration, int vibrato, float elasticity) => new Tween();
        public static Tween DOFade(this UnityEngine.UI.CanvasGroup target, float value, float duration) => new Tween();
    }
}

namespace UnityEngine.Pool
{
    public class ObjectPool<T> where T : class
    {
        private readonly Func<T> create;
        public ObjectPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 100) { create = createFunc; }
        public T Get() => create(); public void Release(T item) { }
    }
}

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class InitializeOnLoadAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)] public sealed class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string path) { }
    }
    public static class EditorApplication
    {
        public static Action delayCall;
        public static Action update;
        public static double timeSinceStartup => 0;
        public static bool isCompiling, isUpdating;
        public static void Exit(int exitCode) { }
    }
    public sealed class PlayerSettings : UnityEngine.Object
    {
        public static UnityEngine.UIOrientation defaultInterfaceOrientation;
        public static string companyName, productName;
    }
    public sealed class SerializedObject : IDisposable
    {
        public SerializedObject(UnityEngine.Object target) { }
        public SerializedProperty FindProperty(string path) => new SerializedProperty();
        public bool ApplyModifiedPropertiesWithoutUndo() => true;
        public void Dispose() { }
    }
    public sealed class SerializedProperty { public int intValue; }
    public sealed class EditorBuildSettingsScene
    {
        public string path;
        public bool enabled;
        public EditorBuildSettingsScene(string path, bool enabled) { }
    }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes; }
    public enum BuildTarget { Android }
    public enum BuildOptions { None }
    public static class BuildPipeline
    {
        public static UnityEditor.Build.Reporting.BuildReport BuildPlayer(string[] scenes, string locationPathName,
            BuildTarget target, BuildOptions options) => new UnityEditor.Build.Reporting.BuildReport();
    }
    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => null;
        public static bool IsValidFolder(string path) => true;
        public static string CreateFolder(string parent, string name) => string.Empty;
        public static void CreateAsset(UnityEngine.Object asset, string path) { }
        public static void SaveAssets() { }
        public static void Refresh() { }
        public static void ImportPackage(string packagePath, bool interactive) { }
    }
    public static class EditorUtility { public static void SetDirty(UnityEngine.Object asset) { } }
}

namespace UnityEditor.Build.Reporting
{
    public enum BuildResult { Succeeded, Failed }
    public struct BuildSummary { public BuildResult result; public ulong totalSize; }
    public sealed class BuildReport { public BuildSummary summary; }
}

namespace UnityEditor.PackageManager
{
    public sealed class PackageInfo
    {
        public string name { get; set; }
        public string resolvedPath { get; set; }
        public static PackageInfo[] GetAllRegisteredPackages() => Array.Empty<PackageInfo>();
    }
}

namespace UnityEditor.SceneManagement
{
    using UnityEngine.SceneManagement;
    public enum NewSceneSetup { EmptyScene }
    public enum NewSceneMode { Single }
    public static class EditorSceneManager
    {
        public static Scene NewScene(NewSceneSetup setup, NewSceneMode mode) => new Scene();
        public static bool SaveScene(Scene scene, string path) => true;
    }
}
