using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] protected InventorySO _inventoryConfig;
        public InventorySO InventoryConfig => _inventoryConfig;

        [SerializeField] protected InventorySaveSystemSO _saveSystem;
        public InventorySaveSystemSO SaveSystem => _saveSystem;

        protected RuntimeInventory _runtimeInventory;
        public RuntimeInventory RuntimeInventory => _runtimeInventory;

        /// <summary>Fired when the CURRENT inventory sells an item</summary>
        public event UnityAction<InventoryUpdate> OnSell = delegate { };
        /// <summary>Fired when items are removed from the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnRemove = delegate { };
        /// <summary>Fired when items are added to the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnAdd = delegate { };
        /// <summary>Fired when the CURRENT inventory buys an item</summary>
        public event UnityAction<InventoryUpdate> OnBuy = delegate { };

        public UnityAction<RuntimeInventory> OnInventoryLoaded = delegate { };
        public UnityEvent<RuntimeInventory> OnInventoryChanged = new UnityEvent<RuntimeInventory>();

        void OnDisable()
        {
            if (_runtimeInventory != null)
            {
                _runtimeInventory.Cleanup();
            }
        }

        private void Start()
        {
            // Initialize the runtime inventory with the configuration data
            _runtimeInventory = new RuntimeInventory(_inventoryConfig);
            if (_saveSystem != null) _runtimeInventory.SetSaveSystem(_saveSystem);
            OnInventoryLoaded?.Invoke(_runtimeInventory);
            OnInventoryChanged?.Invoke(_runtimeInventory);
        }

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        public virtual InventoryUpdate AddItem(ItemSO item, int amount)
        {
            var iu = _runtimeInventory.AddItem(item, amount);
            OnAdd.Invoke(iu);
            return iu;
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        public virtual InventoryUpdate RemoveItem(ItemSO item, int amount)
        {
            var iu = _runtimeInventory.RemoveItem(item, amount);
            OnRemove.Invoke(iu);
            return iu;
        }

        /// <summary>
        /// Finds the first slot containing the specified item in the given tab.
        /// </summary>
        protected InventorySlot FindSlotWithItem(InventoryTabConfigSO tabConfig, ItemSO item)
        {
            return _runtimeInventory.FindSlotWithItem(tabConfig, item);
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
}
