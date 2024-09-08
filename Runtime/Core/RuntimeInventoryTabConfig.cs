using System.Collections.Generic;
using System.Linq;

namespace Slax.InventorySystem.Runtime.Core
{
    /// <summary>
    /// Represents the runtime configuration of an inventory tab.
    /// This includes the tab type and the unlock states of the slots.
    /// </summary>
    [System.Serializable]
    public class RuntimeInventoryTabConfig
    {
        public InventoryTabConfigSO TabConfig { get; private set; }
        public Dictionary<SlotUnlockStateSO, bool> UnlockedStates { get; private set; }

        public int MaxSlots => CalculateMaxSlots();

        public RuntimeInventoryTabConfig(InventoryTabConfigSO tabConfig, List<SerializedSlotUnlockState> serializedStates = null)
        {
            TabConfig = tabConfig;
            UnlockedStates = new Dictionary<SlotUnlockStateSO, bool>();

            // Initialize from tabConfig
            InitializeFromTabConfig(tabConfig.SlotUnlockStates);

            // Apply save data if provided
            if (serializedStates != null)
            {
                InitializeFromSaveData(serializedStates);
            }
        }

        public void InitializeFromSaveData(List<SerializedSlotUnlockState> serializedStates)
        {
            foreach (var serializedState in serializedStates)
            {
                SlotUnlockStateSO state = UnlockedStates.Keys.FirstOrDefault(s => s.ID == serializedState.StateID);
                if (state != null)
                {
                    UnlockedStates[state] = serializedState.Unlocked;
                }
            }
        }

        public void InitializeFromTabConfig(List<SlotUnlockStateSO> slotUnlockStates)
        {
            UnlockedStates.Clear();
            foreach (var state in slotUnlockStates)
            {
                if (state == slotUnlockStates[0])
                {
                    UnlockedStates.Add(state, true);
                }
                else
                {
                    UnlockedStates.Add(state, false);
                }
            }
        }

        public void UnlockSlotState(SlotUnlockStateSO state)
        {
            if (UnlockedStates.ContainsKey(state))
            {
                UnlockedStates[state] = true;
            }
        }

        public void LockSlotState(SlotUnlockStateSO state)
        {
            if (UnlockedStates.ContainsKey(state))
            {
                UnlockedStates[state] = false;
            }
        }

        public bool IsSlotStateUnlocked(SlotUnlockStateSO state)
        {
            return UnlockedStates.ContainsKey(state) && UnlockedStates[state];
        }

        public bool IsInventorySlotUnlocked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots)
            {
                return false;
            }

            int unlockedSlots = 0;
            foreach (var unlockState in UnlockedStates)
            {
                if (unlockState.Value)
                {
                    unlockedSlots += unlockState.Key.AdditionalSlots;
                }
            }

            return slotIndex < unlockedSlots;
        }

        public bool IsInventorySlotLocked(int slotIndex)
        {
            return !IsInventorySlotUnlocked(slotIndex);
        }


        public int CalculateMaxSlots()
        {
            int maxSlots = 0;
            foreach (var unlockState in UnlockedStates)
            {
                maxSlots += unlockState.Key.AdditionalSlots;
            }
            return maxSlots;
        }

        public List<SerializedSlotUnlockState> GetSerializedSlotUnlockStates()
        {
            List<SerializedSlotUnlockState> serializedStates = new List<SerializedSlotUnlockState>();
            foreach (var unlockState in UnlockedStates)
            {
                serializedStates.Add(new SerializedSlotUnlockState(unlockState.Key, unlockState.Value));
            }
            return serializedStates;
        }
    }
}
