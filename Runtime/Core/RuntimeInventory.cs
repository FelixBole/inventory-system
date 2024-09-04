using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    /// A runtime inventory that offers all functionalities for managing
    /// the actions done on the inventory.
    /// </summary>
    [System.Serializable]
    public class RuntimeInventory
    {
        protected Dictionary<InventoryTabConfigSO, List<InventorySlot>> _slotsByTab;
        public Dictionary<InventoryTabConfigSO, List<InventorySlot>> SlotsByTab => _slotsByTab;

        protected Dictionary<InventoryTabConfigSO, RuntimeInventoryTabConfig> _tabConfigs;

        protected InventorySO _inventoryConfig;
        public InventorySO InventoryConfig => _inventoryConfig;

        protected InventorySaveSystemSO _saveSystem;

        protected float _currentWeight = 0f;
        public float CurrentWeight => _currentWeight;
        protected float _currentWeightLimit = 0f;
        /// <summary>The inventory weight limit, which can be manipulated at runtime</summary>
        public float CurrentWeightLimit => _currentWeightLimit;

        public List<string> InvalidItems { get; protected set; } = new List<string>();

        /// <summary>
        /// Event triggered when an item cannot be added due to weight limit.
        /// </summary>
        public UnityAction<float, float> OnWeightLimitReached;

        /// <summary>
        /// Event triggered when an item cannot be added due to size limit.
        /// </summary>
        public UnityAction<ItemSO, float> OnSizeLimitReached;

        public UnityAction<RuntimeInventory> OnInventoryChanged = delegate { };

        public RuntimeInventory(InventorySO inventory)
        {
            _slotsByTab = new Dictionary<InventoryTabConfigSO, List<InventorySlot>>();
            _tabConfigs = new Dictionary<InventoryTabConfigSO, RuntimeInventoryTabConfig>();
            _inventoryConfig = inventory;

            // Initialize slots and tab configurations based on the inventory configuration
            InitializeTabs();
        }

        #region Setup and Cleanup
        /// <summary>
        /// Cleans up the inventory by removing event listeners.
        /// Should be called by the inventory manager onDisable.
        /// </summary>
        public void Cleanup()
        {
            foreach (var tab in _slotsByTab)
            {
                foreach (var slot in tab.Value)
                {
                    slot.OnSlotChanged -= HandleSlotChanged;
                    slot.OnSlotLocked -= slot => OnInventoryChanged?.Invoke(this);
                    slot.OnSlotUnlocked -= slot => OnInventoryChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Initialize tabs and slots based on the InventorySO configuration.
        /// </summary>
        protected void InitializeTabs(SerializedInventory saveData = null)
        {
            _slotsByTab.Clear();
            _tabConfigs.Clear();

            foreach (var tabConfig in _inventoryConfig.TabConfigs)
            {

                SerializedTabUnlockState tabState = saveData?.UnlockedStatesByTab.FirstOrDefault(s => s.TabName == tabConfig.Name);
                var runtimeTabConfig = new RuntimeInventoryTabConfig(tabConfig, tabState?.UnlockedStates);

                _tabConfigs[tabConfig] = runtimeTabConfig;

                var slots = new List<InventorySlot>();
                for (int i = 0; i < runtimeTabConfig.MaxSlots; i++)
                {
                    InventorySlot inventorySlot = new InventorySlot();
                    if (runtimeTabConfig.IsInventorySlotLocked(i))
                    {
                        inventorySlot.LockSlot();
                    }
                    slots.Add(inventorySlot);
                    inventorySlot.OnSlotChanged += HandleSlotChanged;
                    inventorySlot.OnSlotLocked += slot => OnInventoryChanged?.Invoke(this);
                    inventorySlot.OnSlotUnlocked += slot => OnInventoryChanged?.Invoke(this);
                }
                _slotsByTab[tabConfig] = slots;
            }

            OnInventoryChanged?.Invoke(this);
        }
        #endregion

        #region Item Management
        /// <summary>
        /// Adds an item to the appropriate slot in the inventory.
        /// If the item already exists in the appropriate tab, it will be added to the existing slot or a new slot if conditions are met.
        /// If no tab or slot is specified, it finds the first available slot in the first available tab.
        /// </summary>
        public InventoryUpdate AddItem(ItemSO item, int count = 1)
        {
            // Find the first tab that can contain this item
            var itemTab = item.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab == null)
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.Added);

            // Try to find an existing slot for this item in the identified tab
            var existingSlot = FindSlotWithItem(itemTab, item);

            // Handle adding the item to an appropriate slot if it doesn't already exist
            if (existingSlot == null)
            {
                return AddItemToFirstAvailableSlot(itemTab, item, count);
            }

            int existingSlotIndex = _slotsByTab[itemTab].IndexOf(existingSlot);

            return RunChecksAndAddItemToSlot(itemTab, existingSlot, item, existingSlotIndex, count);
        }

        /// <summary>
        /// Adds an item to a specific slot in the inventory.
        /// Used when the item is already known to exist in the inventory.
        /// </summary>
        public InventoryUpdate AddItemToSlot(ItemSO item, InventorySlot slot, int count = 1)
        {
            var itemTab = item.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab == null)
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.Added);

            int index = _slotsByTab[itemTab].IndexOf(slot);
            if (index < 0)
            {
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.SlotNotFound);
            }

            return RunChecksAndAddItemToSlot(itemTab, slot, item, index, count);
        }

        /// <summary>
        /// Adds an item to the first available slot in the inventory.
        /// </summary>
        protected InventoryUpdate AddItemToFirstAvailableSlot(InventoryTabConfigSO tabConfig, ItemSO item, int count)
        {
            var firstAvailableSlot = FindFirstAvailableSlotForItem(tabConfig, item);
            if (firstAvailableSlot != null)
            {
                return AddItemToSpecificSlot(tabConfig, _slotsByTab[tabConfig].IndexOf(firstAvailableSlot), item, count);
            }
            else
            {
                OnSizeLimitReached?.Invoke(item, _currentWeight + item.Weight * count);
                return new InventoryUpdate(this, item, firstAvailableSlot, -1, true, InventoryUpdateType.SizeLimitReached);
            }
        }

        /// <summary>
        /// Runs checks and adds an item to a specific slot in a specific tab.
        /// </summary>
        protected InventoryUpdate RunChecksAndAddItemToSlot(InventoryTabConfigSO tab, InventorySlot slot, ItemSO item, int index, int count)
        {
            // If the item is stackable and the stack limit allows, or if no stack limit is imposed, add to the existing slot
            if (item.IsStackable)
            {
                if (item.StackLimit < 0 || slot.Amount + count <= item.StackLimit)
                {
                    return AddItemToSpecificSlot(tab, index, item, count);
                }

                // If the stack limit is reached and multiple slots are allowed, try adding to another slot
                if (_inventoryConfig.UseSameItemInMultipleSlots)
                {
                    return AddItemToFirstAvailableSlot(tab, item, count);
                }

                // Stack limit reached, cannot add more to this slot
                return new InventoryUpdate(this, item, slot, index, true, InventoryUpdateType.StackLimitReached);
            }

            // If the item is not stackable and multiple slots are allowed, try adding to another slot
            if (_inventoryConfig.UseSameItemInMultipleSlots)
            {
                if (item.IsUnique)
                {
                    // If the item is unique, it cannot be added to a new slot
                    return new InventoryUpdate(this, item, slot, index, true, InventoryUpdateType.StackLimitReached);
                }
                return AddItemToFirstAvailableSlot(tab, item, count);
            }

            if (!item.IsStackable)
            {
                // TODO verify this, it might not be enough to handle all edge-cases, especially regarding fixed slots extension
                return new InventoryUpdate(this, item, slot, index, true, InventoryUpdateType.StackLimitReached);
            }

            // Add to the existing slot (for non-stackable items with single slot allowed)
            return AddItemToSpecificSlot(tab, index, item, count);
        }

        /// <summary>
        /// Adds an item to a specific slot in a specific tab.
        /// </summary>
        protected InventoryUpdate AddItemToSpecificSlot(InventoryTabConfigSO tabConfig, int slotIndex, ItemSO item, int count)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.TabNotFound);

            RuntimeInventoryTabConfig config = GetTabConfig(tabConfig);
            if (config == null || slotIndex >= config.MaxSlots)
            {
                OnSizeLimitReached?.Invoke(item, _currentWeight + item.Weight * count);
                return new InventoryUpdate(this, item, null, slotIndex, false, InventoryUpdateType.SizeLimitReached);
            }

            if (_inventoryConfig.UseWeight)
            {
                float itemWeight = item.Weight * count;
                if (_currentWeight + itemWeight > _inventoryConfig.MaxWeight)
                {
                    OnWeightLimitReached?.Invoke(_currentWeight, _inventoryConfig.MaxWeight);
                    return new InventoryUpdate(this, item, null, slotIndex, false, InventoryUpdateType.WeightLimitReached);
                }
                _currentWeight += itemWeight;
            }

            var slots = _slotsByTab[tabConfig];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return new InventoryUpdate(this, item, null, slotIndex, false, InventoryUpdateType.Added);

            slots[slotIndex].AddItem(item, count);

            var updatedSlot = slots[slotIndex];
            var updateType = InventoryUpdateType.Added;
            var remaining = !updatedSlot.IsEmpty;

            return new InventoryUpdate(this, item, updatedSlot, slotIndex, remaining, updateType);
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        public InventoryUpdate RemoveItem(ItemSO item, int count = 1)
        {
            var itemTab = item.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab == null)
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.Removed);

            var slot = FindSlotForRemoval(itemTab, item);
            if (slot == null)
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.SlotNotFound);

            return RemoveItemFromSpecificSlot(itemTab, _slotsByTab[itemTab].IndexOf(slot), count);
        }

        /// <summary>
        /// Removes an item from a specific slot in the inventory.
        /// Used when the item is already known to exist in the inventory.
        /// </summary>
        public InventoryUpdate RemoveItemFromSlot(ItemSO item, InventorySlot slot, int count = 1)
        {
            var itemTab = item.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab == null)
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.TabNotFound);

            int index = _slotsByTab[itemTab].IndexOf(slot);
            if (index < 0)
            {
                return new InventoryUpdate(this, item, null, -1, false, InventoryUpdateType.SlotNotFound);
            }
            return RemoveItemFromSpecificSlot(itemTab, index, count);
        }

        /// <summary>
        /// Removes an item from a specific slot in a specific tab.
        /// </summary>
        protected InventoryUpdate RemoveItemFromSpecificSlot(InventoryTabConfigSO tabConfig, int slotIndex, int count = 1)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return new InventoryUpdate(this, null, null, -1, false, InventoryUpdateType.TabNotFound);

            var slots = _slotsByTab[tabConfig];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return new InventoryUpdate(this, null, null, slotIndex, false, InventoryUpdateType.SlotNotFound);

            var slot = slots[slotIndex];
            if (slot == null)
                return new InventoryUpdate(this, null, null, slotIndex, false, InventoryUpdateType.SlotNotFound);
            else if (slot.IsEmpty)
                return new InventoryUpdate(this, null, slot, slotIndex, false, InventoryUpdateType.EmptySlotRemoveAttempt);

            if (_inventoryConfig.UseWeight)
            {
                float itemWeight = slot.Item.Weight * count;
                _currentWeight -= itemWeight;
            }

            slot.RemoveItem(count);

            var remaining = !slot.IsEmpty;
            var updateType = InventoryUpdateType.Removed;

            return new InventoryUpdate(this, slot.Item, slot, slotIndex, remaining, updateType);
        }
        #endregion

        #region Slot Management

        /// <summary>
        /// Changes the item in a specific slot in the inventory.
        /// If newAmount is not specified, the amount will remain the same.
        /// </summary>
        public void ChangeItemFromSlot(InventorySlot slot, ItemSO newItem, int newAmount = -1)
        {
            if (slot == null || newItem == null)
                return;

            var itemTab = newItem.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab == null)
                return;

            int index = _slotsByTab[itemTab].IndexOf(slot);
            if (index < 0)
                return;

            if (newAmount < 0)
                newAmount = slot.Amount;

            slot.ChangeItem(newItem, newAmount);
        }

        /// <summary>
        /// Switches the slots at the specified indices in the specified tab.
        /// </summary>
        public void SwitchSlots(InventoryTabConfigSO tabConfig, InventorySlot slot1, InventorySlot slot2)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return;

            var slots = _slotsByTab[tabConfig];
            int index1 = slots.IndexOf(slot1);
            int index2 = slots.IndexOf(slot2);

            if (index1 < 0 || index2 < 0)
                return;

            var temp = slots[index1];
            slots[index1] = slots[index2];
            slots[index2] = temp;

            if (!_inventoryConfig.UseFixedSlots)
            {
                RearrangeSlots(true);
            }
            else
            {
                OnInventoryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Unlocks additional slots for a specific tab.
        /// </summary>
        public void UnlockSlotsForTab(InventoryTabConfigSO tabConfig, SlotUnlockStateSO unlockState)
        {
            var config = GetTabConfig(tabConfig);
            if (config != null)
            {
                config.UnlockSlotState(unlockState);
            }

            OnInventoryChanged?.Invoke(this);
        }

        /// <summary>
        /// Reacts to a slot change event.
        /// </summary>
        protected void HandleSlotChanged(InventorySlot slot)
        {
            if (!_inventoryConfig.UseFixedSlots && slot.IsEmpty)
            {
                RearrangeSlots(false);
            }
            OnInventoryChanged?.Invoke(this);
        }

        /// <summary>
        /// Rearranges the slots in the inventory if the inventory does
        /// not used the fixed slots extension.
        /// </summary>
        protected void RearrangeSlots(bool invokeInventoryChanged = true)
        {
            foreach (var tab in _slotsByTab)
            {
                tab.Value.Sort((a, b) =>
                {
                    // Priority 1: Non-empty unlocked slots come first
                    if (!a.IsEmpty && !a.IsLocked && (b.IsEmpty || b.IsLocked)) return -1;
                    if (!b.IsEmpty && !b.IsLocked && (a.IsEmpty || a.IsLocked)) return 1;

                    // Priority 2: Empty unlocked slots come after non-empty unlocked
                    if (a.IsEmpty && !a.IsLocked && (!b.IsEmpty || b.IsLocked)) return -1;
                    if (b.IsEmpty && !b.IsLocked && (!a.IsEmpty || a.IsLocked)) return 1;

                    // Priority 3: Non-empty locked slots come next
                    if (!a.IsEmpty && a.IsLocked && (b.IsEmpty || !b.IsLocked)) return -1;
                    if (!b.IsEmpty && b.IsLocked && (a.IsEmpty || !a.IsLocked)) return 1;

                    // Priority 4: Empty locked slots come last
                    if (a.IsEmpty && a.IsLocked) return 1;
                    if (b.IsEmpty && b.IsLocked) return -1;

                    return 0; // If both slots have the same state
                });
            }

            if (invokeInventoryChanged) OnInventoryChanged?.Invoke(this);
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// Allows the user to set a custom save system. 
        /// Defaults to internal JSON save system.
        /// </summary>
        public void SetSaveSystem(InventorySaveSystemSO saveSystem)
        {
            _saveSystem = saveSystem;
        }

        /// <summary>
        /// The default save system save method for the InventorySystem.
        /// </summary>
        public void SaveInventory()
        {
            var serializedInventory = GetSaveData();
            _saveSystem.SaveInventory(serializedInventory, _inventoryConfig.Name);
        }

        /// <summary>
        /// The default save system load method for the InventorySystem.
        /// </summary>
        public void LoadInventory(List<ItemSO> allItems)
        {
            var serializedInventory = _saveSystem.LoadInventory(_inventoryConfig.Name);
            if (serializedInventory != null)
            {
                LoadSaveData(serializedInventory, allItems);
            }
            OnInventoryChanged?.Invoke(this);
        }

        /// <summary>
        /// Serialize Inventory into a data structure.
        /// </summary>
        public SerializedInventory GetSaveData()
        {
            var serializedInventory = new SerializedInventory
            {
                InventoryName = _inventoryConfig.Name
            };

            foreach (var tab in _slotsByTab)
            {
                int slotIndex = 0;
                foreach (var slot in tab.Value)
                {
                    if (slot.Item != null)
                    {
                        serializedInventory.Slots.Add(new SerializedInventorySlot
                        {
                            ItemID = slot.Item.ID,
                            Amount = slot.Amount,
                            IsLocked = slot.IsLocked,
                            SlotIndex = slotIndex
                        });
                    }
                    slotIndex++;
                }
            }

            serializedInventory.CurrentWeightLimit = _currentWeightLimit;

            SerializeUnlockedStates(serializedInventory);
            return serializedInventory;
        }

        /// <summary>
        /// Loads the inventory from a serialized data structure.
        /// </summary>
        public void LoadSaveData(SerializedInventory serializedInventory, List<ItemSO> allItems)
        {
            InvalidItems.Clear();
            InitializeTabs(serializedInventory); // Ensure we have the correct number of slots

            int slotIndex = 0; // Used for non-fixed slots

            foreach (var tab in _slotsByTab)
            {
                slotIndex = 0;
                foreach (var slot in serializedInventory.Slots)
                {
                    var item = allItems.Find(it => it.ID == slot.ItemID);

                    // If item is not found, skip (purges invalid items)
                    if (item == null)
                    {
                        if (!InvalidItems.Contains(slot.ItemID))
                        {
                            InvalidItems.Add(slot.ItemID); // This is for debug and displaying warnings to the user
                        }
                        continue;
                    }

                    // Place in correct tab
                    foreach (var tabConfig in item.TabConfigs)
                    {
                        if (tab.Key == tabConfig)
                        {
                            if (_inventoryConfig.UseFixedSlots)
                            {
                                if (slot.SlotIndex >= tab.Value.Count) break;
                                tab.Value[slot.SlotIndex].AddItem(item, slot.Amount);
                                if (slot.IsLocked) tab.Value[slot.SlotIndex].LockSlot();
                                else tab.Value[slot.SlotIndex].UnlockSlot();

                            }
                            else
                            {
                                if (slotIndex >= tab.Value.Count) break;
                                tab.Value[slotIndex].AddItem(item, slot.Amount);
                                if (slot.IsLocked) tab.Value[slotIndex].LockSlot();
                                else tab.Value[slotIndex].UnlockSlot();
                                slotIndex++;
                            }

                            _currentWeight += item.Weight * slot.Amount;
                            break;
                        }
                    }
                }
            }

            // Weight can be simply ignored if the weight system is not used but it doesn't cost anything to set it
            _currentWeightLimit = serializedInventory.CurrentWeightLimit > _inventoryConfig.MaxWeight ? _inventoryConfig.MaxWeight : serializedInventory.CurrentWeightLimit;

            DeserializeUnlockedStates(serializedInventory);

            if (InvalidItems.Count > 0)
            {
                Debug.LogWarning($"The following items were not found in the item database and were not loaded: {string.Join(", ", InvalidItems)}");
            }
        }

        protected void SerializeUnlockedStates(SerializedInventory serializedInventory)
        {
            foreach (var tabConfig in _slotsByTab.Keys)
            {
                var runtimeTabConfig = GetTabConfig(tabConfig);
                if (runtimeTabConfig != null)
                {
                    var states = runtimeTabConfig.GetSerializedSlotUnlockStates();
                    serializedInventory.UnlockedStatesByTab.Add(new SerializedTabUnlockState(tabConfig.Name, states));
                }
            }
        }

        protected void DeserializeUnlockedStates(SerializedInventory serializedInventory)
        {
            foreach (var serializedTabState in serializedInventory.UnlockedStatesByTab)
            {
                var tabType = _slotsByTab.Keys.FirstOrDefault(t => t.Name == serializedTabState.TabName);
                var slots = GetSlotsForTab(tabType);
                if (tabType != null)
                {
                    var runtimeTabConfig = GetTabConfig(tabType);
                    runtimeTabConfig?.InitializeFromSaveData(serializedTabState.UnlockedStates);
                    if (runtimeTabConfig != null)
                    {
                        for (int i = 0; i < slots.Count; i++)
                        {
                            if (!runtimeTabConfig.IsInventorySlotUnlocked(i))
                            {
                                slots[i].LockSlot();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Gets the slots for a specific tab type.
        /// </summary>
        public List<InventorySlot> GetSlotsForTab(InventoryTabConfigSO tabConfig)
        {
            return _slotsByTab.ContainsKey(tabConfig) ? _slotsByTab[tabConfig] : null;
        }

        /// <summary>
        /// Finds the first slot containing the specified item in the given tab.
        /// </summary>
        public InventorySlot FindSlotWithItem(InventoryTabConfigSO tabConfig, ItemSO item)
        {
            if (!_slotsByTab.ContainsKey(tabConfig)) return null;

            // If the system uses multiple slots for the same item, return the first slot found where the amount is less than the stack limit
            if (_inventoryConfig.UseSameItemInMultipleSlots && item.IsStackable && item.StackLimit >= 0)
            {
                return _slotsByTab[tabConfig].Find(slot => slot.Item == item && slot.Amount < item.StackLimit);
            }

            return _slotsByTab[tabConfig].Find(slot => slot.Item == item);
        }

        public InventorySlot FindFirstAvailableSlotForItem(InventoryTabConfigSO tab, ItemSO item)
        {
            if (!_slotsByTab.ContainsKey(tab)) return null;

            if (_inventoryConfig.UseSameItemInMultipleSlots && item.IsStackable && item.StackLimit >= 0)
            {
                var slot = _slotsByTab[tab].Find(slot => slot.Item == item && slot.Amount < item.StackLimit);
                if (slot != null) return slot;
            }

            return FindFirstUnlockedEmptySlot(tab);
        }

        /// <summary>
        /// Finds the last slot containing the specified item in the given tab.
        /// </summary>
        public InventorySlot FindSlotForRemoval(InventoryTabConfigSO tabConfig, ItemSO item)
        {
            if (!_slotsByTab.ContainsKey(tabConfig)) return null;

            if (_inventoryConfig.UseSameItemInMultipleSlots && item.IsStackable && item.StackLimit >= 0)
            {
                // Find last slot with the item
                return _slotsByTab[tabConfig].FindLast(slot => slot.Item == item && slot.Amount > 0);
            }

            return _slotsByTab[tabConfig].FindLast(slot => slot.Item == item && slot.Amount > 0);
        }

        /// <summary>
        /// Finds the first available slot in the given tab.
        /// </summary>
        public InventorySlot FindFirstUnlockedEmptySlot(InventoryTabConfigSO tabConfig)
        {
            if (!_slotsByTab.ContainsKey(tabConfig)) return null;

            return _slotsByTab[tabConfig].Find(slot => slot.IsEmpty && !slot.IsLocked);
        }

        /// <summary>
        /// Gets the tab configuration for a specific tab type.
        /// </summary>
        public RuntimeInventoryTabConfig GetTabConfig(InventoryTabConfigSO tabConfig)
        {
            return _tabConfigs.ContainsKey(tabConfig) ? _tabConfigs[tabConfig] : null;
        }

        /// <summary>
        /// Unlocks a specific slot in a specific tab.
        /// </summary>
        public void UnlockSlot(InventoryTabConfigSO tabConfig, int slotIndex)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return;

            var slots = _slotsByTab[tabConfig];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            var slot = slots[slotIndex];
            if (slot != null)
            {
                slot.UnlockSlot();
            }
        }

        /// <summary>
        /// Locks a specific slot in a specific tab.
        /// </summary>
        public void LockSlot(InventoryTabConfigSO tabConfig, int slotIndex)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return;

            var slots = _slotsByTab[tabConfig];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            var slot = slots[slotIndex];
            if (slot != null)
            {
                slot.LockSlot();
            }
        }
        #endregion

        #region Inventory Runtime Update
        /// <summary>
        /// Changes the weight limit of the inventory during runtime,
        /// triggering the inventory changed event. When the inventory
        /// will be saved, this value will be saved as well.
        /// </summary>
        public void ChangeWeightLimit(float newLimit)
        {
            _currentWeightLimit = newLimit;
            OnInventoryChanged?.Invoke(this);
        }
        #endregion
    }

    public struct InventoryUpdate
    {
        /// <summary>The item concerned by the update</summary>
        /// <remarks>Can be null if the update is not related to an item</remarks>
        public ItemSO Item;

        /// <summary>The slot concerned by the update</summary>
        public InventorySlot Slot;

        /// <summary>The index of the slot that was updated</summary>
        public int SlotIndex;

        /// <summary>If the item is remaining in the inventory firing the event</summary>
        public bool Remaining;

        /// <summary>The current inventory</summary>
        public RuntimeInventory Inventory;

        /// <summary>The type of update (e.g., Added, Removed, Sold, Bought)</summary>
        public InventoryUpdateType UpdateType;

        public InventoryUpdate(RuntimeInventory inventory, ItemSO item, InventorySlot slot, int slotIndex, bool remaining, InventoryUpdateType updateType)
        {
            Item = item;
            Slot = slot;
            SlotIndex = slotIndex;
            Remaining = remaining;
            Inventory = inventory;
            UpdateType = updateType;
        }
    }
}

