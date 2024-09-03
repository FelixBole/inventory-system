using UnityEngine;

namespace Slax.Inventory
{
    public abstract class InventorySaveSystemSO : ScriptableObject
    {
        public abstract void SaveInventory(SerializedInventory inventoryData, string inventoryName);
        public abstract SerializedInventory LoadInventory(string inventoryName);
    }
}