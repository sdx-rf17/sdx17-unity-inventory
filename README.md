# Sdx17 Inventory Framework

**Version 0.1.0 — UNTESTED IN UNITY**

> This package has never been run in the Unity editor or in Play mode.
> It was written and reviewed as C# source only, in an environment with
> no Unity installed. Do not treat it as tested, verified, or
> production-ready until the test checklist at the end of this file has
> been run in Unity 2022.3 LTS and every step passes.

A lightweight inventory framework for Unity 2022.3 LTS (URP-compatible,
no paid dependencies): item definitions as ScriptableObjects, a 24-slot
inventory with stacking and a weight limit, a 5-slot hotbar, equip / use /
drop, events for UI and gameplay, and save/load hooks for the Sdx17
Secure Save system.

## What it is

- `ItemDefinition` ScriptableObjects: id, name, rarity, stackable flag,
  max stack, weight, type, heal / damage, icon, prefab.
- `Inventory`: 24 slots, stack merging, weight limit, Add / Remove / Has /
  Count, equip, use, drop, 5-slot hotbar.
- `InventoryService`: singleton that owns the player inventory and raises
  `OnChanged`, `OnItemAdded`, `OnItemRemoved`, `OnItemEquipped`,
  `OnItemUsed` events.
- Save/load through the `ISaveData` interface — the inventory never
  touches files, PlayerPrefs, or encryption itself.
- `InventoryUIHooks`: snapshot data (grid + hotbar) and click/key
  forwarding, so any UI system can render the inventory without the
  inventory knowing about UI.
- Editor support: custom inspector for items, a window to create the
  8 demo item assets, and menu items under `Sdx17 > Inventory`.
- A demo scene guide and a Play-mode self-test that logs PASS/FAIL.

## What it is NOT

- Not tested in Unity. No Play-mode run, no editor import, and no build
  have been performed.
- Not a UI. It ships no Canvas, no prefabs, and no art. The UI system
  renders from `InventoryUIHooks` snapshots.
- Not a save system. It only produces and restores `InventorySaveData`;
  the Sdx17 Secure Save system must implement `ISaveData`.
- Not a combat, quest, or health system. `Use()` returns a heal amount;
  the caller's health system applies it. `Equip()` records the equipped
  weapon; the combat system reads `EquippedItem.Damage`.
- No multiplayer, no networking, no database.

## Setup (under 5 minutes)

1. Copy this package into your Unity project (as a UPM package or
   directly under `Packages/com.sdx17.inventory`).
2. In Unity, wait for import and check the Console for compile errors.
3. Create an empty GameObject named `InventoryService` and add the
   `InventoryService` component — or use
   `Sdx17 > Inventory > Add Inventory Service to Scene`.
4. Add your items: either run `Sdx17 > Inventory > Create Demo Items`
   (creates 8 `.asset` files under `Assets/Sdx17/Inventory/DemoItems`),
   or create your own via `Assets > Create > Sdx17 > Inventory > Item
   Definition`. Drag the items into the service's Item Catalog field
   (or register them in code with `RegisterItem`).
5. Press Play and add an item from code, e.g.
   `InventoryService.Instance.AddItemById("health_potion", 3)`.

## File table

