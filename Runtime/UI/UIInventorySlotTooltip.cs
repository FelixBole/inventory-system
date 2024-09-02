using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slax.Inventory
{
    /// <summary>
    /// This class displays a tooltip when hovering over an inventory item
    /// slot. It can show item details, stats, or any other relevant
    /// information.
    /// </summary>
    public class UIInventorySlotTooltip : MonoBehaviour
    {
        [SerializeField] private GameObject _tooltip;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemDescriptionText;
        [SerializeField] private TextMeshProUGUI _itemStatsText;

        private RectTransform _tooltipRectTransform;

        private void Start()
        {
            _tooltipRectTransform = _tooltip.GetComponent<RectTransform>();
            _tooltip.SetActive(false);
        }

        public void ShowTooltip(ItemSO item, Vector2 position)
        {
            if (item == null) return;

            _itemNameText.text = item.Name;
            _itemDescriptionText.text = item.Description;
            // Add any item-specific stats here
            _itemStatsText.text = $"Weight: {item.Weight}";

            _tooltip.SetActive(true);
            _tooltipRectTransform.position = position;
        }

        public void HideTooltip()
        {
            _tooltip.SetActive(false);
        }
    }
}
