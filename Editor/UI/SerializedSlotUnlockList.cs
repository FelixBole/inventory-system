using System.Collections;
using System.Collections.Generic;
using Slax.InventorySystem.Runtime.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Slax.InventorySystem.Editor
{
    public class SerializedSlotUnlockList : SearchableSerializedObjectList
    {
        public SerializedSlotUnlockList(VisualElement container, System.Action<SerializedObject> onInventoryEditClicked, List<SerializedObject> slotUnlocks = null) : base(container, onInventoryEditClicked, null, InventorySystemAssetLoader.LoadSlotUnlockStatesInProject, slotUnlocks)
        {
            _onDeleteClicked = (so) => HandleDelete(so);
        }

        public override List<SerializedObject> DrawList(List<SerializedObject> objects = null)
        {
            objects = base.DrawList(objects);

            _listView.bindItem = (element, index) =>
            {
                if (index >= objects.Count || index < 0) return;

                SerializedObject slotUnlock = objects[index];

                if (slotUnlock == null)
                {
                    Debug.LogError("SerializedObject is null");
                    return;
                }

                SlotUnlockStateSO slotUnlockState = slotUnlock.targetObject as SlotUnlockStateSO;

                if (slotUnlockState == null)
                {
                    Debug.LogError("SlotUnlockState is null");
                    return;
                }

                var title = element.Q<Label>(_titleLabelName);
                title.text = slotUnlockState.name;
                var info = element.Q<Label>(_infoLabelName);
                info.text = slotUnlockState.AdditionalSlots + " slots";

                SetupControls(element, slotUnlock);
            };

            return objects;
        }

        void HandleDelete(SerializedObject slotUnlock)
        {
            if (EditorUtility.DisplayDialog("Delete Slot Unlock", "Are you sure you want to delete this slot unlock?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(slotUnlock.targetObject));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _objects.Remove(slotUnlock);
                Reload();
            }
        }
    }
}
