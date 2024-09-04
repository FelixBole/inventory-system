using System.Collections.Generic;
using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    /// An inventory, could be the inventory of the player or a merchant's inventory. 
    /// This class holds the information about the items contained in the Inventory
    /// and offers basic functionalities to Add / Remove items and make verifications.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/Inventory")]
    public class InventorySO : ScriptableObject
    {
        public string Name = "Inventory";
        [SerializeField] protected List<InventoryTabConfigSO> _tabConfigs = new List<InventoryTabConfigSO>();
        public List<InventoryTabConfigSO> TabConfigs => _tabConfigs;

        [SerializeField] protected bool _useFixedSlots = false;
        /// <summary>
        /// When true, the inventory slots will not be re-arranged when an item is removed
        /// and the saved slot index will be used to place the item.
        /// </summary>
        public bool UseFixedSlots => _useFixedSlots;

        [SerializeField, Tooltip("When set to true, makes it so that when an item in a slot reaches its stack limit, it will place the item in the next available slot.")] protected bool _useSameItemInMultipleSlots = false;
        public bool UseSameItemInMultipleSlots => _useSameItemInMultipleSlots;

        #region Extensions: Weight System
        [SerializeField] protected bool _useWeight = false;
        public bool UseWeight => _useWeight;
        [SerializeField, Tooltip("Absolute max weight for the inventory, so that the save data cannot be manipulated to go over this value.")] protected float _maxWeight = 100f;
        public float MaxWeight => _maxWeight;
        #endregion
    }
}
