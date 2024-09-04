using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Slax.Inventory.Editor
{
    [CustomEditor(typeof(InventorySO))]
    public class InventorySOEditor : UnityEditor.Editor
    {
        private InventorySO _inventory;

        private SerializedProperty _nameProperty;
        private SerializedProperty _tabConfigsProperty;

        // Weight System
        private SerializedProperty _useWeightProperty;
        private SerializedProperty _maxWeightProperty;

        private bool _extensionsFoldout = true;
        private bool _weightFoldout = false;
        private bool _useFixedSlotsFoldout = false;
        private bool _useSameItemInMultipleSlotsFoldout = false;

        private void OnEnable()
        {
            _inventory = (InventorySO)target;
            _extensionsFoldout = true;
            _weightFoldout = _inventory.UseWeight;
            _useFixedSlotsFoldout = _inventory.UseFixedSlots;
            _useSameItemInMultipleSlotsFoldout = _inventory.UseSameItemInMultipleSlots;

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

            if (_inventory.TabConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("No Tab Configurations assigned. Please assign Tab Configurations to create and manage the inventory data.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;

            // Display the number of slots in each tab
            for (int i = 0; i < _tabConfigsProperty.arraySize; i++)
            {
                SerializedProperty tabConfig = _tabConfigsProperty.GetArrayElementAtIndex(i);
                var tab = (InventoryTabConfigSO)tabConfig.objectReferenceValue;
                if (tab == null) continue;

                // Sum all the tab unlock states slots
                var sumSlots = tab.SlotUnlockStates.Sum(s => s.AdditionalSlots);

                var str = $"{tab.name}: {tab.SlotUnlockStates.Count} Unlock State | {sumSlots} Total Slots";
                
                EditorGUILayout.LabelField(str);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawExtensions()
        {
            _extensionsFoldout = EditorGUILayout.Foldout(_extensionsFoldout, "Extensions", true);
            if (_extensionsFoldout)
            {
                EditorGUI.indentLevel++;

                // Weight System
                DrawWeightExtension();

                DrawUseFixedSlots();
                DrawUseSameItemInMultipleSlots();

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

        private void DrawUseFixedSlots()
        {
            _useFixedSlotsFoldout = EditorGUILayout.Foldout(_useFixedSlotsFoldout, "Use Fixed Slots", true);
            if (!_useFixedSlotsFoldout) return;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_useFixedSlots"), new GUIContent("Use Fixed Slots"));
            if (_inventory.UseFixedSlots)
            {
                EditorGUILayout.HelpBox("Use Fixed Slots enabled: The inventory slots will not be re-arranged when an item is removed and the saved slot index will be used to place the item.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Use Fixed Slots disabled: The inventory slots will be re-arranged when an item is removed.", MessageType.None);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawUseSameItemInMultipleSlots()
        {
            _useSameItemInMultipleSlotsFoldout = EditorGUILayout.Foldout(_useSameItemInMultipleSlotsFoldout, "Use Same Item In Multiple Slots", true);
            if (!_useSameItemInMultipleSlotsFoldout) return;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_useSameItemInMultipleSlots"), new GUIContent("Use Same Item In Multiple Slots"));
            if (_inventory.UseSameItemInMultipleSlots)
            {
                EditorGUILayout.HelpBox("Use Same Item In Multiple Slots enabled: When an item in a slot reaches its stack limit, it will place the item in the next available slot if there is one.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Use Same Item In Multiple Slots disabled: When an item in a slot reaches its stack limit it will ignore adding to the inventory.", MessageType.None);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
}
