using System;
using UnityEngine;

namespace Sdx17.Inventory
{
    /// <summary>
    /// One slot in the inventory or hotbar. A slot holds one item definition
    /// and a count. Empty slots have a null definition and a count of 0.
    /// Serializable so Unity can show it in the inspector and save it.
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private ItemDefinition definition;
        [SerializeField] private int count;

        public ItemDefinition Definition => definition;
        public int Count => count;
        public bool IsEmpty => definition == null || count <= 0;

        /// <summary>True when this slot holds the given item and can take more.</summary>
        public bool CanMerge(ItemDefinition item)
        {
            if (item == null || IsEmpty)
                return false;
            if (definition != item)
                return false;
            if (!definition.Stackable)
                return false;
            return count < definition.MaxStack;
        }

        /// <summary>
        /// Adds as many as fit in this slot. Returns the amount that did not
        /// fit (0 when everything was added). Only call after checking the
        /// slot is empty or CanMerge.
        /// </summary>
        public int Add(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
                return amount;
            if (!IsEmpty && definition != item)
                return amount;

            if (IsEmpty)
            {
                definition = item;
                count = 0;
            }

            int space = item.Stackable ? item.MaxStack - count : 1 - count;
            if (space <= 0)
                return amount;

            int added = Mathf.Min(space, amount);
            count += added;
            return amount - added;
        }

        /// <summary>
        /// Removes up to <paramref name="amount"/> from the slot.
        /// Returns the amount actually removed. Clears the slot at 0.
        /// </summary>
        public int Remove(int amount)
        {
            if (IsEmpty || amount <= 0)
                return 0;
            int removed = Mathf.Min(count, amount);
            count -= removed;
            if (count <= 0)
                Clear();
            return removed;
        }

        public void Clear()
        {
            definition = null;
            count = 0;
        }

        /// <summary>Used by load: restores a slot without merge rules.</summary>
        public void Set(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                Clear();
                return;
            }
            definition = item;
            count = item.Stackable ? Mathf.Min(amount, item.MaxStack) : 1;
        }
    }
}
