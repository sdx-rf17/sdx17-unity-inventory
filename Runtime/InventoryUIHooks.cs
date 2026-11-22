using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sdx17.Inventory
{
    /// <summary>
    /// One row of data for a UI slot (grid cell or hotbar cell). The UI
    /// system reads snapshots of these and renders them; the inventory
    /// never touches UnityEngine.UI itself, so any UI approach works.
    /// </summary>
    [Serializable]
    public struct SlotViewData
    {
        public int Index;
        public bool IsEmpty;
        public string ItemId;
        public string DisplayName;
        public string Rarity;
        public string Type;
        public int Count;
        public float Weight;
        public Sprite Icon;
        public bool IsEquipped;
        public bool IsUsable;
        public bool IsEquippable;
    }

    /// <summary>
    /// Integration point for the Sdx17 UI system (or any UI). Attach to a
    /// GameObject, assign the service, and subscribe to the events — or
    /// poll GetGridSnapshot / GetHotbarSnapshot when the service raises
    /// OnChanged. This class builds no GameObjects: it only translates
    /// inventory state into data the UI renders, and forwards UI input
    /// (clicks, hotbar keys) back to the service.
    /// </summary>
    public class InventoryUIHooks : MonoBehaviour
    {
        [SerializeField] private InventoryService service;

        /// <summary>Raised with a fresh grid snapshot after every change.</summary>
        public event Action<IReadOnlyList<SlotViewData>> OnGridSnapshot;
        /// <summary>Raised with a fresh hotbar snapshot after every change.</summary>
        public event Action<IReadOnlyList<SlotViewData>> OnHotbarSnapshot;
        /// <summary>Raised when the equipped item changes (null = nothing).</summary>
        public event Action<ItemDefinition> OnEquippedChanged;

        private ItemDefinition lastEquipped;

        private void Awake()
        {
            if (service == null)
                service = InventoryService.Instance;
        }

        private void OnEnable()
        {
            if (service != null)
                service.OnChanged += PublishSnapshots;
            PublishSnapshots();
        }

        private void OnDisable()
        {
            if (service != null)
                service.OnChanged -= PublishSnapshots;
        }

        public IReadOnlyList<SlotViewData> GetGridSnapshot()
        {
            return BuildSnapshot(service?.PlayerInventory?.Slots);
        }

        public IReadOnlyList<SlotViewData> GetHotbarSnapshot()
        {
            return BuildSnapshot(service?.PlayerInventory?.Hotbar);
        }

        /// <summary>UI calls this when the player clicks a slot.</summary>
        public void OnSlotClicked(int index, bool isHotbar)
        {
            if (service == null || service.PlayerInventory == null)
                return;
            var slots = isHotbar ? service.PlayerInventory.Hotbar : service.PlayerInventory.Slots;
            if (index < 0 || index >= slots.Length)
                return;
            var slot = slots[index];
            if (slot == null || slot.IsEmpty)
                return;
            // Clicking a weapon equips it; clicking a consumable uses it.
            if (slot.Definition.IsEquippable)
                service.EquipItem(slot.Definition);
            else if (slot.Definition.IsUsable)
                service.UseItem(slot.Definition);
        }

        /// <summary>UI calls this when the player presses a hotbar key (0-4).</summary>
        public void OnHotbarKeyPressed(int hotbarIndex)
        {
            OnSlotClicked(hotbarIndex, true);
        }

        private void PublishSnapshots()
        {
            OnGridSnapshot?.Invoke(GetGridSnapshot());
            OnHotbarSnapshot?.Invoke(GetHotbarSnapshot());
            var equipped = service?.PlayerInventory?.EquippedItem;
            if (equipped != lastEquipped)
            {
                lastEquipped = equipped;
                OnEquippedChanged?.Invoke(equipped);
            }
        }

        private List<SlotViewData> BuildSnapshot(InventorySlot[] slots)
        {
            var result = new List<SlotViewData>();
            if (slots == null || service == null || service.PlayerInventory == null)
                return result;
            var equipped = service.PlayerInventory.EquippedItem;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                var view = new SlotViewData { Index = i, IsEmpty = true };
                if (slot != null && !slot.IsEmpty)
                {
                    var def = slot.Definition;
                    view.IsEmpty = false;
                    view.ItemId = def.Id;
                    view.DisplayName = def.DisplayName;
                    view.Rarity = def.Rarity.ToString();
                    view.Type = def.Type.ToString();
                    view.Count = slot.Count;
                    view.Weight = def.Weight * slot.Count;
                    view.Icon = def.Icon;
                    view.IsEquipped = equipped == def;
                    view.IsUsable = def.IsUsable;
                    view.IsEquippable = def.IsEquippable;
                }
                result.Add(view);
            }
            return result;
        }
    }
}
