using System.Collections.Generic;
using UnityEngine;

namespace Slax.InventorySystem.Runtime.Core
{

    /// <summary>
    /// This class allows to define different slot configurations per tab,
    /// including unlock states, and whether or not the tab uses a size limit.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/InventoryTabConfig")]
    public class InventoryTabConfigSO : ScriptableObject
    {
        [SerializeField] protected string _name = "New Tab";
        public string Name => _name;
        [SerializeField] protected string _description = "Description";
        public string Description => _description;

        [SerializeField] protected Sprite _icon;
        public Sprite Icon => _icon;

        [SerializeField] protected Color _iconColor = Color.white;
        public Color IconColor => _iconColor;

        [SerializeField] protected Color _backgroundColorInactive = Color.black;
        public Color BackgroundColorInactive => _backgroundColorInactive;

        [SerializeField] protected Color _backgroundColorActive = Color.white;
        public Color BackgroundColorActive => _backgroundColorActive;

        [SerializeField] protected List<SlotUnlockStateSO> _slotUnlockStates = new List<SlotUnlockStateSO>();
        public List<SlotUnlockStateSO> SlotUnlockStates => _slotUnlockStates;
    }
}