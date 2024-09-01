using UnityEngine;

namespace Slax.Inventory
{
    [System.Serializable]
    public class ItemStack
    {
        [SerializeField] protected ItemSO _item;
        public ItemSO Item => _item;

        public int Amount;

        public ItemStack()
        {
            _item = null;
            Amount = 0;
        }

        public ItemStack(ItemSO item, int amount)
        {
            _item = item;
            Amount = amount;
        }
    }

}