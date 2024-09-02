using System.IO;
using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    /// The default class provided by the Inventory System to save and load the Inventory data.
    /// This class is not mandatory and can be replaced by a custom implementation.
    /// </summary>
    public class DefaultInventorySaveSystem : IInventorySaveSystem
    {
        private string GetSavePath(string inventoryName)
        {
            return Path.Combine(Application.persistentDataPath, inventoryName + ".json");
        }

        public void SaveInventory(SerializedInventory inventoryData, string inventoryName)
        {
            string savePath = GetSavePath(inventoryName);
            string json = JsonUtility.ToJson(inventoryData, true);
            File.WriteAllText(savePath, json);
        }

        public SerializedInventory LoadInventory(string inventoryName)
        {
            string savePath = GetSavePath(inventoryName);
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<SerializedInventory>(json);
            }
            return null;
        }
    }
}
