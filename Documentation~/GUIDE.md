# Sdx17 Inventory Framework — User Guide

**Version 0.1.0 — UNTESTED IN UNITY**

> Read this first. This package has never been run in the Unity editor
> or in Play mode. It was written and reviewed as C# source only, in an
> environment with no Unity installed. Nothing in this guide has been
> verified by running it. Do not treat the framework as tested,
> verified, or production-ready until the test checklist in the
> `README.md` has been run in Unity 2022.3 LTS and every step passes.
> The word "tested" does not appear anywhere in this guide as a claim
> about this package — because it has not been.

- Package: `com.sdx17.inventory`, version `0.1.0`
- Requires: Unity 2022.3 LTS (see `package.json`: `"unity": "2022.3"`,
  `"unityRelease": "0f1"`)
- Namespace: `Sdx17.Inventory` (demo: `Sdx17.Inventory.Samples`,
  self-test: `Sdx17.Inventory.Tests`)
- Render pipeline: URP-compatible; also works with the Built-in
  Render Pipeline, because the runtime code touches no rendering at
  all — it is pure inventory logic plus one optional `MonoBehaviour`
  bridge for UI data.
- Dependencies: none. No paid dependencies, no third-party code.
- License: MIT, Copyright 2026 Sdx17.

---

## 1. Quickstart (5 minutes)

This is the fastest path from an empty project to a working
inventory in Play mode. It uses the included demo driver, so no UI
and no item assets are needed.

### Step 1 — Install the package

Copy the package folder into your project as a UPM package
(`Packages/com.sdx17.inventory`), or copy its contents directly
into your project. Wait for Unity to finish importing, then check
the Console for compile errors. If there are any, stop here — the
package is not usable in that Unity version until they are fixed.

### Step 2 — Add the service to a scene

Create a new scene (File > New Scene; Basic 3D is fine), then do
one of these:

- Menu: **Sdx17 > Inventory > Add Inventory Service to Scene**
  (creates a GameObject named `InventoryService` with the component),
  or
- Create an empty GameObject named `InventoryService` yourself and
  add the `InventoryService` component to it.

There must be exactly one `InventoryService` per scene. It is a
singleton: the first instance survives scene loads
(`DontDestroyOnLoad`), and any extra instance destroys itself in
`Awake()`.

### Step 3 — Add the demo driver

1. Create an empty GameObject named `Demo`.
2. Add the `DemoInventory` component to it. It lives in
   `Samples~/Demo/DemoInventory.cs` — import the sample first if
   your Unity version requires it (Package Manager > the package >
   Samples > Import).
3. Drag the `InventoryService` GameObject into the demo's Service
   field. (You can leave it empty — the demo finds the service
   automatically via `InventoryService.Instance`, falling back to
   `FindObjectOfType<InventoryService>()`.)
4. Leave **Add Starter Items On Start** ticked.

### Step 4 — Press Play

On Start, the demo registers the 8 demo items (see §3.3) and adds
starter items: 1 Iron Sword, 5 Health Potions, 3 Bread, 10 Iron
Ore, 50 Gold Coins. Every change is logged to the Console — no UI
is needed to verify it. You should see a line like:

```
[DemoInventory] Slots used 5/24, weight ... , potions 5, equipped none.
```

Then try the number keys:

| Key | Action |
|-----|--------|
| 1 | Add 1 Health Potion |
| 2 | Use 1 Health Potion (logs the heal amount) |
| 3 | Equip the Iron Sword |
| 4 | Assign slot 0 to hotbar slot 0 |
| 5 | Drop 1 Gold Coin in front of the player |
| 6 | Clear the inventory |

If you see `[DemoInventory] No InventoryService in the scene`,
repeat Step 2.

### Step 5 — Run the self-test (optional but recommended)

Add the `InventorySelfTest` component (from
`Tests~/InventorySelfTest.cs`) to a GameObject in a test scene
that also has an `InventoryService`, and press Play. Every check
logs `PASS` or `FAIL` with its name, and the run ends with
`DONE: <passed> passed, <failed> failed.` The README's test
checklist requires `0 failed` before the package may be called
tested. Note: the self-test uses a fresh `Inventory` instance for
its logic checks, then exercises the service's add/equip/use path
on the live inventory. It needs no test framework; it runs in
plain Play mode.

### Step 6 — Create your own items

Either:

- Menu: **Sdx17 > Inventory > Create Demo Items** (or open
  **Sdx17 > Inventory > Demo Items Window** and click
  **Create Demo Items**). This creates 8 `.asset` files under
  `Assets/Sdx17/Inventory/DemoItems`. Existing assets with the
  same name are left untouched — re-running never overwrites
  hand edits. Or:
- **Assets > Create > Sdx17 > Inventory > Item Definition**
  (also available as **Sdx17 > Inventory > Create Item
  Definition**, which creates `Assets/New Item.asset`).

Then drag the item assets into the `InventoryService` component's
**Item Catalog** field, or register them in code with
`RegisterItem` / `RegisterItems` (see §4.2). Items that are not
registered cannot be resolved by id, which breaks save/load
and `AddItemById`.

---

## 2. Concepts

### 2.1 ItemDefinition — one kind of item

`ItemDefinition` is a `ScriptableObject`. One asset = one kind of
item (e.g. "Health Potion"), not one physical potion. All potions
in the game share the same `ItemDefinition` asset; the inventory
stores how many of them are in each slot.

Fields (all read-only at runtime through properties):

| Field | Property | Meaning |
|-------|----------|---------|
| id | `Id` | Unique id, lowercase with underscores, e.g. `health_potion`. Used in save data and in `AddItemById` / `FindItem`. |
| displayName | `DisplayName` | Player-facing name, e.g. "Health Potion". |
| type | `Type` | `ItemType`: `Weapon`, `Consumable`, `Material`, `Currency`. Drives which actions are valid. |
| rarity | `Rarity` | `ItemRarity`: `Common`, `Uncommon`, `Rare`, `Epic`, `Legendary`. **Data only in v0.1.0** — it does not change stats. The UI system reads it to pick a frame colour or label. |
| stackable | `Stackable` | Whether multiple units share one slot. |
| maxStack | `MaxStack` | Max units per slot. Forced to 1 when `stackable` is false. |
| weight | `Weight` | Weight of one unit. |
| healAmount | `HealAmount` | Health restored when a consumable is used. |
| damage | `Damage` | Damage value of a weapon. Read by the combat system; the inventory never deals damage itself. |
| icon | `Icon` | `Sprite` shown by the UI. |
| prefab | `Prefab` | `GameObject` spawned in the world when the item is dropped. Optional — dropping works without one. |
| description | `Description` | Flavour / tooltip text. |

Derived helpers: `IsEquippable` (true only for `Weapon`),
`IsUsable` (true only for `Consumable`).

**Never mutate an `ItemDefinition` at runtime.** Instances are
shared data. The editor's `OnValidate()` keeps data sane: empty
id/name fall back to `"item"` / `"New Item"`, `maxStack` is clamped
to at least 1 (and to 1 when not stackable), and weight, heal and
damage are clamped to 0 or above.

