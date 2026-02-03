using PuzzleGallery.Features.MainMenu.Layout;
using PuzzleGallery.Services.Logging;
using UnityEditor;
using UnityEngine;

namespace PuzzleGallery.Editor
{
    [CustomEditor(typeof(TabBarLayoutController))]
    public sealed class TabBarLayoutControllerEditor : UnityEditor.Editor
    {
        private TabBarLayoutController _controller;

        private void OnEnable()
        {
            _controller = (TabBarLayoutController)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Layout", GUILayout.Height(30)))
            {
                ApplyLayout();
            }

            if (GUILayout.Button("Reset Layout", GUILayout.Height(30)))
            {
                ResetLayout();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Apply Layout: Distributes tabs evenly and positions dividers.\n" +
                "Reset Layout: Clears all anchor modifications.\n" +
                "Auto-Setup: Finds TabBarView and child RectTransforms.",
                MessageType.Info);
        }

        private void ApplyLayout()
        {
            Undo.RecordObject(_controller, "Apply TabBar Layout");

            var containerRect = GetSerializedField<RectTransform>("_containerRect");
            var tabRects = GetSerializedField<RectTransform[]>("_tabRects");
            var dividers = GetSerializedField<RectTransform[]>("_dividers");
            var selectionIndicator = GetSerializedField<RectTransform>("_selectionIndicator");

            if (containerRect == null)
            {
                Logs.Warning("Container RectTransform is not assigned.");
                return;
            }

            if (tabRects == null || tabRects.Length == 0)
            {
                Logs.Warning("Tab RectTransforms are not assigned.");
                return;
            }

            foreach (var tab in tabRects)
            {
                if (tab != null)
                {
                    Undo.RecordObject(tab, "Apply TabBar Layout");
                }
            }

            if (dividers != null)
            {
                foreach (var divider in dividers)
                {
                    if (divider != null)
                    {
                        Undo.RecordObject(divider, "Apply TabBar Layout");
                    }
                }
            }

            if (selectionIndicator != null)
            {
                Undo.RecordObject(selectionIndicator, "Apply TabBar Layout");
            }

            Canvas.ForceUpdateCanvases();

            _controller.ApplyLayout();

            EditorUtility.SetDirty(_controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_controller.gameObject.scene);

            Logs.Info("TabBar layout applied successfully.");
        }

        private void ResetLayout()
        {
            var tabRects = GetSerializedField<RectTransform[]>("_tabRects");

            if (tabRects == null || tabRects.Length == 0)
            {
                Logs.Warning("Tab RectTransforms are not assigned.");
                return;
            }

            foreach (var tab in tabRects)
            {
                if (tab != null)
                {
                    Undo.RecordObject(tab, "Reset TabBar Layout");

                    tab.anchorMin = Vector2.zero;
                    tab.anchorMax = Vector2.one;
                    tab.offsetMin = Vector2.zero;
                    tab.offsetMax = Vector2.zero;

                    EditorUtility.SetDirty(tab);
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_controller.gameObject.scene);
            Logs.Info("TabBar layout reset to defaults.");
        }

        private T GetSerializedField<T>(string fieldName) where T : class
        {
            var field = typeof(TabBarLayoutController).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_controller) as T;
        }
    }
}
