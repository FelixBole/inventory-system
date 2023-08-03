using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    /// The Inventory of the Player. This class holds the information about the items contained in the Inventory
    /// and offers basic functionalities to Add / Remove items and make verifications.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/Inventory")]
    public class InventorySO : ScriptableObject
    {
        [Header("Inventory Settings")]
        public string Name = "Inventory";
        public float BuyPriceMultiplier = 1f;
        public float SellPriceMultiplier = 1f;

        [Header("Inventory Content")]
        [SerializeField] private List<ItemStack> _items = new List<ItemStack>();
        public List<ItemStack> Items => _items;
        public bool IsEmpty => _items.Count == 0;

        [SerializeField] private List<ItemStack> _defaultItems = new List<ItemStack>();

        [Header("Currency settings")]
        [SerializeField] private float _currency = 0;
        public float Currency => _currency;
        [SerializeField] private float _maxCurrency = 999999999f;
        [SerializeField] private bool _allowSubZeroCurrency = false;

        [Header("Size / Weight settings")]
        public bool UseWeight = false;
        public bool UseSize = false;

        public float MaxWeight = 0f;
        public int MaxSize = 0;

        public void Add(ItemSO item, int count = 1)
        {
            if (count <= 0) return;

            for (int i = 0; i < _items.Count; i++)
            {
                ItemStack currentItemStack = _items[i];

                if (item == currentItemStack.Item)
                {
                    currentItemStack.Amount += count;

                    return;
                }
            }

            _items.Add(new ItemStack(item, count));
        }

        public void Remove(ItemSO item, int count = 1)
        {
            if (!Contains(item)) throw new MissingItemException();
            if (count <= 0) return;

            for (int i = 0; i < _items.Count; i++)
            {
                ItemStack currentItemStack = _items[i];

                if (currentItemStack.Item == item)
                {
                    currentItemStack.Amount -= count;

                    if (currentItemStack.Amount <= 0)
                        _items.Remove(currentItemStack);

                    return;
                }
            }
        }

        public bool Contains(ItemSO item)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (item == _items[i].Item)
                {
                    return true;
                }
            }

            return false;
        }

        public int Count(ItemSO item)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                ItemStack currentItemStack = _items[i];

                if (item == currentItemStack.Item)
                {
                    return currentItemStack.Amount;
                }
            }

            return 0;
        }

        public void Init()
        {
            if (_items == null)
            {
                _items = new List<ItemStack>();
            }

            _items.Clear();

            foreach (ItemStack item in _defaultItems)
            {
                _items.Add(item);
            }
        }

        public float SetCurrency(float value)
        {
            if (value > _maxCurrency) value = _maxCurrency;
            _currency = value;
            return _currency;
        }

        public CurrencyUpdate UpdateCurrency(float value)
        {
            if (!_allowSubZeroCurrency && _currency + value < 0)
            {
                _currency += value;
                return new CurrencyUpdate(true, _currency);
            }

            if (_currency + value < 0)
            {
                return new CurrencyUpdate(false, _currency);
            }

            _currency += value;
            return new CurrencyUpdate(true, _currency);
        }
    }

    public struct CurrencyUpdate
    {
        public bool Updated;

        /// <summary>New Currency value</summary>
        public float Currency;

        public CurrencyUpdate(bool updated, float currency)
        {
            this.Currency = currency;
            this.Updated = updated;
        }
    }
}
