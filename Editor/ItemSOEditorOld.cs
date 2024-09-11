using UnityEngine;
using UnityEditor;
using System;
using Slax.InventorySystem.Runtime.Core;
using UnityEngine.UIElements;

namespace Slax.InventorySystem.Editor
{
    // [CustomEditor(typeof(ItemSO))]
    public class ItemSOEditorOld : EditorWithQuickAccessMethods
    {
        const int PREVIEW_SIZE = 64;

        ItemSO _item;
        Vector2 _scrollPos;

        bool _baseInfoFoldout = true;
        bool _colorFoldout = false;
        bool _helpFoldout = false;
        bool _configurationFoldout = true;
        bool _extensionsFoldout = false;
        bool _weightExtensionFoldout = false;
        bool _lootExtensionFoldout = false;
        bool _currencyExtensionFoldout = false;

        SerializedProperty _isUniqueProperty;
        SerializedProperty _isStackableProperty;
        SerializedProperty _stackLimitProperty;

        Texture2D _itemBackground;

        void OnEnable()
        {
            _item = (ItemSO)target;
            _weightExtensionFoldout = _item.Weight != 0;
            _extensionsFoldout = _weightExtensionFoldout || _lootExtensionFoldout || _currencyExtensionFoldout;

            _isUniqueProperty = serializedObject.FindProperty("_isUnique");
            _isStackableProperty = serializedObject.FindProperty("_isStackable");
            _stackLimitProperty = serializedObject.FindProperty("_stackLimit");

            _itemBackground = MakeTex(1, 1, _item.BackgroundColor);

            _colorFoldout = _item.Color != Color.white || _item.BackgroundColor != Color.white || _item.SelectedColor != Color.white;
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

            string tabs = "None";

            // If any of the item tabConfigs are null, skip
            if (_item.TabConfigs?.Exists(x => x == null) == true)
            {
                EditorGUILayout.HelpBox("There are null tab configurations assigned to this item. Please remove them.", MessageType.Error);
            }
            else
            {
                tabs = string.Join(", ", _item.TabConfigs?.ConvertAll(x => x.Name).ToArray());
            }
            EditorGUILayout.LabelField($"Tabs: {tabs}");
            EV();

            if (_item.PreviewSprite)
            {
                DrawTextureInBox(_itemBackground, _item.PreviewSprite.texture, PREVIEW_SIZE);
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

                EditorGUI.indentLevel++;
                _colorFoldout = EditorGUILayout.Foldout(_colorFoldout, "Colors", true);
                if (_colorFoldout)
                {
                    BV(true);
                    EditorGUILayout.HelpBox("These colors are optional and are used to quickly access a color for the item when rendering in any sort of UI.", MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    PropertyFieldFor("_color", "Color");
                    PropertyFieldFor("_backgroundColor", "Background Color");
                    PropertyFieldFor("_selectedColor", "Selected Color");
                    if (EditorGUI.EndChangeCheck())
                    {
                        _itemBackground = MakeTex(1, 1, _item.BackgroundColor);
                    }
                    EV();

                    DrawColorCopyPasteButtons();

                }
                EditorGUI.indentLevel--;

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

        void DrawColorCopyPasteButtons()
        {
            if (GUILayout.Button("Copy colors"))
            {
                // Store colors in system clipboard, using consistent separator ("--").
                EditorGUIUtility.systemCopyBuffer = $"{ColorUtility.ToHtmlStringRGBA(_item.Color)}--{ColorUtility.ToHtmlStringRGBA(_item.BackgroundColor)}--{ColorUtility.ToHtmlStringRGBA(_item.SelectedColor)}";
            }

            if (GUILayout.Button("Paste colors"))
            {
                string[] colors = EditorGUIUtility.systemCopyBuffer.Split(new[] { "--" }, StringSplitOptions.None);

                if (colors.Length == 3)
                {
                    Color color, backgroundColor, selectedColor;

                    // Attempt to parse the HTML color strings and assign to the properties if successful.
                    if (ColorUtility.TryParseHtmlString($"#{colors[0]}", out color))
                    {
                        SerializedProperty colorProperty = serializedObject.FindProperty("_color");
                        colorProperty.colorValue = color;
                    }

                    if (ColorUtility.TryParseHtmlString($"#{colors[1]}", out backgroundColor))
                    {
                        SerializedProperty backgroundColorProperty = serializedObject.FindProperty("_backgroundColor");
                        backgroundColorProperty.colorValue = backgroundColor;
                    }

                    if (ColorUtility.TryParseHtmlString($"#{colors[2]}", out selectedColor))
                    {
                        SerializedProperty selectedColorProperty = serializedObject.FindProperty("_selectedColor");
                        selectedColorProperty.colorValue = selectedColor;
                    }

                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
