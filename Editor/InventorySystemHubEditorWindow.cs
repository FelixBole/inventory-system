using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Slax.InventorySystem.Runtime.Core;
using Slax.Utils.Editor;
using Slax.UIToolkit.Editor;

namespace Slax.InventorySystem.Editor
{
    public class InventorySystemHubEditorWindow : EditorWindow
    {
        [SerializeField] VisualTreeAsset _uxml = default;
        [SerializeField] VisualTreeAsset _itemBtnUxml = default;
        [SerializeField] VisualTreeAsset _inventoryListItemUxml = default;
        [SerializeField] VisualTreeAsset _actionTypeListItemUxml = default;

        static InventorySystemHubEditorWindow _window;

        ItemCreationWizardEditor _itemCreationWizard;

        ListView _itemListSneakPeak;
        VisualElement _dashboardContainer;
        VisualElement _itemDetailContainer;
        VisualElement _allItemsView;

        #region Serialized Assets
        protected SerializedObject _selectedItem; // The item being edited in the ItemDetail view.
        protected SerializedObject _selectedItemDatabase;
        protected List<SerializedObject> _serializedInventories = new List<SerializedObject>();
        protected List<SerializedObject> _serializedItems = new List<SerializedObject>();
        protected List<SerializedObject> _serializedInventoryTabs = new List<SerializedObject>();
        protected List<SerializedObject> _serializedItemActionTypes = new List<SerializedObject>();
        protected List<SerializedObject> _serializedItemDatabases = new List<SerializedObject>();
        protected List<SerializedObject> _serializedInventorySaveSystems = new List<SerializedObject>();
        protected List<SerializedObject> _serializedSlotUnlockStates = new List<SerializedObject>();
        #endregion

        [MenuItem("Window/Slax/Inventory/Inventory System Hub")]
        public static void ShowWindow()
        {
            _window = GetWindow<InventorySystemHubEditorWindow>();
            _window.titleContent = new GUIContent("Inventory System Hub");
            _window.Show();
        }

        public void CreateGUI()
        {
            LoadAssets();

            VisualElement uxml = _uxml.CloneTree();
            rootVisualElement.Add(uxml);

            _itemCreationWizard = new ItemCreationWizardEditor();
            _itemCreationWizard.Setup(rootVisualElement);
            _itemCreationWizard.Hide();

            SetupItemSearch();

            _itemDetailContainer = rootVisualElement.Q<VisualElement>("container-item-details");
            _dashboardContainer = rootVisualElement.Q<VisualElement>("container-dashboard");
            _allItemsView = rootVisualElement.Q<VisualElement>("container-all-items");

            _itemListSneakPeak = rootVisualElement.Q<ListView>("item-list-sneakpeak");

            var navBtnMain = rootVisualElement.Q<Button>("nav-btn-main");
            navBtnMain.clicked += () => ShowMainView();
            var navBtnCreateItem = rootVisualElement.Q<Button>("nav-btn-create-item");
            navBtnCreateItem.clicked += () => ToggleItemCreationView();

            var button = rootVisualElement.Q<Button>("refresh-btn");
            button.clicked += () =>
            {
                LoadAssets();
                PopulateItemListSneakPeak();
                PopulateItemDatabaseListSneakPeak();
                PopulateInventoryList();
                PopulateActionTypes();
                ShowMainView();
            };

            PopulateInventoryList();
            PopulateActionTypes();
            PopulateItemListSneakPeak();
            PopulateItemDatabaseListSneakPeak();

            // TODO: tmp location of this, should be in its own method
            var allItemsBtn = rootVisualElement.Q<Button>("nav-btn-all-items");
            allItemsBtn.clicked += () => RenderAllItemsView();
        }

        void HideAll()
        {
            _dashboardContainer.style.display = DisplayStyle.None;
            _itemDetailContainer.style.display = DisplayStyle.None;
            _allItemsView.style.display = DisplayStyle.None;
            _itemCreationWizard.Hide();
        }

        void ShowMainView()
        {
            if (_dashboardContainer == null) return;
            HideAll();
            _dashboardContainer.style.display = DisplayStyle.Flex;
        }