### 2.2 InventorySlot — one stack

`InventorySlot` is one slot in the inventory or the hotbar. It
holds one `ItemDefinition` and a `Count`. Empty slots have a null
definition and a count of 0 (`IsEmpty`).

`CanMerge(item)` is true when the slot already holds the same
stackable item and has room under `MaxStack`. `Add(item, amount)`
adds as many as fit and **returns the amount that did not fit**
(0 when everything fit). `Remove(amount)` removes up to that
many and **returns the amount actually removed**, clearing the
slot at 0. `Clear()` empties the slot. `Set(item, amount)` restores
a slot without merge rules — it is used by save/load, not by
gameplay code.

### 2.3 Inventory — 24 slots + 5-slot hotbar

`Inventory` is pure logic: no `MonoBehaviour`, no UI, no saving.
It owns 24 main slots (`SlotCount = 24`) and 5 hotbar slots
(`HotbarSlotCount = 5`), a weight limit, and the currently
equipped item.

- `CurrentWeight` is the total weight of everything in the main
  slots **and** the hotbar. `MaxWeight` is the limit (default 100).
- `UsedSlots` counts non-empty main slots (hotbar slots are not
  counted).
- `EquippedItem` is the currently equipped weapon, or null.

**All-or-nothing adds.** `Add(item, amount)` first checks the
weight limit and whether the whole amount fits (`CanFit`). If
either check fails, it returns `false` and adds **nothing** — no
partial stacks are left behind. When it succeeds it merges into
existing stacks first, then fills empty slots.

**Removal order.** `Remove(item, amount)` removes from the hotbar
first, then the main slots, and returns how many were actually
removed (less than requested when not enough was held). If the
removed item was the equipped item and none remain, the equipped
slot is cleared automatically.

**Equip / use / drop rules** (enforced by the inventory, so
callers do not have to check):

- Only weapons equip. `Equip(item)` returns `false` for
  non-weapons and for items the player does not hold. Equipping
  does **not** remove the item from the inventory — it only
  records the choice. `Unequip()` clears it.
- Only consumables are used. `Use(item)` consumes one unit and
  returns its `HealAmount`. For any other type, or an item not
  held, it returns 0 and consumes nothing. **The inventory never
  heals the player** — the caller applies the returned heal
  amount to its own health system.
- `Drop(item, amount, position)` removes the items and, when the
  definition has a `Prefab`, spawns it at `position` with the name
  `"<DisplayName> (Dropped)"`. It returns `false` when the item
  is not held in that quantity. Dropping works without a prefab;
  the items are simply removed.

**Hotbar.** `AssignToHotbar(slotIndex, hotbarIndex)` moves a
whole stack from a main slot into a hotbar slot, or swaps the two
stacks when the hotbar slot is occupied. Slot indexes are 0–23,
hotbar indexes 0–4. Bad indexes or an empty source slot return
`false`.

### 2.4 InventoryService — the singleton owner

`InventoryService` is the `MonoBehaviour` gameplay code talks to.
It owns one `Inventory` (`PlayerInventory`), exposes the item
catalog (`ItemCatalog`), raises events for every change, and wires
save/load through `ISaveData`. **Talk to the service, never to a
raw `Inventory`,** so events fire for every change.

Key behaviours:

- **Singleton.** `Instance` is set in `Awake()`. A second
  instance destroys its own GameObject. The surviving instance
  calls `DontDestroyOnLoad`, so it persists across scene loads.
- **Catalog.** The `itemCatalog` list (inspector field) plus
  `RegisterItem` / `RegisterItems` define which definitions can
  be resolved by id. `RegisterItem` refuses duplicates: the first
  registration of an id wins and later ones return `false`.
  `FindItem(id)` returns the definition or null.
- **Events.** Five C# events — `OnChanged`, `OnItemAdded`,
  `OnItemRemoved`, `OnItemEquipped`, `OnItemUsed` — plus two
  `UnityEvent`s for inspector wiring: `OnChangedEvent` and
  `OnItemEquippedEvent`. `OnChanged` fires after every successful
  mutation (add, remove, equip, unequip, use, drop, hotbar
  assign, load). Failed operations fire nothing.
- **Save/load.** `SetSaveProvider(ISaveData)` wires a save
  provider (in practice the Sdx17 Secure Save system).
  `Save()` / `Load()` delegate to it and return `false` when no
  provider is set (`Load()` also returns `false` when no save
  exists). `ToSaveData()` / `LoadFromSaveData(data)` let you
  handle the `InventorySaveData` yourself without a provider.

### 2.5 Save data — ids, not references

`InventorySaveData` is plain data: a `Slots` array (24
`InventorySaveEntry` values), a `Hotbar` array (5), and an
`EquippedItemId` string. Each `InventorySaveEntry` holds an
`ItemId` string and a `Count`. Items are stored **by id string,
not by Unity object reference**, so a save survives asset moves.

On load (`FromSaveData(data, resolve)`), each id is resolved back
to a definition through the lookup you supply — in practice
`InventoryService.FindItem`, which searches the catalog. **Unknown
ids are skipped, never treated as an error that blocks the rest
of the load.** An equipped id is only restored when it resolves
to an equippable item the player actually holds after the load.
Passing null data or a null resolver restores nothing.

### 2.6 UI hooks — snapshots, not a UI

`InventoryUIHooks` is a `MonoBehaviour` that translates inventory
state into data any UI can render. It builds **no GameObjects**
and references no UI framework, so any UI approach works.

Each slot becomes a `SlotViewData` struct: `Index`, `IsEmpty`,
`ItemId`, `DisplayName`, `Rarity` (string), `Type` (string),
`Count`, `Weight` (total weight of that stack), `Icon`
(`Sprite`), `IsEquipped`, `IsUsable`, `IsEquippable`.

- Subscribe to `OnGridSnapshot` (24 rows) and
  `OnHotbarSnapshot` (5 rows) for data, or poll
  `GetGridSnapshot()` / `GetHotbarSnapshot()` when the service
  raises `OnChanged`. Snapshots are re-published automatically
  after every change, and `OnEquippedChanged` fires only when
  the equipped item actually changes.
- Forward input back: `OnSlotClicked(index, isHotbar)` —
  clicking a weapon equips it, clicking a consumable uses it,
  anything else does nothing. `OnHotbarKeyPressed(hotbarIndex)`
  forwards hotbar keys 0–4 (i.e. number keys 1–5 in the demo).

If the `service` field is left empty, the hooks resolve
`InventoryService.Instance` in `Awake()`.

### 2.7 The 8 demo items

Created in code by `DemoItems.CreateAll()` (and registered with
`DemoItems.CreateAndRegister(service)`), or as `.asset` files via
**Sdx17 > Inventory > Create Demo Items**:

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

---

## 3. API reference

Every public class and member that exists in `Runtime/`. All types
are in the `Sdx17.Inventory` namespace unless noted. This reference
was written from the source files — no member listed here is
invented. Private/serialized backing fields are not listed.

