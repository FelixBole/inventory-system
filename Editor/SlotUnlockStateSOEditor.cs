using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Slax.InventorySystem.Runtime.Core;

#if false

namespace Slax.InventorySystem.Editor
{
    [CustomEditor(typeof(SlotUnlockStateSO))]
    public class SlotUnlockStateSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _uxml;

        private SlotUnlockStateSO _slotUnlockStateSO;

        private void OnEnable()
        {
            _slotUnlockStateSO = (SlotUnlockStateSO)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            // return base.CreateInspectorGUI();

            var root = new VisualElement();
            _uxml.CloneTree(root); // Instantiates visual elements from the uxml file

            var foldout = new Foldout()
            {
                viewDataKey = "xxx", // Custom keys to remember the foldout state on domain reload
                text = "Slot Unlock State"
            };
            InspectorElement.FillDefaultInspector(foldout, serializedObject, this);

            root.Add(foldout);

            return root;
        }
    }
}

#endif
