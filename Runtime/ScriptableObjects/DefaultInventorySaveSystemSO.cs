using System.IO;
using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    /// The default class provided by the Inventory System to save and load the Inventory data.
    /// This class is not mandatory and can be replaced by a custom implementation.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultInventorySaveSystemSO", menuName = "Slax/Inventory/Default Inventory Save System")]
    public class DefaultInventorySaveSystemSO : InventorySaveSystemSO
    {
        private string GetSavePath(string inventoryName)
        {
            return Path.Combine(Application.persistentDataPath, inventoryName + ".json");
        }

        public override void SaveInventory(SerializedInventory inventoryData, string inventoryName)
        {
            string savePath = GetSavePath(inventoryName);
            string json = JsonUtility.ToJson(inventoryData, true);
            File.WriteAllText(savePath, json);
        }

        public override SerializedInventory LoadInventory(string inventoryName)
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
