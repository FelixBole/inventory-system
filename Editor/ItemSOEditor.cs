using UnityEngine;
using UnityEditor;
using Slax.InventorySystem.Runtime.Core;
using Slax.Utils.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Slax.InventorySystem.Editor
{
    [CustomEditor(typeof(ItemSO))]
    public class ItemSOEditor : UnityEditor.Editor
    {
        [SerializeField] protected VisualTreeAsset _uxml;
        [SerializeField] protected VisualTreeAsset _pilTabUxml;

        protected Foldout _foldoutHelp;

        protected VisualElement _container;
        protected VisualElement _containerToolbarResult;
        protected ToolbarToggle _toolbarToggleMain;
        protected ToolbarToggle _toolbarToggleUi;
        protected ToolbarToggle _toolbarToggleConfig;
        protected ToolbarToggle _toolbarToggleExtensions;

        protected VisualElement _containerMainInfo;
        protected VisualElement _containerUI;
        protected VisualElement _containerConfig;
        protected VisualElement _containerExtensions;

        protected VisualElement _containerTabs;
        protected VisualElement _itemSprite;
        protected Label _labelItemStacks;
        protected VisualElement _warningNoTabs;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            _uxml.CloneTree(root);

            // Open Item Editor Button
            root.Q<Button>("open-editor-button").clicked += () =>
            {
                ItemSOEditorWindow.OpenFromInspector(target as ItemSO);
            };

            // Registering Stuff and setting up the UI
            RegisterVisualElementVars(root);
            RegisterToolbarToggleEvents();
            RegisterConfigCallbacks(root);

            SetupToggles();
            CustomEditorUtility.ToggleDisplay(_warningNoTabs, false);

            _foldoutHelp.value = false;

            // Track changes in the tab configs
            var propertyFieldTabConfigs = root.Q<PropertyField>("pf-tab-configs");

            propertyFieldTabConfigs.TrackSerializedObjectValue(serializedObject, (so) =>
            {
                RedrawTabs();
            });

            // Drawing the UI

            DrawRecap(root);
            // DrawFoldoutDefaultInspector(root);

            return root;
        }

        void RegisterVisualElementVars(VisualElement root)
        {
            // Main container
            _container = root.Q<VisualElement>("window-container");

            // Help section
            _foldoutHelp = root.Q<Foldout>("foldout-help");

            // Toolbar Content
            _containerToolbarResult = root.Q<VisualElement>("container-toolbar-result");

            _toolbarToggleMain = root.Q<ToolbarToggle>("toolbar-toggle-main");
            _toolbarToggleUi = root.Q<ToolbarToggle>("toolbar-toggle-ui");
            _toolbarToggleConfig = root.Q<ToolbarToggle>("toolbar-toggle-config");
            _toolbarToggleExtensions = root.Q<ToolbarToggle>("toolbar-toggle-extensions");

            _containerMainInfo = root.Q<VisualElement>("container-main-info");
            _containerUI = root.Q<VisualElement>("container-ui");
            _containerConfig = root.Q<VisualElement>("container-config");
            _containerExtensions = root.Q<VisualElement>("container-extensions");

            // Recap section elements
            _containerTabs = root.Q<VisualElement>("container-tabs");
            _itemSprite = root.Q<VisualElement>("item-sprite");
            _labelItemStacks = root.Q<Label>("label-item-stacks");
            _warningNoTabs = root.Q<VisualElement>("warning-no-tabs");
        }

        void RegisterConfigCallbacks(VisualElement root)
        {
            var isUniquePF = root.Q<PropertyField>("pf-is-unique");
            var isStackablePF = root.Q<PropertyField>("pf-is-stackable");
            var stackLimitPF = root.Q<PropertyField>("pf-stack-limit");
            var btnSetStackInfinite = root.Q<Button>("btn-set-stack-infinite");

            isUniquePF.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (evt.newValue)
                {
                    _labelItemStacks.text = "";
                    CustomEditorUtility.ToggleDisplay(new VisualElement[] { isStackablePF, stackLimitPF, btnSetStackInfinite }, false);
                }
                else
                {
                    _labelItemStacks.text = serializedObject.FindProperty("_stackLimit").intValue.ToString();
                    CustomEditorUtility.ToggleDisplay(new VisualElement[] { isStackablePF, stackLimitPF, btnSetStackInfinite }, true);
                }
            });

            isStackablePF.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (!evt.newValue)
                {
                    _labelItemStacks.text = "";
                }
                else
                {
                    _labelItemStacks.text = serializedObject.FindProperty("_stackLimit").intValue.ToString();
                }
            });

            stackLimitPF.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                bool isStackable = serializedObject.FindProperty("_isStackable").boolValue;
                bool isUnique = serializedObject.FindProperty("_isUnique").boolValue;

                if (!isUnique)
                {
                    int value = evt.newValue;
                    if (value < 0)
                    {
                        _labelItemStacks.text = "∞";
                        CustomEditorUtility.ToggleDisplay(btnSetStackInfinite, false);
                    }
                    else
                    {
                        _labelItemStacks.text = value.ToString();
                        CustomEditorUtility.ToggleDisplay(btnSetStackInfinite, true);
                    }
                }
            });

            btnSetStackInfinite.clicked += () =>
            {
                int current = serializedObject.FindProperty("_stackLimit").intValue;
                serializedObject.FindProperty("_stackLimit").intValue = -1;
                serializedObject.ApplyModifiedProperties();
                stackLimitPF.SendEvent(ChangeEvent<int>.GetPooled(current, -1));
                CustomEditorUtility.ToggleDisplay(btnSetStackInfinite, false);
            };
        }

        void RedrawTabs()
        {
            _containerTabs.Clear();
            SerializedProperty tabsProperty = serializedObject.FindProperty("_tabConfigs");

            CustomEditorUtility.ToggleDisplay(_warningNoTabs, tabsProperty.arraySize == 0);

            for (int i = 0; i < tabsProperty.arraySize; i++)
            {
                SerializedProperty tabProperty = tabsProperty.GetArrayElementAtIndex(i);
                InventoryTabConfigSO tabConfig = tabProperty.objectReferenceValue as InventoryTabConfigSO;

                if (tabConfig == null) continue;

                // Create a new tab
                VisualElement tab = _pilTabUxml.CloneTree();
                tab.Q<Label>().text = tabConfig.name;
                _containerTabs.Add(tab);
            }
        }

        void DrawRecap(VisualElement root)
        {
            var previewSpritePropertyField = root.Q<PropertyField>("pf-preview-sprite");

            previewSpritePropertyField.RegisterCallback<ChangeEvent<Object>>((evt) =>
            {
                Sprite sprite = evt.newValue as Sprite;
                if (sprite == null) return;
                _itemSprite.style.backgroundImage = sprite.texture;
            });

            var colorPF = root.Q<PropertyField>("pf-color");
            var backgroundColorPF = root.Q<PropertyField>("pf-background-color");
            var selectedColorPF = root.Q<PropertyField>("pf-selected-color");

            colorPF.RegisterCallback<ChangeEvent<Color>>((evt) =>
            {
                _itemSprite.style.unityBackgroundImageTintColor = evt.newValue;
            });

            backgroundColorPF.RegisterCallback<ChangeEvent<Color>>((evt) =>
            {
                _itemSprite.style.backgroundColor = evt.newValue;
            });

            selectedColorPF.RegisterCallback<ChangeEvent<Color>>((evt) =>
            {
                _itemSprite.style.borderBottomColor = evt.newValue;
            });

            SerializedProperty previewSpriteProperty = serializedObject.FindProperty("_previewSprite");

            Sprite previewSprite = previewSpriteProperty.objectReferenceValue as Sprite;
            _itemSprite.style.backgroundImage = previewSprite.texture;

            // Commented because was able to set scale to fit on the bg of the VisualElement in UI Builder
            // var size = new BackgroundSize(BackgroundSizeType.Contain);
            // var bgSize = new StyleBackgroundSize(size);
            // spriteContainer.style.backgroundSize = bgSize;

            RedrawTabs();
        }

        void SetupToggles()
        {
            _toolbarToggleMain.value = true;
            _toolbarToggleUi.value = true;
            _toolbarToggleConfig.value = true;
            _toolbarToggleExtensions.value = false;
            CustomEditorUtility.ToggleDisplay(_containerExtensions, false);
            CustomEditorUtility.ToggleDisplay(new VisualElement[] { _containerMainInfo, _containerUI, _containerConfig }, true);
        }

        void RegisterToolbarToggleEvents()
        {
            _toolbarToggleMain.RegisterValueChangedCallback((evt) =>
            {
                CustomEditorUtility.ToggleDisplay(_containerMainInfo, evt.newValue);
            });

            _toolbarToggleUi.RegisterValueChangedCallback((evt) =>
            {
                CustomEditorUtility.ToggleDisplay(_containerUI, evt.newValue);
            });

            _toolbarToggleConfig.RegisterValueChangedCallback((evt) =>
            {
                CustomEditorUtility.ToggleDisplay(_containerConfig, evt.newValue);
            });

            _toolbarToggleExtensions.RegisterValueChangedCallback((evt) =>
            {
                CustomEditorUtility.ToggleDisplay(_containerExtensions, evt.newValue);
            });
        }

        void DrawFoldoutDefaultInspector(VisualElement root)
        {
            var foldout = new Foldout() { viewDataKey = "itemSoFoldout", text = "Full Inspector" };
            foldout.AddToClassList("fixed-foldout-margin");
            InspectorElement.FillDefaultInspector(foldout, serializedObject, this);

            // We add to container and not root because container is already in the scroll view
            _container.Add(foldout);
        }
    }
}
