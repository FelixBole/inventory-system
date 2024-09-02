namespace Slax.Inventory
{
    [System.Serializable]
    public class SerializedSlotUnlockState
    {
        public string StateID;
        public bool Unlocked;

        public SerializedSlotUnlockState(SlotUnlockStateSO state, bool unlocked)
        {
            StateID = state.ID;
            Unlocked = unlocked;
        }
    }
}