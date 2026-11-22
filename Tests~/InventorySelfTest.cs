using UnityEngine;

namespace Sdx17.Inventory.Tests
{
    /// <summary>
    /// Play-mode self-test. Attach to a GameObject in a test scene with an
    /// InventoryService, press Play, and read the Console: every check logs
    /// PASS or FAIL with its name. It uses a fresh Inventory instance (not
    /// the service's live inventory) for the logic checks, then exercises
    /// the service add/equip/use path on the live inventory.
    /// No test framework is required; this runs in plain Play mode.
    /// </summary>
    public class InventorySelfTest : MonoBehaviour
    {
        [SerializeField] private InventoryService service;

        private int passed;
        private int failed;

        private void Start()
        {
            if (service == null)
                service = InventoryService.Instance;
            RunAll();
            Debug.Log($"[InventorySelfTest] DONE: {passed} passed, {failed} failed.");
        }

        private void Check(bool condition, string name)
        {
            if (condition)
            {
                passed++;
                Debug.Log($"[InventorySelfTest] PASS: {name}");
            }
            else
            {
                failed++;
                Debug.LogError($"[InventorySelfTest] FAIL: {name}");
            }
        }

        private void RunAll()
        {
            var potion = ItemDefinition.CreateRuntime("test_potion", "Test Potion", ItemType.Consumable, ItemRarity.Common, true, 10, 0.5f, 25, 0, "Test.");
            var sword = ItemDefinition.CreateRuntime("test_sword", "Test Sword", ItemType.Weapon, ItemRarity.Common, false, 1, 3.5f, 0, 12, "Test.");
            var ore = ItemDefinition.CreateRuntime("test_ore", "Test Ore", ItemType.Material, ItemRarity.Common, true, 99, 1.0f, 0, 0, "Test.");
            var coin = ItemDefinition.CreateRuntime("test_coin", "Test Coin", ItemType.Currency, ItemRarity.Common, true, 999, 0.01f, 0, 0, "Test.");

            var inventory = new Inventory(100f);

            // Add / count / stack merge
            Check(inventory.Add(potion, 5), "Add 5 potions succeeds");
            Check(inventory.Count(potion) == 5, "Count is 5 after add");
            Check(inventory.Add(potion, 8), "Add 8 more potions merges across stacks");
            Check(inventory.Count(potion) == 13, "Count is 13 after merge (10 + 3)");
            Check(inventory.UsedSlots == 2, "Two slots used after stack split at maxStack 10");

            // Has / remove
            Check(inventory.Has(potion, 13), "Has 13 potions");
            Check(!inventory.Has(potion, 14), "Does not have 14 potions");
            Check(inventory.Remove(potion, 3) == 3, "Remove 3 potions");
            Check(inventory.Count(potion) == 10, "Count is 10 after remove");

            // Non-stackable weapons occupy one slot each
            Check(inventory.Add(sword, 1), "Add sword succeeds");
            Check(inventory.Add(sword, 1), "Second sword takes its own slot");
            Check(inventory.Count(sword) == 2, "Two swords held");

            // Weight limit refuses the whole add
            var heavy = new Inventory(1f);
            Check(!heavy.Add(ore, 2), "Weight limit refuses 2 ore in a 1-weight inventory");
            Check(heavy.Count(ore) == 0, "Refused add leaves nothing behind");

            // Equip rules
            Check(!inventory.Equip(potion), "Cannot equip a consumable");
            Check(inventory.Equip(sword), "Equip sword succeeds");
            Check(inventory.EquippedItem == sword, "Equipped item is the sword");
            inventory.Unequip();
            Check(inventory.EquippedItem == null, "Unequip clears equipped item");

            // Use rules
            int before = inventory.Count(potion);
            int healed = inventory.Use(potion);
            Check(healed == 25, "Use potion heals 25");
            Check(inventory.Count(potion) == before - 1, "Use consumes one potion");
            Check(inventory.Use(sword) == 0, "Use on a weapon heals 0 and consumes nothing");
            Check(inventory.Count(sword) == 2, "Sword count unchanged after failed use");

            // Hotbar
            Check(inventory.AssignToHotbar(0, 0), "Assign slot 0 to hotbar 0");
            Check(!inventory.Hotbar[0].IsEmpty, "Hotbar slot 0 is filled");
            Check(!inventory.AssignToHotbar(99, 0), "Bad slot index is refused");
            Check(!inventory.AssignToHotbar(0, 99), "Bad hotbar index is refused");

            // Drop without a prefab removes the item and does not crash
            Check(inventory.Drop(coin, 1, Vector3.zero) == false, "Drop of unheld coin fails");
            inventory.Add(coin, 5);
            Check(inventory.Drop(coin, 2, Vector3.zero), "Drop 2 coins succeeds with no prefab");
            Check(inventory.Count(coin) == 3, "3 coins remain after drop");

            // Save / load round trip by id
            var data = inventory.ToSaveData();
            var restored = new Inventory(100f);
            restored.FromSaveData(data, id => id switch
            {
                "test_potion" => potion,
                "test_sword" => sword,
                "test_ore" => ore,
                "test_coin" => coin,
                _ => null
            });
            Check(restored.Count(potion) == inventory.Count(potion), "Save/load restores potion count");
            Check(restored.Count(coin) == 3, "Save/load restores coin count");

            // Unknown ids in a save are skipped, not fatal
            var corrupt = new InventorySaveData
            {
                Slots = new[] { new InventorySaveEntry("no_such_item", 5) },
                Hotbar = new InventorySaveEntry[0],
                EquippedItemId = string.Empty
            };
            var safe = new Inventory(100f);
            safe.FromSaveData(corrupt, _ => null);
            Check(safe.UsedSlots == 0, "Unknown save id is skipped safely");

            // Service path (live inventory)
            if (service != null)
            {
                service.RegisterItem(potion);
                service.RegisterItem(sword);
                int serviceBefore = service.PlayerInventory.Count(potion);
                Check(service.AddItem(potion, 2), "Service AddItem succeeds");
                Check(service.PlayerInventory.Count(potion) == serviceBefore + 2, "Service inventory gained 2 potions");
                Check(service.EquipItem(sword) == service.PlayerInventory.Has(sword), "Service EquipItem matches Has(sword)");
                Check(service.AddItemById("test_potion", 1), "Service AddItemById resolves by id");
                Check(!service.AddItemById("no_such_item", 1), "Service AddItemById refuses unknown id");
            }
            else
            {
                Debug.LogWarning("[InventorySelfTest] No InventoryService in scene — service checks skipped.");
            }

            Check(inventory.Add(null, 1) == false, "Add null item is refused");
            Check(inventory.Add(potion, 0) == false, "Add zero amount is refused");
            Check(inventory.Remove(null, 1) == 0, "Remove null item removes 0");
        }
    }
}
