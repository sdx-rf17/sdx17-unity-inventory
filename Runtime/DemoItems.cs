using System.Collections.Generic;
using UnityEngine;

namespace Sdx17.Inventory
{
    /// <summary>
    /// The 8 demo items, created in code at runtime so the demo scene works
    /// with no .asset files at all. Each item is also documented in the
    /// README with the values to use when creating matching .asset files
    /// in the editor instead.
    /// </summary>
    public static class DemoItems
    {
        public static IReadOnlyList<ItemDefinition> CreateAll()
        {
            return new[]
            {
                // Weapons
                ItemDefinition.CreateRuntime(
                    "iron_sword", "Iron Sword", ItemType.Weapon, ItemRarity.Common,
                    false, 1, 3.5f, 0, 12,
                    "A plain iron sword. Reliable, nothing fancy."),
                ItemDefinition.CreateRuntime(
                    "hunter_bow", "Hunter Bow", ItemType.Weapon, ItemRarity.Uncommon,
                    false, 1, 2.0f, 0, 9,
                    "A bow strung for hunting. Light and quick."),

                // Consumables
                ItemDefinition.CreateRuntime(
                    "health_potion", "Health Potion", ItemType.Consumable, ItemRarity.Common,
                    true, 10, 0.5f, 25, 0,
                    "Restores 25 health when used."),
                ItemDefinition.CreateRuntime(
                    "bread", "Bread", ItemType.Consumable, ItemRarity.Common,
                    true, 20, 0.3f, 10, 0,
                    "Fresh bread. Restores 10 health."),

                // Materials
                ItemDefinition.CreateRuntime(
                    "iron_ore", "Iron Ore", ItemType.Material, ItemRarity.Common,
                    true, 99, 1.0f, 0, 0,
                    "Raw iron ore. Smelt it or sell it."),
                ItemDefinition.CreateRuntime(
                    "oak_wood", "Oak Wood", ItemType.Material, ItemRarity.Common,
                    true, 99, 0.8f, 0, 0,
                    "Sturdy oak wood for crafting."),
                ItemDefinition.CreateRuntime(
                    "magic_crystal", "Magic Crystal", ItemType.Material, ItemRarity.Rare,
                    true, 20, 0.4f, 0, 0,
                    "A crystal that hums faintly. Rare crafting material."),

                // Currency
                ItemDefinition.CreateRuntime(
                    "gold_coin", "Gold Coin", ItemType.Currency, ItemRarity.Common,
                    true, 999, 0.01f, 0, 0,
                    "Currency. Spend it at shops."),
            };
        }

        /// <summary>
        /// Convenience: creates the demo items and registers them with the
        /// service so save/load can resolve them by id. Returns the items.
        /// </summary>
        public static IReadOnlyList<ItemDefinition> CreateAndRegister(InventoryService service)
        {
            var items = CreateAll();
            service?.RegisterItems(items);
            return items;
        }
    }
}