### 3.1 Enums

**`ItemType`** — `Weapon`, `Consumable`, `Material`, `Currency`.
Drives which actions are valid: only Weapons equip, only
Consumables are used, Currency is never equipped or used,
Materials are crafting inputs.

**`ItemRarity`** — `Common`, `Uncommon`, `Rare`, `Epic`,
`Legendary`. Data only in v0.1.0; does not change stats.

### 3.2 `ItemDefinition : ScriptableObject`

Create via **Assets > Create > Sdx17 > Inventory > Item
Definition**, or **Sdx17 > Inventory > Create Item Definition**.

| Member | Type | Notes |
|--------|------|-------|
| `Id` | `string` (get) | Unique id, e.g. `health_potion`. |
| `DisplayName` | `string` (get) | Player-facing name. |
| `Type` | `ItemType` (get) | Category. |
| `Rarity` | `ItemRarity` (get) | Data only in v0.1.0. |
| `Stackable` | `bool` (get) | Whether units share a slot. |
| `MaxStack` | `int` (get) | Max units per slot; 1 when not stackable. |
| `Weight` | `float` (get) | Weight of one unit. |
| `HealAmount` | `int` (get) | Health restored on use (consumables). |
| `Damage` | `int` (get) | Damage value (weapons). |
| `Icon` | `Sprite` (get) | UI icon. |
| `Prefab` | `GameObject` (get) | Spawned on drop; may be null. |
| `Description` | `string` (get) | Tooltip / flavour text. |
| `IsEquippable` | `bool` (get) | True only when `Type == Weapon`. |
| `IsUsable` | `bool` (get) | True only when `Type == Consumable`. |
| `CreateRuntime(string itemId, string itemName, ItemType itemType, ItemRarity itemRarity, bool isStackable, int maxStackSize, float itemWeight, int heal, int damageValue, string itemDescription)` | `static ItemDefinition` | Runtime factory used by `DemoItems`. Editor-created `.asset` files are the normal path; this exists so the demo runs without any assets. Clamps `maxStack` to ≥ 1 (1 when not stackable), weight/heal/damage to ≥ 0, and sets the object name to the id. |

### 3.3 `DemoItems` (static)

| Member | Notes |
|--------|-------|
| `CreateAll()` → `IReadOnlyList<ItemDefinition>` | Creates the 8 demo items in code (see §2.7). |
| `CreateAndRegister(InventoryService service)` → `IReadOnlyList<ItemDefinition>` | Creates the demo items and registers them with the service via `RegisterItems`, so save/load can resolve them by id. Returns the items. Safe to call with a null service (returns the items unregistered). |

### 3.4 `InventorySlot` (`[Serializable]`)

| Member | Notes |
|--------|-------|
| `Definition` | `ItemDefinition` (get). Null when empty. |
| `Count` | `int` (get). 0 when empty. |
| `IsEmpty` | `bool` (get). True when definition is null or count ≤ 0. |
| `CanMerge(ItemDefinition item)` → `bool` | True when this slot holds the given stackable item and has room under `MaxStack`. False for null items, empty slots, a different item, or non-stackables. |
| `Add(ItemDefinition item, int amount)` → `int` | Adds as many as fit. **Returns the amount that did not fit** (0 when everything was added). Only call after checking the slot is empty or `CanMerge`. Returns the full amount unchanged for a null item, a non-positive amount, or a different item already in the slot. Non-stackable items take at most 1 per slot. |
| `Remove(int amount)` → `int` | Removes up to `amount`. **Returns the amount actually removed.** Clears the slot at 0. Returns 0 for an empty slot or non-positive amount. |
| `Clear()` | Empties the slot (definition null, count 0). |
| `Set(ItemDefinition item, int amount)` | Restores a slot without merge rules (used by load). Null item or non-positive amount clears the slot. Count is clamped to `MaxStack` for stackables, 1 otherwise. |

### 3.5 `Inventory` (`[Serializable]`)

Constants: `SlotCount = 24`, `HotbarSlotCount = 5`.

Constructors: `Inventory()` (weight limit 100) and
`Inventory(float weightLimit)` (clamped to ≥ 0).

| Member | Notes |
|--------|-------|
| `Slots` | `InventorySlot[]` (get). 24 slots. |
| `Hotbar` | `InventorySlot[]` (get). 5 slots. |
| `MaxWeight` | `float` (get). Default 100. |
| `EquippedItem` | `ItemDefinition` (get). Null when nothing is equipped. |
| `CurrentWeight` | `float` (get). Total weight of main slots + hotbar. |
| `UsedSlots` | `int` (get). Non-empty main slots only. |
| `Has(ItemDefinition item, int amount = 1)` → `bool` | True when at least `amount` are held (main slots + hotbar). False for null items or non-positive amounts. |
| `Count(ItemDefinition item)` → `int` | Total held across main slots + hotbar. 0 for null items. |
| `Add(ItemDefinition item, int amount = 1)` → `bool` | All-or-nothing add (see §2.3). Merges into existing stacks first, then empty slots. Returns `false` and adds nothing when the weight limit or a full inventory prevents the whole amount from fitting, or for null items / non-positive amounts. |
| `CanFit(ItemDefinition item, int amount)` → `bool` | Checks fit without changing anything. Same merge-then-empty-slot logic as `Add`. Returns `false` for null items or non-positive amounts. |
| `Remove(ItemDefinition item, int amount = 1)` → `int` | Removes hotbar first, then main slots. **Returns the amount actually removed.** Clears the equipped item if it was removed entirely. Returns 0 for null items / non-positive amounts. |
| `AssignToHotbar(int slotIndex, int hotbarIndex)` → `bool` | Moves a whole stack from a main slot to a hotbar slot, swapping when occupied. `false` for bad indexes or an empty source slot. |
| `Equip(ItemDefinition item)` → `bool` | Equips a held weapon. `false` for null, non-weapons, or items not held. Does not remove the item. |
| `Unequip()` | Clears the equipped item. |
| `Use(ItemDefinition item)` → `int` | Consumes one consumable and returns its `HealAmount`. Returns 0 (consuming nothing) for null, non-consumables, or items not held. The caller applies healing. |
| `Drop(ItemDefinition item, int amount, Vector3 position)` → `bool` | Removes the items and spawns `item.Prefab` at `position` when one is set (named `"<DisplayName> (Dropped)"`). `false` when the item is not held in that quantity, or for null items / non-positive amounts. |
| `Clear()` | Empties all main + hotbar slots and clears the equipped item. |
| `SetMaxWeight(float limit)` | Sets the weight limit, clamped to ≥ 0. |
| `ToSaveData()` → `InventorySaveData` | Serialises slots, hotbar, and the equipped item's id. |
| `FromSaveData(InventorySaveData data, Func<string, ItemDefinition> resolve)` | Restores state from save data. Clears first, then restores slots and hotbar slot-by-slot; unknown ids are skipped. The equipped id is restored only when it resolves to an equippable item the player holds. Does nothing for null data or a null resolver. |

