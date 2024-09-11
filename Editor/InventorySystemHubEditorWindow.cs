using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Slax.InventorySystem.Runtime.Core;
using Slax.Utils.Editor;
using Slax.UIToolkit.Editor;

namespace Slax.InventorySystem.Editor
{
    public class InventorySystemHubEditorWindow : EditorWindow
    {
        [SerializeField] VisualTreeAsset _uxml = default;
        [SerializeField] VisualTreeAsset _itemBtnUxml = default;

        static InventorySystemHubEditorWindow _window;

        ItemCreationWizardEditor _itemCreationWizard;

        VisualElement _dashboardContainer;
        VisualElement _itemDetailContainer;
        VisualElement _inventoryDetailContainer;
        VisualElement _actionTypeDetailContainer;
        VisualElement _inventoryTabDetailContainer;
        VisualElement _slotUnlockDetailContainer;
        SerializedItemGallery _itemGallery;

        InventorySystemAssetLoader _assetLoader;
        SerializedItemList _serializedItemList;
        SerializedInventoryList _serializedInventoryList;
        SerializedActionTypeList _serializedActionsList;
        SerializedInventoryTabList _serializedInventoryTabList;

        SerializedObject _selectedItem; // The item being edited in the ItemDetail view.
        SerializedObject _selectedItemDatabase;
        SerializedObject _selectedInventory;
        SerializedObject _selectedActionType;
        SerializedObject _selectedInventoryTab;
        SerializedObject _selectedSlotUnlockState;

        [MenuItem("Window/Slax/Inventory/Inventory System Hub")]
        public static void ShowWindow()
        {
            _window = GetWindow<InventorySystemHubEditorWindow>();
            _window.titleContent = new GUIContent("Inventory System Hub");
            _window.Show();
        }

        void OnEnable()
        {
            _assetLoader = new InventorySystemAssetLoader(loadAssetsOnCreation: true);
        }

        public void CreateGUI()
        {
            VisualElement uxml = _uxml.CloneTree();
            rootVisualElement.Add(uxml);

            _itemCreationWizard = new ItemCreationWizardEditor(rootVisualElement);
            _itemCreationWizard.Hide();

            _dashboardContainer = rootVisualElement.Q<VisualElement>("container-dashboard");

            RegisterDetailContainers();

            var mainContent = rootVisualElement.Q<VisualElement>("main-content");

            _itemGallery = new SerializedItemGallery(_assetLoader.SerializedItems);
            _itemGallery.Hide();
            mainContent.Add(_itemGallery.GetContainer());

            var dashboardItems = rootVisualElement.Q<VisualElement>("dashboard-items");
            _serializedItemList = new SerializedItemList(dashboardItems, HandleItemClicked, _itemBtnUxml);

            _serializedInventoryList = new SerializedInventoryList(rootVisualElement.Q<VisualElement>("dashboard-inventories-list"), HandleInventoryEditClicked);

            var listViewActionTypes = rootVisualElement.Q<VisualElement>("dashboard-action-types-list");
            _serializedActionsList = new SerializedActionTypeList(listViewActionTypes, HandleActionEditClicked);

            _serializedInventoryTabList = new SerializedInventoryTabList(rootVisualElement.Q<VisualElement>("dashboard-inventory-tab-list"), HandleInventoryTabClicked);

            var serializedSlotUnlockList = new SerializedSlotUnlockList(rootVisualElement.Q<VisualElement>("dashboard-slot-unlock-list"), HandleSlotUnlockClicked);

            var navBtnMain = rootVisualElement.Q<Button>("nav-btn-main");
            navBtnMain.clicked += () => ShowMainView();
            var navBtnCreateItem = rootVisualElement.Q<Button>("nav-btn-create-item");
            navBtnCreateItem.clicked += () => ShowView(() => _itemCreationWizard.Show());

            var button = rootVisualElement.Q<Button>("refresh-btn");
            button.clicked += () =>
            {
                _assetLoader.Reload();
                _serializedItemList.Reload();
                _serializedInventoryList.Reload();
                _serializedActionsList.Reload();
                _serializedInventoryTabList.Reload();
                serializedSlotUnlockList.Reload();

                PopulateItemDatabaseListSneakPeak();
                ShowMainView();
            };

            _serializedItemList.DrawList(_assetLoader.SerializedItems);
            _serializedInventoryList.DrawList(_assetLoader.SerializedInventories);
            _serializedActionsList.DrawList(_assetLoader.SerializedItemActionTypes);
            _serializedInventoryTabList.DrawList(_assetLoader.SerializedInventoryTabs);
            serializedSlotUnlockList.DrawList(_assetLoader.SerializedSlotUnlockStates);
            PopulateItemDatabaseListSneakPeak();

            // TODO: tmp location of this, should be in its own method
            var allItemsBtn = rootVisualElement.Q<Button>("nav-btn-all-items");
            allItemsBtn.clicked += () => HandleItemGalleryClicked();
        }

