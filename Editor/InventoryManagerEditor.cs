using Slax.Inventory.Editor;
using UnityEditor;
using UnityEngine;

namespace Slax.Inventory
{
    [CustomEditor(typeof(InventoryManager))]
    public class InventoryManagerEditor : UnityEditor.Editor
    {
        InventoryManager _inventoryManager;

        bool _eventsFoldout = true;

        void OnEnable()
        {
            _inventoryManager = (InventoryManager)target;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                RuntimeInventoryViewerEditorWindow.CloseWindow();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("This component manages the inventory system and provides the necessary tools to interact with the inventory system.", MessageType.None);

            DrawInventoryConfig();

            DrawSaveSystem();

            DrawEvents();

            DrawToolsButton();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawInventoryConfig()
        {
            EditorGUILayout.Space(5);

            if (!_inventoryManager.InventoryConfig)
            {
                EditorGUILayout.HelpBox("No Inventory Config assigned. Please assign an Inventory Config to create and manage the inventory data.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_inventoryConfig"), new GUIContent("Inventory Config"));

            EditorGUILayout.Space(5);
        }

        void DrawSaveSystem()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_inventoryManager.SaveSystem == null)
            {
                EditorGUILayout.HelpBox("No Save System assigned. Please assign a Save System to save and load the inventory data.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_saveSystem"), new GUIContent("Save System"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        void DrawEvents()
        {
            _eventsFoldout = EditorGUILayout.Foldout(_eventsFoldout, "Events", true);
            if (_eventsFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnInventoryChanged"), new GUIContent("On Inventory Changed"));
            }
        }

        void DrawToolsButton()
        {
            var style = new GUIStyle(GUI.skin.button);
            if (!EditorApplication.isPlaying)
            {
                style.normal.textColor = Color.gray;
                style.active.textColor = Color.gray;
                style.hover.textColor = Color.gray;
            }

            if (GUILayout.Button("Open Inventory Viewer", style, GUILayout.Height(30)) && EditorApplication.isPlaying)
            {
                RuntimeInventoryViewerEditorWindow.OpenWindow(_inventoryManager);
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("The Inventory Viewer tool is only available in Play Mode.", MessageType.Info);
            }
        }
    }
}