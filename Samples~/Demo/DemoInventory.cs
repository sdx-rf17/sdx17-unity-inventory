using UnityEngine;

namespace Sdx17.Inventory.Samples
{
    /// <summary>
    /// Minimal demo driver. Attach to a GameObject in the demo scene next to
    /// an InventoryService. On Start it registers the 8 demo items and adds
    /// a few to the inventory, then the number keys exercise the framework:
    /// 1 add a health potion, 2 use a health potion, 3 equip the iron sword,
    /// 4 assign slot 0 to hotbar slot 0, 5 drop one gold coin in front of
    /// the player, 6 clear the inventory. Every action is logged so the
    /// demo is verifiable from the Console alone, with no UI needed.
    /// </summary>
    public class DemoInventory : MonoBehaviour
    {
        [SerializeField] private InventoryService service;
        [SerializeField] private bool addStarterItemsOnStart = true;

        private ItemDefinition healthPotion;
        private ItemDefinition ironSword;
        private ItemDefinition goldCoin;

        private void Awake()
        {
            if (service == null)
                service = InventoryService.Instance;
            if (service == null)
                service = FindObjectOfType<InventoryService>();
        }

        private void Start()
        {
            if (service == null)
            {
                Debug.LogError("[DemoInventory] No InventoryService in the scene. Add one via Sdx17 > Inventory > Add Inventory Service to Scene.");
                return;
            }

            var items = DemoItems.CreateAndRegister(service);
            foreach (var item in items)
            {
                if (item.Id == "health_potion") healthPotion = item;
                if (item.Id == "iron_sword") ironSword = item;
                if (item.Id == "gold_coin") goldCoin = item;
            }

            if (addStarterItemsOnStart)
            {
                service.AddItemById("iron_sword", 1);
                service.AddItemById("health_potion", 5);
                service.AddItemById("bread", 3);
                service.AddItemById("iron_ore", 10);
                service.AddItemById("gold_coin", 50);
            }

            service.OnChanged += LogInventory;
            service.OnItemEquipped += item => Debug.Log($"[DemoInventory] Equipped: {item.DisplayName}");
            LogInventory();
            Debug.Log("[DemoInventory] Keys: 1 add potion, 2 use potion, 3 equip sword, 4 hotbar slot 0, 5 drop coin, 6 clear.");
        }

        private void Update()
        {
            if (service == null)
                return;
            if (Input.GetKeyDown(KeyCode.Alpha1))
                service.AddItem(healthPotion, 1);
            if (Input.GetKeyDown(KeyCode.Alpha2) && healthPotion != null)
                Debug.Log($"[DemoInventory] Used potion, healed {service.UseItem(healthPotion)}.");
            if (Input.GetKeyDown(KeyCode.Alpha3) && ironSword != null)
                Debug.Log($"[DemoInventory] Equip sword: {service.EquipItem(ironSword)}");
            if (Input.GetKeyDown(KeyCode.Alpha4))
                Debug.Log($"[DemoInventory] Assign slot 0 to hotbar 0: {service.AssignToHotbar(0, 0)}");
            if (Input.GetKeyDown(KeyCode.Alpha5) && goldCoin != null)
                service.DropItem(goldCoin, 1, transform.position + transform.forward * 2f);
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                service.PlayerInventory.Clear();
                Debug.Log("[DemoInventory] Inventory cleared.");
            }
        }

        private void LogInventory()
        {
            var inventory = service.PlayerInventory;
            Debug.Log($"[DemoInventory] Slots used {inventory.UsedSlots}/24, weight {inventory.CurrentWeight:F1}/{inventory.MaxWeight:F1}, " +
                      $"potions {service.PlayerInventory.Count(healthPotion)}, equipped {(inventory.EquippedItem != null ? inventory.EquippedItem.DisplayName : "none")}.");
        }
    }
}
