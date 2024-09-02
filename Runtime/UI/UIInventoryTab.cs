using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slax.Inventory
{
    public class UIInventoryTab : MonoBehaviour
    {
        public InventoryTabConfigSO Config;
        public Button Tab;
        public Image Image;
        public GridLayoutGroup ItemSlotContainer;

        protected List<UIInventoryItemSlot> _slots = new List<UIInventoryItemSlot>();

        void OnEnable()
        {
            if (Config == null) return;
            Image.sprite = Config.TabType.Icon;
            Image.color = Config.TabType.Color;
        }

        /// <summary>
        /// Draw items matching the tab's type.
        /// </summary>
        public void Draw(RuntimeInventory runtimeInventory, UIInventoryItemSlot prefab)
        {
            Clear();

            // Get the slots associated with the current tab type
            List<InventorySlot> slots = runtimeInventory.GetSlotsForTab(Config.TabType);

            foreach (var slot in slots)
            {
                var s = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                s.Init(slot);
                s.transform.SetParent(ItemSlotContainer.transform, false);
                _slots.Add(s);
            }
        }

        /// <summary>
        /// Clear the current UI tab.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                Destroy(_slots[i].gameObject);
            }
            _slots.Clear();
        }
    }
}
