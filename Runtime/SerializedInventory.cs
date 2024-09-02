using System;
using System.Collections.Generic;

namespace Slax.Inventory
{
    [Serializable]
    public class SerializedInventory
    {
        public string InventoryName;
        public List<SerializedInventorySlot> Slots = new List<SerializedInventorySlot>();
        public Dictionary<string, List<SerializedSlotUnlockState>> UnlockedStatesByTab = new Dictionary<string, List<SerializedSlotUnlockState>>();
    }

    [Serializable]
    public class SerializedInventorySlot
    {
        public string ItemID;
        public int Amount;
    }
}