### 3.6 `InventoryService : MonoBehaviour`

| Member | Notes |
|--------|-------|
| `Instance` | `static InventoryService` (get, private set). The singleton. Null before the first `Awake()`, and set back to null when that instance is destroyed. |
| `PlayerInventory` | `Inventory` (get, private set). Created in `Awake()` with the inspector `maxWeight`. Null until `Awake()` runs. |
| `ItemCatalog` | `IReadOnlyList<ItemDefinition>` (get). The registered definitions. |
| `OnChanged` | `event Action`. Fires after every successful mutation. |
| `OnItemAdded` | `event Action<ItemDefinition, int>`. Fires with (item, amount) after a successful add. |
| `OnItemRemoved` | `event Action<ItemDefinition, int>`. Fires with (item, amount actually removed) after a removal that removed ≥ 1. |
| `OnItemEquipped` | `event Action<ItemDefinition>`. Fires after a successful equip. |
| `OnItemUsed` | `event Action<ItemDefinition, int>`. Fires with (item, heal amount) after a use that consumed one unit. |
| `OnChangedEvent` | `UnityEvent` (get). Inspector-wirable equivalent of `OnChanged`. |
| `OnItemEquippedEvent` | `UnityEvent<ItemDefinition>` (get). Inspector-wirable equivalent of `OnItemEquipped`. |
| `SetSaveProvider(ISaveData provider)` | Wires the save provider. Passing null unwires it (`Save()` / `Load()` then return `false`). |
| `RegisterItem(ItemDefinition item)` → `bool` | Registers one definition so save/load can resolve it by id. `false` for null items and for duplicate ids (first registration wins). |
| `RegisterItems(IEnumerable<ItemDefinition> items)` | Registers each item via `RegisterItem`. Safe with null input (does nothing). |
| `FindItem(string id)` → `ItemDefinition` | Returns the catalog entry with this id, or null (including for null/empty ids). |
| `AddItem(ItemDefinition item, int amount = 1)` → `bool` | Adds via the inventory; on success fires `OnItemAdded` then `OnChanged`. Returns `false` when there is no inventory or the add is refused. |
| `AddItemById(string id, int amount = 1)` → `bool` | Resolves the id with `FindItem`, then `AddItem`. `false` for unknown ids. |
| `RemoveItem(ItemDefinition item, int amount = 1)` → `int` | Removes via the inventory; when ≥ 1 was removed, fires `OnItemRemoved` then `OnChanged`. Returns the amount actually removed (0 when there is no inventory or a null item). |
| `EquipItem(ItemDefinition item)` → `bool` | Equips via the inventory; on success fires `OnItemEquipped`, then `OnItemEquippedEvent`, then `OnChanged`. |
| `UnequipItem()` | Unequips and fires `OnChanged`. Does nothing when there is no inventory. |
| `UseItem(ItemDefinition item)` → `int` | Uses one consumable via the inventory and returns the heal amount. Fires `OnItemUsed` then `OnChanged` only when a unit was actually consumed. Returns 0 for null items, non-consumables, or no inventory. |
| `DropItem(ItemDefinition item, int amount, Vector3 position)` → `bool` | Drops via the inventory; on success fires `OnChanged`. |
| `AssignToHotbar(int slotIndex, int hotbarIndex)` → `bool` | Assigns via the inventory; on success fires `OnChanged`. |
| `Save()` → `bool` | Saves through the wired provider (`SaveInventory(ToSaveData())`). `false` when no provider is set or there is no inventory. |
| `Load()` → `bool` | Loads through the wired provider. `false` when no provider is set, there is no inventory, no save exists (`HasInventorySave()`), or the loaded data is null. On success restores via `FromSaveData(data, FindItem)` and fires `OnChanged`. |
| `ToSaveData()` → `InventorySaveData` | Returns `PlayerInventory.ToSaveData()`, or null when there is no inventory. |
| `LoadFromSaveData(InventorySaveData data)` | Restores from the given data via `FromSaveData(data, FindItem)` and fires `OnChanged`. Does nothing when there is no inventory or the data is null. |

Lifecycle notes: `Awake()` destroys duplicate instances, sets
`Instance`, calls `DontDestroyOnLoad(gameObject)`, and creates
`PlayerInventory = new Inventory(maxWeight)` using the inspector
value at that moment. Changing `maxWeight` in the inspector after
`Awake()` does **not** change the live inventory's limit — use
`PlayerInventory.SetMaxWeight(limit)` at runtime. `OnDestroy()`
clears `Instance` when the singleton itself is destroyed.

### 3.7 Save/load contract — `ISaveData`, `InventorySaveData`, `InventorySaveEntry`

**`ISaveData`** (interface — implement this in your save system,
in practice the Sdx17 Secure Save system):

| Member | Notes |
|--------|-------|
| `SaveInventory(InventorySaveData data)` | Persist the snapshot. |
| `LoadInventory()` → `InventorySaveData` | Return the stored snapshot (or null when there is none). |
| `HasInventorySave()` → `bool` | True when a save exists. |

**`InventorySaveData`** (`[Serializable]`):

| Member | Notes |
|--------|-------|
| `Slots` | `InventorySaveEntry[]` (get/set). Null is replaced with an empty array. |
| `Hotbar` | `InventorySaveEntry[]` (get/set). Null is replaced with an empty array. |
| `EquippedItemId` | `string` (get/set). Null is replaced with `string.Empty`. |

**`InventorySaveEntry`** (`[Serializable]`):

| Member | Notes |
|--------|-------|
| `ItemId` | `string` (get/set). Null is replaced with `string.Empty`. |
| `Count` | `int` (get/set). Clamped to ≥ 0. |
| `InventorySaveEntry()` | Parameterless constructor (empty id, count 0). |
| `InventorySaveEntry(string id, int amount)` | Sets id (null → empty) and count (clamped to ≥ 0). |

### 3.8 `InventoryUIHooks : MonoBehaviour`

| Member | Notes |
|--------|-------|
| `OnGridSnapshot` | `event Action<IReadOnlyList<SlotViewData>>`. Raised with a fresh 24-row grid snapshot after every change. |
| `OnHotbarSnapshot` | `event Action<IReadOnlyList<SlotViewData>>`. Raised with a fresh 5-row hotbar snapshot after every change. |
| `OnEquippedChanged` | `event Action<ItemDefinition>`. Raised when the equipped item changes (null = nothing equipped). |
| `GetGridSnapshot()` → `IReadOnlyList<SlotViewData>` | Builds a fresh 24-row snapshot now. Returns an empty list when there is no service or inventory. |
| `GetHotbarSnapshot()` → `IReadOnlyList<SlotViewData>` | Builds a fresh 5-row snapshot now. |
| `OnSlotClicked(int index, bool isHotbar)` | UI calls this on a click. Does nothing for a null service/inventory, out-of-range indexes, or empty slots. Clicking a weapon equips it; clicking a consumable uses it; other types do nothing. |
| `OnHotbarKeyPressed(int hotbarIndex)` | UI calls this for a hotbar key (0–4). Forwards to `OnSlotClicked(hotbarIndex, true)`. |

