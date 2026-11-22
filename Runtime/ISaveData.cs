using System;
using UnityEngine;

namespace Sdx17.Inventory
{
    /// <summary>
    /// Save/load contract. The Inventory Service hands its state to any
    /// object implementing this interface — in practice the Sdx17 Secure
    /// Save system — and restores from it. The inventory never touches
    /// PlayerPrefs, files, or encryption itself.
    /// </summary>
    public interface ISaveData
    {
        void SaveInventory(InventorySaveData data);
        InventorySaveData LoadInventory();
        bool HasInventorySave();
    }

    /// <summary>
    /// Plain data snapshot of an inventory. Item references are stored by
    /// id string, not by Unity object reference, so the save survives asset
    /// moves. The loader resolves ids through a lookup the caller supplies.
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        [SerializeField] private InventorySaveEntry[] slots = Array.Empty<InventorySaveEntry>();
        [SerializeField] private InventorySaveEntry[] hotbar = Array.Empty<InventorySaveEntry>();
        [SerializeField] private string equippedItemId = string.Empty;

        public InventorySaveEntry[] Slots { get => slots; set => slots = value ?? Array.Empty<InventorySaveEntry>(); }
        public InventorySaveEntry[] Hotbar { get => hotbar; set => hotbar = value ?? Array.Empty<InventorySaveEntry>(); }
        public string EquippedItemId { get => equippedItemId; set => equippedItemId = value ?? string.Empty; }
    }

    [Serializable]
    public class InventorySaveEntry
    {
        [SerializeField] private string itemId = string.Empty;
        [SerializeField] private int count;

        public string ItemId { get => itemId; set => itemId = value ?? string.Empty; }
        public int Count { get => count; set => count = Mathf.Max(0, value); }

        public InventorySaveEntry() { }

        public InventorySaveEntry(string id, int amount)
        {
            itemId = id ?? string.Empty;
            count = Mathf.Max(0, amount);
        }
    }
}
