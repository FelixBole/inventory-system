using System.Collections.Generic;
using Slax.UIToolkit.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Slax.InventorySystem.Editor
{
    /// <summary>
    /// Handles the drawing of an ItemSO gallery in the editor.
    /// </summary>
    public class SerializedItemGallery
    {
        VisualElement _container = new VisualElement();
        VisualElement _gallery = new VisualElement();
        List<SerializedObject> _serializedItems = new List<SerializedObject>();
        System.Action<SerializedObject> _onItemClicked;

        Color _bgColor = Color.white;
        Color _tintColor = Color.white;
        Color _selectedColor = Color.white;

        public SerializedItemGallery(List<SerializedObject> serializedItems, VisualElement targetContainer = null)
        {
            _container = targetContainer ?? new GroupBox();
            _gallery = new GroupBox();
            _gallery.style.flexDirection = FlexDirection.Row;
            _gallery.style.flexWrap = Wrap.Wrap;

            _container.Add(_gallery);

            _serializedItems = serializedItems;

            DrawColorFields();
        }

        public VisualElement GetContainer() => _container;

        public void Show() => Toggle(true);
        public void Hide() => Toggle(false);
        void Toggle(bool isVisible) => _container.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        public SerializedItemGallery SetSerializedItems(List<SerializedObject> serializedItems)
        {
            _serializedItems = new List<SerializedObject>(serializedItems);
            return this;
        }

        public void Draw(System.Action<SerializedObject> clicked, int itemSize = 50)
        {
            _onItemClicked = clicked;

            _gallery.Clear();
            _gallery.style.display = DisplayStyle.Flex;
            _gallery.style.flexDirection = FlexDirection.Row;
            _gallery.style.flexWrap = Wrap.Wrap;

            foreach (SerializedObject item in _serializedItems)
            {
                if (item == null) continue;

                Sprite itemSprite = item.FindProperty("_previewSprite").objectReferenceValue as Sprite;
                Color itemBackgroundColor = item.FindProperty("_backgroundColor").colorValue;
                Color selectedColor = item.FindProperty("_selectedColor").colorValue;
                Color tintColor = item.FindProperty("_color").colorValue;

                var ve = new Button();
                ve.style.width = itemSize;
                ve.style.height = itemSize;
                ve.style.borderBottomColor = selectedColor;
                ve.style.borderBottomWidth = 2;
                ve.style.backgroundColor = itemBackgroundColor;
                ve.style.unityBackgroundImageTintColor = tintColor;
                ve.style.backgroundImage = itemSprite.texture;

                ve.clicked += () =>
                {
                    clicked(item);
                };

                _gallery.Add(ve);
            }
        }

        public void DrawWithNewItems(List<SerializedObject> newItems, System.Action<SerializedObject> clicked, int itemSize = 50)
        {
            _onItemClicked = clicked;
            Toggle(true);
            SetSerializedItems(newItems).Draw(clicked, itemSize);
        }

        public void DrawColorFields()
        {
            var rowBgColor = new Row();
            var bgColor = new ColorField();
            bgColor.value = _bgColor;
            var bgLabel = new Label("Background Color");
            bgLabel.style.flexGrow = 1;
            rowBgColor.Add(bgLabel);
            rowBgColor.Add(bgColor);

            var rowTintColor = new Row();
            var tintColor = new ColorField();
            tintColor.value = _tintColor;
            var tintLabel = new Label("Tint Color");
            tintLabel.style.flexGrow = 1;
            rowTintColor.Add(tintLabel);
            rowTintColor.Add(tintColor);

            var rowSelectedColor = new Row();
            var selectedColor = new ColorField();
            selectedColor.value = _selectedColor;
            var selectedLabel = new Label("Selected Color");
            selectedLabel.style.flexGrow = 1;
            rowSelectedColor.Add(selectedLabel);
            rowSelectedColor.Add(selectedColor);

            _container.Add(rowBgColor);
            _container.Add(rowTintColor);
            _container.Add(rowSelectedColor);

            bgColor.RegisterCallback<ChangeEvent<Color>>(evt => { _bgColor = evt.newValue; });

            tintColor.RegisterCallback<ChangeEvent<Color>>(evt => { _tintColor = evt.newValue; });

            selectedColor.RegisterCallback<ChangeEvent<Color>>(evt => { _selectedColor = evt.newValue; });

            var btnBg = new Button();
            btnBg.text = "Apply background color to all";

            var btnTint = new Button();
            btnTint.text = "Apply tint color to all";

            var btnSelected = new Button();
            btnSelected.text = "Apply selected color to all";

            var btn = new Button();
            btn.text = "Apply all colors to all";

            var row = new Row();
            row.Add(btnBg);
            row.Add(btnTint);
            row.Add(btnSelected);
            row.Add(btn);

            _container.Add(row);

            btnBg.clicked += () =>
            {
                foreach (SerializedObject item in _serializedItems)
                {
                    item.FindProperty("_backgroundColor").colorValue = bgColor.value;
                    item.ApplyModifiedProperties();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DrawWithNewItems(InventorySystemAssetLoader.LoadItemsInProject(), _onItemClicked);
            };

            btnTint.clicked += () =>
            {
                foreach (SerializedObject item in _serializedItems)
                {
                    item.FindProperty("_color").colorValue = tintColor.value;
                    item.ApplyModifiedProperties();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DrawWithNewItems(InventorySystemAssetLoader.LoadItemsInProject(), _onItemClicked);
            };

            btnSelected.clicked += () =>
            {
                foreach (SerializedObject item in _serializedItems)
                {
                    item.FindProperty("_selectedColor").colorValue = selectedColor.value;
                    item.ApplyModifiedProperties();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DrawWithNewItems(InventorySystemAssetLoader.LoadItemsInProject(), _onItemClicked);
            };

            btn.clicked += () =>
            {
                foreach (SerializedObject item in _serializedItems)
                {
                    item.FindProperty("_backgroundColor").colorValue = bgColor.value;
                    item.FindProperty("_color").colorValue = tintColor.value;
                    item.FindProperty("_selectedColor").colorValue = selectedColor.value;
                    item.ApplyModifiedProperties();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                DrawWithNewItems(InventorySystemAssetLoader.LoadItemsInProject(), _onItemClicked);
            };
        }
    }
}
