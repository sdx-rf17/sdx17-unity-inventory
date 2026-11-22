# Demo — Sdx17 Inventory Framework

Minimal demo scene setup. No `.unity` scene file is included in this
package — build the scene yourself in under 5 minutes with the steps
below, then press Play.

## Scene setup

1. Create a new scene (File > New Scene, Basic 3D is fine).
2. Create an empty GameObject named `InventoryService` and add the
   `InventoryService` component (or use Sdx17 > Inventory > Add
   Inventory Service to Scene).
3. Create an empty GameObject named `Demo` and add the `DemoInventory`
   component from this folder (`Samples~/Demo/DemoInventory.cs`).
   Drag the `InventoryService` GameObject into its Service field
   (it also finds the service automatically if left empty).
4. Optional: create an empty GameObject named `InventoryUIHooks` and add
   the `InventoryUIHooks` component, with the service assigned, if you
   want to watch the grid/hotbar snapshots in a UI you build.
5. Press Play.

## What happens

On Start, the demo registers the 8 demo items and adds starter items:
1 Iron Sword, 5 Health Potions, 3 Bread, 10 Iron Ore, 50 Gold Coins.
Every change is logged to the Console — no UI is needed to verify it.

## Keys (in Play mode)

| Key | Action |
|-----|--------|
| 1 | Add 1 Health Potion |
| 2 | Use 1 Health Potion (logs heal amount) |
| 3 | Equip the Iron Sword |
| 4 | Assign slot 0 to hotbar slot 0 |
| 5 | Drop 1 Gold Coin in front of the player |
| 6 | Clear the inventory |

## Expected Console output

- `[DemoInventory] Slots used 5/24, weight ...` on start.
- `[DemoInventory] Equipped: Iron Sword` after pressing 3.
- No errors. If you see `[DemoInventory] No InventoryService in the
  scene`, repeat step 2.
