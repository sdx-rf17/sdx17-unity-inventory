# Changelog

All notable changes to the Sdx17 Inventory Framework are documented here.
The format follows Keep a Changelog, and versioning follows Semantic Versioning.

## [0.1.0] - 2026-11-21

### Added
- `ItemDefinition` ScriptableObject: id, display name, rarity, stackable flag,
  max stack, weight, type (Weapon / Consumable / Material / Currency),
  heal and damage values, icon, and prefab reference.
- `Inventory`: 24 slots, stack merging, weight limit, Add / Remove / Has /
  Count, equip, use, and drop, plus a 5-slot hotbar.
- `InventoryService`: singleton MonoBehaviour, `OnChanged`, `OnItemAdded`,
  `OnItemEquipped` events, and save/load hooks through `ISaveData`
  for integration with the Sdx17 Secure Save system.
- `DemoItems` factory: creates the 8 demo items in code at runtime.
- `InventoryUIHooks`: event bridge the UI system subscribes to, plus a
  slot snapshot API for grid and hotbar rendering.
- Editor support: custom inspector for `ItemDefinition`, an editor window
  to create the 8 demo item assets, and menu items under `Sdx17/Inventory`.
- Demo: `DemoInventory` sample script and setup guide in `Samples~/Demo`.
- Self-test: `InventorySelfTest` MonoBehaviour logging PASS/FAIL for core
  paths in Play mode.

### Status
- v0.1.0 — UNTESTED IN UNITY. No Unity editor or Play-mode run has been
  performed. Do not treat this as tested or production-ready.
