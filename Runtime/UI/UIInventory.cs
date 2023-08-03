using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slax.Inventory
{
    public class UIInventory : MonoBehaviour
    {
        [SerializeField] private InventorySO _inventory;
        [SerializeField] private List<UIInventoryTab> _tabs = new List<UIInventoryTab>();
        [SerializeField] private UIInventoryItemStack _itemStackPrefab;

        [Header("Tab Settings")]
        [SerializeField] private Color _inactiveTabColor;
        [SerializeField] private Color _activeTabColor;

        private UIInventoryTab _activeTab;

        void OnEnable()
        {
            if (_tabs.Count == 0) return;

            Reset();

            _activeTab = _tabs[0];
            DrawActiveTab();
        }

        public void ChangeTab(UIInventoryTab tab)
        {
            _activeTab = tab;
            Reset();
            DrawActiveTab();
        }

        private void DrawActiveTab()
        {
            _activeTab.gameObject.SetActive(true);
            _activeTab.Tab.image.color = _activeTabColor;

            if (_activeTab.Type == ItemTabType.MIXED)
            {
                _activeTab.DrawAll(_inventory, _itemStackPrefab);
            }
            else
            {
                _activeTab.Draw(_inventory, _itemStackPrefab);
            }

        }

        private void Reset()
        {
            foreach (UIInventoryTab t in _tabs)
            {
                t.Tab.image.color = _inactiveTabColor;
                t.gameObject.SetActive(false);
            }
        }
    }
}