**`SlotViewData`** (struct, `[Serializable]`) — one row of UI
data. Fields: `Index` (`int`), `IsEmpty` (`bool`), `ItemId`
(`string`), `DisplayName` (`string`), `Rarity` (`string`, from
`ItemRarity.ToString()`), `Type` (`string`, from
`ItemType.ToString()`), `Count` (`int`), `Weight` (`float` — total
weight of that stack), `Icon` (`Sprite`), `IsEquipped` (`bool`),
`IsUsable` (`bool`), `IsEquippable` (`bool`). Empty slots have
`IsEmpty = true` and default values for the rest.

### 3.9 Editor tooling

In the `Sdx17.Inventory.Editor` namespace (editor-only assembly
`Sdx17.Inventory.Editor.asmdef`, which references the runtime):

| Member | Notes |
|--------|-------|
| `ItemDefinitionEditor` | Custom inspector for `ItemDefinition`: grouped sections, hides Heal for non-consumables (per the README; verify visually in the editor — see the test checklist). |
| `InventoryEditorWindow` | Editor window (**Sdx17 > Inventory > Demo Items Window**). Button **Create Demo Items** creates the 8 demo `.asset` files under `Assets/Sdx17/Inventory/DemoItems`; existing assets are not overwritten. Button **Create Single Item Definition** creates one asset in the same folder. Static entry point `CreateDemoItems()` is also on the menu as **Sdx17 > Inventory > Create Demo Items**. |
| `InventoryMenuItems.CreateItemDefinition()` | Menu **Sdx17 > Inventory > Create Item Definition**: creates a new `ItemDefinition` asset at `Assets/New Item.asset` (unique path), selects it, and focuses the Project window. |
| `InventoryMenuItems.AddInventoryServiceToScene()` | Menu **Sdx17 > Inventory > Add Inventory Service to Scene**: selects the existing service if one is already in the scene; otherwise creates a GameObject named `InventoryService` with the component (undoable). |

### 3.10 Demo and self-test (Samples~ / Tests~)

**`DemoInventory`** (`Sdx17.Inventory.Samples`, in
`Samples~/Demo/DemoInventory.cs`) — demo driver. Inspector
fields: `service` (`InventoryService`, auto-resolved when empty)
and `addStarterItemsOnStart` (`bool`, default on). On Start it
registers the 8 demo items, adds the starter items (§1), subscribes
to `OnChanged` / `OnItemEquipped` for Console logging, and logs the
key list. In `Update()` it maps keys 1–6 to add / use / equip /
hotbar / drop / clear (see §1). Key 6 calls
`service.PlayerInventory.Clear()` directly and logs
`"[DemoInventory] Inventory cleared."`.

**`InventorySelfTest`** (`Sdx17.Inventory.Tests`, in
`Tests~/InventorySelfTest.cs`) — Play-mode self-test. Inspector
field: `service` (auto-resolved when empty). On Start it runs
`RunAll()` and logs `DONE: <passed> passed, <failed> failed.`
Each check logs `PASS: <name>` or `FAIL: <name>`. It covers:
add / count / stack merging across the `maxStack` boundary,
`Has` / `Remove`, non-stackable weapons taking one slot each,
the weight limit refusing a whole add and leaving nothing
behind, equip rules (cannot equip a consumable), unequip, use
rules (potion heals 25 and consumes one; a weapon heals 0 and
is not consumed), hotbar assignment and bad indexes, drop of an
unheld item failing and drop with no prefab succeeding, save/load
round-trip by id, unknown save ids being skipped safely, the
service path (`AddItem`, `AddItemById` with a known and an
unknown id, `EquipItem` matching `Has`), and null / zero-amount
guards. When no service is in the scene the service checks are
skipped with a warning.

---

## 4. Config reference

### 4.1 `InventoryService` inspector

