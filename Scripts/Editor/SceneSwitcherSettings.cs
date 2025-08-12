using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSwitcherSettings", menuName = "Tools/Scene Switcher Settings")]
public class SceneSwitcherSettings : ScriptableObject
{
    public List<SceneAsset> includedScenes = new List<SceneAsset>();
    public bool autoIncludeBuildScenes = true;

    [Tooltip("Scenes listed here will be ignored even if Auto Include is enabled.")]
    public List<SceneAsset> excludeScenes = new List<SceneAsset>();
}
