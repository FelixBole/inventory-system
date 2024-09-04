using UnityEngine;
using UnityEngine.Events;

namespace Slax.Inventory
{
    [System.Serializable]
    /// <summary>
    /// Represents individual slots in the inventory, which can be specialized (e.g. for equipment slots, armor slot) or generic.
    /// </summary>
    public class InventorySlot
    {
        [SerializeField] protected ItemSO _item;
        [SerializeField] protected int _amount;

        public ItemSO Item => _item;
        public int Amount => _amount;
        [SerializeField] protected bool _isLocked = false;
        public bool IsLocked => _isLocked;

        public UnityAction<InventorySlot> OnSlotChanged = delegate { };
        public UnityAction<InventorySlot> OnSlotLocked = delegate { };
        public UnityAction<InventorySlot> OnSlotUnlocked = delegate { };

        public InventorySlot()
        {
            _item = null;
            _amount = 0;
        }

        public bool IsEmpty => _item == null || _amount <= 0;

        public void ChangeItem(ItemSO item, int amount)
        {
            if (_isLocked) return;

            _item = item;
            _amount = amount;

            OnSlotChanged?.Invoke(this);
        }

        public void AddItem(ItemSO item, int amount)
        {
            if (_isLocked) return;

            if (_item == null)
            {
                _item = item;
                _amount = amount;
            }
            else if (_item == item)
            {
                _amount += amount;
            }

            OnSlotChanged?.Invoke(this);
        }

        public void RemoveItem(int amount)
        {
            _amount -= amount;
            if (_amount <= 0)
            {
                ClearSlot();
            }

            OnSlotChanged?.Invoke(this);
        }

        public void UnlockSlot()
        {
            _isLocked = false;

            OnSlotUnlocked?.Invoke(this);
        }

        public void LockSlot()
        {
            _isLocked = true;

            OnSlotLocked?.Invoke(this);
        }

        public void ClearSlot()
        {
            _item = null;
            _amount = 0;
        }
    }
}