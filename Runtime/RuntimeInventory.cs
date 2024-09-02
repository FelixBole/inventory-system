using System.Collections.Generic;
using UnityEngine.Events;

namespace Slax.Inventory
{
    /// <summary>
    /// A runtime inventory that offers all functionalities for managing
    /// the actions done on the inventory.
    /// </summary>
    [System.Serializable]
    public class RuntimeInventory
    {
        protected Dictionary<ItemTabTypeSO, List<InventorySlot>> _slotsByTab;
        public Dictionary<ItemTabTypeSO, List<InventorySlot>> SlotsByTab => _slotsByTab;

        protected Dictionary<ItemTabTypeSO, RuntimeInventoryTabConfig> _tabConfigs;

        protected InventorySO _inventoryConfig;
        public InventorySO InventoryConfig => _inventoryConfig;

        protected IInventorySaveSystem _saveSystem = new DefaultInventorySaveSystem();

        protected float _currentWeight = 0f;
        public float CurrentWeight => _currentWeight;

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
            _slotsByTab = new Dictionary<ItemTabTypeSO, List<InventorySlot>>();
            _tabConfigs = new Dictionary<ItemTabTypeSO, RuntimeInventoryTabConfig>();
            _inventoryConfig = inventory;

            // Initialize slots and tab configurations based on the inventory configuration
            InitializeTabs();
        }

        /// <summary>
        /// Initialize tabs and slots based on the InventorySO configuration.
        /// </summary>
        protected void InitializeTabs()
        {
            _slotsByTab.Clear();
            _tabConfigs.Clear();

            foreach (var tabConfig in _inventoryConfig.TabConfigs)
            {
                var runtimeTabConfig = new RuntimeInventoryTabConfig(tabConfig);
                _tabConfigs[tabConfig.TabType] = runtimeTabConfig;

                var slots = new List<InventorySlot>();
                for (int i = 0; i < runtimeTabConfig.MaxSlots; i++)
                {
                    InventorySlot inventorySlot = new InventorySlot();
                    if (runtimeTabConfig.IsInventorySlotLocked(i))
                    {
                        inventorySlot.LockSlot();
                    }
                    slots.Add(inventorySlot);
                    inventorySlot.OnSlotChanged += slot => OnInventoryChanged?.Invoke(this);
                    inventorySlot.OnSlotLocked += slot => OnInventoryChanged?.Invoke(this);
                    inventorySlot.OnSlotUnlocked += slot => OnInventoryChanged?.Invoke(this);
                }
                _slotsByTab[tabConfig.TabType] = slots;
            }

            OnInventoryChanged?.Invoke(this);
        }

        /// <summary>
        /// Adds an item to a specific slot in a specific tab.
        /// </summary>
        public void AddItemToSlot(ItemTabTypeSO tabType, int slotIndex, ItemSO item, int count = 1)
        {
            if (!_slotsByTab.ContainsKey(tabType))
                return;

            RuntimeInventoryTabConfig config = GetTabConfig(tabType);

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

            var slots = _slotsByTab[tabType];
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            slots[slotIndex].AddItem(item, count);
        }

        /// <summary>
        /// Removes an item from a specific slot in a specific tab.
        /// </summary>
        public void RemoveItemFromSlot(ItemTabTypeSO tabType, int slotIndex, int count = 1)
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
        /// Gets the slots for a specific tab type.
        /// </summary>
        public List<InventorySlot> GetSlotsForTab(ItemTabTypeSO tabType)
        {
            return _slotsByTab.ContainsKey(tabType) ? _slotsByTab[tabType] : null;
        }

        /// <summary>
        /// Finds the first slot containing the specified item in the given tab.
        /// </summary>
        public InventorySlot FindSlot(ItemTabTypeSO tabType, ItemSO item)
        {
            if (!_slotsByTab.ContainsKey(tabType)) return null;

            return _slotsByTab[tabType].Find(slot => slot.Item == item);
        }

        /// <summary>
        /// Gets the tab configuration for a specific tab type.
        /// </summary>
        public RuntimeInventoryTabConfig GetTabConfig(ItemTabTypeSO tabType)
        {
            return _tabConfigs.ContainsKey(tabType) ? _tabConfigs[tabType] : null;
        }

