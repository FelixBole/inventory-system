using UnityEngine;
using System.Collections.Generic;

namespace Slax.Inventory
{
    public class MultiInventoryManager : MonoBehaviour
    {
        [SerializeField] protected List<InventorySO> _inventoryConfigs = new List<InventorySO>();
        protected List<RuntimeInventory> _runtimeInventories = new List<RuntimeInventory>();

        protected IInventorySaveSystem _saveSystem;

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
        public void SetSaveSystem(IInventorySaveSystem saveSystem)
        {
            _saveSystem = saveSystem ?? new DefaultInventorySaveSystem();

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
