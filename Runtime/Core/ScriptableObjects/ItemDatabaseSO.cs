using System.Collections.Generic;
using UnityEngine;

namespace Slax.InventorySystem.Runtime.Core
{
    /// <summary>
    /// This class is used to store a list of items that can be used in the
    /// inventory system. It is used to create a database of items that can be
    /// referenced by their ItemID. It should be provided to the InventorySO when
    /// loading the inventory to ensure that the stored items are correctly
    /// referenced.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/ItemDatabase")]
    public class ItemDatabaseSO : ScriptableObject
    {
        [SerializeField] protected List<ItemSO> _items = new List<ItemSO>();
        protected Dictionary<string, ItemSO> _itemDictionary;

        protected void OnEnable()
        {
            InitializeDatabase();
        }

        public List<ItemSO> GetItemsList() => new List<ItemSO>(_items);

        protected void InitializeDatabase()
        {
            _itemDictionary = new Dictionary<string, ItemSO>();
            foreach (var item in _items)
            {
                if (!_itemDictionary.ContainsKey(item.ID))
                {
                    _itemDictionary.Add(item.ID, item);
                }
                else
                {
                    Debug.LogWarning($"Duplicate ItemID found: {item.ID}. Please ensure all ItemIDs are unique.");
                }
            }
        }

        /// <summary>
        /// Finds the ItemSO associated with the given ItemID.
        /// </summary>
        public ItemSO GetItemByID(string itemID)
        {
            _itemDictionary.TryGetValue(itemID, out ItemSO item);
            return item;
        }

        /// <summary>
        /// Adds a new item to the database.
        /// </summary>
        public void AddItem(ItemSO item)
        {
            if (item != null && !_itemDictionary.ContainsKey(item.ID))
            {
                _items.Add(item);
                _itemDictionary.Add(item.ID, item);
            }
        }

        /// <summary>
        /// Removes an item from the database.
        /// </summary>
        public void RemoveItem(ItemSO item)
        {
            if (item != null && _itemDictionary.ContainsKey(item.ID))
            {
                _items.Remove(item);
                _itemDictionary.Remove(item.ID);
            }
        }
    }
}