        /// <summary>
        /// Unlocks additional slots for a specific tab.
        /// </summary>
        public void UnlockSlotsForTab(ItemTabTypeSO tabType, SlotUnlockStateSO unlockState)
        {
            var config = GetTabConfig(tabType);
            if (config != null)
            {
                config.UnlockSlotState(unlockState);
            }

            OnInventoryChanged?.Invoke(this);
        }

        /// <summary>
        /// Creates a new save data structure for the inventory.
        /// </summary>
        public SerializedInventory CreateNewSaveData()
        {
            var newInventoryData = new SerializedInventory
            {
                InventoryName = _inventoryConfig.Name
            };

            foreach (var tab in _slotsByTab)
            {
                // Initialize slots and unlock states based on the default configuration
                var tabType = tab.Key;
                var runtimeTabConfig = GetTabConfig(tabType);
                if (runtimeTabConfig != null)
                {
                    newInventoryData.UnlockedStatesByTab[tabType.Name] = runtimeTabConfig.GetSerializedSlotUnlockStates();
                }
            }

            return newInventoryData;
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
                foreach (var slot in tab.Value)
                {
                    if (slot.Item != null)
                    {
                        serializedInventory.Slots.Add(new SerializedInventorySlot
                        {
                            ItemID = slot.Item.ID,
                            Amount = slot.Amount
                        });
                    }
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
            InitializeTabs(); // Ensure we have the correct number of slots

            int slotIndex = 0;
            foreach (var tab in _slotsByTab)
            {
                for (int i = 0; i < tab.Value.Count; i++)
                {
                    if (slotIndex >= serializedInventory.Slots.Count)
                        break;

                    var serializedSlot = serializedInventory.Slots[slotIndex];
                    var item = allItems.Find(it => it.ID == serializedSlot.ItemID);
                    if (item != null)
                    {
                        tab.Value[i].AddItem(item, serializedSlot.Amount);
                    }

                    slotIndex++;
                }
            }

            DeserializeUnlockedStates(serializedInventory);
        }

        /// <summary>
        /// Allows the user to set a custom save system. 
        /// Defaults to internal JSON save system.
        /// </summary>
        public void SetSaveSystem(IInventorySaveSystem saveSystem)
        {
            _saveSystem = saveSystem ?? new DefaultInventorySaveSystem();
        }

        /// <summary>
        /// The default save system save method for the InventorySystem.
        /// </summary>
        public void SaveInventory()
        {
            var serializedInventory = GetSaveData();
            _saveSystem.SaveInventory(serializedInventory, _inventoryConfig.name);
        }

        /// <summary>
        /// The default save system load method for the InventorySystem.
        /// </summary>
        public void LoadInventory(List<ItemSO> allItems)
        {
            var serializedInventory = _saveSystem.LoadInventory(_inventoryConfig.name);
            if (serializedInventory != null)
            {
                LoadSaveData(serializedInventory, allItems);
            }
        }

        private void SerializeUnlockedStates(SerializedInventory serializedInventory)
        {
            foreach (var tabType in _slotsByTab.Keys)
            {
                var runtimeTabConfig = GetTabConfig(tabType);
                if (runtimeTabConfig != null)
                {
                    serializedInventory.UnlockedStatesByTab[tabType.Name] = runtimeTabConfig.GetSerializedSlotUnlockStates();
                }
            }

            OnInventoryChanged?.Invoke(this);
        }

        private void DeserializeUnlockedStates(SerializedInventory serializedInventory)
        {
            foreach (var tabType in _slotsByTab.Keys)
            {
                if (serializedInventory.UnlockedStatesByTab.TryGetValue(tabType.Name, out var serializedUnlockStates))
                {
                    var runtimeTabConfig = GetTabConfig(tabType);
                    runtimeTabConfig?.InitializeFromSaveData(serializedUnlockStates);
                }
            }

            OnInventoryChanged?.Invoke(this);
        }

        #region Helper Methods
        /// <summary>
        /// Unlocks a specific slot in a specific tab.
        /// </summary>
        public void UnlockSlot(ItemTabTypeSO tabType, int slotIndex)
        {
            if (!_slotsByTab.ContainsKey(tabType))
                return;

            var slots = _slotsByTab[tabType];
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
        public void LockSlot(ItemTabTypeSO tabType, int slotIndex)
        {
            if (!_slotsByTab.ContainsKey(tabType))
                return;

            var slots = _slotsByTab[tabType];
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