        void ShowItemDetailView()
        {
            if (_itemDetailContainer == null) return;
            HideAll();
            _itemDetailContainer.style.display = DisplayStyle.Flex;
            RenderItemDetailView();
        }

        void SetupItemSearch()
        {
            var searchField = rootVisualElement.Q<ToolbarSearchField>("searchfield-items");
            searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string search = evt.newValue;
                if (string.IsNullOrEmpty(search))
                {
                    PopulateItemListSneakPeak();
                    return;
                }

                List<SerializedObject> searchResults = new List<SerializedObject>();
                foreach (SerializedObject item in _serializedItems)
                {
                    if (item.FindProperty("_name").stringValue.ToLower().Contains(search.ToLower()))
                    {
                        searchResults.Add(item);
                    }
                }

                PopulateItemListSneakPeak(searchResults);
            });
        }

        void ToggleItemCreationView()
        {
            HideAll();
            _itemCreationWizard.Show();
        }

        void PopulateItemListSneakPeak(List<SerializedObject> items = null)
        {
            _itemListSneakPeak.Clear();

            if (items == null)
            {
                items = _serializedItems;
            }

            if (items.Count == 0) return;

            // Set the number of items in the list
            _itemListSneakPeak.itemsSource = items;

            // Define the item creation function
            _itemListSneakPeak.makeItem = () =>
            {
                VisualElement itemBtnUxml = _itemBtnUxml.CloneTree();
                VisualElement itemBtn = itemBtnUxml.Q<VisualElement>("button-item");
                return itemBtn;
            };

            // Define the data binding function
            _itemListSneakPeak.bindItem = (element, index) =>
            {
                if (index >= items.Count || index < 0) return;

                SerializedObject item = items[index];

                if (item == null) return;

                Sprite itemSprite = item.FindProperty("_previewSprite").objectReferenceValue as Sprite;
                Color itemBackgroundColor = item.FindProperty("_backgroundColor").colorValue;

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
                    var img = btn.Q<VisualElement>("button-item-texture");
                    img.style.backgroundImage = itemSprite.texture;
                    img.style.backgroundColor = itemBackgroundColor;
                }

                // Set the click event
                btn.clicked += () =>
                {
                    _selectedItem = item;
                    ShowItemDetailView();
                };
            };

            // Optional: Set item height if necessary
            _itemListSneakPeak.fixedItemHeight = 50;
        }

        void PopulateItemDatabaseListSneakPeak()
        {
            var itemDatabaseList = rootVisualElement.Q<VisualElement>("dashboard-databases-list");

            itemDatabaseList.Clear();

            if (_serializedItemDatabases.Count == 0)
            {
                itemDatabaseList.Add(AlertBox.Warning("No Item Databases found in the project."));
                return;
            }

            for (int i = 0; i < _serializedItemDatabases.Count; i++)
            {
                var db = _serializedItemDatabases[i];
                var dbContainer = new ContentInfoButton();
                dbContainer.titleText = db.targetObject.name;
                dbContainer.infoText = db.FindProperty("_items").arraySize.ToString() + " items";
                dbContainer.clicked += () =>
                {
                    _selectedItemDatabase = db;
                    ShowItemDatabaseDetailView();	
                };
                itemDatabaseList.Add(dbContainer);
            }
        }

        void PopulateInventoryList()
        {
            ListView inventoryList = rootVisualElement.Q<ListView>("list-view-dashboard-inventories");

            inventoryList.Clear();

            if (_serializedInventories.Count == 0)
            {
                inventoryList.Add(AlertBox.Warning("No Inventories found in the project."));
                return;
            }

            inventoryList.itemsSource = _serializedInventories;

            Debug.Log(_inventoryListItemUxml);

            inventoryList.makeItem = () =>
            {
                VisualElement itemBtnUxml = _inventoryListItemUxml.CloneTree();
                return itemBtnUxml;
            };

            inventoryList.bindItem = (element, index) =>
            {
                if (index >= _serializedInventories.Count || index < 0) return;

                SerializedObject inventory = _serializedInventories[index];

                if (inventory == null) return;

                InventorySO inv = inventory.targetObject as InventorySO;

                if (inv == null) return;

                var title = element.Q<Label>("InventoryListElement__title");
                title.text = inv.Name;

                var tabs = element.Q<Label>("InventoryListElement__tabs");
                tabs.text = inv.TabConfigs.Count + " tabs";

                var editBtn = element.Q<Button>("InventoryListElement__controls-edit");
                editBtn.clicked += () => {
                    Debug.Log("Edit Inventory: " + inv.Name);
                };

                var deleteBtn = element.Q<Button>("InventoryListElement__controls-delete");
                deleteBtn.clicked += () => {
                    Debug.Log("Delete Inventory: " + inv.Name);
                };
            };

            inventoryList.fixedItemHeight = 60;
        }

        void PopulateActionTypes()
        {
            ListView actionTypesList = rootVisualElement.Q<ListView>("list-view-dashboard-action-types");

            actionTypesList.Clear();

            if (_serializedItemActionTypes.Count == 0)
            {
                actionTypesList.Add(AlertBox.Warning("No Item Action Types found in the project."));
                return;
            }

            actionTypesList.itemsSource = _serializedItemActionTypes;

            actionTypesList.makeItem = () => _actionTypeListItemUxml.CloneTree();
            

            actionTypesList.bindItem = (element, index) =>
            {
                if (index >= _serializedItemActionTypes.Count || index < 0) return;

                SerializedObject actionType = _serializedItemActionTypes[index];

                if (actionType == null) return;

                ItemActionTypeSO type = actionType.targetObject as ItemActionTypeSO;

                if (type == null) return;

                var title = element.Q<Label>("ActionTypeListItem__title");
                title.text = type.Name;

                var deleteBtn = element.Q<Button>("ActionTypeListItem__delete");
                deleteBtn.clicked += () => {
                    Debug.Log("Delete Action Type: " + type.Name);
                };
            };

            actionTypesList.fixedItemHeight = 35;
        }

        void ShowItemDatabaseDetailView()
        {
            HideAll();
            _itemDetailContainer.style.display = DisplayStyle.Flex;
            RenderItemDatabaseDetailView();
        }

        // TODO Put in its own class
        void RenderAllItemsView()
        {
            LoadAssets();
            HideAll();
            _allItemsView.Clear();
            _allItemsView.style.display = DisplayStyle.Flex;
            _allItemsView.style.flexDirection = FlexDirection.Row;
            _allItemsView.style.flexWrap = Wrap.Wrap;

            // Create image grid of all items
            foreach (SerializedObject item in _serializedItems)
            {
                if (item == null) continue;

                Sprite itemSprite = item.FindProperty("_previewSprite").objectReferenceValue as Sprite;
                Color itemBackgroundColor = item.FindProperty("_backgroundColor").colorValue;
                Color selectedColor = item.FindProperty("_selectedColor").colorValue;
                Color tintColor = item.FindProperty("_color").colorValue;

                var ve = new VisualElement();
                ve.style.width = 50;
                ve.style.height = 50;
                ve.style.borderBottomColor = selectedColor;
                ve.style.borderBottomWidth = 2;
                ve.style.backgroundColor = itemBackgroundColor;
                ve.style.unityBackgroundImageTintColor = tintColor;
                ve.style.backgroundImage = itemSprite.texture;

                _allItemsView.Add(ve);
            }
        }

        void RenderItemDatabaseDetailView()
        {
            _itemDetailContainer.Clear();
            if (_selectedItemDatabase == null)
            {
                return;
            }

            void OnDeleteConfirm()
            {
                ItemDatabaseSO selected = _selectedItemDatabase.targetObject as ItemDatabaseSO;
                if (selected == null) return;
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selected));
                _selectedItemDatabase = null;
                LoadAssets();
                PopulateItemDatabaseListSneakPeak();
                ShowMainView();
            }

            _itemDetailContainer.Add(new InspectorElement(_selectedItemDatabase));
            var dialogBtn = CustomEditorUtility.MakeButtonWithDialog("Delete Database", "Database Deletion", "Are you sure you want to delete this database ?", "Confirm", "Cancel", OnDeleteConfirm);
            _itemDetailContainer.Add(dialogBtn);
        }

        void RenderItemDetailView()
        {
            _itemDetailContainer.Clear();
            if (_selectedItem == null)
            {
                return;
            }

            void OnDeleteConfirm()
            {
                ItemSO selected = _selectedItem.targetObject as ItemSO;
                if (selected == null) return;
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selected));
                _selectedItem = null;
                LoadAssets();
                PopulateItemListSneakPeak();
                ShowMainView();
            }

            _itemDetailContainer.Add(new InspectorElement(_selectedItem));
            var dialogBtn = CustomEditorUtility.MakeButtonWithDialog("Delete Item", "Item Deletion", "Are you sure you want to delete this item ?", "Confirm", "Cancel", OnDeleteConfirm);
            _itemDetailContainer.Add(dialogBtn);
        }

        /// **********************************************************************************************
        /// *                                      ASSET LOADING                                         *
        /// **********************************************************************************************


        #region Load Assets
        /// <summary>
        /// Loads all Inventory Related Assets in the project.
        /// </summary>
        protected void LoadAssets()
        {
            _serializedInventories = GetInventoriesInProject();
            _serializedItems = GetItemsInProject();
            _serializedInventoryTabs = GetInventoryTabsInProject();
            _serializedItemActionTypes = GetItemActionTypesInProject();
            _serializedItemDatabases = GetItemDatabasesInProject();
            _serializedInventorySaveSystems = GetInventorySaveSystemsInProject();
            _serializedSlotUnlockStates = GetSlotUnlockStatesInProject();
        }

        /// <summary>
        /// Returns a list of all InventorySO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetInventoriesInProject()
        {
            List<SerializedObject> inventories = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:InventorySO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventorySO inventory = AssetDatabase.LoadAssetAtPath<InventorySO>(path);
                inventories.Add(new SerializedObject(inventory));
            }

            return inventories;
        }

        /// <summary>
        /// Returns a list of all ItemSO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetItemsInProject()
        {
            List<SerializedObject> items = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:ItemSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
                items.Add(new SerializedObject(item));
            }

            return items;
        }

        /// <summary>
        /// Returns a list of all InventoryTabConfigSO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetInventoryTabsInProject()
        {
            List<SerializedObject> tabs = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:InventoryTabConfigSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventoryTabConfigSO tab = AssetDatabase.LoadAssetAtPath<InventoryTabConfigSO>(path);
                tabs.Add(new SerializedObject(tab));
            }

            return tabs;
        }

        /// <summary>
        /// Returns a list of all ItemActionTypeSO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetItemActionTypesInProject()
        {
            List<SerializedObject> actionTypes = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:ItemActionTypeSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemActionTypeSO actionType = AssetDatabase.LoadAssetAtPath<ItemActionTypeSO>(path);
                actionTypes.Add(new SerializedObject(actionType));
            }

            return actionTypes;
        }

        /// <summary>
        /// Returns a list of all ItemDatabaseSO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetItemDatabasesInProject()
        {
            List<SerializedObject> databases = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:ItemDatabaseSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemDatabaseSO database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(path);
                databases.Add(new SerializedObject(database));
            }

            return databases;
        }

        /// <summary>
        /// Returns a list of all InventorySaveSystemSO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetInventorySaveSystemsInProject()
        {
            List<SerializedObject> saveSystems = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:InventorySaveSystemSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventorySaveSystemSO saveSystem = AssetDatabase.LoadAssetAtPath<InventorySaveSystemSO>(path);
                saveSystems.Add(new SerializedObject(saveSystem));
            }

            return saveSystems;
        }

        /// <summary>
        /// Returns a list of all SlotUnlockStateSO assets in the project.
        /// </summary>
        protected List<SerializedObject> GetSlotUnlockStatesInProject()
        {
            List<SerializedObject> states = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:SlotUnlockStateSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SlotUnlockStateSO state = AssetDatabase.LoadAssetAtPath<SlotUnlockStateSO>(path);
                states.Add(new SerializedObject(state));
            }

            return states;
        }
        #endregion
    }
}