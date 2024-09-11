using System.Collections.Generic;
using Slax.InventorySystem.Runtime.Core;
using Slax.UIToolkit.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Slax.InventorySystem.Editor
{
    public class SerializedItemList
    {
        const string LIST_VIEW_NAME = "item-list-sneakpeak";
        const string BTN_ITEM_TEXTURE = "button-item-texture";
        const int LIST_VIEW_ITEM_HEIGHT = 50;
        VisualTreeAsset _itemBtnUxml = default;

        VisualElement _container = new VisualElement();
        ListView _listView = new ListView();

        System.Action<SerializedObject> _onItemClicked;

        public SerializedItemList(VisualElement container, System.Action<SerializedObject> onItemClicked, VisualTreeAsset itemBtnUxml)
        {
            _container = container;
            _itemBtnUxml = itemBtnUxml;
            _listView = _container.Q<ListView>(LIST_VIEW_NAME);
            _onItemClicked = onItemClicked;
            SetupSearch();
        }

        public void Reload()
        {
            DrawList();
        }

        public void DrawList(List<SerializedObject> items = null, int itemHeight = LIST_VIEW_ITEM_HEIGHT)
        {
            if (items == null) items = InventorySystemAssetLoader.LoadItemsInProject();

            _listView.Clear();

            if (items.Count == 0) return;

            _listView.itemsSource = items;
            _listView.makeItem = () =>
            {
                VisualElement itemBtnUxml = _itemBtnUxml.CloneTree();
                VisualElement itemBtn = itemBtnUxml.Q<VisualElement>("button-item");
                return itemBtn;
            };

            _listView.bindItem = (element, index) =>
            {
                if (index >= items.Count || index < 0) return;

                SerializedObject item = items[index];

                if (item == null) return;

                Sprite itemSprite = item.FindProperty("_previewSprite").objectReferenceValue as Sprite;
                Color itemBackgroundColor = item.FindProperty("_backgroundColor").colorValue;
                Color tintColor = item.FindProperty("_color").colorValue;
                Color selectedColor = item.FindProperty("_selectedColor").colorValue;

                var btn = element as Button;

                var label = btn.Q<Label>("button-item-label-name");
                label.text = item.FindProperty("_name").stringValue;

                int tabConfigs = item.FindProperty("_tabConfigs").arraySize;
                var containerTabPill = btn.Q<VisualElement>("container-tab-pill");
                containerTabPill.Clear();

                for (int i = 0; i < tabConfigs; i++)
                {
                    InventoryTabConfigSO tab = item.FindProperty("_tabConfigs").GetArrayElementAtIndex(i).objectReferenceValue as InventoryTabConfigSO;
                    if (tab != null)
                    {
                        Pill pill = new Pill();
                        pill.labelText = tab.name;
                        containerTabPill?.Add(pill);
                    }
                }

                if (itemSprite != null)
                {
                    var img = btn.Q<VisualElement>(BTN_ITEM_TEXTURE);
                    img.style.backgroundImage = itemSprite.texture;
                    img.style.backgroundColor = itemBackgroundColor;
                    img.style.unityBackgroundImageTintColor = tintColor;
                    img.style.borderBottomColor = selectedColor;
                }

                btn.clicked += () => _onItemClicked.Invoke(item);
            };

            _listView.fixedItemHeight = itemHeight;
        }

        void SetupSearch()
        {
            var searchField = _container.Q<ToolbarSearchField>("searchfield-items");
            searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string search = evt.newValue;
                if (string.IsNullOrWhiteSpace(search))
                {
                    DrawList();
                    return;
                }

                List<SerializedObject> searchResults = new List<SerializedObject>();
                foreach (SerializedObject item in InventorySystemAssetLoader.LoadItemsInProject())
                {
                    if (item.FindProperty("_name").stringValue.ToLower().Contains(search.ToLower()))
                    {
                        searchResults.Add(item);
                    }
                }

                DrawList(searchResults);
            });
        }
    }
}