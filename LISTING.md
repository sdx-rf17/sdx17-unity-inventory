# Asset Store Listing — DRAFT (not submitted)

> **This is a draft only. No Asset Store listing exists yet, nothing
> has been submitted, and no sales or reviews exist.** Do not copy
> this file to the Asset Store until every item in
> `SELLING_REQUIREMENTS.md` has passed, including the full Unity
> test run. The package is **v0.1.0 — UNTESTED IN UNITY**.

---

## Title

**Sdx17 Inventory Framework**

## Short description

Lightweight Unity inventory: 24 slots, stacking, weight limits, 5-slot hotbar, equip/use/drop, events and save hooks. No paid dependencies.

*(139 characters — under the 200-character limit. Re-count before
submitting; the Asset Store counts what you paste, not what this
file says.)*

## Long description

**Sdx17 Inventory Framework** is a lightweight inventory system
for Unity 2022.3 LTS. It gives you the inventory core — items,
slots, stacking, weight, hotbar, equip/use/drop, events, and save
hooks — without dragging in a UI framework, a save system, or a
paid dependency you did not ask for.

**What you get**

- **Item definitions as ScriptableObjects.** Create items in the
  editor with id, display name, type (Weapon / Consumable /
  Material / Currency), rarity, stackable flag, max stack, weight,
  heal and damage values, icon, world prefab, and description.
  A custom inspector keeps the data sane, and a one-click window
  creates 8 demo items for you.
- **24-slot inventory with stacking and a weight limit.**
  Stacks merge into existing slots first, then fill empty ones.
  Adds are all-or-nothing: if the whole amount does not fit, or
  would exceed the weight limit, nothing is added — the player
  never sees a half-added stack.
- **5-slot hotbar.** Move a whole stack from the inventory to the
  hotbar (or swap them) with one call.
- **Equip / use / drop.** Only weapons equip, only consumables
  are used, and the rules are enforced by the inventory so your
  gameplay code does not have to check. Using a consumable
  returns its heal amount for your health system to apply.
  Dropping spawns the item's prefab in the world.
- **Events for UI and gameplay.** `OnChanged`, `OnItemAdded`,
  `OnItemRemoved`, `OnItemEquipped`, `OnItemUsed`, plus two
  UnityEvents for inspector wiring. A `InventoryUIHooks`
  component hands any UI clean snapshot data (grid + hotbar)
  and forwards clicks and hotbar keys back to the service.
- **Save/load hooks.** Save data stores item ids, not object
  references, so saves survive asset moves. Unknown ids on load
  are skipped instead of failing the whole load. The inventory
  never touches files, PlayerPrefs, or encryption itself — you
  wire your save system in through one small `ISaveData`
  interface.
- **Demo scene guide and Play-mode self-test.** Build the demo
  scene in about 5 minutes and exercise the framework from the
  number keys, with every action logged to the Console. A
  self-test component logs PASS/FAIL for the core paths.

**What it is not**

This is an inventory core, not a complete game system. It ships
**no UI** (no Canvas, no prefabs, no art), **no save system**
(only the hooks for one), **no combat, quest, or health system**,
and **no multiplayer or networking**. If you want a
drag-and-drop inventory UI out of the box, this package is not
for you — it is for developers who want a small, readable core
they can wire into their own UI and systems.

**Honest status**

Version 0.1.0. The code has been written and reviewed as C#
source. It has not yet been run in the Unity editor or in Play
mode in the publisher's environment — the listing will only be
submitted after the full test checklist (clean import, demo
scene, self-test, save/load, standalone build) passes in Unity
2022.3 LTS. The listing text will be updated to say so, one way
or the other, before submission.

## Keywords (10)

1. inventory
2. inventory system
3. item system
4. hotbar
5. rpg inventory
6. stacking
7. scriptableobject
8. save load
9. equipment
10. survival inventory

## Category

**Tools / Game Toolkits** — suggested category. Re-check the
category at listing time against the Asset Store's current
category list; categories change, and the wrong category hurts
discoverability more than a wrong keyword.

## Suggested price

**$25**

### Why $25 — justification vs. comparables

| Asset (comparable) | Price |
|--------------------|-------|
| Vault Inventory | $39.99 |
| Universal Inventory System | $14 |
| Inventory System (older asset) | $4.99 |

