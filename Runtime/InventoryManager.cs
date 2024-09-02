using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] protected InventorySO _inventoryConfig;
        private RuntimeInventory _runtimeInventory;

        public InventorySO InventoryConfig => _inventoryConfig;
        public RuntimeInventory RuntimeInventory => _runtimeInventory;

        /// <summary>Fired when the CURRENT inventory sells an item</summary>
        public event UnityAction<InventoryUpdate> OnSell = delegate { };
        /// <summary>Fired when items are removed from the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnRemove = delegate { };
        /// <summary>Fired when items are added to the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnAdd = delegate { };
        /// <summary>Fired when the CURRENT inventory buys an item</summary>
        public event UnityAction<InventoryUpdate> OnBuy = delegate { };

        public UnityEvent<RuntimeInventory> OnInventoryChanged = new UnityEvent<RuntimeInventory>();

        private void Start()
        {
            // Initialize the runtime inventory with the configuration data
            _runtimeInventory = new RuntimeInventory(_inventoryConfig);
            OnInventoryChanged?.Invoke(_runtimeInventory);
        }

        /// <summary>
        /// Adds an item to a specified slot in a specified tab.
        /// </summary>
        public virtual InventoryUpdate AddItemToSlot(ItemTabTypeSO tabType, int slotIndex, ItemSO item, int amount)
        {
            _runtimeInventory.AddItemToSlot(tabType, slotIndex, item, amount);
            var slots = _runtimeInventory.GetSlotsForTab(tabType);
            var slot = slots != null && slotIndex >= 0 && slotIndex < slots.Count ? slots[slotIndex] : null;

            InventoryUpdate iu = new InventoryUpdate(slot, slotIndex, slot != null && !slot.IsEmpty, _inventoryConfig, InventoryUpdateType.Added);
            OnAdd.Invoke(iu);
            return iu;
        }

        /// <summary>
        /// Removes an item from a specified slot in a specified tab.
        /// </summary>
        public virtual InventoryUpdate RemoveItemFromSlot(ItemTabTypeSO tabType, int slotIndex, int amount)
        {
            _runtimeInventory.RemoveItemFromSlot(tabType, slotIndex, amount);
            var slots = _runtimeInventory.GetSlotsForTab(tabType);
            var slot = slots != null && slotIndex >= 0 && slotIndex < slots.Count ? slots[slotIndex] : null;

            InventoryUpdate iu = new InventoryUpdate(slot, slotIndex, slot != null && !slot.IsEmpty, _inventoryConfig, InventoryUpdateType.Removed);
            OnRemove.Invoke(iu);
            return iu;
        }

        /// <summary>
        /// Finds the first slot containing the specified item in the given tab.
        /// </summary>
        protected InventorySlot FindSlot(ItemTabTypeSO tabType, ItemSO item)
        {
            return _runtimeInventory.FindSlot(tabType, item);
        }

        /// <summary>
        /// Loads the inventory state from saved data.
        /// </summary>
        public void LoadInventory(List<ItemSO> allItems)
        {
            _runtimeInventory.LoadInventory(allItems);
        }

        /// <summary>
        /// Saves the current inventory state.
        /// </summary>
        public void SaveInventory()
        {
            _runtimeInventory.SaveInventory();
        }
    }

    public struct InventoryUpdate
    {
        /// <summary>The slot concerned by the update</summary>
        public InventorySlot Slot;

        /// <summary>The index of the slot that was updated</summary>
        public int SlotIndex;

        /// <summary>If the item is remaining in the inventory firing the event</summary>
        public bool Remaining;

        /// <summary>The current inventory configuration (InventorySO)</summary>
        public InventorySO Inventory;

        /// <summary>The type of update (e.g., Added, Removed, Sold, Bought)</summary>
        public InventoryUpdateType UpdateType;

        public InventoryUpdate(InventorySlot slot, int slotIndex, bool remaining, InventorySO inventory, InventoryUpdateType updateType)
        {
            this.Slot = slot;
            this.SlotIndex = slotIndex;
            this.Remaining = remaining;
            this.Inventory = inventory;
            this.UpdateType = updateType;
        }
    }
}
