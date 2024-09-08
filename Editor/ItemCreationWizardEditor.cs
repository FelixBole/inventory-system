using Slax.InventorySystem.Runtime.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Slax.InventorySystem.Editor
{
    /// <summary>
    /// Class for handling the item creation wizard editor.
    /// Not inheriting from <see cref="EditorWindow"/> so that it can be used by other windows.
    /// </summary>
    public class ItemCreationWizardEditor
    {
        protected VisualElement _itemCreationContainer;
        protected TextField _itemCreationPathField;
        protected TextField _itemCreationIdField;
        protected TextField _itemCreationNameField;
        protected TextField _itemCreationDescriptionField;
        protected ObjectField _itemCreationPreviewSpriteField;
        protected ObjectField _itemCreationTabConfigField;
        protected string _itemCreationPath = "Assets/InventorySystem/Items";

        public void Setup(VisualElement element)
        {
            _itemCreationContainer = element.Q<VisualElement>("container-item-creation");
            _itemCreationPathField = _itemCreationContainer.Q<TextField>("text-field-item-creation-path");
            _itemCreationIdField = _itemCreationContainer.Q<TextField>("text-field-item-creation-id");
            _itemCreationNameField = _itemCreationContainer.Q<TextField>("text-field-item-creation-name");
            _itemCreationDescriptionField = _itemCreationContainer.Q<TextField>("text-field-item-creation-description");
            _itemCreationPreviewSpriteField = _itemCreationContainer.Q<ObjectField>("object-field-item-creation-preview-sprite");
            _itemCreationTabConfigField = _itemCreationContainer.Q<ObjectField>("object-field-item-creation-tab");

            var itemCreationButton = _itemCreationContainer.Q<Button>("button-item-creation-create");

            _itemCreationPathField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                _itemCreationPath = evt.newValue;
            });

            itemCreationButton.clicked += () =>
            {
                CreateItem();
            };
        }

        public void Hide()
        {
            _itemCreationContainer.style.display = DisplayStyle.None;
        }

        public void Show()
        {
            _itemCreationContainer.style.display = DisplayStyle.Flex;
        }

        protected ItemSO CreateItem()
        {
            ItemSO item = ScriptableObject.CreateInstance<ItemSO>();
            SerializedObject serializedItem = new SerializedObject(item);

            serializedItem.FindProperty("_id").stringValue = _itemCreationIdField.value;
            serializedItem.FindProperty("_name").stringValue = _itemCreationNameField.value;
            serializedItem.FindProperty("_description").stringValue = _itemCreationDescriptionField.value;
            serializedItem.FindProperty("_previewSprite").objectReferenceValue = _itemCreationPreviewSpriteField.value as Sprite;

            // Find property _tabConfigs (list) and add to that list
            serializedItem.FindProperty("_tabConfigs").arraySize++;
            serializedItem.FindProperty("_tabConfigs").GetArrayElementAtIndex(serializedItem.FindProperty("_tabConfigs").arraySize - 1).objectReferenceValue = _itemCreationTabConfigField.value as InventoryTabConfigSO;

            serializedItem.ApplyModifiedProperties();

            string path = _itemCreationPath;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", path);
            }

            AssetDatabase.CreateAsset(item, $"{path}/{_itemCreationNameField.value}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _itemCreationIdField.value = "";
            _itemCreationNameField.value = "";
            _itemCreationDescriptionField.value = "";
            _itemCreationPreviewSpriteField.value = null;

            var asset = AssetDatabase.LoadAssetAtPath<ItemSO>($"{path}/{_itemCreationNameField.value}.asset");
            return asset;
        } 
    }
}