This package sits between the $14 and $39.99 comparables: it is
a focused, dependency-free inventory core with a hotbar,
save/load hooks, and editor tooling — more complete than the
cheapest options, smaller and cheaper than the full-featured
$39.99 kits that bundle a UI. $25 is inside the **$15–$35**
range a new publisher should target to get first reviews.

**Do not price at $95 on day one.** A new publisher with no
reviews, no sales history, and a v0.1.0 package has no evidence
to support a premium price, and a high price with zero reviews
suppresses the first sales that would earn reviews. Price to
get the first reviews, then revisit the price with evidence.

**Revenue note (not a promise):** Unity pays publishers 70% of
the list price. At $25, that is **$17.50 per sale before tax,
refunds, and any applicable fees**. This is arithmetic, not a
sales forecast — **the price does not guarantee any sales at
all**, and no sales are claimed or projected here.

## What's included

- Full C# source (namespace `Sdx17.Inventory`), MIT licensed
- `ItemDefinition` ScriptableObjects + custom inspector
- `Inventory` (24 slots + 5-slot hotbar, stacking, weight limit,
  equip / use / drop, save data)
- `InventorySlot` (merge / add / remove / clear / restore)
- `InventoryService` singleton with events and save/load wiring
- `ISaveData` interface + `InventorySaveData` /
  `InventorySaveEntry` plain-data save format
- `DemoItems` factory — the 8 demo items in code
- `InventoryUIHooks` — grid/hotbar snapshots + input forwarding
- Editor window + menu items under `Sdx17 > Inventory`
  (create demo items, create item definition, add service to
  scene)
- Demo driver (`DemoInventory`) + demo scene setup guide
  (`Samples~/Demo/README.md`)
- Play-mode self-test (`InventorySelfTest`, logs PASS/FAIL)
- `README.md`, full user guide (`Documentation~/GUIDE.md`),
  `CHANGELOG.md`, MIT `LICENSE`

## Requirements / Dependencies

- **Unity 2022.3 LTS** (package declares `"unity": "2022.3"`,
  `"unityRelease": "0f1"`). Test before use on any other version.
- **URP-compatible, Built-in Render Pipeline compatible.** The
  runtime touches no rendering, so either pipeline works.
- **No paid dependencies. No third-party code.** Everything in
  the package is original code by Sdx17, MIT licensed. See
  `ThirdPartyNotices.txt`.
- No .NET packages, no external services, no account needed.

## Demo scene description

The package ships a demo scene **guide**, not a pre-built
`.unity` scene file — you build the scene yourself in about
5 minutes (steps in `Samples~/Demo/README.md`):

1. New scene, add an `InventoryService` (menu:
   **Sdx17 > Inventory > Add Inventory Service to Scene**).
2. Add a `Demo` GameObject with the `DemoInventory` component.
3. Press Play. The demo registers the 8 demo items and adds
   starter items (1 Iron Sword, 5 Health Potions, 3 Bread,
   10 Iron Ore, 50 Gold Coins), logging every change to the
   Console — no UI needed.
4. Number keys exercise the framework: **1** add a potion,
   **2** use a potion, **3** equip the sword, **4** assign slot 0
   to hotbar 0, **5** drop a coin in front of the player,
   **6** clear the inventory.

Screenshots and video for the listing must be real Unity
captures of this demo — see `SELLING_REQUIREMENTS.md`. No
concept art may be presented as a screenshot.

## Support

- **Publisher:** Sdx17
- **GitHub:** https://github.com/sdx-rf17
- **Contact:** via the GitHub profile linked above. **No email
  address is listed here** — no support email exists for this
  publisher, and no email address may be invented for the
  listing. Add a real support email only if and when the
  publisher creates one.
- Support covers the code in this package, for the Unity version
  listed above. It does not cover building your game's UI,
  combat, quest, or save system for you.

## Version history

- **0.1.0** (2026-11-21) — First release. Item definitions,
  24-slot inventory with stacking and weight limit, 5-slot
  hotbar, equip / use / drop, service singleton with events,
  save/load hooks via `ISaveData`, UI snapshot bridge, editor
  window and menu items, demo driver + guide, Play-mode
  self-test. **UNTESTED IN UNITY** at the time this draft was
  written — see `SELLING_REQUIREMENTS.md` before submitting.

---

*Draft only. Not submitted. No listing, sales, or reviews exist.*
*Publisher: Sdx17 — https://github.com/sdx-rf17*
