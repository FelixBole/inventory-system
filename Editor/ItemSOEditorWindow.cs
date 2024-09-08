using Slax.InventorySystem.Runtime.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemSOEditorWindow : EditorWindow
{
    private ItemSO _item = null;

    public static void OpenFromInspector(ItemSO item)
    {
        
        ItemSOEditorWindow wnd = GetWindow<ItemSOEditorWindow>();
        string itemName = item.name;
        wnd.titleContent = new GUIContent("Item Editor - " + itemName);
        wnd._item = item;
        wnd.Show();
        wnd.CreateGUI();
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.Clear();

        if (_item != null)
        {
            VisualElement itemInspector = new Box();
            root.Add(itemInspector);
            itemInspector.Add(new InspectorElement(_item));
            root.Q<Button>("open-editor-button").style.display = DisplayStyle.None;
        }
    }
}
