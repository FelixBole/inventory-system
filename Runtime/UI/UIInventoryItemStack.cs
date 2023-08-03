using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Slax.Inventory
{
    public class UIInventoryItemStack : MonoBehaviour
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        private ItemSO _item;
        private int _amount;

        public void Init(ItemSO item, int amount)
        {
            _item = item;
            _amount = amount;
            _spriteImage.sprite = _item.PreviewSprite;
            _amountText.text = _amount.ToString();
        }
    }
}
