using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private InventorySO _inventory;
        public InventorySO Inventory => _inventory;

        /// <summary>Fired when the CURRENT inventory sells an item</summary>
        public event UnityAction<InventoryUpdate> OnSell = delegate { };
        /// <summary>Fired when items are removed from the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnRemove = delegate { };
        /// <summary>Fired when items are added to the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnAdd = delegate { };
        /// <summary>Fired when the CURRENT inventory buys an item</summary>
        public event UnityAction<InventoryUpdate> OnBuy = delegate { };

        public void AddItem(ItemSO item, int amount)
        {
            _inventory.Add(item, amount);

            ItemStack stack = FindStack(item);
            InventoryUpdate iu = new InventoryUpdate(stack, 0f, true, _inventory);
        }

        public void RemoveItem(ItemSO item, int amount)
        {
            _inventory.Remove(item, amount);

            ItemStack stack = new ItemStack(item, amount);
            InventoryUpdate iu = new InventoryUpdate(stack, 0f, FindStack(item) != null, _inventory);
            OnRemove.Invoke(iu);
        }

        /// <summary>
        /// Sells item(s) from this inventory
        /// </summary>
        public float Sell(ItemSO item, int amount)
        {
            if (!_inventory.Contains(item)) throw new MissingItemException();
            int remaining = _inventory.Count(item);
            if (amount > remaining) amount = remaining;

            float price = item.Price * amount * _inventory.SellPriceMultiplier;
            _inventory.UpdateCurrency(price);

            OnSell.Invoke(new InventoryUpdate());

            return price;
        }

        /// <summary>
        /// When this inventory buys an item from another inventory.
        /// The price multiplier comes from the settings of the inventory
        /// settings this inventory is buying from.
        /// </summary>
        public float Buy(ItemSO item, int amount, float priceMultiplier)
        {
            float price = item.Price * amount * priceMultiplier;
            if (price > _inventory.Currency) return 0;

            _inventory.UpdateCurrency(-price);
            _inventory.Add(item, amount);

            ItemStack stack = new ItemStack(item, amount);
            InventoryUpdate iu = new InventoryUpdate(stack, price, true, _inventory);

            OnBuy.Invoke(iu);

            return price;
        }

        /// <summary>
        /// When this inventory buys an item from another inventory for free
        /// thus not taking into consideration the pricing of the external inventory
        /// </summary>
        public void BuyForFree(ItemSO item, int amount)
        {
            _inventory.Add(item, amount);

            ItemStack stack = new ItemStack(item, amount);
            InventoryUpdate iu = new InventoryUpdate(stack, 0f, true, _inventory);

            OnBuy.Invoke(iu);
        }

        private ItemStack FindStack(ItemSO item) => _inventory.Items.Find(o => o.Item == item);
    }

    public struct InventoryUpdate
    {
        /// <summary>The Items concerned by the update</summary>
        public ItemStack Stack;

        /// <summary>Price of the transaction if there was one, set to 0 if none happened</summary>
        public float Price;

        /// <summary>If the item is remaining in the inventory firing the event</summary>
        public bool Remaining;

        /// <summary>The current inventory</summary>
        public InventorySO Inventory;

        public InventoryUpdate(ItemStack stack, float price, bool remaining, InventorySO inventory)
        {
            this.Stack = stack;
            this.Price = price;
            this.Remaining = remaining;
            this.Inventory = inventory;
        }
    }
}
