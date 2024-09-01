using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slax.Inventory
{
    public class UIInventoryItemStack : MonoBehaviour
    {
        [SerializeField] protected Image _spriteImage;
        [SerializeField] protected TextMeshProUGUI _amountText;
        protected ItemSO _item;
        protected int _amount;

        public void Init(ItemSO item, int amount)
        {
            _item = item;
            _amount = amount;
            _spriteImage.sprite = _item.PreviewSprite;
            _amountText.text = _amount.ToString();
        }
    }
}
