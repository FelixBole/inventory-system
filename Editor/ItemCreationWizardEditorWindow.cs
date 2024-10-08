using Slax.InventorySystem.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemCreationWizardEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset _uxml = default;

    [MenuItem("Window/Slax/Inventory/Item Creation Wizard")]
    public static void OpenWindow()
    {
        ItemCreationWizardEditorWindow wnd = GetWindow<ItemCreationWizardEditorWindow>();
        wnd.titleContent = new GUIContent("ItemCreationWizardEditorWindow");
    }

    public void CreateGUI()
    {
        VisualElement tree = _uxml.Instantiate();
        rootVisualElement.Add(tree);
        new ItemCreationWizardEditor(rootVisualElement);
    }
}
