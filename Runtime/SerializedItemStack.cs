namespace Slax.Inventory
{
    [System.Serializable]
    public class SerializedItemStack
    {
        public string ItemGuid;
        public int Amount;

        public SerializedItemStack(string itemGuid, int amount)
        {
            this.ItemGuid = itemGuid;
            this.Amount = amount;
        }
    }

}