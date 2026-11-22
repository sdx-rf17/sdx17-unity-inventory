using System;
using UnityEngine;

namespace Sdx17.Inventory
{
    /// <summary>
    /// The inventory itself: 24 slots plus a 5-slot hotbar, with a weight
    /// limit. Pure logic — no MonoBehaviour, no UI, no saving. The
    /// InventoryService owns one instance and raises events around it.
    /// Weight is checked before adding: an add that would exceed the limit
    /// is refused in full (no partial add) so the player never sees a
    /// half-added stack.
    /// </summary>
    [Serializable]
    public class Inventory
    {
        public const int SlotCount = 24;
        public const int HotbarSlotCount = 5;

        [SerializeField] private InventorySlot[] slots = new InventorySlot[SlotCount];
        [SerializeField] private InventorySlot[] hotbar = new InventorySlot[HotbarSlotCount];
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private ItemDefinition equippedItem;

        public InventorySlot[] Slots => slots;
        public InventorySlot[] Hotbar => hotbar;
        public float MaxWeight => maxWeight;
        public ItemDefinition EquippedItem => equippedItem;

        public Inventory()
        {
            InitialiseArrays();
        }

        public Inventory(float weightLimit)
        {
            maxWeight = Mathf.Max(0f, weightLimit);
            InitialiseArrays();
        }

        private void InitialiseArrays()
        {
            if (slots == null || slots.Length != SlotCount)
                slots = new InventorySlot[SlotCount];
            if (hotbar == null || hotbar.Length != HotbarSlotCount)
                hotbar = new InventorySlot[HotbarSlotCount];
            for (int i = 0; i < slots.Length; i++)
                slots[i] ??= new InventorySlot();
            for (int i = 0; i < hotbar.Length; i++)
                hotbar[i] ??= new InventorySlot();
        }

        /// <summary>Total weight of everything in slots and hotbar.</summary>
        public float CurrentWeight
        {
            get
            {
                float total = 0f;
                foreach (var slot in slots)
                    if (slot != null && !slot.IsEmpty)
                        total += slot.Definition.Weight * slot.Count;
                foreach (var slot in hotbar)
                    if (slot != null && !slot.IsEmpty)
                        total += slot.Definition.Weight * slot.Count;
                return total;
            }
        }

        public int UsedSlots
        {
            get
            {
                int used = 0;
                foreach (var slot in slots)
                    if (slot != null && !slot.IsEmpty)
                        used++;
                return used;
            }
        }

        public bool Has(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;
            return Count(item) >= amount;
        }

        public int Count(ItemDefinition item)
        {
            if (item == null)
                return 0;
            int total = 0;
            foreach (var slot in slots)
                if (slot != null && slot.Definition == item)
                    total += slot.Count;
            foreach (var slot in hotbar)
                if (slot != null && slot.Definition == item)
                    total += slot.Count;
            return total;
        }

        /// <summary>
        /// Adds items, merging into existing stacks first, then empty slots.
        /// Returns false and adds nothing when the weight limit or a full
        /// inventory prevents the whole amount from fitting.
        /// </summary>
        public bool Add(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;
            if (CurrentWeight + item.Weight * amount > maxWeight)
                return false;
            if (!CanFit(item, amount))
                return false;

            int remaining = amount;
            foreach (var slot in slots)
            {
                if (remaining <= 0)
                    break;
                if (slot.CanMerge(item))
                    remaining = slot.Add(item, remaining);
            }
            foreach (var slot in slots)
            {
                if (remaining <= 0)
                    break;
                if (slot.IsEmpty)
                    remaining = slot.Add(item, remaining);
            }
            return remaining == 0;
        }

        /// <summary>Checks fit without changing anything.</summary>
        public bool CanFit(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return false;
            int remaining = amount;
            foreach (var slot in slots)
            {
                if (remaining <= 0)
                    return true;
                if (slot.CanMerge(item))
                    remaining -= Mathf.Min(item.MaxStack - slot.Count, remaining);
            }
            foreach (var slot in slots)
            {
                if (remaining <= 0)
                    return true;
                if (slot.IsEmpty)
                    remaining -= item.Stackable ? Mathf.Min(item.MaxStack, remaining) : 1;
            }
            return remaining <= 0;
        }

        /// <summary>
        /// Removes items, hotbar first then main slots. Returns the amount
        /// actually removed (less than requested when not enough was held).
        /// </summary>
        public int Remove(ItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return 0;
            int remaining = amount;
            foreach (var slot in hotbar)
            {
                if (remaining <= 0)
                    break;
                if (slot != null && slot.Definition == item)
                    remaining -= slot.Remove(remaining);
            }
            foreach (var slot in slots)
            {
                if (remaining <= 0)
                    break;
                if (slot != null && slot.Definition == item)
                    remaining -= slot.Remove(remaining);
            }
            if (equippedItem == item && Count(item) == 0)
                equippedItem = null;
            return amount - remaining;
        }

