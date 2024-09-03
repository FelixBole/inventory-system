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

        public List<string> InvalidItems { get; private set; } = new List<string>();

        /// <summary>
        /// Event triggered when an item cannot be added due to weight limit.
        /// </summary>
        public UnityAction<float, float> OnWeightLimitReached;

        /// <summary>
        /// Event triggered when an item cannot be added due to size limit.
        /// </summary>
        public UnityAction<ItemSO, int> OnSizeLimitReached;

        public UnityAction<RuntimeInventory> OnInventoryChanged = delegate { };

        public RuntimeInventory(InventorySO inventory)
        {
            _slotsByTab = new Dictionary<InventoryTabConfigSO, List<InventorySlot>>();
            _tabConfigs = new Dictionary<InventoryTabConfigSO, RuntimeInventoryTabConfig>();
            _inventoryConfig = inventory;

            // Initialize slots and tab configurations based on the inventory configuration
            InitializeTabs();
        }

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

        /// <summary>
        /// Adds an item to the first available slot in the first available tab.
        /// Easy to use, but less efficient than the other overload add methods.
        /// </summary>
        public void AddItemToSlot(ItemSO item, int count = 1)
        {
            var itemTab = item.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab != null)
            {
                AddItemToSlot(itemTab, item, count);
            }
        }

        /// <summary>
        /// Adds an item to the first available slot in a specific tab.
        /// </summary>
        public void AddItemToSlot(InventoryTabConfigSO tabConfig, ItemSO item, int count = 1)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return;

            var existingSlot = FindSlot(tabConfig, item);
            if (existingSlot != null)
            {
                int existingSlotIndex = _slotsByTab[tabConfig].IndexOf(existingSlot);
                AddItemToSlot(tabConfig, existingSlotIndex, item, count);
            }
            else
            {
                int firstAvailableSlot = _slotsByTab[tabConfig].FindIndex(slot => slot.IsEmpty && !slot.IsLocked);
                if (firstAvailableSlot >= 0)
                {
                    AddItemToSlot(tabConfig, firstAvailableSlot, item, count);
                }
                else
                {
                    OnSizeLimitReached?.Invoke(item, -1);
                }
            }
        }

        /// <summary>
        /// Adds an item to a specific slot in a specific tab.
        /// </summary>
        public void AddItemToSlot(InventoryTabConfigSO tabConfig, int slotIndex, ItemSO item, int count = 1)
        {
            if (!_slotsByTab.ContainsKey(tabConfig))
                return;

            RuntimeInventoryTabConfig config = GetTabConfig(tabConfig);

            if (config == null)
                return;

            if (slotIndex >= config.MaxSlots)
            {
                OnSizeLimitReached?.Invoke(item, slotIndex);
                return;
            }

            if (_inventoryConfig.UseWeight)
            {
                float itemWeight = item.Weight * count;
                if (_currentWeight + itemWeight > _inventoryConfig.MaxWeight)
                {
                    OnWeightLimitReached?.Invoke(_currentWeight, _inventoryConfig.MaxWeight);
                    return;
                }
                _currentWeight += itemWeight;
            }

            var slots = _slotsByTab[tabConfig];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            slots[slotIndex].AddItem(item, count);
        }

        /// <summary>
        /// Removes an item from the inventory.
        /// Easier to use than the other remove methods, but less efficient.
        /// </summary>
        public void RemoveItemFromSlot(ItemSO item, int count = 1)
        {
            var itemTab = item.TabConfigs.FirstOrDefault(t => _slotsByTab.ContainsKey(t));
            if (itemTab != null)
            {
                var slot = FindSlot(itemTab, item);
                if (slot != null)
                {
                    RemoveItemFromSlot(itemTab, _slotsByTab[itemTab].IndexOf(slot), count);
                }
            }
        }

        /// <summary>
        /// Removes an item from a specific slot in a specific tab.
        /// </summary>
        public void RemoveItemFromSlot(InventoryTabConfigSO tabType, int slotIndex, int count = 1)
        {
            if (!_slotsByTab.ContainsKey(tabType))
                return;

            var slots = _slotsByTab[tabType];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            var slot = slots[slotIndex];
            if (slot != null && !slot.IsEmpty)
            {
                if (_inventoryConfig.UseWeight)
                {
                    float itemWeight = slot.Item.Weight * count;
                    _currentWeight -= itemWeight;
                }
                slot.RemoveItem(count);
            }
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

            DeserializeUnlockedStates(serializedInventory);

            if (InvalidItems.Count > 0)
            {
                Debug.LogWarning($"The following items were not found in the item database and were not loaded: {string.Join(", ", InvalidItems)}");
            }
        }

        private void SerializeUnlockedStates(SerializedInventory serializedInventory)
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

        private void DeserializeUnlockedStates(SerializedInventory serializedInventory)
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
        public InventorySlot FindSlot(InventoryTabConfigSO tabConfig, ItemSO item)
        {
            if (!_slotsByTab.ContainsKey(tabConfig)) return null;

            return _slotsByTab[tabConfig].Find(slot => slot.Item == item);
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
    }
}
