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

        #region Extensions: Weight System
        [SerializeField] protected bool _useWeight = false;
        public bool UseWeight => _useWeight;
        [SerializeField] protected float _maxWeight = 100f;
        public float MaxWeight => _maxWeight;
        #endregion
    }
}
