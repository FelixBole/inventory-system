using UnityEditor;
using UnityEngine;

namespace Slax.Inventory.Editor
{
    [CustomEditor(typeof(InventorySO))]
    public class InventorySOEditor : UnityEditor.Editor
    {
        private SerializedProperty _nameProperty;
        private SerializedProperty _tabConfigsProperty;

        // Weight System
        private SerializedProperty _useWeightProperty;
        private SerializedProperty _maxWeightProperty;

        private bool _extensionsFoldout = true;
        private bool _weightFoldout = false;

        private void OnEnable()
        {
            // Initialize serialized properties
            _nameProperty = serializedObject.FindProperty("Name");
            _tabConfigsProperty = serializedObject.FindProperty("_tabConfigs");

            // Weight System
            _useWeightProperty = serializedObject.FindProperty("_useWeight");
            _maxWeightProperty = serializedObject.FindProperty("_maxWeight");
        }

        public override void OnInspectorGUI()
        {
            // Begin property changes check
            serializedObject.Update();

            // Inventory Settings
            EditorGUILayout.PropertyField(_nameProperty);

            // Display Tab Configurations
            DrawTabConfigs();

            // Extensions Foldout
            DrawExtensions();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTabConfigs()
        {
            EditorGUILayout.PropertyField(_tabConfigsProperty, new GUIContent("Tab Configurations"), true);
        }

        private void DrawExtensions()
        {
            _extensionsFoldout = EditorGUILayout.Foldout(_extensionsFoldout, "Extensions", true);
            if (_extensionsFoldout)
            {
                EditorGUI.indentLevel++;

                // Weight System
                DrawWeightExtension();

                EditorGUI.indentLevel--;
            }
        }

        private void DrawWeightExtension()
        {
            _weightFoldout = EditorGUILayout.Foldout(_weightFoldout, "Weight System", true);
            if (_weightFoldout)
            {
                EditorGUI.indentLevel++;

                _useWeightProperty.boolValue = EditorGUILayout.Toggle("Use Weight System", _useWeightProperty.boolValue);

                if (_useWeightProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(_maxWeightProperty, new GUIContent("Max Weight"));
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