| Setting | Field | Default | Notes |
|---------|-------|---------|-------|
| Max weight | `maxWeight` | `100` | Weight limit applied when `PlayerInventory` is created in `Awake()`. Changing it after `Awake()` has no effect on the live inventory — use `PlayerInventory.SetMaxWeight(limit)` at runtime. Adds that would exceed the limit are refused in full. |
| Item catalog | `itemCatalog` | empty list | Definitions the service can resolve by id. Required for `AddItemById` and for save/load. Drag assets here, or register in code. |
| On Changed Event | `onChangedEvent` | — | `UnityEvent` fired after every successful mutation (same moment as the C# `OnChanged`). |
| On Item Equipped Event | `onItemEquippedEvent` | — | `UnityEvent<ItemDefinition>` fired after a successful equip. |

### 4.2 Per-item settings (`ItemDefinition` assets)

See §2.1. The rules that matter most:

- **Id must be unique** across everything the service can
  resolve. `RegisterItem` refuses a duplicate id — the first
  registration wins, silently. Two different assets with the
  same id will break `AddItemById` and save/load in confusing
  ways (the wrong one is found, or the second one never
  registers). Keep ids lowercase with underscores.
- **Non-stackable items always have `maxStack` 1.** The
  inspector's `OnValidate()` enforces this, and so does
  `CreateRuntime`.
- **Weight is per unit.** A stack of 10 potions at 0.5 weighs 5.
  `CurrentWeight` includes the hotbar.
- **Heal only matters for consumables; damage only for
  weapons.** Setting `HealAmount` on a weapon does nothing
  (`Use()` returns 0 for weapons). Setting `Damage` on a
  consumable does nothing.
- **Rarity changes nothing in v0.1.0.** It is display data for
  the UI only.
- **Prefab is optional.** Without one, `Drop()` still removes
  the items; nothing is spawned.

### 4.3 `Inventory` runtime settings

- `Inventory(float weightLimit)` — construct with a custom limit
  (clamped to ≥ 0). The service always uses the inspector
  `maxWeight` instead.
- `SetMaxWeight(float limit)` — change the limit at runtime
  (clamped to ≥ 0). Lowering it below the current weight does
  not remove items; it only blocks future adds until the weight
  drops under the limit.
- Slot counts are fixed constants: 24 main slots, 5 hotbar
  slots. There is no setting to change them in v0.1.0 — that
  would be a code change.

### 4.4 `DemoInventory` inspector

| Setting | Field | Default | Notes |
|---------|-------|---------|-------|
| Service | `service` | empty (auto-resolved) | The `InventoryService` to drive. |
| Add Starter Items On Start | `addStarterItemsOnStart` | on | Demo only. Turn it off to start with an empty inventory. |

---

## 5. Worked examples

### 5.1 Picking up loot

```csharp
using Sdx17.Inventory;

public class LootPickup : MonoBehaviour
{
    // Called by your pickup trigger / interaction system.
    public void PickUp(ItemDefinition item, int amount)
    {
        var service = InventoryService.Instance;
        if (service == null) return;

        bool added = service.AddItem(item, amount);
        if (!added)
        {
            // Either the weight limit or a full inventory refused
            // the whole amount. Nothing was added. Show "Inventory
            // full" / "Too heavy" to the player.
            Debug.Log("Could not pick up: inventory full or too heavy.");
        }
    }
}
```

### 5.2 Using a potion from code (and applying the heal)

```csharp
var service = InventoryService.Instance;
int healed = service.UseItem(potionDefinition);
// healed is 0 when the item is not a consumable or not held.
// The inventory consumed one unit only when healed > 0
// (for a consumable with HealAmount > 0). Apply it yourself:
if (healed > 0)
    playerHealth.Heal(healed);
```

Note: a consumable with `HealAmount` 0 still consumes one unit
when used and returns 0 — the service fires `OnItemUsed` because
a unit was consumed. Do not use `healed > 0` as the test for
"was something consumed"; check `Count` before/after if that
matters, the way `InventoryService.UseItem` itself does.

### 5.3 Equipping a weapon and reading its damage (combat)

```csharp
var service = InventoryService.Instance;
if (service.EquipItem(swordDefinition))
{
    // Equipped. The combat system reads the damage itself:
    int damage = service.PlayerInventory.EquippedItem.Damage;
}
```

### 5.4 Quest reward and quest turn-in check

```csharp
// Grant a reward:
service.AddItemById("gold_coin", 50);

// Check a requirement (10 iron ore):
bool canTurnIn = service.PlayerInventory.Has(ironOreDefinition, 10);

// Take the turn-in items — always check the return value:
int removed = service.RemoveItem(ironOreDefinition, 10);
if (removed < 10)
{
    // Not enough was held; RemoveItem already removed what
    // there was. Handle the shortfall in your quest logic.
}
```

### 5.5 Saving and loading (two ways)

With a save provider (the intended path — implement `ISaveData`
in the Sdx17 Secure Save system, or any object):

```csharp
service.SetSaveProvider(mySecureSave);
service.Save();   // returns false when no provider is set
service.Load();   // returns false when no provider is set,
                  // when no save exists, or when data is null
```

Without a provider — handle the data yourself:

```csharp
InventorySaveData data = service.ToSaveData();
// ... store `data` wherever you like (your own save system) ...
service.LoadFromSaveData(data);
```

In both cases, every item in the save must be registered in the
service catalog (inspector `itemCatalog`, or `RegisterItem` /
`RegisterItems`), or its id cannot be resolved and that entry is
skipped on load.

### 5.6 Rendering the inventory in your own UI

```csharp
// Attach InventoryUIHooks to a GameObject, assign the service
// (or leave it empty — it resolves InventoryService.Instance).
var hooks = GetComponent<InventoryUIHooks>();
hooks.OnGridSnapshot += rows => RenderGrid(rows);     // 24 SlotViewData
hooks.OnHotbarSnapshot += rows => RenderHotbar(rows); // 5 SlotViewData
hooks.OnEquippedChanged += item => ShowEquipped(item);

// Forward a click from your UI:
hooks.OnSlotClicked(slotIndex, isHotbar: false);
// Forward a hotbar key (0-4):
hooks.OnHotbarKeyPressed(0);
```

Read `SlotViewData.Rarity` for frame colours, `Count` for the
stack label, `Icon` for the sprite, and `IsEquipped` /
`IsUsable` / `IsEquippable` to decide what a click should do
(the hooks already apply the equip/use rule on click — see
§3.8).

---

## 6. Integration with the other Sdx17 systems

Honest status first: **only four of these integrations have real
code behind them in v0.1.0** — Secure Save, Combat, Quest, and UI.
The rest have no code in this package and no API in the other
systems to call yet, because those systems do not exist as code
in this repo. For those, this section gives the integration
*pattern* using only the public API documented in §3, clearly
labelled as not yet built. Do not treat a pattern below as a
shipped feature.

### 6.1 Secure Save — implemented (interface)

**Status: real.** The inventory defines the `ISaveData`
interface (`SaveInventory`, `LoadInventory`,
`HasInventorySave`) and the inventory never touches files,
PlayerPrefs, or encryption itself.

- Implement `ISaveData` in the save system and call
  `InventoryService.SetSaveProvider(...)`.
- Items must be registered in the service catalog (or via
  `RegisterItem`) or load cannot resolve their ids — unknown
  ids are skipped, not fatal.
- Save after meaningful changes, or subscribe to `OnChanged`
  and save on a timer — **not on every event**. Saving on
  every `OnChanged` fires on every single add/remove/use and
  will hammer whatever storage the provider uses.
- `Load()` returns `false` when no save exists, so callers can
  distinguish "no save yet" from "save loaded".

### 6.2 UI system — implemented (snapshot bridge)

**Status: real.** `InventoryUIHooks` is the bridge (see §2.6,
§3.8, §5.6). Subscribe to `OnGridSnapshot` /
`OnHotbarSnapshot`, forward clicks with `OnSlotClicked` /
`OnHotbarKeyPressed`, and read `SlotViewData.Rarity` for frame
colours. The inventory builds no GameObjects and references no
UI framework, so any UI approach works. The UI system owns
layout, drag-and-drop, tooltips, and sounds — none of those
exist in this package.

### 6.3 Combat — implemented (read-only)

**Status: real, read-only.** Read
`InventoryService.Instance.PlayerInventory.EquippedItem` (null
when nothing is equipped) and use its `Damage`. Equipping here
only records the choice; the combat system owns attacks,
cooldowns, hit detection, and damage application. The
inventory never deals damage, and `Use()` never heals — it
only returns a number for the caller's health system.

### 6.4 Quest — implemented (add / check / remove)

**Status: real.** Grant rewards with `AddItemById(id, amount)`
and check requirements with `Has(item, amount)` /
`Count(item)`. Remove quest turn-ins with `RemoveItem(item,
amount)` and **check its return value** — it reports how many
were actually removed, which can be less than requested. See
§5.4. There is no quest-tracking code in this package; the
quest system owns objectives and completion.

### 6.5 NPC — pattern only, not built

**Status: no code.** There is no NPC system in this repo and no
inventory API for NPCs. The pattern, when an NPC system exists,
is the same public API a player uses: give an NPC loot with
`AddItem` / `AddItemById` on a second `Inventory` instance
(`new Inventory(weightLimit)` — the class is plain C#, not a
`MonoBehaviour`, so NPC inventories do not need a service), and
check what an NPC holds with `Has` / `Count`. Note that
`InventoryService` is a **player** singleton — NPC inventories
should be plain `Inventory` objects owned by the NPC, not
additional services. Trading between a player and an NPC would
be `RemoveItem` on one side and `AddItem` on the other, with
both return values checked.

### 6.6 Dialogue — pattern only, not built

**Status: no code.** There is no dialogue system in this repo.
The pattern, when one exists, is to gate dialogue options on
inventory state — e.g. show a "Give 10 Iron Ore" option only
when `PlayerInventory.Has(ironOre, 10)` is true — and to apply
the consequences with `RemoveItem` / `AddItemById`, checking
return values. Dialogue owns the conversation; the inventory
only answers `Has` / `Count` and performs the exchange.

### 6.7 Matchmaking — pattern only, not built

**Status: no code.** There is no matchmaking system in this
repo, and the inventory has no networking. The pattern, when a
matchmaking system exists, is to keep the inventory strictly
local and server-authoritative: the server owns the real
inventory state, and this package's save/load (`ToSaveData` /
`LoadFromSaveData`, id-based) is the shape a server snapshot
could take. Do not sync the live `Inventory` object itself
across the network, and do not trust a client-sent
`InventorySaveData` without server-side validation — nothing
in v0.1.0 validates that a save is legitimate.

