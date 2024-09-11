using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slax.InventorySystem.Runtime.Core;

namespace Slax.InventorySystem.Runtime.UI
{
    public class UIInventoryTab : MonoBehaviour
    {
        public InventoryTabConfigSO Config;
        public Button Tab;
        public Image SpriteImage;
        public Image BackgroundImage;
        public TextMeshProUGUI TabNameText;
        public GridLayoutGroup ItemSlotContainer;

        protected List<UIInventoryItemSlot> _slots = new List<UIInventoryItemSlot>();

        protected RuntimeInventory _inventory = null;

        protected InventorySlot _baseSlot = null;
        protected InventorySlot _targetSlot = null;

        void OnEnable()
        {
            if (Config == null) return;
            SpriteImage.sprite = Config.Icon;
            SpriteImage.color = Config.IconColor;
            BackgroundImage.color = Config.BackgroundColorInactive;
            TabNameText.text = Config.Name;
        }

        /// <summary>
        /// Draw items matching the tab's type.
        /// </summary>
        public void Draw(RuntimeInventory runtimeInventory, UIInventoryItemSlot prefab)
        {
            _inventory = runtimeInventory;
            BackgroundImage.color = Config.BackgroundColorActive;

            Clear();

            // Get the slots associated with the current tab type
            List<InventorySlot> slots = runtimeInventory.GetSlotsForTab(Config);

            foreach (var slot in slots)
            {
                var s = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                s.Init(slot);
                s.OnSelected += HandleSelectSlot;
                s.transform.SetParent(ItemSlotContainer.transform, false);
                _slots.Add(s);
            }
        }

        public void Unselect()
        {
            BackgroundImage.color = Config.BackgroundColorInactive;
            Clear();
        }

        /// <summary>
        /// Clear the current UI tab.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].OnSelected -= HandleSelectSlot;
                Destroy(_slots[i].gameObject);
            }
            _slots.Clear();
        }

        protected void HandleSelectSlot(InventorySlot slot)
        {
            if (_baseSlot == null)
            {
                _baseSlot = slot;
            }
            else
            {
                _targetSlot = slot;
                SwitchSlots();
            }
        }

        protected void SwitchSlots()
        {
            if (_baseSlot == null || _targetSlot == null) return;

            _inventory.SwitchSlots(Config, _baseSlot, _targetSlot);
            _baseSlot = null;
            _targetSlot = null;
        }
    }
}
