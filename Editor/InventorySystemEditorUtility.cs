using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if false

namespace Slax.InventorySystem.Editor
{
    public static class InventorySystemEditorUtility
    {
        [SerializeField] private static VisualTreeAsset _itemImageContainerUXML = null;

        // public static VisualElement DrawItemImage(VisualElement root, SerializedObject itemSerializedObject)
        // {
        //     if (_itemImageContainerUXML == null) return root;

        //     Sprite previewSprite = itemSerializedObject.FindProperty("_previewSprite").objectReferenceValue as Sprite;
            

        //     if (previewSprite == null) return root;

        //     VisualElement itemImageContainer = _itemImageContainerUXML.CloneTree();

        // }
    }
}

#endif