        void RegisterDetailContainers()
        {
            _itemDetailContainer = rootVisualElement.Q<VisualElement>("container-item-details");
            _inventoryDetailContainer = rootVisualElement.Q<VisualElement>("container-inventory-details");
            _actionTypeDetailContainer = rootVisualElement.Q<VisualElement>("container-action-type-details");
            _inventoryTabDetailContainer = rootVisualElement.Q<VisualElement>("container-inventory-tab-details");
            _slotUnlockDetailContainer = rootVisualElement.Q<VisualElement>("container-slot-unlock-details");
        }

        void HideAll()
        {
            _dashboardContainer.style.display = DisplayStyle.None;
            _itemDetailContainer.style.display = DisplayStyle.None;
            _inventoryDetailContainer.style.display = DisplayStyle.None;
            _slotUnlockDetailContainer.style.display = DisplayStyle.None;
            _actionTypeDetailContainer.style.display = DisplayStyle.None;
            _inventoryTabDetailContainer.style.display = DisplayStyle.None;
        
            _itemGallery.Hide();
            _itemCreationWizard.Hide();
        }

        void ShowView(System.Action act)
        {
            HideAll();
            act();
        }

        void ShowMainView()
        {
            if (_dashboardContainer == null) return;
            ShowView(() => _dashboardContainer.style.display = DisplayStyle.Flex);
        }

        // TODO move out
        void PopulateItemDatabaseListSneakPeak()
        {
            var itemDatabaseList = rootVisualElement.Q<VisualElement>("dashboard-databases-list");

            itemDatabaseList.Clear();

            if (_assetLoader.SerializedItemDatabases.Count == 0) return;

            for (int i = 0; i < _assetLoader.SerializedItemDatabases.Count; i++)
            {
                var db = _assetLoader.SerializedItemDatabases[i];
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

        void ShowItemDatabaseDetailView()
        {
            HideAll();
            _itemDetailContainer.style.display = DisplayStyle.Flex;
            RenderItemDatabaseDetailView();
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
                _assetLoader.ReloadItemDatabases();
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
                _assetLoader.ReloadItems();
                _serializedItemList.Reload();
                ShowMainView();
            }

            _itemDetailContainer.Add(new InspectorElement(_selectedItem));
            var dialogBtn = CustomEditorUtility.MakeButtonWithDialog("Delete Item", "Item Deletion", "Are you sure you want to delete this item ?", "Confirm", "Cancel", OnDeleteConfirm);
            _itemDetailContainer.Add(dialogBtn);
        }

        void HandleItemGalleryClicked()
        {
            HideAll();
            _itemGallery.DrawWithNewItems(_assetLoader.ReloadItems(), (serializedItem) =>
            {
                HandleItemClicked(serializedItem);
            });
        }

        void HandleItemClicked(SerializedObject item)
        {
            _selectedItem = item;
            if (_itemDetailContainer == null) return;

            ShowView(() =>
            {
                _itemDetailContainer.style.display = DisplayStyle.Flex;
                RenderItemDetailView();
            });
        }

        void HandleInventoryEditClicked(SerializedObject inventory)
        {
            _selectedInventory = inventory;
            if (_inventoryDetailContainer == null) return;

            ShowView(() =>
            {
                _inventoryDetailContainer.style.display = DisplayStyle.Flex;
                _inventoryDetailContainer.Clear();
                _inventoryDetailContainer.Add(new InspectorElement(_selectedInventory));
            });
        }

        void HandleActionEditClicked(SerializedObject act)
        {
            _selectedActionType = act;
            if (_selectedActionType == null) return;
            ShowView(() =>
            {
                _actionTypeDetailContainer.style.display = DisplayStyle.Flex;
                _actionTypeDetailContainer.Clear();
                _actionTypeDetailContainer.Add(new InspectorElement(_selectedActionType));
            });
        }

        void HandleInventoryTabClicked(SerializedObject tab)
        {
            _selectedInventoryTab = tab;
            if (_selectedInventoryTab == null) return;
            ShowView(() =>
            {
                _inventoryTabDetailContainer.style.display = DisplayStyle.Flex;
                _inventoryTabDetailContainer.Clear();
                _inventoryTabDetailContainer.Add(new InspectorElement(_selectedInventoryTab));
            });
        }

        void HandleSlotUnlockClicked(SerializedObject slotUnlock)
        {
            _selectedSlotUnlockState = slotUnlock;
            if (_selectedSlotUnlockState == null) return;
            ShowView(() =>
            {
                _slotUnlockDetailContainer.style.display = DisplayStyle.Flex;
                _slotUnlockDetailContainer.Clear();
                _slotUnlockDetailContainer.Add(new InspectorElement(_selectedSlotUnlockState));
            });
        }
    }
}