### 6.8 Multiplayer — pattern only, not built

**Status: no code, and explicitly out of scope for v0.1.0.**
The README states: no multiplayer, no networking, no database.
`InventoryService` is a local singleton with `DontDestroyOnLoad`;
there is no network identity, no ownership, and no replication.
In a multiplayer game the server must own inventory state and
validate every add/remove/equip/use; this package can serve as
the local representation only, fed by server-approved snapshots
through `LoadFromSaveData`. Treat any claim that this package
"supports multiplayer" as false for v0.1.0.

### 6.9 Admin — pattern only, not built

**Status: no code.** There is no admin system in this repo. The
pattern, when one exists, is to use the same public API an
in-game console would: `AddItemById` to grant items,
`RemoveItem` to take them, `PlayerInventory.Clear()` to wipe an
inventory, and `ToSaveData()` to inspect a player's saved
state. All of those are already public and need no new API.
Admin tooling owns permissions and logging; the inventory does
not log who changed what.

### 6.10 Terrain Tools — no meaningful integration

**Status: no code, and no natural integration point.** Terrain
tools generate or edit terrain; the inventory holds items. The
only connection is indirect: terrain or world objects that drop
loot would call `AddItem` / `AddItemById` (the pickup pattern in
§5.1), and an item's `Prefab` is what `Drop()` spawns into the
world. There is nothing else to integrate, and this guide does
not invent a terrain API.

### 6.11 Developer Plugin — pattern only, not built

**Status: no code.** There is no developer plugin in this repo.
The closest existing pieces are the editor tooling in §3.9
(menu items, demo-item window, custom inspector) and the
Play-mode self-test (§3.10), which are the developer-facing
surface of this package today. A future developer plugin could
wrap those — e.g. one-click demo-scene setup, or a window that
runs `InventorySelfTest` and reports the result — but none of
that exists in v0.1.0.

---

## 7. Troubleshooting / FAQ

### Q1. "I added the service, but `InventoryService.Instance` is null."

**A.** `Instance` is set in `Awake()`. If you read it before any
`Awake()` has run (e.g. from another object's `Awake()` that
executes first, or from a static constructor), it is still null.
Read it in `Start()` or later, or make sure the service's
GameObject is active in the scene. Also check that the service
GameObject is not disabled — a disabled `MonoBehaviour` never
runs `Awake()`. If you have two services in the scene, the
second destroys itself in `Awake()` and `Instance` stays
pointing at the first — that is by design, not a bug.

### Q2. "I pressed Play and got `[DemoInventory] No InventoryService in the scene`."

**A.** The demo driver could not find a service. Add one with
**Sdx17 > Inventory > Add Inventory Service to Scene**, or
create the GameObject manually and add the component. If the
service exists but the demo still cannot find it, check that
the service GameObject is active and in the same scene the demo
is in (the demo falls back to `FindObjectOfType`, which only
finds active objects in loaded scenes). Note that the service
is `DontDestroyOnLoad` — if you re-enter Play mode after
stopping, a stale service from the previous run should have
been destroyed with the scene; if one survives, the new scene's
service will destroy itself and the old one remains, which can
look like the catalog is empty.

### Q3. "`AddItemById("health_potion", 3)` returns false, but the item exists."

**A.** `AddItemById` resolves the id through `FindItem`, which
only searches the service's **catalog**. The item asset must be
in the inspector `itemCatalog` list, or registered in code with
`RegisterItem` / `RegisterItems` (the demo does this via
`DemoItems.CreateAndRegister`). Check three things, in order:
(1) the id string matches exactly — ids are case-sensitive and
the demo id is `health_potion`, not `Health_Potion`;
(2) the item is actually registered — `RegisterItem` returns
`false` for a duplicate id, so a second asset with the same id
never registers;
(3) the add itself is not being refused — a `false` from
`AddItem` also means the weight limit or a full inventory
refused the whole amount (see Q4). To tell the two apart, call
`FindItem("health_potion")` — if it returns null, the item is
not registered; if it returns the item, the add was refused.

### Q4. "Add returns false even though there are empty slots."

**A.** The weight limit is the usual cause. `Add` refuses the
whole amount when `CurrentWeight + item.Weight * amount` would
exceed `MaxWeight` (default 100) — even when slots are free.
Check `PlayerInventory.CurrentWeight` against
`PlayerInventory.MaxWeight` in the Console or debugger. The
other cause is `CanFit`: a non-stackable item needs one empty
slot per unit, so adding 5 swords needs 5 empty slots, not 1.
Remember that `Add` is all-or-nothing by design — it never
leaves a partial stack behind, so a refused add means zero
items were added, not "some".

### Q5. "Save works, but after loading my items are gone."

**A.** Almost always a catalog problem. Save data stores item
**ids**, not references. On load, each id is resolved through
`FindItem`, which searches the catalog. If the items are not
registered at load time — for example, because the catalog was
empty in the new scene, or because the demo items were created
in code in the previous scene and never re-registered — every
entry is skipped silently and the inventory loads empty. Fix:
make sure the same items are registered before calling
`Load()` / `LoadFromSaveData` (drag them into the inspector
catalog, or call `DemoItems.CreateAndRegister(service)` /
`RegisterItems` first). Also check `Load()`'s return value:
`false` means no provider was set, no save exists, or the
loaded data was null — in all three cases nothing is loaded
and nothing is logged. Finally, confirm the provider actually
persisted the data — `Save()` returns `true` whenever a
provider is set, even if the provider itself failed to write.

### Q6. "Equipping a weapon does nothing in my game."

**A.** `EquipItem` only records the choice — it sets
`PlayerInventory.EquippedItem` and fires `OnItemEquipped`. It
does not change the player's model, animations, or damage.
Your combat system must read `EquippedItem` and use its
`Damage` (see §5.3). Also check the equip rules: only items
with `Type == Weapon` equip (`IsEquippable`), and the item
must actually be held — `Equip` returns `false` for a weapon
the player does not own. If you equip by id, resolve it first
with `FindItem` and confirm `Has(item)` is true.

### Q7. "Using a potion consumes it but my health does not change."

**A.** By design. `UseItem` returns the heal amount; it never
touches a health system, because this package has no health
system. Your code must apply the returned value (see §5.2).
If you called `PlayerInventory.Use(item)` directly instead of
`service.UseItem(item)`, the same applies — and you also
bypassed the `OnItemUsed` / `OnChanged` events, so your UI
will not refresh. Always go through the service.

