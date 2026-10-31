#if UNITY_ANDROID
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace DigAndWonder.Editor
{
    public static class ApplicationIconSetup
    {
        public const string IconPath = "Assets/_Game/Art/AppIcon/dig-and-wonder-app-icon.png";

        [MenuItem("Dig & Wonder/Apply application icon")]
        public static void Apply()
        {
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Ícone do aplicativo ausente.");
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            foreach (PlatformIconKind kind in new[] { AndroidPlatformIconKind.Adaptive })
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                foreach (PlatformIcon icon in icons)
                {
                    if (kind == AndroidPlatformIconKind.Adaptive) icon.SetTextures(new[] { texture, texture });
                    else icon.SetTexture(texture);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[DigAndWonderIcon] Ícones adaptativos Android configurados.");
        }

        public static void BuildApk()
        {
            Apply();
            PrototypeProjectSetup.BuildAndroidDebugApk();
        }
    }
}
#endif
