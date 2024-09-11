# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2024-09-11
### Editor Tooling update
- Add central management hub for inventory assets
- Add item gallery with quick multi item operations
- Add quick lookup / search of items from the editor window
- Add dependencies to my UI Toolkit lib for faster Editor edition

## [1.0.0] - 2024-09-02
Complete revamp of the Inventory System.
### Added
- **Separation of Configuration and Runtime Data**:
  - Introduced `RuntimeInventory` class, which handles all runtime inventory operations separately from the configuration data (`InventorySO`).
  - `InventorySO` now serves purely as a configuration object, defining the structure and properties of an inventory without holding runtime data.
  - `RuntimeInventory` manages the actual inventory state during gameplay, including items, slots, and runtime behaviors.

- **Flexible Save/Load System**:
  - Updated the save/load system to work with `RuntimeInventory`, ensuring that only runtime data is serialized and deserialized.
  - Introduced `InventorySaveSystemSO` class, allowing developers to implement custom save/load mechanisms or use the provided default JSON save system.
  - Implemented `DefaultInventorySaveSystem`, which saves runtime inventory data as JSON files in the `Application.persistentDataPath`.

- **Dynamic Slot Management**:
  - Added methods to `RuntimeInventory` for dynamic slot management, allowing slots to be added, removed, or unlocked during gameplay.

- **Item Slot Enhancements**:
  - Implemented item stacking within `InventorySlot`, providing flexible item management.

- **Event-Driven Architecture**:
  - Integrated event-driven behaviors into `RuntimeInventory`, triggering custom actions like `Use`, `Equip`, and `Drop` via events in `ItemSO`.

- **Runtime Viewing Tools**:
  - Added a runtime inventory analyzer window that can be displayed when the game is running with the click of a button on the inspector of the InventoryManager

- **Weight System as an Optional Extension**:
  - Added a `Weight` property to `ItemSO` and a `UseWeight` toggle to `InventorySO` to manage inventory weight limits.
  - Implemented the `OnWeightLimitReached` event to notify when the inventory’s weight limit is exceeded.

- **Size System as an Optional Extension**:
  - Introduced a slot limit system using `SlotUnlockStateSO` to progressively unlock slots during gameplay.
  - The first `SlotUnlockStateSO` defines the initial number of unlocked slots, replacing the previous `_baseSlots` approach.

- **Use Same Items in Multiple Slots Optional Extension**:
  - If toggled, this system will allow the same item to live in multiple slots, making it possible to have multiple stacks of the same item if it reaches its max stack size.

### Changed
- **Refactored Inventory Management**:
  - Updated `InventoryManager` to manage `RuntimeInventory` instead of directly interacting with `InventorySO`, improving clarity and extending functionality.
  - `UIInventoryTab` and related UI classes now interact with `RuntimeInventory` to dynamically update UI based on runtime data.

- **Enhanced Item and Tab Customization**:
  - Replaced `ItemActionType` and `ItemTabType` enums with `ItemActionTypeSO` and `InventoryTabConfig` ScriptableObjects, providing greater flexibility and extensibility for item actions and inventory tabs.
  
- **Refactored Access Modifiers**:
  - Updated private properties across the package to be protected, facilitating easier extensions and modifications by developers.

### Fixed
- **Event Handling Improvements**:
  - Ensured that events such as `OnAdd`, `OnRemove`, etc., in `RuntimeInventory` are triggered consistently and reliably.

### Deprecated
- **Old ItemStack System**:
  - Deprecated the old `ItemStack` class in favor of the new `InventorySlot` system, which offers improved modularity and easier management of items within inventories.

### Removed
- **Legacy ItemStack System**:
  - Removed reliance on the `ItemStack` class and related methods, streamlining the inventory system.
  - Eliminated the need for enums (`ItemActionType`, `ItemTabType`), ensuring that new item actions and tabs can be added via ScriptableObjects without modifying core code.

### Documentation
- **Updated README**:
  - Expanded the README file to include detailed instructions on setting up and using the inventory system, with sections covering item creation, slot management, script communication, and saving/loading.
  - Added Mermaid diagrams to the README to illustrate class communication and relationships within the system.

## [0.1.4] - 2024-08-03

### Feat
- Modified `InventoryManager` to accept `virtual` methods, making it easier to extend.

## [0.1.3] - 2024-08-03

### Refactor
- Moved package to a Unity project for faster iterations.

## [0.1.2] - 2024-08-03

### Fixed
- Fixed missing meta file error.

## [0.1.1] - 2024-08-03

### Fixed
- Fixed samples not being available on installation.
