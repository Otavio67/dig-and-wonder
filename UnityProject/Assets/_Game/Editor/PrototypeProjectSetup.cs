using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
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
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private static bool tmpImportRequested;
        private static double batchDeadline;

        static PrototypeProjectSetup()
        {
            EditorApplication.delayCall += EnsureProjectSetup;
        }

        // Permite preparar e verificar a primeira abertura também sem interface gráfica.
        public static void PrepareForBatchMode()
        {
            ContentCatalogImporter.ImportAll();
            EnsureProjectSetup();
            batchDeadline = EditorApplication.timeSinceStartup + 180;
            EditorApplication.update += WaitForBatchPreparation;
        }

        [MenuItem("Dig & Wonder/Build Android debug APK")]
        public static void BuildAndroidDebugApk()
        {
            // executeMethod can run before the delayed Editor initializer. Configure before
            // generating the manifest so Android never launches in reversePortrait.
            EnsureProjectSetup();
            EnemyPresentationBaker.Bake();
            AssetDatabase.SaveAssets();
            string outputPath = GetBuildOutputPath();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                .Select(scene => scene.path).ToArray();
            if (scenes.Length == 0)
                throw new System.InvalidOperationException("Nenhuma cena está habilitada para o build.");

            BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);
            bool succeeded = report.summary.result == BuildResult.Succeeded;
            Debug.Log($"[DigAndWonderBuild] {report.summary.result}: {outputPath} ({report.summary.totalSize} bytes)");
            EditorApplication.Exit(succeeded ? 0 : 1);
        }

        private static string GetBuildOutputPath()
        {
            string[] arguments = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == "-buildOutput") return Path.GetFullPath(arguments[i + 1]);
            }
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.tools/build/DigAndWonder-debug.apk"));
        }

        private static void WaitForBatchPreparation()
        {
            if (EditorApplication.timeSinceStartup > batchDeadline)
            {
                EditorApplication.update -= WaitForBatchPreparation;
                Debug.LogError("[DigAndWonderSetup] Tempo esgotado ao importar os recursos essenciais.");
                EditorApplication.Exit(1);
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(TmpSettingsPath) == null ||
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ScenePath) == null ||
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/_Game/Resources/Data/CombatBalance.asset") == null) return;
            EditorApplication.update -= WaitForBatchPreparation;
            AssetDatabase.SaveAssets();
            Debug.Log("[DigAndWonderSetup] Dados, cena e fontes preparados.");
            EditorApplication.Exit(0);
        }

        [MenuItem("Dig & Wonder/Prepare prototype scene")]
        private static void EnsureProjectSetup()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            ConfigureInputSystem();
            PlayerSettings.companyName = "DigAndWonder";
            PlayerSettings.productName = "Dig & Wonder";

            // Unity 6 distribui o TMP e seus recursos essenciais dentro do uGUI.
            if (!File.Exists(TmpSettingsPath) && !tmpImportRequested)
            {
                var uiPackage = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                    .FirstOrDefault(package => package.name == "com.unity.ugui");
                string tmpEssentialPackage = uiPackage == null ? null : Path.Combine(uiPackage.resolvedPath,
                    "Package Resources/TMP Essential Resources.unitypackage");
                if (tmpEssentialPackage == null || !File.Exists(tmpEssentialPackage))
                    throw new FileNotFoundException("Recursos essenciais do TMP não encontrados no pacote uGUI.");
                tmpImportRequested = true;
                AssetDatabase.ImportPackage(tmpEssentialPackage, false);
            }

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

        private static void ConfigureInputSystem()
        {
            // activeInputHandler é uma opção serializada, sem setter público no Unity 6.
            PlayerSettings[] settings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            if (settings.Length == 0)
                throw new System.InvalidOperationException("PlayerSettings não está disponível para configurar o Input System.");
            using var serializedSettings = new SerializedObject(settings[0]);
            SerializedProperty handler = serializedSettings.FindProperty("activeInputHandler");
            if (handler == null)
                throw new System.InvalidOperationException("Opção activeInputHandler ausente nesta versão do Unity.");
            if (handler.intValue == 1) return;
            handler.intValue = 1; // 0=Input Manager antigo, 1=Input System novo.
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
