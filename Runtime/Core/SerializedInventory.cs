using System;
using System.Collections.Generic;

namespace Slax.InventorySystem.Runtime.Core
{
    [Serializable]
    public class SerializedInventory
    {
        public string InventoryName;
        public List<SerializedInventorySlot> Slots = new List<SerializedInventorySlot>();
        public List<SerializedTabUnlockState> UnlockedStatesByTab = new List<SerializedTabUnlockState>();
        public float CurrentWeightLimit = 100f;
    }

    [Serializable]
    public class SerializedInventorySlot
    {
        public string ItemID;
        public int Amount;
        public bool IsLocked;
        public int SlotIndex;
    }

    [Serializable]
    public class SerializedTabUnlockState
    {
        public string TabName;
        public List<SerializedSlotUnlockState> UnlockedStates;

        public SerializedTabUnlockState(string tabName, List<SerializedSlotUnlockState> unlockedStates)
        {
            TabName = tabName;
            UnlockedStates = unlockedStates;
        }
    }
}
