using UnityEngine;

namespace Slax.Inventory
{
    [CreateAssetMenu(fileName = "DemoCustomInventorySaveSystem", menuName = "Slax/Inventory/Demo/Custom Inventory Save System")]
    public class DemoCustomInventorySaveSystem : InventorySaveSystemSO
    {
        public override void SaveInventory(SerializedInventory inventoryData, string inventoryName)
        {
            Debug.Log("Saving inventory with demo custom inventory save system: " + inventoryName);
        }

        public override SerializedInventory LoadInventory(string inventoryName)
        {
            Debug.Log("Loading inventory with demo custom inventory save system: " + inventoryName);
            return null;
        }
    }
}
