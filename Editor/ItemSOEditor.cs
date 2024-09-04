using UnityEngine;
using UnityEditor;

namespace Slax.Inventory
{
    [CustomEditor(typeof(ItemSO))]
    public class ItemSOEditor : EditorWithQuickAccessMethods
    {
        ItemSO _item;
        Vector2 _scrollPos;

        bool _baseInfoFoldout = true;
        bool _helpFoldout = false;
        bool _configurationFoldout = true;
        bool _extensionsFoldout = false;
        bool _weightExtensionFoldout = false;
        bool _lootExtensionFoldout = false;
        bool _currencyExtensionFoldout = false;

        SerializedProperty _isUniqueProperty;
        SerializedProperty _isStackableProperty;
        SerializedProperty _stackLimitProperty;

        void OnEnable()
        {
            _item = (ItemSO)target;
            _weightExtensionFoldout = _item.Weight != 0;
            _extensionsFoldout = _weightExtensionFoldout || _lootExtensionFoldout || _currencyExtensionFoldout;

            _isUniqueProperty = serializedObject.FindProperty("_isUnique");
            _isStackableProperty = serializedObject.FindProperty("_isStackable");
            _stackLimitProperty = serializedObject.FindProperty("_stackLimit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHelp();

            BSV(ref _scrollPos);

            DrawQuickView();

            Space();

            DrawBaseInfo();

            DrawConfiguration();

            DrawExtensions();

            ESV();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawHelp()
        {
            EditorGUI.indentLevel++;
            _helpFoldout = EditorGUILayout.Foldout(_helpFoldout, "Help", true);

            if (_helpFoldout)
            {
                EditorGUILayout.HelpBox("This scriptable object represents an item in the inventory system. It contains all the necessary information to create and manage items in the inventory system. It will need at least one Tab Configuration (and generally only one) for the inventory system to know where to place it.", MessageType.None);
            }
            EditorGUI.indentLevel--;
        }

        void DrawQuickView()
        {
            BV(true);

            BH();
            BV();
            EditorGUILayout.LabelField(_item.Name, EditorStyles.boldLabel);
            string tabs = string.Join(", ", _item.TabConfigs.ConvertAll(x => x.Name).ToArray());
            if (string.IsNullOrEmpty(tabs))
            {
                tabs = "None";
            }
            EditorGUILayout.LabelField($"Tabs: {tabs}");
            EV();

            if (_item.PreviewSprite)
            {
                GUI.DrawTexture(GUILayoutUtility.GetRect(64, 64), _item.PreviewSprite.texture, ScaleMode.ScaleToFit, true, 1);
            }
            EH();

            EV();
        }

        void DrawBaseInfo()
        {
            EditorGUI.indentLevel++;
            _baseInfoFoldout = EditorGUILayout.Foldout(_baseInfoFoldout, "Base Information", true);
            EditorGUI.indentLevel--;

            if (_baseInfoFoldout)
            {
                BV(true);
                Space();
                PropertyFieldFor("_id", "ID");
                PropertyFieldFor("_name", "Name");
                PropertyFieldFor("_description", "Description");
                BH();
                PropertyFieldFor("_previewSprite", "Icon");

                EH();
                EV();
            }
        }

        void DrawConfiguration()
        {
            Space();
            EditorGUI.indentLevel++;
            _configurationFoldout = EditorGUILayout.Foldout(_configurationFoldout, "Configuration", true);

            if (!_configurationFoldout)
            {
                EditorGUI.indentLevel--;
                return;
            }

            BV(true);

            PropertyFieldFor("_actionTypes", "Action Types");
            PropertyFieldFor("_tabConfigs", "Tab Configurations");

            if (_item.TabConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no tab configurations assigned to this item. The inventory will not be able to know what tab to set this item to.", MessageType.Warning);
            }

            PropertyFieldFor("_prefab", "Prefab");

            PropertyFieldFor("_isUnique", "Is Unique");
            if (_item.IsUnique)
            {
                _isStackableProperty.boolValue = false;
                _stackLimitProperty.intValue = 1;
            }
            else
            {
                PropertyFieldFor("_isStackable", "Is Stackable");
            }

            if (_item.IsStackable)
            {
                EditorGUILayout.LabelField("Stack Limit");
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Infinite Stack Limit"))
                {
                    _stackLimitProperty.intValue = -1;
                }
                _stackLimitProperty.intValue = EditorGUILayout.IntField("Stack Limit", _stackLimitProperty.intValue);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;

            EV();
        }

        void DrawExtensions()
        {
            Space();
            EditorGUI.indentLevel++;

            _extensionsFoldout = EditorGUILayout.Foldout(_extensionsFoldout, "Extensions", true);

            if (_extensionsFoldout)
            {

                GUI.backgroundColor = Color.blue;
                BV(true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.HelpBox("Extensions are additional data that can be added to the item to provide more functionality. Please ensure that your InventorySO is correctly configured if you decide to use some of these extensions.", MessageType.None);
                Space();
                DrawWeightExtension();
                Space();
                DrawLootExtension();
                Space();
                EV();
            }

            EditorGUI.indentLevel--;
        }

        void DrawWeightExtension()
        {
            EditorGUI.indentLevel++;
            _weightExtensionFoldout = EditorGUILayout.Foldout(_weightExtensionFoldout, "Weight Extension", true);
            if (_weightExtensionFoldout)
            {
                BV(true);
                PropertyFieldFor("_weight", "Weight");
                EV();
            }
            EditorGUI.indentLevel--;
        }

        void DrawLootExtension()
        {
            EditorGUI.indentLevel++;
            _lootExtensionFoldout = EditorGUILayout.Foldout(_lootExtensionFoldout, "Loot Extension", true);
            if (_lootExtensionFoldout)
            {
                BV(true);
                PropertyFieldFor("_minDrops", "Min Drops");
                PropertyFieldFor("_maxDrops", "Max Drops");
                EV();
            }
            EditorGUI.indentLevel--;
        }

        void DrawCurrencyExtension()
        {
            EditorGUI.indentLevel++;
            _currencyExtensionFoldout = EditorGUILayout.Foldout(_currencyExtensionFoldout, "Currency Extension", true);
            if (_currencyExtensionFoldout)
            {
                BV(true);
                // TODO: Add currency extension fields
                EV();
            }
            EditorGUI.indentLevel--;
        }
    }
}