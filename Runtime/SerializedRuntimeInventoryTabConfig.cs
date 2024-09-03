using System.Collections.Generic;

namespace Slax.Inventory
{
    [System.Serializable]
    public class SerializedRuntimeInventoryTabConfig
    {
        public string TabTypeName;
        public List<SerializedSlotUnlockState> UnlockedStates;

        public SerializedRuntimeInventoryTabConfig(RuntimeInventoryTabConfig runtimeConfig)
        {
            TabTypeName = runtimeConfig.TabConfig.Name;
            UnlockedStates = runtimeConfig.GetSerializedSlotUnlockStates();
        }
    }
}