        /// <summary>
        /// Moves a stack from a main slot into a hotbar slot (or swaps when
        /// the hotbar slot is occupied). Returns false for bad indexes or an
        /// empty source slot.
        /// </summary>
        public bool AssignToHotbar(int slotIndex, int hotbarIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length)
                return false;
            if (hotbarIndex < 0 || hotbarIndex >= hotbar.Length)
                return false;
            var source = slots[slotIndex];
            if (source == null || source.IsEmpty)
                return false;

            var target = hotbar[hotbarIndex];
            var heldDefinition = target.Definition;
            var heldCount = target.Count;
            target.Set(source.Definition, source.Count);
            if (heldDefinition == null)
                source.Clear();
            else
                source.Set(heldDefinition, heldCount);
            return true;
        }

        /// <summary>
        /// Equips a weapon the player holds. Returns false for non-weapons
        /// or items not in the inventory. Equipping does not remove the item.
        /// </summary>
        public bool Equip(ItemDefinition item)
        {
            if (item == null || !item.IsEquippable)
                return false;
            if (!Has(item))
                return false;
            equippedItem = item;
            return true;
        }

        public void Unequip()
        {
            equippedItem = null;
        }

        /// <summary>
        /// Uses one consumable and returns its heal amount (0 for other
        /// types or when the item is not held). The caller applies healing
        /// to the player; the inventory only consumes the item.
        /// </summary>
        public int Use(ItemDefinition item)
        {
            if (item == null || !item.IsUsable)
                return 0;
            if (!Has(item))
                return 0;
            Remove(item, 1);
            return item.HealAmount;
        }

        /// <summary>
        /// Drops items on the ground: removes them from the inventory and
        /// spawns the item prefab at <paramref name="position"/> when the
        /// definition has one. Returns false when the item is not held.
        /// </summary>
        public bool Drop(ItemDefinition item, int amount, Vector3 position)
        {
            if (item == null || amount <= 0)
                return false;
            if (!Has(item, amount))
                return false;
            Remove(item, amount);
            if (item.Prefab != null)
            {
                var dropped = UnityEngine.Object.Instantiate(item.Prefab, position, Quaternion.identity);
                dropped.name = item.DisplayName + " (Dropped)";
            }
            return true;
        }

        public void Clear()
        {
            foreach (var slot in slots)
                slot?.Clear();
            foreach (var slot in hotbar)
                slot?.Clear();
            equippedItem = null;
        }

        public void SetMaxWeight(float limit)
        {
            maxWeight = Mathf.Max(0f, limit);
        }

        /// <summary>Serialises the inventory into plain data for saving.</summary>
        public InventorySaveData ToSaveData()
        {
            var data = new InventorySaveData
            {
                Slots = Serialise(slots),
                Hotbar = Serialise(hotbar),
                EquippedItemId = equippedItem != null ? equippedItem.Id : string.Empty
            };
            return data;
        }

        private static InventorySaveEntry[] Serialise(InventorySlot[] source)
        {
            var entries = new InventorySaveEntry[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                var slot = source[i];
                entries[i] = slot != null && !slot.IsEmpty
                    ? new InventorySaveEntry(slot.Definition.Id, slot.Count)
                    : new InventorySaveEntry(string.Empty, 0);
            }
            return entries;
        }

        /// <summary>
        /// Restores state from save data. <paramref name="resolve"/> maps an
        /// item id back to its definition; unknown ids are skipped, never
        /// treated as an error that blocks the rest of the load.
        /// </summary>
        public void FromSaveData(InventorySaveData data, Func<string, ItemDefinition> resolve)
        {
            if (data == null || resolve == null)
                return;
            Clear();
            Restore(slots, data.Slots, resolve);
            Restore(hotbar, data.Hotbar, resolve);
            if (!string.IsNullOrEmpty(data.EquippedItemId))
            {
                var equipped = resolve(data.EquippedItemId);
                if (equipped != null && equipped.IsEquippable && Has(equipped))
                    equippedItem = equipped;
            }
        }

        private static void Restore(InventorySlot[] target, InventorySaveEntry[] entries, Func<string, ItemDefinition> resolve)
        {
            if (entries == null)
                return;
            int limit = Mathf.Min(target.Length, entries.Length);
            for (int i = 0; i < limit; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.ItemId) || entry.Count <= 0)
                    continue;
                var definition = resolve(entry.ItemId);
                if (definition == null)
                    continue;
                target[i].Set(definition, entry.Count);
            }
        }
    }
}
