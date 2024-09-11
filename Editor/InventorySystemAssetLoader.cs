

using System.Collections.Generic;
using Slax.InventorySystem.Runtime.Core;
using UnityEditor;

namespace Slax.InventorySystem.Editor
{
    /// <summary>
    /// Handles loading of Inventory System assets in the project.
    /// </summary>
    public class InventorySystemAssetLoader
    {
        #region Fields
        List<SerializedObject> _serializedInventories = new List<SerializedObject>();
        public List<SerializedObject> SerializedInventories => _serializedInventories;
        
        List<SerializedObject> _serializedItems = new List<SerializedObject>();
        public List<SerializedObject> SerializedItems => _serializedItems;

        List<SerializedObject> _serializedInventoryTabs = new List<SerializedObject>();
        public List<SerializedObject> SerializedInventoryTabs => _serializedInventoryTabs;

        List<SerializedObject> _serializedItemActionTypes = new List<SerializedObject>();
        public List<SerializedObject> SerializedItemActionTypes => _serializedItemActionTypes;

        List<SerializedObject> _serializedItemDatabases = new List<SerializedObject>();
        public List<SerializedObject> SerializedItemDatabases => _serializedItemDatabases;

        List<SerializedObject> _serializedInventorySaveSystems = new List<SerializedObject>();
        public List<SerializedObject> SerializedInventorySaveSystems => _serializedInventorySaveSystems;

        List<SerializedObject> _serializedSlotUnlockStates = new List<SerializedObject>();
        public List<SerializedObject> SerializedSlotUnlockStates => _serializedSlotUnlockStates;
        #endregion

        public InventorySystemAssetLoader(bool loadAssetsOnCreation = true)
        {
            if (loadAssetsOnCreation) LoadAssets();
        }

        /// <summary>
        /// Loads all Inventory Related Assets in the project.
        /// </summary>
        public void LoadAssets()
        {
            _serializedInventories = LoadInventoriesInProject();
            _serializedItems = LoadItemsInProject();
            _serializedInventoryTabs = LoadInventoryTabsInProject();
            _serializedItemActionTypes = LoadItemActionTypesInProject();
            _serializedItemDatabases = LoadItemDatabasesInProject();
            _serializedInventorySaveSystems = LoadInventorySaveSystemsInProject();
            _serializedSlotUnlockStates = LoadSlotUnlockStatesInProject();
        }

        public void Reload()
        {
            LoadAssets();
        }

        #region Reload Methods
        public List<SerializedObject> ReloadInventories()
        {
            _serializedInventories = LoadInventoriesInProject();
            return _serializedInventories;
        }

        public List<SerializedObject> ReloadItems()
        {
            _serializedItems = LoadItemsInProject();
            return _serializedItems;
        }

        public List<SerializedObject> ReloadInventoryTabs()
        {
            _serializedInventoryTabs = LoadInventoryTabsInProject();
            return _serializedInventoryTabs;
        }

        public List<SerializedObject> ReloadItemActionTypes()
        {
            _serializedItemActionTypes = LoadItemActionTypesInProject();
            return _serializedItemActionTypes;
        }

        public List<SerializedObject> ReloadItemDatabases()
        {
            _serializedItemDatabases = LoadItemDatabasesInProject();
            return _serializedItemDatabases;
        }

        public List<SerializedObject> ReloadInventorySaveSystems()
        {
            _serializedInventorySaveSystems = LoadInventorySaveSystemsInProject();
            return _serializedInventorySaveSystems;
        }

        public List<SerializedObject> ReloadSlotUnlockStates()
        {
            _serializedSlotUnlockStates = LoadSlotUnlockStatesInProject();
            return _serializedSlotUnlockStates;
        }
        #endregion


        #region Static Load Methods
        /// <summary>
        /// Returns a list of all InventorySO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadInventoriesInProject()
        {
            List<SerializedObject> inventories = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:InventorySO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventorySO inventory = AssetDatabase.LoadAssetAtPath<InventorySO>(path);
                inventories.Add(new SerializedObject(inventory));
            }

            return inventories;
        }

        /// <summary>
        /// Returns a list of all ItemSO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadItemsInProject()
        {
            List<SerializedObject> items = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:ItemSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
                items.Add(new SerializedObject(item));
            }

            return items;
        }

        /// <summary>
        /// Returns a list of all InventoryTabConfigSO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadInventoryTabsInProject()
        {
            List<SerializedObject> tabs = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:InventoryTabConfigSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventoryTabConfigSO tab = AssetDatabase.LoadAssetAtPath<InventoryTabConfigSO>(path);
                tabs.Add(new SerializedObject(tab));
            }

            return tabs;
        }

        /// <summary>
        /// Returns a list of all ItemActionTypeSO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadItemActionTypesInProject()
        {
            List<SerializedObject> actionTypes = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:ItemActionTypeSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemActionTypeSO actionType = AssetDatabase.LoadAssetAtPath<ItemActionTypeSO>(path);
                actionTypes.Add(new SerializedObject(actionType));
            }

            return actionTypes;
        }

        /// <summary>
        /// Returns a list of all ItemDatabaseSO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadItemDatabasesInProject()
        {
            List<SerializedObject> databases = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:ItemDatabaseSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemDatabaseSO database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(path);
                databases.Add(new SerializedObject(database));
            }

            return databases;
        }

        /// <summary>
        /// Returns a list of all InventorySaveSystemSO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadInventorySaveSystemsInProject()
        {
            List<SerializedObject> saveSystems = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:InventorySaveSystemSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventorySaveSystemSO saveSystem = AssetDatabase.LoadAssetAtPath<InventorySaveSystemSO>(path);
                saveSystems.Add(new SerializedObject(saveSystem));
            }

            return saveSystems;
        }

        /// <summary>
        /// Returns a list of all SlotUnlockStateSO assets in the project.
        /// Can also be called to reload.
        /// </summary>
        public static List<SerializedObject> LoadSlotUnlockStatesInProject()
        {
            List<SerializedObject> states = new List<SerializedObject>();

            string[] guids = AssetDatabase.FindAssets("t:SlotUnlockStateSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SlotUnlockStateSO state = AssetDatabase.LoadAssetAtPath<SlotUnlockStateSO>(path);
                states.Add(new SerializedObject(state));
            }

            return states;
        }
        #endregion
    }
}