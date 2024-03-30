using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rize.Tools
{
    public class QuickPrefabOpenTool : EditorWindow
    {
        private const string SearchPath = "Assets/";

        private static readonly Vector2 WindowSize = new(400, 600);
        private static List<string> _allPrefabs;
        private static List<string> _fittingPrefabs;

        private string _inputText = "";
        private GUIStyle _blueBackgroundLabelStyle;
        private int _currentSelectedIndex;

        [MenuItem("Examples/My Editor Window %g")]
        public static void ShowExample()
        {
            FindPrefabs();
            var window = GetWindow<QuickPrefabOpenTool>();
            window._currentSelectedIndex = 0;
            window.titleContent = new GUIContent("Quick prefab open");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, WindowSize.x, WindowSize.y);
        }

        private void OnEnable()
        {
            _blueBackgroundLabelStyle = new GUIStyle(EditorStyles.label);
            _blueBackgroundLabelStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 1.0f, 0.5f));
        }

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            var e = Event.current;
            var keyCode = e.keyCode;
            if (e.rawType == EventType.KeyUp)
            {
                switch (keyCode)
                {
                    case KeyCode.Return:
                        if (_fittingPrefabs.Count > 0) OpenPrefabInEditMode(_fittingPrefabs[_currentSelectedIndex]);
                        break;
                    case KeyCode.Escape:
                        Close();
                        break;
                    case KeyCode.DownArrow:
                        _currentSelectedIndex++;
                        break;
                    case KeyCode.UpArrow:
                        _currentSelectedIndex--;
                        break;
                }
            }

            GUILayout.BeginArea(new Rect(0, 0, WindowSize.x, WindowSize.y));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.SetNextControlName("InputField");
            _inputText = GUILayout.TextField(_inputText, GUILayout.Width(400));
            GUILayout.EndHorizontal();

            _fittingPrefabs ??= new List<string>();
            _fittingPrefabs.Clear();

            foreach (var prefab in _allPrefabs)
            {
                var fileName = Path.GetFileNameWithoutExtension(prefab);
                if (fileName.ToLower().StartsWith(_inputText.ToLower()))
                {
                    _fittingPrefabs.Add(prefab);
                }
            }

            _currentSelectedIndex = Mathf.Clamp(_currentSelectedIndex, 0, _fittingPrefabs.Count - 1);

            for (var i = 0; i < _fittingPrefabs.Count; i++)
            {
                var prefab = _fittingPrefabs[i];
                var fileName = Path.GetFileNameWithoutExtension(prefab);
                var isSelected = i == _currentSelectedIndex;
                var style = isSelected ? _blueBackgroundLabelStyle : EditorStyles.label;
                GUILayout.Label(fileName, style);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();

            GUI.FocusControl("InputField");
        }

        private void OpenPrefabInEditMode(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                AssetDatabase.OpenAsset(prefab);
                Close();
            }
        }

        private static void FindPrefabs()
        {
            _allPrefabs ??= new List<string>();
            _allPrefabs.Clear();
            var files = Directory.GetFiles(SearchPath, "*.prefab", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var assetPath = file.Replace(Application.dataPath, "Assets");
                _allPrefabs.Add(assetPath);
            }

            _allPrefabs.Sort((a, b) =>
                string.Compare(Path.GetFileNameWithoutExtension(a), Path.GetFileNameWithoutExtension(b), StringComparison.Ordinal));
        }

        private static Texture2D MakeTex(int width, int height, Color color)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
