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

        protected UIInventoryTab _activeTab;
        protected RuntimeInventory _inventory = null;

        protected void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RedrawInventory;
            }
        }

        public void Init(RuntimeInventory inventory)
        {
            _inventory = inventory;
            _inventory.OnInventoryChanged += RedrawInventory;
            _activeTab = _uiTabs.Count > 0 ? _uiTabs[0] : null;

            foreach (var tab in _uiTabs)
            {
                tab.Tab.onClick.AddListener(() => {
                    ChangeTab(tab);
                });
            }

            DrawActiveTab();
        }

        public void ChangeTab(UIInventoryTab tab)
        {
            if (_activeTab != null)
            {
                _activeTab.Clear();
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

        public void Dlg()
        {
            Debug.Log("Clcked");
        }
    }
}