| Path | Contents |
|------|----------|
| `package.json` | UPM package manifest (`com.sdx17.inventory`, 0.1.0) |
| `Runtime/ItemDefinition.cs` | `ItemDefinition`, `ItemType`, `ItemRarity` |
| `Runtime/InventorySlot.cs` | One stack slot: merge, add, remove |
| `Runtime/Inventory.cs` | 24 slots + 5 hotbar, weight limit, equip/use/drop, save data |
| `Runtime/InventoryService.cs` | Singleton, events, save/load wiring |
| `Runtime/ISaveData.cs` | `ISaveData` interface, `InventorySaveData`, `InventorySaveEntry` |
| `Runtime/DemoItems.cs` | Factory creating the 8 demo items in code |
| `Runtime/InventoryUIHooks.cs` | `SlotViewData` snapshots + UI input forwarding |
| `Runtime/Sdx17.Inventory.Runtime.asmdef` | Runtime assembly definition |
| `Editor/ItemDefinitionEditor.cs` | Custom inspector for `ItemDefinition` |
| `Editor/InventoryEditorWindow.cs` | Window + menu item creating the 8 demo `.asset` files |
| `Editor/InventoryMenuItems.cs` | Menu items: create item, add service to scene |
| `Editor/Sdx17.Inventory.Editor.asmdef` | Editor assembly definition (references runtime) |
| `Samples~/Demo/DemoInventory.cs` | Demo driver: starter items + number-key actions |
| `Samples~/Demo/README.md` | Demo scene setup guide |
| `Tests~/InventorySelfTest.cs` | Play-mode self-test logging PASS/FAIL |
| `CHANGELOG.md` | Version history |
| `LICENSE` | MIT, Copyright 2026 Sdx17 |

## Config options

| Setting | Where | Default | Notes |
|---------|-------|---------|-------|
| Max weight | `InventoryService` inspector, `maxWeight` | 100 | Adds that would exceed it are refused in full |
| Item catalog | `InventoryService` inspector, `itemCatalog` | empty | Definitions the service can resolve by id (needed for save/load) |
| Add starter items | `DemoInventory` inspector | on | Demo only |

Per-item settings live on each `ItemDefinition`: id (unique, lowercase
with underscores), display name, type, rarity, stackable, max stack
(forced to 1 when not stackable), weight, heal (consumables), damage
(weapons), icon, prefab (spawned on drop), description.

## The 8 demo items

| Id | Name | Type | Rarity | Stack | Max | Weight | Heal / Damage |
|----|------|------|--------|-------|-----|--------|---------------|
| `iron_sword` | Iron Sword | Weapon | Common | No | 1 | 3.5 | 12 dmg |
| `hunter_bow` | Hunter Bow | Weapon | Uncommon | No | 1 | 2.0 | 9 dmg |
| `health_potion` | Health Potion | Consumable | Common | Yes | 10 | 0.5 | 25 heal |
| `bread` | Bread | Consumable | Common | Yes | 20 | 0.3 | 10 heal |
| `iron_ore` | Iron Ore | Material | Common | Yes | 99 | 1.0 | — |
| `oak_wood` | Oak Wood | Material | Common | Yes | 99 | 0.8 | — |
| `magic_crystal` | Magic Crystal | Material | Rare | Yes | 20 | 0.4 | — |
| `gold_coin` | Gold Coin | Currency | Common | Yes | 999 | 0.01 | — |

To create matching `.asset` files by hand instead of using the editor
window: `Assets > Create > Sdx17 > Inventory > Item Definition`, then
fill in the fields from the table above.

## Public API

```csharp
using Sdx17.Inventory;

// The service is a singleton; get it once and keep the reference.
var service = InventoryService.Instance;

// Add / remove / query
bool added = service.AddItemById("health_potion", 3);
bool added2 = service.AddItem(itemDefinition, 1);
int removed = service.RemoveItem(itemDefinition, 1);
bool has = service.PlayerInventory.Has(itemDefinition, 2);
int count = service.PlayerInventory.Count(itemDefinition);
float weight = service.PlayerInventory.CurrentWeight; // vs MaxWeight 100

// Equip / use / drop
bool equipped = service.EquipItem(swordDefinition);   // weapons only
service.UnequipItem();
int healed = service.UseItem(potionDefinition);       // consumables only; apply to health
bool dropped = service.DropItem(coinDefinition, 1, transform.position);

// Hotbar (5 slots, indexes 0-4)
bool assigned = service.AssignToHotbar(slotIndex: 0, hotbarIndex: 0);

// Events (C# events, plus UnityEvents OnChangedEvent / OnItemEquippedEvent)
service.OnChanged += RefreshUI;
service.OnItemAdded += (item, amount) => Debug.Log($"+{amount} {item.DisplayName}");
service.OnItemEquipped += item => Debug.Log($"Equipped {item.DisplayName}");
service.OnItemUsed += (item, healed) => Debug.Log($"Used {item.DisplayName}, +{healed} HP");

// Save / load — wire a Secure Save provider that implements ISaveData
service.SetSaveProvider(mySecureSave);
service.Save();
service.Load();
// Or handle the data yourself:
InventorySaveData data = service.ToSaveData();
service.LoadFromSaveData(data);

// UI integration — subscribe to snapshots, render them your way
var hooks = GetComponent<InventoryUIHooks>();
hooks.OnGridSnapshot += rows => RenderGrid(rows);     // 24 SlotViewData
hooks.OnHotbarSnapshot += rows => RenderHotbar(rows); // 5 SlotViewData
hooks.OnSlotClicked(index, isHotbar: false);          // forward a UI click
hooks.OnHotbarKeyPressed(0);                          // forward hotbar key 1
```

