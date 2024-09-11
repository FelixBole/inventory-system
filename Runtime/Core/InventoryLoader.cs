using UnityEngine;

namespace Slax.InventorySystem.Runtime.Core
{
    /// <summary>
    /// A simple loader that loads the inventory on start.
    /// </summary>
    public class InventoryLoader : MonoBehaviour
    {
        public InventoryManager InventoryManager;
        public ItemDatabaseSO ItemDatabase;

        void OnEnable()
        {
            InventoryManager.OnInventoryLoaded += LoadInventory;
        }

        void OnDisable()
        {
            InventoryManager.OnInventoryLoaded -= LoadInventory;
        }

        void LoadInventory(RuntimeInventory inventory)
        {
            inventory.LoadInventory(ItemDatabase.GetItemsList());
        }
    }
}