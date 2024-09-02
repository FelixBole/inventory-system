using System.Collections.Generic;
using UnityEngine;

namespace Slax.Inventory
{

    /// <summary>
    /// This class allows to define different slot configurations per tab,
    /// including unlock states, and whether or not the tab uses a size limit.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/InventoryTabConfig")]
    public class InventoryTabConfigSO : ScriptableObject
    {
        [SerializeField] protected ItemTabTypeSO _tabType;
        public ItemTabTypeSO TabType => _tabType;
        [SerializeField] protected List<SlotUnlockStateSO> _slotUnlockStates = new List<SlotUnlockStateSO>();
        public List<SlotUnlockStateSO> SlotUnlockStates => _slotUnlockStates;
    }
}