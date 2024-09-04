using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Slax.Inventory
{
    public class UIInventoryItemSlot : MonoBehaviour
    {
        [SerializeField] protected Image _spriteImage;
        [SerializeField] protected TextMeshProUGUI _amountText;
        [SerializeField] protected Image _lockedImage;
        protected InventorySlot _slot;
        public UnityAction<InventorySlot> OnSelected = delegate { };
        public void Init(InventorySlot slot)
        {
            _slot = slot;

            if (_spriteImage != null && _slot.Item != null)
            {
                _spriteImage.sprite = _slot.Item.PreviewSprite;
            }

            if (_amountText != null)
            {
                if (_slot.Amount <= 0)
                    _amountText.text = "";
                else
                    _amountText.text = _slot.Amount.ToString();
            }

            _lockedImage.enabled = slot.IsLocked;
        }

        public void Select()
        {
            if (_slot.IsLocked) return;
            OnSelected?.Invoke(_slot);
        }
    }
}
