using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityToolbarExtender;
using System.IO;
using System.Linq;

namespace SceneSwitcherToolbar
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold,
                fixedWidth = 120 // Add some padding for better appearance
            };
        }
    }

    [InitializeOnLoad]
    public static class SceneSwitcherManager
    {
        static SceneSwitcherSettings settings;

        static SceneSwitcherManager()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            LoadSettings();
        }

        static void LoadSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:SceneSwitcherSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<SceneSwitcherSettings>(path);
            }
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);

            // "Scenes" button
            if (GUILayout.Button(new GUIContent("Scenes", "List of Scenes"),
                                 ToolbarStyles.commandButtonStyle,
                                 GUILayout.MinWidth(120), GUILayout.Height(22)))
            {
                ShowSceneDropdown();
            }

            // Settings icon button (ScriptableObject icon)
            GUIContent icon = EditorGUIUtility.IconContent("ScriptableObject Icon");
            icon.tooltip = "Open SceneSwitcherSettings";

            if (GUILayout.Button(icon, GUILayout.Width(28), GUILayout.Height(22)))
            {
                OpenSettingsAsset();
            }

            // If no settings found, show a create button
            if (settings == null)
            {
                if (GUILayout.Button("Create Settings", GUILayout.MinWidth(120), GUILayout.Height(22)))
                {
                    CreateSettingsAsset();
                }
            }

            GUILayout.EndHorizontal();
        }

        static void ShowSceneDropdown()
        {
            GenericMenu menu = new GenericMenu();

            if (settings == null)
            {
                menu.AddDisabledItem(new GUIContent("No SceneSwitcherSettings asset found"));
                menu.ShowAsContext();
                return;
            }

            HashSet<string> scenePaths = new HashSet<string>();

            if (settings.autoIncludeBuildScenes)
            {
                foreach (var s in EditorBuildSettings.scenes.Where(s => s.enabled))
                {
                    string scenePath = s.path;
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

                    if (!settings.excludeScenes.Contains(sceneAsset))
                        scenePaths.Add(scenePath);
                }
            }

            foreach (var sceneAsset in settings.includedScenes)
            {
                string path = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(path))
                    scenePaths.Add(path);
            }

            if (scenePaths.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No scenes found"));
            }
            else
            {
                foreach (var scenePath in scenePaths)
                {
                    string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    menu.AddItem(new GUIContent(sceneName), false, () => LoadScene(scenePath));
                }
            }

            menu.ShowAsContext();
        }

        static void LoadScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        static void OpenSettingsAsset()
        {
            if (settings == null)
            {
                LoadSettings();
            }

            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                Debug.LogWarning("SceneSwitcherSettings asset not found. Please create one via Create > Tools > Scene Switcher Settings.");
            }
        }

        // Create SceneSwitcherSettings asset if not found
        static void CreateSettingsAsset()
        {
            string folderPath = "Assets/Editor/SceneSwitcher";
            string assetPath = "Assets/Editor/SceneSwitcher/SceneSwitcherSettings.asset";

            // Ensure the parent folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                // Create the parent directory if it doesn't exist
                string parentFolderPath = "Assets/Editor";
                if (!AssetDatabase.IsValidFolder(parentFolderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor");
                }

                // Create the folder for SceneSwitcher if it doesn't exist
                AssetDatabase.CreateFolder("Assets/Editor", "SceneSwitcher");
            }

            // Check if the asset already exists
            if (File.Exists(assetPath))
            {
                return; // Skip if the asset already exists
            }

            // Create the SceneSwitcherSettings asset
            SceneSwitcherSettings settings = ScriptableObject.CreateInstance<SceneSwitcherSettings>();

            // Create the asset and save it
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();

            // Refresh the AssetDatabase to reflect changes
            AssetDatabase.Refresh();

            // Log the creation
            Debug.Log("SceneSwitcherSettings asset created at " + assetPath);

            // Reload the settings to reflect the newly created asset
            LoadSettings();
        }
    }
}
