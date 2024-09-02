using System.Collections.Generic;
using UnityEngine;

namespace Slax.Inventory
{
    public class UIInventoryTabManager : MonoBehaviour
    {
        [SerializeField] protected List<UIInventoryTab> _tabs = new List<UIInventoryTab>();

        [Header("Settings")]
        [SerializeField] protected Color _inactive;
        [SerializeField] protected Color _active;

        void OnEnable()
        {
            if (_tabs.Count == 0) return;

            SetAllTabsInactive();

            if (_tabs.Count > 0)
            {
                SetActiveTab(0);
            }
        }

        public void DrawTab(int tabIndex, RuntimeInventory inventory, UIInventoryItemSlot prefab)
        {
            if (tabIndex < 0 || tabIndex >= _tabs.Count) return;

            SetAllTabsInactive();
            SetActiveTab(tabIndex);

            UIInventoryTab activeTab = _tabs[tabIndex];
            activeTab.Draw(inventory, prefab);
        }

        private void SetAllTabsInactive()
        {
            foreach (UIInventoryTab t in _tabs)
            {
                t.Tab.image.color = _inactive;
                t.gameObject.SetActive(false);
            }
        }

        private void SetActiveTab(int tabIndex)
        {
            _tabs[tabIndex].gameObject.SetActive(true);
            _tabs[tabIndex].Tab.image.color = _active;
        }
    }
}
