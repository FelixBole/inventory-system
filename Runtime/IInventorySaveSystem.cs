namespace Slax.Inventory
{
    public interface IInventorySaveSystem
    {
        void SaveInventory(SerializedInventory inventoryData, string inventoryName);
        SerializedInventory LoadInventory(string inventoryName);
    }

}