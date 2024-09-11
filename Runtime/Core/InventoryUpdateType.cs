namespace Slax.InventorySystem.Runtime.Core
{
    /// <summary>
    /// Represents the type of update that was made to the inventory.
    /// 
    /// example usage for custom types:
    /// InventoryUpdateType customType = InventoryUpdateType.Custom("CustomType");
    /// </summary>
    public class InventoryUpdateType
    {
        private readonly string _value;

        protected InventoryUpdateType(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }

        // Predefined types
        public static InventoryUpdateType Added { get; } = new InventoryUpdateType("Added");
        public static InventoryUpdateType Removed { get; } = new InventoryUpdateType("Removed");
        public static InventoryUpdateType TabNotFound { get; } = new InventoryUpdateType("TabNotFound");
        public static InventoryUpdateType SlotNotFound { get; } = new InventoryUpdateType("SlotNotFound");
        public static InventoryUpdateType EmptySlotRemoveAttempt { get; } = new InventoryUpdateType("EmptySlotRemoveAttempt");
        public static InventoryUpdateType SizeLimitReached { get; } = new InventoryUpdateType("SizeLimitReached");
        public static InventoryUpdateType StackLimitReached { get; } = new InventoryUpdateType("StackLimitReached");
        public static InventoryUpdateType WeightLimitReached { get; } = new InventoryUpdateType("WeightLimitReached");
        public static InventoryUpdateType Sold { get; } = new InventoryUpdateType("Sold");
        public static InventoryUpdateType Bought { get; } = new InventoryUpdateType("Bought");
        public static InventoryUpdateType Stolen { get; } = new InventoryUpdateType("Stolen");

        // Allows users to define custom types
        public static InventoryUpdateType Custom(string value) => new InventoryUpdateType(value);
    }
}
