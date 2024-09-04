using UnityEngine;

namespace Slax.Inventory
{
    [CreateAssetMenu(menuName = "Slax/Inventory/SlotUnlockState")]
    /// <summary>
    /// A slot unlock state that can be used to unlock additional slots in the inventory.
    /// This should be added to the InventorySO that uses a size limit extension to
    /// handle the unlocking of additional slots and unlocking the base slots.
    /// </summary>
    public class SlotUnlockStateSO : ScriptableObject
    {
        [SerializeField] protected string _id;
        public string ID => _id;

        [SerializeField] protected int _additionalSlots;
        public int AdditionalSlots => _additionalSlots;
    }
}
