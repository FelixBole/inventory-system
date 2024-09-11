using System;
using System.Collections.Generic;
using Slax.UIToolkit;
using Slax.UIToolkit.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Slax.InventorySystem.Editor
{
    public class SearchableSerializedObjectList
    {
        protected readonly string _titleLabelName = "label__title";
        protected readonly string _infoLabelName = "label__info";
        protected readonly string _editButtonName = "controls__edit";
        protected readonly string _deleteButtonName = "controls__delete";

        protected VisualElement _container = new VisualElement();
        protected ListView _listView = new ListView();
        public ListView ListView => _listView;

        protected Func<List<SerializedObject>> _loadAssetsFunc;
        protected Action<SerializedObject> _onEditClicked;
        protected Action<SerializedObject> _onDeleteClicked;

        protected List<SerializedObject> _objects = new List<SerializedObject>();
        public List<SerializedObject> Objects => _objects;

        protected float _itemHeight = 60;

        public SearchableSerializedObjectList(VisualElement container, Action<SerializedObject> onInventoryEditClicked, Action<SerializedObject> onDeleteClicked, Func<List<SerializedObject>> loadAssetsFunc, List<SerializedObject> objects = null)
        {
            _container = container;
            _listView = _container.Q<ListView>();

            if (_listView == null)
            {
                _listView = new ListView();
                _container.Add(_listView);
            }

            _onEditClicked = onInventoryEditClicked;
            _onDeleteClicked = onDeleteClicked;
            _objects = objects ?? loadAssetsFunc();
            _loadAssetsFunc = loadAssetsFunc;
            SetupSearch();
        }

        public void Reload()
        {
            DrawList();
        }

        public void SetItemHeight(float height)
        {
            _itemHeight = height;
        }

        public virtual List<SerializedObject> DrawList(List<SerializedObject> objects = null)
        {
            if (objects == null)
            {
                objects = _loadAssetsFunc();
                _objects = objects;
            }

            _listView.Clear();
            _listView.itemsSource = objects;
            _listView.makeItem = () =>
            {
                var listItemContainer = new VisualElement();
                QuickVisualElementProcessing.SetPadding(listItemContainer, 5);

                var ve = new VisualElement();
                listItemContainer.Add(ve);
                Color c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                QuickVisualElementProcessing.SetColorToBorders(ve, c);
                QuickVisualElementProcessing.SetBorderWidth(ve, 2);
                QuickVisualElementProcessing.SetBorderRadius(ve, 5);

                var rowTop = new Row();
                var rowBtm = new Row();
                ve.Add(rowTop);
                ve.Add(rowBtm);

                var title = Typography.H5("--").Grow();
                title.name = _titleLabelName;
                var info = Typography.Paragraph("");
                info.name = _infoLabelName;

                rowTop.Add(title);
                rowTop.Add(info);

                var editBtn = ActionButton.EditButton();
                editBtn.name = _editButtonName;
                var deleteBtn = ActionButton.DeleteButton();
                deleteBtn.name = _deleteButtonName;

                rowBtm.Add(editBtn);
                rowBtm.Add(deleteBtn);

                return listItemContainer;
            };
            _listView.selectionType = SelectionType.None;
            _listView.fixedItemHeight = _itemHeight;

            return objects;
        }

        protected void SetupControls(VisualElement element, SerializedObject obj)
        {
            var editBtn = element.Q<Button>(_editButtonName);
            if (editBtn.userData == null)
            {
                editBtn.clicked += () => HandleEditClicked(obj);
                editBtn.userData = true; // Set flag to avoid multiple registrations
            }

            var deleteBtn = element.Q<Button>(_deleteButtonName);

            if (deleteBtn.userData == null)
            {
                deleteBtn.clicked += () => HandleDeleteClicked(obj);
                deleteBtn.userData = true; // Set flag to avoid multiple registrations
            }
        }

        protected void SetupSearch()
        {
            var searchField = _container.Q<ToolbarSearchField>();

            if (searchField == null)
            {
                searchField = new ToolbarSearchField();
                _container.Insert(1, searchField); // Insert after label
            }

            searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string search = evt.newValue;
                if (string.IsNullOrWhiteSpace(search))
                {
                    DrawList();
                    return;
                }

                string actualSearch = search.ToLower();

                List<SerializedObject> searchResults = new List<SerializedObject>();
                foreach (SerializedObject obj in _objects)
                {
                    string name = obj.FindProperty("_name")?.stringValue?.ToLower() ?? string.Empty;
                    string objectName = obj.targetObject.name;

                    if (name.Contains(actualSearch) || objectName.Contains(actualSearch))
                    {
                        searchResults.Add(obj);
                    }
                }

                DrawList(searchResults);
            });
        }

        protected void HandleEditClicked(SerializedObject obj)
        {
            _onEditClicked?.Invoke(obj);
        }

        protected void HandleDeleteClicked(SerializedObject obj)
        {
            _onDeleteClicked?.Invoke(obj);
        }
    }
}
