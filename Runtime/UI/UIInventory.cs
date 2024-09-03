using System.Collections.Generic;
using UnityEngine;

namespace Slax.Inventory
{
    public class UIInventory : MonoBehaviour
    {
        [SerializeField] protected List<UIInventoryTab> _uiTabs = new List<UIInventoryTab>();
        [SerializeField] protected UIInventoryItemSlot _itemSlotPrefab;

        [Header("Tab Settings")]
        [SerializeField] protected Color _inactiveTabColor;
        [SerializeField] protected Color _activeTabColor;

        [Header("Weight Info")]
        [SerializeField] protected UIInventoryWeight _uiInventoryWeight;

        protected UIInventoryTab _activeTab;
        protected RuntimeInventory _inventory = null;

        protected InventorySlot _baseSlot = null;
        protected InventorySlot _targetSlot = null;

        protected void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RedrawInventory;
            }
        }

        protected void OnEnable()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged += RedrawInventory;
            }

            if (_activeTab != null)
            {
                _activeTab.Unselect();
            }
            else
            {
                _activeTab = _uiTabs.Count > 0 ? _uiTabs[0] : null;
            }
            DrawActiveTab();
        }

        public void Init(RuntimeInventory inventory)
        {
            _inventory = inventory;
            _inventory.OnInventoryChanged += RedrawInventory;
            _activeTab = _uiTabs.Count > 0 ? _uiTabs[0] : null;

            if (_inventory.InventoryConfig.UseWeight)
            {
                _uiInventoryWeight.Init(_inventory);
            }
            else
            {
                _uiInventoryWeight.gameObject.SetActive(false);
            }

            foreach (var tab in _uiTabs)
            {
                tab.Tab.onClick.AddListener(() =>
                {
                    ChangeTab(tab);
                });
            }

            DrawActiveTab();
        }

        public void ChangeTab(UIInventoryTab tab)
        {
            if (_activeTab != null)
            {
                _activeTab.Unselect();
            }
            _activeTab = tab;
            DrawActiveTab();
        }

        protected void RedrawInventory(RuntimeInventory inventory)
        {
            DrawActiveTab();
        }

        protected void DrawActiveTab()
        {
            if (_activeTab == null || _inventory == null) return;

            _activeTab.Draw(_inventory, _itemSlotPrefab);
        }
    }
}