### Q8. "My UI does not update when items change."

**A.** Two common causes. First, you may be reading the
inventory directly instead of subscribing to events — poll
`GetGridSnapshot()` / `GetHotbarSnapshot()` only when the
service raises `OnChanged`, or subscribe to
`InventoryUIHooks.OnGridSnapshot` / `OnHotbarSnapshot`, which
re-publish after every change. Second, you may be mutating a
raw `Inventory` (e.g. `service.PlayerInventory.Add(...)` or
`.Clear()`) instead of going through the service methods —
direct `Inventory` calls fire no events at all, so nothing
listening to `OnChanged` will ever know. (The demo's key 6
does exactly this with `Clear()` and then logs manually.)
Rule: gameplay code talks to `InventoryService`, never to a
raw `Inventory`.

### Q9. "I changed `maxWeight` in the inspector during Play mode and nothing happened."

**A.** The service creates `PlayerInventory` once, in
`Awake()`, from the inspector value at that moment. Later
inspector edits do not reach the live inventory. Use
`PlayerInventory.SetMaxWeight(limit)` at runtime. Note the
same applies to the `itemCatalog` list: adding an asset to it
in the inspector during Play mode does not register it —
registration happens through `RegisterItem` /
`RegisterItems`, which the service does not re-run. Register
new items in code.

### Q10. "Can I change the inventory size from 24 slots / 5 hotbar slots?"

**A.** Not in v0.1.0 without editing code. The sizes are
`const int SlotCount = 24` and `const int HotbarSlotCount = 5`
on `Inventory`, and the UI hooks, save data, and demo all
assume those sizes. Changing the constants is a one-line code
change, but it is untested — the self-test and the demo assume
24/5 — so treat it as a fork, not a setting, and re-run the
full test checklist afterwards.

---

## 8. Performance notes

These are reasoning notes from the code, not measurements —
**no profiling has been done**, because the package has never
run in Unity. Treat every claim here as "this is what the code
does", not as a benchmark.

- **Adds are O(slots).** `Add` scans up to 24 slots twice
  (merge pass, then empty-slot pass), and `CanFit` scans them
  the same way before that — so one `AddItem` call can scan the
  slots up to three times including the weight check. At 24
  slots this is trivial, but do not call `AddItem` in a tight
  loop for hundreds of items; batch them or accept the cost.
- **`CurrentWeight` is computed, not cached.** Every access
  iterates all 24 main slots plus 5 hotbar slots and multiplies
  weight by count. It is called inside `Add`, so an add already
  pays for a full weight scan. Do not poll `CurrentWeight`
  every frame for a HUD — read it when `OnChanged` fires and
  cache the value.
- **`Count` / `Has` scan everything.** Both iterate all 29
  slots. Fine for gameplay code and quest checks; do not call
  them per-frame per-item.
- **Events are synchronous and unguarded.** `OnChanged` invokes
  every subscriber inline, on the calling thread, with no
  try/catch. A subscriber that throws will break the call that
  raised the event. Keep subscribers short and exception-free,
  and never call back into the service from inside an event
  handler in a way that re-enters the same mutation
  (e.g. calling `AddItem` from `OnItemAdded`) — the code does
  not guard against re-entrancy.
- **Snapshots allocate.** Every `PublishSnapshots` builds two
  fresh `List<SlotViewData>` (24 + 5 entries) and fires both
  events, even if nobody is listening to one of them. That is
  one small allocation per inventory change — fine for gameplay
  frequency, not for per-frame changes (which should not happen
  anyway, since inventory changes are player actions).
- **Save data allocates arrays.** `ToSaveData` allocates a 24-
  entry and a 5-entry array on every call. Do not call it every
  frame; save after meaningful changes or on a timer (see
  §6.1).
- **No per-frame work exists.** Nothing in the runtime runs in
  `Update` except the demo driver (`DemoInventory`), which
  only polls six keys. The service and the inventory do no work
  when nothing changes.

---

## 9. Limitations (honest)

- **UNTESTED IN UNITY.** No editor import, no Play-mode run, no
  build, and no profiling have ever been performed. Every
  behaviour in this guide is read from source, not observed.
  Run the 7-step test checklist in the `README.md` before
  calling anything in this package tested.
- **Not a UI.** No Canvas, no prefabs, no art, no drag-and-drop,
  no tooltips, no sounds. `InventoryUIHooks` only hands your UI
  data; you build and render the UI.
- **Not a save system.** It only produces and restores
  `InventorySaveData`. Without an `ISaveData` implementation
  (the Sdx17 Secure Save system, or your own), nothing is
  persisted. `Save()` returning `true` only means a provider
  was set — it does not prove the provider wrote anything.
- **Not a combat, quest, health, NPC, dialogue, matchmaking,
  multiplayer, admin, or terrain system.** Those integrations
  are either read-only (§6.2–§6.4) or patterns for systems that
  do not exist yet (§6.5–§6.11). Do not buy or use this package
  expecting any of them.
- **No multiplayer, no networking, no database, no server
  validation.** A client-sent save is trusted as-is by
  `FromSaveData`; nothing validates that it is legitimate.
- **Fixed sizes.** 24 slots, 5 hotbar slots, one equipped item.
  No setting changes them in v0.1.0.
- **No item durability, no crafting, no shops, no currency
  spending, no item stacking across the hotbar and main slots
  as one pool** (they are separate arrays; `Count` / `Has`
  sum both, but `Add` only fills main slots — the hotbar is
  filled only by `AssignToHotbar`).
- **No sorting, filtering, searching, or categorisation UI.**
  Slots are a flat array in insertion order.
- **Single-player, single-inventory assumption.** The service
  is a singleton owning one player inventory. NPC or companion
  inventories must be plain `Inventory` objects you manage
  yourself, with no events and no save wiring unless you add
  them.
- **Rarity is cosmetic.** It changes no stats, drop rates, or
  prices in v0.1.0.
- **Weight is the only limit.** There is no per-slot weight, no
  volume, and no encumbrance effect — exceeding the limit
  simply refuses the add.
- **Error reporting is minimal.** Most failures are a `false`
  or `0` return with no log and no reason code. Callers must
  check return values; the package will not tell you *why* an
  add failed (weight vs. full vs. unknown id) — see Q3/Q4 for
  how to tell them apart.

---

## 10. Where to go next

- `README.md` — the short version of this guide, plus the
  7-step test checklist that must pass before v0.1.0 may be
  called tested.
- `CHANGELOG.md` — version history (0.1.0 only).
- `Samples~/Demo/README.md` — demo scene setup.
- `LISTING.md` — Asset Store listing draft (not a live listing).
- `SELLING_REQUIREMENTS.md` — everything that must pass before
  this package is submitted anywhere.

*Sdx17 Inventory Framework v0.1.0 — UNTESTED IN UNITY. MIT License,
Copyright 2026 Sdx17.*
