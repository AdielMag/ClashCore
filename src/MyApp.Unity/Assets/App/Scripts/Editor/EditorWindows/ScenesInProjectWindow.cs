// ScenesInProjectWindow.cs — Unity Editor utility window
// Put this script in an **Editor** folder (e.g. Assets/Editor)
// Menu:  Tools ▸ Scenes In Project  (Cmd + G)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ScenesInProjectWindow : EditorWindow
{
    private const float _kSideMargin = 18f;
    private const float _kRowHeight = 30f;
    private const float _kTitleHeight = 50f;
    private static readonly Color _rowGreen = new Color(0.2f, 0.6f, 0.3f, 1f);
    private static readonly Color _rowRed = new Color(0.6f, 0.2f, 0.2f, 1f);
    private static readonly Color _rowHover = new Color(0.3f, 0.3f, 0.3f, 1f);

    private Vector2 _scroll;
    private List<string> _scenePaths;
    private string _searchTerm = string.Empty;

    [MenuItem("Tools/Scenes In Project %g")] // Cmd + G
    public static void ShowWindow()
    {
        var window = GetWindow<ScenesInProjectWindow>("Scenes In Project");
        window.minSize = new Vector2(400, 500);
        window.RefreshSceneList();
    }

    private void RefreshSceneList()
    {
        _scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                                   .Select(AssetDatabase.GUIDToAssetPath)
                                   .OrderBy(path => Path.GetFileNameWithoutExtension(path))
                                   .ToList();
    }

    private void OnEnable() => RefreshSceneList();

    private void OnGUI()
    {
        DrawTitle();
        DrawSearchBar();

        GUILayout.Space(10);
        _scroll = GUILayout.BeginScrollView(_scroll);
        DrawSceneRows();
        GUILayout.EndScrollView();
    }

    private void DrawTitle()
    {
        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            normal = { textColor = Color.white }
        };

        var rect = GUILayoutUtility.GetRect(position.width, _kTitleHeight);
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));
        EditorGUI.LabelField(rect, "Scenes In Project", titleStyle);
    }

    private void DrawSearchBar()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(50));
        string newSearchTerm = GUILayout.TextField(_searchTerm);
        if (newSearchTerm != _searchTerm)
        {
            _searchTerm = newSearchTerm;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawSceneRows()
    {
        var buildScenes = EditorBuildSettings.scenes.Select(s => s.path).ToHashSet();
        var evt = Event.current;

        foreach (var path in _scenePaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);

            if (!string.IsNullOrEmpty(_searchTerm) && 
                !fileName.ToLower().Contains(_searchTerm.ToLower()))
            {
                continue; // skip if doesn't match search
            }

            var inBuild = buildScenes.Contains(path);

            var rowRect = GUILayoutUtility.GetRect(position.width, _kRowHeight);
            rowRect.x += _kSideMargin;
            rowRect.width -= 2 * _kSideMargin;

            var baseColor = inBuild ? _rowGreen : _rowRed;
            if (rowRect.Contains(evt.mousePosition))
                baseColor = Color.Lerp(baseColor, _rowHover, 0.5f);

            EditorGUI.DrawRect(rowRect, baseColor);

            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
            };

            // Layout for button, select, and toggle
            var buttonRect = rowRect;
            buttonRect.width -= 60f; // leave space for select and toggle

            var selectRect = new Rect(rowRect.xMax - 75f, rowRect.y + 5f, 50f, 20f);
            var toggleRect = new Rect(rowRect.xMax - 20f, rowRect.y + 5f, 18f, 18f);

            if (GUI.Button(buttonRect, fileName, labelStyle))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(path);
            }

            if (GUI.Button(selectRect, "Select"))
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (sceneAsset != null)
                    Selection.activeObject = sceneAsset;
            }

            bool newState = GUI.Toggle(toggleRect, inBuild, GUIContent.none);
            if (newState != inBuild)
                ToggleSceneInBuild(path, newState);

            GUILayout.Space(2);
        }
    }

    private void ToggleSceneInBuild(string path, bool add)
    {
        var scenes = EditorBuildSettings.scenes.ToList();

        if (add)
        {
            if (!scenes.Any(s => s.path == path))
                scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        else
        {
            scenes = scenes.Where(s => s.path != path).ToList();
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
