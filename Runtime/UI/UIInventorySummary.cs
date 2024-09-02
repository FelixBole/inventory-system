using UnityEngine;
using TMPro;

namespace Slax.Inventory
{
    /// <summary>
    /// This class provides a summary of the inventory, such as total 
    /// weight, available slots, or other stats, and displays them in
    /// the UI.
    /// </summary>
    public class UIInventorySummary : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _totalWeightText;
        [SerializeField] private TextMeshProUGUI _availableSlotsText;

        public void UpdateSummary(RuntimeInventory inventory)
        {
            if (inventory == null) return;

            // Update total weight text
            if (_totalWeightText != null && inventory.InventoryConfig.UseWeight)
            {
                _totalWeightText.text = $"Total Weight: {inventory.CurrentWeight} / {inventory.InventoryConfig.MaxWeight}";
            }
        }
    }
}
