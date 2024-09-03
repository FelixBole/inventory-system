using UnityEngine;
using UnityEngine.UI;

namespace Slax.Inventory
{
    public class UIInventoryWeight : MonoBehaviour
    {
        [SerializeField] protected TMPro.TextMeshProUGUI _weightText;
        [SerializeField] protected Image _weightBar;
        [SerializeField] protected Color _lowWeightColor = Color.green;
        [SerializeField] protected Color _highWeightColor = Color.red;
        protected RuntimeInventory _inventory = null;

        void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= UpdateWeight;
            }
        }

        public void Init(RuntimeInventory inventory)
        {
            _inventory = inventory;
            inventory.OnInventoryChanged += UpdateWeight;
            UpdateWeight(inventory);
        }

        protected void UpdateWeight(RuntimeInventory inventory)
        {
            _weightText.text = $"{inventory.CurrentWeight} / {inventory.InventoryConfig.MaxWeight}";
            _weightBar.fillAmount = (float)inventory.CurrentWeight / inventory.InventoryConfig.MaxWeight;
            _weightBar.color = Color.Lerp(_lowWeightColor, _highWeightColor, (float)inventory.CurrentWeight / inventory.InventoryConfig.MaxWeight);
        }
    }
}