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
    }
}