Rules the API enforces, so callers do not have to:

- Adding more than the weight limit allows is refused in full — no
  partial stacks are left behind.
- Adding more than fits in 24 slots is refused in full.
- Only weapons equip, only consumables are used; other types return
  `false` / `0` and consume nothing.
- Save data stores item ids, not object references; unknown ids on load
  are skipped instead of failing the whole load.

## Integration notes with other Sdx17 systems

- **Secure Save:** implement `ISaveData` (`SaveInventory`,
  `LoadInventory`, `HasInventorySave`) in the save system and call
  `InventoryService.SetSaveProvider(...)`. Items must be registered in
  the service catalog (or via `RegisterItem`) or load cannot resolve
  their ids. Save after meaningful changes, or subscribe to
  `OnChanged` and save on a timer — not on every event.
- **UI system:** subscribe to `InventoryUIHooks.OnGridSnapshot` /
  `OnHotbarSnapshot` for data, and forward clicks with
  `OnSlotClicked` / `OnHotbarKeyPressed`. Read `SlotViewData.Rarity`
  for frame colours. The inventory builds no GameObjects.
- **Combat:** read `InventoryService.Instance.PlayerInventory.EquippedItem`
  (null when nothing is equipped) and use its `Damage`. Equipping here
  only records the choice; the combat system owns attacks and cooldowns.
- **Quest:** grant rewards with `AddItemById(id, amount)` and check
  requirements with `Has(item, amount)` / `Count(item)`. Remove quest
  turn-ins with `RemoveItem(item, amount)` and check its return value
  — it reports how many were actually removed.

## Test checklist for Unity (run before calling this tested)

1. Import the package into a Unity 2022.3 LTS project. Confirm zero
   compile errors in the Console.
2. Open `Sdx17 > Inventory > Demo Items Window`, click Create Demo
   Items. Confirm 8 `.asset` files appear under
   `Assets/Sdx17/Inventory/DemoItems`.
3. Follow `Samples~/Demo/README.md` to build the demo scene. Press
   Play. Confirm the Console shows the starter-inventory log and no
   errors, and keys 1–6 each log their action.
4. Add `InventorySelfTest` to a GameObject in a test scene with an
   `InventoryService` and press Play. Confirm the Console ends with
   `DONE: 0 failed` (every check logs PASS or FAIL).
5. In the demo scene, select an `ItemDefinition` asset and confirm the
   custom inspector shows the grouped sections and hides Heal for
   non-consumables.
6. Test save/load: wire a temporary `ISaveData` implementation (or use
   `ToSaveData` / `LoadFromSaveData` in code), add items, save, clear,
   load, and confirm counts match.
7. Build a standalone player (any platform) and confirm the demo scene
   runs with no errors.

Only after all seven steps pass may the version note change from
UNTESTED IN UNITY.

## License

MIT — see `LICENSE`. Copyright 2026 Sdx17.
