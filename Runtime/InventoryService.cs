using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sdx17.Inventory
{
    /// <summary>
    /// Runtime owner of the player's inventory. One per scene: the first
    /// instance becomes the singleton and survives scene loads; extras
    /// destroy themselves. Gameplay code talks to InventoryService, never
    /// to a raw Inventory, so events fire for every change.
    /// Save/load goes through ISaveData — wire the Sdx17 Secure Save
    /// implementation in via SetSaveProvider, or leave it unset and call
    /// ToSaveData / LoadFromSaveData yourself.
    /// </summary>
    public class InventoryService : MonoBehaviour
    {
        public static InventoryService Instance { get; private set; }

        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private List<ItemDefinition> itemCatalog = new List<ItemDefinition>();

        // C# events for gameplay code; UnityEvents for inspector wiring.
        public event Action OnChanged;
        public event Action<ItemDefinition, int> OnItemAdded;
        public event Action<ItemDefinition, int> OnItemRemoved;
        public event Action<ItemDefinition> OnItemEquipped;
        public event Action<ItemDefinition, int> OnItemUsed;

        [SerializeField] private UnityEvent onChangedEvent = new UnityEvent();
        [SerializeField] private UnityEvent<ItemDefinition> onItemEquippedEvent = new UnityEvent<ItemDefinition>();

        public UnityEvent OnChangedEvent => onChangedEvent;
        public UnityEvent<ItemDefinition> OnItemEquippedEvent => onItemEquippedEvent;

        public Inventory PlayerInventory { get; private set; }
        public IReadOnlyList<ItemDefinition> ItemCatalog => itemCatalog;

        private ISaveData saveProvider;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PlayerInventory = new Inventory(maxWeight);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetSaveProvider(ISaveData provider)
        {
            saveProvider = provider;
        }

        /// <summary>
        /// Registers a definition so save/load can resolve it by id.
        /// Duplicate ids are refused: the first registration wins.
        /// </summary>
        public bool RegisterItem(ItemDefinition item)
        {
            if (item == null)
                return false;
            foreach (var existing in itemCatalog)
            {
                if (existing != null && existing.Id == item.Id)
                    return false;
            }
            itemCatalog.Add(item);
            return true;
        }

        public void RegisterItems(IEnumerable<ItemDefinition> items)
        {
            if (items == null)
                return;
            foreach (var item in items)
                RegisterItem(item);
        }

        public ItemDefinition FindItem(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            foreach (var item in itemCatalog)
            {
                if (item != null && item.Id == id)
                    return item;
            }
            return null;
        }

        public bool AddItem(ItemDefinition item, int amount = 1)
        {
            if (PlayerInventory == null)
                return false;
            bool added = PlayerInventory.Add(item, amount);
            if (added)
            {
                OnItemAdded?.Invoke(item, amount);
                NotifyChanged();
            }
            return added;
        }

        public bool AddItemById(string id, int amount = 1)
        {
            var item = FindItem(id);
            return item != null && AddItem(item, amount);
        }

        public int RemoveItem(ItemDefinition item, int amount = 1)
        {
            if (PlayerInventory == null || item == null)
                return 0;
            int removed = PlayerInventory.Remove(item, amount);
            if (removed > 0)
            {
                OnItemRemoved?.Invoke(item, removed);
                NotifyChanged();
            }
            return removed;
        }

        public bool EquipItem(ItemDefinition item)
        {
            if (PlayerInventory == null)
                return false;
            bool equipped = PlayerInventory.Equip(item);
            if (equipped)
            {
                OnItemEquipped?.Invoke(item);
                onItemEquippedEvent?.Invoke(item);
                NotifyChanged();
            }
            return equipped;
        }

        public void UnequipItem()
        {
            if (PlayerInventory == null)
                return;
            PlayerInventory.Unequip();
            NotifyChanged();
        }

        /// <summary>
        /// Uses one consumable. Returns the heal amount; the caller (health
        /// system) applies it. Non-consumables return 0 and consume nothing.
        /// </summary>
        public int UseItem(ItemDefinition item)
        {
            if (PlayerInventory == null || item == null || !item.IsUsable)
                return 0;
            int countBefore = PlayerInventory.Count(item);
            int healed = PlayerInventory.Use(item);
            if (PlayerInventory.Count(item) < countBefore)
            {
                OnItemUsed?.Invoke(item, healed);
                NotifyChanged();
            }
            return healed;
        }

        public bool DropItem(ItemDefinition item, int amount, Vector3 position)
        {
            if (PlayerInventory == null)
                return false;
            bool dropped = PlayerInventory.Drop(item, amount, position);
            if (dropped)
                NotifyChanged();
            return dropped;
        }

        public bool AssignToHotbar(int slotIndex, int hotbarIndex)
        {
            if (PlayerInventory == null)
                return false;
            bool assigned = PlayerInventory.AssignToHotbar(slotIndex, hotbarIndex);
            if (assigned)
                NotifyChanged();
            return assigned;
        }

        /// <summary>Saves through the wired provider. False when none is set.</summary>
        public bool Save()
        {
            if (saveProvider == null || PlayerInventory == null)
                return false;
            saveProvider.SaveInventory(PlayerInventory.ToSaveData());
            return true;
        }

        /// <summary>Loads through the wired provider. False when none is set or no save exists.</summary>
        public bool Load()
        {
            if (saveProvider == null || PlayerInventory == null)
                return false;
            if (!saveProvider.HasInventorySave())
                return false;
            var data = saveProvider.LoadInventory();
            if (data == null)
                return false;
            PlayerInventory.FromSaveData(data, FindItem);
            NotifyChanged();
            return true;
        }

        public InventorySaveData ToSaveData()
        {
            return PlayerInventory?.ToSaveData();
        }

        public void LoadFromSaveData(InventorySaveData data)
        {
            if (PlayerInventory == null || data == null)
                return;
            PlayerInventory.FromSaveData(data, FindItem);
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            OnChanged?.Invoke();
            onChangedEvent?.Invoke();
        }
    }
}
