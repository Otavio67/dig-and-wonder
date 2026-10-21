// Author: Daniele Giardini - http://www.demigiant.com
// Restored from the official DOTween module bootstrap. The project had the
// core DLL and individual modules, but was missing this reflection bridge.

#define DOTWEEN_NOPHYSICS
#define DOTWEEN_NOPHYSICS2D

using System;
using System.Reflection;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;

#pragma warning disable 1591
namespace DG.Tweening
{
    public static class DOTweenModuleUtils
    {
        private static bool initialized;

        // Called by DOTweenComponent through reflection during startup.
        [UnityEngine.Scripting.Preserve]
        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            DOTweenExternalCommand.SetOrientationOnPath -= Physics.SetOrientationOnPath;
            DOTweenExternalCommand.SetOrientationOnPath += Physics.SetOrientationOnPath;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= PlaymodeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += PlaymodeStateChanged;
#endif
        }

        // Preserves the reflection path used by DOTween when managed stripping
        // is enabled in a player build.
        [UnityEngine.Scripting.Preserve]
        private static void Preserver()
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            MethodInfo method = typeof(MonoBehaviour).GetMethod("Stub");
        }

#if UNITY_EDITOR
        private static void PlaymodeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (DOTween.instance == null) return;
            DOTween.instance.OnApplicationPause(UnityEditor.EditorApplication.isPaused);
        }
#endif

        public static class Physics
        {
            public static void SetOrientationOnPath(PathOptions options, Tween tween, Quaternion rotation, Transform transform)
            {
#if !DOTWEEN_NOPHYSICS
                if (options.isRigidbody) ((Rigidbody)tween.target).rotation = rotation;
                else transform.rotation = rotation;
#else
                transform.rotation = rotation;
#endif
            }

            public static bool HasRigidbody2D(Component target)
            {
#if !DOTWEEN_NOPHYSICS2D
                return target.GetComponent<Rigidbody2D>() != null;
#else
                return false;
#endif
            }

            [UnityEngine.Scripting.Preserve]
            public static bool HasRigidbody(Component target)
            {
#if !DOTWEEN_NOPHYSICS
                return target.GetComponent<Rigidbody>() != null;
#else
                return false;
#endif
            }

            [UnityEngine.Scripting.Preserve]
            public static TweenerCore<Vector3, Path, PathOptions> CreateDOTweenPathTween(
                MonoBehaviour target, bool tweenRigidbody, bool isLocal, Path path, float duration, PathMode pathMode)
            {
                TweenerCore<Vector3, Path, PathOptions> tween = null;
                bool rigidbodyTweened = false;
#if !DOTWEEN_NOPHYSICS
                if (tweenRigidbody)
                {
                    Rigidbody body = target.GetComponent<Rigidbody>();
                    if (body != null)
                    {
                        rigidbodyTweened = true;
                        tween = isLocal ? body.DOLocalPath(path, duration, pathMode) : body.DOPath(path, duration, pathMode);
                    }
                }
#endif
#if !DOTWEEN_NOPHYSICS2D
                if (!rigidbodyTweened && tweenRigidbody)
                {
                    Rigidbody2D body = target.GetComponent<Rigidbody2D>();
                    if (body != null)
                    {
                        rigidbodyTweened = true;
                        tween = isLocal ? body.DOLocalPath(path, duration, pathMode) : body.DOPath(path, duration, pathMode);
                    }
                }
#endif
                if (!rigidbodyTweened)
                    tween = isLocal ? target.transform.DOLocalPath(path, duration, pathMode) : target.transform.DOPath(path, duration, pathMode);
                return tween;
            }
        }
    }
}
