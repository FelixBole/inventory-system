using System.Collections.Generic;
using Slax.InventorySystem.Runtime.Core;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Slax.UIToolkit;
using Slax.UIToolkit.Editor;

namespace Slax.InventorySystem.Editor
{
    public class SerializedInventoryList : SearchableSerializedObjectList
    {
        public SerializedInventoryList(VisualElement container, System.Action<SerializedObject> onInventoryEditClicked, List<SerializedObject> inventories = null) : base(container, onInventoryEditClicked, null, InventorySystemAssetLoader.LoadInventoriesInProject, inventories)
        {
            _onDeleteClicked = (so) => HandleDelete(so);
        }

        public override List<SerializedObject> DrawList(List<SerializedObject> inventories = null)
        {
            inventories = base.DrawList(inventories);

            _listView.bindItem = (element, index) =>
            {
                if (index >= inventories.Count || index < 0) return;

                SerializedObject inventory = inventories[index];

                if (inventory == null)
                {
                    Debug.LogError("SerializedObject is null");
                    return;
                }

                InventorySO inv = inventory.targetObject as InventorySO;

                if (inv == null)
                {
                    Debug.LogError("InventorySO is null");
                    return;
                }

                var title = element.Q<Label>(_titleLabelName);
                title.text = inv.Name;

                var tabs = element.Q<Label>(_infoLabelName);
                tabs.text = inv.TabConfigs.Count + " tabs";

                SetupControls(element, inventory);
            };

            _listView.fixedItemHeight = 60;

            return inventories;
        }

        void HandleDelete(SerializedObject inventory)
        {
            if (EditorUtility.DisplayDialog("Delete Inventory", "Are you sure you want to delete this inventory?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(inventory.targetObject));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _objects.Remove(inventory);
                Reload();
            }
        }
        
    }
}