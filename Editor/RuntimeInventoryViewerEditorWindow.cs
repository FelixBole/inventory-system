using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Slax.Inventory.Editor
{
    public class RuntimeInventoryViewerEditorWindow : EditorWindow
    {
        private InventoryManager _selectedInventoryManager;
        private RuntimeInventory _runtimeInventory;
        private Vector2 _scrollPosition;
        private Vector2 _sidebarScrollPosition;
        private string _searchQuery = string.Empty;

        private List<string> _eventLogs = new List<string>();
        private Vector2 _eventLogScrollPosition;

        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Inventory", "Logs" };

        private ItemTabTypeSO _selectedInventoryTab;

        [MenuItem("Slax/Runtime Inventory Viewer")]
        public static void ShowWindow()
        {
            GetWindow<RuntimeInventoryViewerEditorWindow>("Runtime Inventory Viewer");
        }

        public static void OpenWindow(InventoryManager inventoryManager)
        {
            var window = GetWindow<RuntimeInventoryViewerEditorWindow>("Runtime Inventory Viewer");
            window._selectedInventoryManager = inventoryManager;
            window._runtimeInventory = inventoryManager.RuntimeInventory;
            window.SubscribeToEvents();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                UnsubscribeFromEvents();
                _selectedInventoryManager = null;
                _runtimeInventory = null;
            }
        }

        private void SubscribeToEvents()
        {
            if (_runtimeInventory != null)
            {
                _runtimeInventory.OnInventoryChanged += LogInventoryChanged;

                if (_runtimeInventory.SlotsByTab != null)
                {
                    foreach (var tab in _runtimeInventory.SlotsByTab)
                    {
                        foreach (var slot in tab.Value)
                        {
                            slot.OnSlotChanged += LogSlotChanged;
                            slot.OnSlotLocked += LogSlotLocked;
                            slot.OnSlotUnlocked += LogSlotUnlocked;
                        }
                    }
                }
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_runtimeInventory != null)
            {
                _runtimeInventory.OnInventoryChanged -= LogInventoryChanged;

                if (_runtimeInventory.SlotsByTab != null)
                {
                    foreach (var tab in _runtimeInventory.SlotsByTab)
                    {
                        foreach (var slot in tab.Value)
                        {
                            slot.OnSlotChanged -= LogSlotChanged;
                            slot.OnSlotLocked -= LogSlotLocked;
                            slot.OnSlotUnlocked -= LogSlotUnlocked;
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Runtime Inventory Viewer", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This tool only works in Play mode. Please start the game to inspect RuntimeInventory.", MessageType.Warning);
                return;
            }

            var newSelectedManager = (InventoryManager)EditorGUILayout.ObjectField("Inventory Manager", _selectedInventoryManager, typeof(InventoryManager), true);

            if (newSelectedManager != _selectedInventoryManager)
            {
                UnsubscribeFromEvents();
                _selectedInventoryManager = newSelectedManager;

                if (_selectedInventoryManager != null)
                {
                    _runtimeInventory = _selectedInventoryManager.RuntimeInventory;
                    _selectedInventoryTab = null;
                    SubscribeToEvents();
                }
            }

            if (_runtimeInventory == null)
            {
                if (_selectedInventoryManager != null)
                {
                    EditorGUILayout.HelpBox("No RuntimeInventory associated with the selected InventoryManager.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("Select an InventoryManager in the scene to view its RuntimeInventory.", MessageType.Info);
                }
                return;
            }

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            switch (_selectedTab)
            {
                case 0:
                    DrawInventoryTab();
                    break;
                case 1:
                    DrawLogsTab();
                    break;
            }
        }

        private void DrawInventoryTab()
        {
            EditorGUILayout.BeginHorizontal();

            // Sidebar for tab selection
            _sidebarScrollPosition = EditorGUILayout.BeginScrollView(_sidebarScrollPosition, GUILayout.Width(150));
            DrawSidebar();
            EditorGUILayout.EndScrollView();

            // Main content area
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DisplayRuntimeInventoryInfo();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            if (_runtimeInventory == null || _runtimeInventory.SlotsByTab == null) return;

            GUILayout.Label("Tabs", EditorStyles.boldLabel);

            foreach (var tab in _runtimeInventory.SlotsByTab.Keys)
            {
                if (GUILayout.Button(tab.Name))
                {
                    _selectedInventoryTab = tab;
                }
            }
        }

        private void DrawLogsTab()
        {
            if (GUILayout.Button("Clear Logs"))
            {
                _eventLogs.Clear();
            }

            GUILayout.Space(10);
            GUILayout.Label("Event Logs", EditorStyles.boldLabel);

            _eventLogScrollPosition = EditorGUILayout.BeginScrollView(_eventLogScrollPosition, GUILayout.Height(150));
            foreach (var log in _eventLogs)
            {
                GUILayout.Label(log, EditorStyles.label);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DisplayRuntimeInventoryInfo()
        {
            if (_runtimeInventory == null || _runtimeInventory.InventoryConfig == null) return;

            GUILayout.Space(10);
            GUILayout.Label($"Inventory Name: {_runtimeInventory.InventoryConfig.Name}", EditorStyles.largeLabel);
            GUILayout.Label($"Current Weight: {_runtimeInventory.CurrentWeight} / {_runtimeInventory.InventoryConfig.MaxWeight}", EditorStyles.label);

            if (_selectedInventoryTab != null)
            {
                GUILayout.Space(5);
                GUILayout.Label($"Tab: {_selectedInventoryTab.Name}", EditorStyles.boldLabel);

                var slots = _runtimeInventory.GetSlotsForTab(_selectedInventoryTab);
                if (slots == null) return;

                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (slot == null) continue;

                    if (!string.IsNullOrEmpty(_searchQuery) && (slot.Item == null || !slot.Item.Name.ToLower().Contains(_searchQuery.ToLower())))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.BeginHorizontal();

                    string slotStatus = slot.IsLocked ? "Locked" : slot.IsEmpty ? "Empty" : $"{slot.Item.Name} (x{slot.Amount})";
                    GUILayout.Label($"Slot {i + 1}: {slotStatus}", EditorStyles.label);

                    if (slot.IsLocked)
                    {
                        if (GUILayout.Button("Unlock", GUILayout.Width(70)))
                        {
                            slot.UnlockSlot();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Lock", GUILayout.Width(70)))
                        {
                            slot.LockSlot();
                        }

                        if (!slot.IsEmpty)
                        {
                            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                            {
                                slot.ClearSlot();
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    ItemSO item = (ItemSO)EditorGUILayout.ObjectField("Item", slot.Item, typeof(ItemSO), false);
                    if (item != null && item != slot.Item)
                    {
                        slot.AddItem(item, 1);
                    }

                    if (GUILayout.Button("+"))
                    {
                        slot.AddItem(slot.Item, 1);
                    }

                    if (GUILayout.Button("-"))
                    {
                        slot.RemoveItem(1);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                GUILayout.Label("Select a tab from the sidebar to view details.", EditorStyles.helpBox);
            }
        }

        private void LogInventoryChanged(RuntimeInventory runtimeInventory)
        {
            _eventLogs.Add($"[{System.DateTime.Now:HH:mm:ss}] Inventory changed.");
            Repaint();
        }

        private void LogSlotChanged(InventorySlot slot)
        {
            _eventLogs.Add($"[{System.DateTime.Now:HH:mm:ss}] Slot changed: {slot.Item?.Name ?? "Empty"} x{slot.Amount}.");
            Repaint();
        }

        private void LogSlotLocked(InventorySlot slot)
        {
            _eventLogs.Add($"[{System.DateTime.Now:HH:mm:ss}] Slot locked.");
            Repaint();
        }

        private void LogSlotUnlocked(InventorySlot slot)
        {
            _eventLogs.Add($"[{System.DateTime.Now:HH:mm:ss}] Slot unlocked.");
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
