using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigAndWonder.Editor
{
    /// <summary>Prepara uma cena de entrada e configurações mobile ao abrir o projeto no Editor.</summary>
    [InitializeOnLoad]
    public static class PrototypeProjectSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/Prototype.unity";

        static PrototypeProjectSetup()
        {
            EditorApplication.delayCall += EnsureProjectSetup;
        }

        [MenuItem("Dig & Wonder/Prepare prototype scene")]
        private static void EnsureProjectSetup()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.activeInputHandler = 1; // 0=Input Manager antigo, 1=Input System novo
            PlayerSettings.companyName = "DigAndWonder";
            PlayerSettings.productName = "Dig & Wonder";

            const string tmpEssentialPackage = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
            if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset") && File.Exists(tmpEssentialPackage))
                AssetDatabase.ImportPackage(tmpEssentialPackage, false);

            string sceneDirectory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(sceneDirectory)) Directory.CreateDirectory(sceneDirectory);
            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();
        }
    }
}
