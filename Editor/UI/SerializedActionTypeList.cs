using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using System;

namespace Slax.InventorySystem.Editor
{
    public class SerializedActionTypeList : SearchableSerializedObjectList
    {
        public SerializedActionTypeList(
            VisualElement container,
            Action<SerializedObject> onInventoryEditClicked,
            List<SerializedObject> objects = null
        ) : base(container, onInventoryEditClicked, null, InventorySystemAssetLoader.LoadItemActionTypesInProject, objects)
        {
            _onDeleteClicked = (so) => HandleDelete(so);
        }

        public override List<SerializedObject> DrawList(List<SerializedObject> objects = null)
        {
            objects = base.DrawList(objects);

            _listView.bindItem = (element, index) =>
            {
                if (index < 0 || index >= objects?.Count) return;

                SerializedObject so = objects[index];

                if (so == null) return;

                var title = element.Q<Label>(_titleLabelName);

                string objName = so.FindProperty("_name")?.stringValue;
                objName = objName ?? so.targetObject.name;

                if (title != null)
                {
                    title.text = objName;
                }

                SetupControls(element, so);
            };

            _listView.fixedItemHeight = 60;

            return objects;
        }

        void HandleDelete(SerializedObject so)
        {
            if (so == null) return;

            if (EditorUtility.DisplayDialog("Delete Action Type", "Are you sure you want to delete this action type?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(so.targetObject));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _objects.Remove(so);
                Reload();
            }
        }
    }
}
