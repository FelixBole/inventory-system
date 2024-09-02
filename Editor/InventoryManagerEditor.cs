using Slax.Inventory.Editor;
using UnityEditor;
using UnityEngine;

namespace Slax.Inventory
{
    [CustomEditor(typeof(InventoryManager))]
    public class InventoryManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawDefaultInspector();

            if (GUILayout.Button("Open Inventory Tools"))
            {
                RuntimeInventoryViewerEditorWindow.OpenWindow((InventoryManager)target);
            }
        }
    }
}