using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slax.Inventory
{
    public class UIInventoryItemSlot : MonoBehaviour
    {
        [SerializeField] protected Image _spriteImage;
        [SerializeField] protected TextMeshProUGUI _amountText;
        [SerializeField] protected Image _lockedImage;
        protected ItemSO _item;
        protected int _amount;

        public void Init(InventorySlot slot)
        {
            _item = slot.Item;
            _amount = slot.Amount;

            if (_spriteImage != null && _item != null)
            {
                _spriteImage.sprite = _item.PreviewSprite;
            }

            if (_amountText != null)
            {
                if (_amount <= 0)
                    _amountText.text = "";
                else
                    _amountText.text = _amount.ToString();
            }

            _lockedImage.enabled = slot.IsLocked;
        }
    }
}
