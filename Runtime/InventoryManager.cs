using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] protected InventorySO _inventory;
        public InventorySO Inventory => _inventory;

        /// <summary>Fired when the CURRENT inventory sells an item</summary>
        public event UnityAction<InventoryUpdate> OnSell = delegate { };
        /// <summary>Fired when items are removed from the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnRemove = delegate { };
        /// <summary>Fired when items are added to the current inventory</summary>
        public event UnityAction<InventoryUpdate> OnAdd = delegate { };
        /// <summary>Fired when the CURRENT inventory buys an item</summary>
        public event UnityAction<InventoryUpdate> OnBuy = delegate { };

        public virtual InventoryUpdate AddItem(ItemSO item, int amount)
        {
            _inventory.Add(item, amount);

            ItemStack stack = FindStack(item);
            InventoryUpdate iu = new InventoryUpdate(stack, 0f, true, _inventory);
            return iu;
        }

        public virtual InventoryUpdate RemoveItem(ItemSO item, int amount)
        {
            _inventory.Remove(item, amount);

            ItemStack stack = new ItemStack(item, amount);
            InventoryUpdate iu = new InventoryUpdate(stack, 0f, FindStack(item) != null, _inventory);
            OnRemove.Invoke(iu);
            return iu;
        }

        /// <summary>
        /// Sells item(s) from this inventory
        /// </summary>
        public virtual InventoryUpdate Sell(ItemSO item, int amount)
        {
            if (!_inventory.Contains(item)) throw new MissingItemException();
            int remaining = _inventory.Count(item);
            if (amount > remaining) amount = remaining;

            float price = item.Price * amount * _inventory.SellPriceMultiplier;
            _inventory.UpdateCurrency(price);

            InventoryUpdate iu = new InventoryUpdate(new ItemStack(item, amount), price, FindStack(item) != null, _inventory);

            OnSell.Invoke(iu);

            return iu;
        }

        /// <summary>
        /// When this inventory buys an item from another inventory.
        /// The price multiplier comes from the settings of the inventory
        /// settings this inventory is buying from.
        /// </summary>
        public virtual InventoryUpdate Buy(ItemSO item, int amount, float priceMultiplier)
        {
            float price = item.Price * amount * priceMultiplier;
            if (price > _inventory.Currency) throw new NotEnoughCurrencyException();

            _inventory.UpdateCurrency(-price);
            _inventory.Add(item, amount);

            ItemStack stack = new ItemStack(item, amount);
            InventoryUpdate iu = new InventoryUpdate(stack, price, true, _inventory);

            OnBuy.Invoke(iu);

            return iu;
        }

        /// <summary>
        /// When this inventory buys an item from another inventory for free
        /// thus not taking into consideration the pricing of the external inventory
        /// </summary>
        public virtual InventoryUpdate BuyForFree(ItemSO item, int amount)
        {
            _inventory.Add(item, amount);

            ItemStack stack = new ItemStack(item, amount);
            InventoryUpdate iu = new InventoryUpdate(stack, 0f, true, _inventory);

            OnBuy.Invoke(iu);
            return iu;
        }

        protected ItemStack FindStack(ItemSO item) => _inventory.Items.Find(o => o.Item == item);
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
