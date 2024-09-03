using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Slax.Inventory
{
    /// <summary>
    /// Manages multiple inventories.
    /// Kept for reference but not really useful. It's better to use individual
    /// inventory managers for each inventory.
    /// </summary>
    public class MultiInventoryManager : MonoBehaviour
    {
        [SerializeField] protected List<InventorySO> _inventoryConfigs = new List<InventorySO>();
        protected List<RuntimeInventory> _runtimeInventories = new List<RuntimeInventory>();

        [SerializeField] protected InventorySaveSystemSO _saveSystem;

        public UnityAction<List<RuntimeInventory>> OnAllInventoriesLoaded = delegate { };

        protected void Start()
        {
            // Initialize runtime inventories based on the configurations
            foreach (var inventoryConfig in _inventoryConfigs)
            {
                _runtimeInventories.Add(new RuntimeInventory(inventoryConfig));
            }
        }

        /// <summary>
        /// Sets up the save system to be used by all inventories.
        /// If this method is not called, or if null is passed, the default
        /// save system will be used.
        /// </summary>
        public void SetSaveSystem(InventorySaveSystemSO saveSystem)
        {
            _saveSystem = saveSystem;

            // Apply the save system to all runtime inventories
            foreach (var runtimeInventory in _runtimeInventories)
            {
                runtimeInventory.SetSaveSystem(_saveSystem);
            }
        }

        /// <summary>
        /// Saves all inventories using the configured save system.
        /// </summary>
        public void SaveAllInventories()
        {
            foreach (var runtimeInventory in _runtimeInventories)
            {
                runtimeInventory.SaveInventory();
            }
        }

        /// <summary>
        /// Loads all inventories using the configured save system.
        /// </summary>
        public void LoadAllInventories(List<ItemSO> allItems)
        {
            foreach (var runtimeInventory in _runtimeInventories)
            {
                runtimeInventory.LoadInventory(allItems);
            }
            OnAllInventoriesLoaded?.Invoke(_runtimeInventories);
        }

        /// <summary>
        /// Gets the runtime inventory for a specific configuration.
        /// </summary>
        public RuntimeInventory GetRuntimeInventory(InventorySO inventoryConfig)
        {
            int index = _inventoryConfigs.IndexOf(inventoryConfig);
            return index >= 0 ? _runtimeInventories[index] : null;
        }
    }
}
