using UnityEngine;

namespace Sdx17.Inventory
{
    /// <summary>
    /// Category of an item. Drives which actions are valid: only Weapons can
    /// be equipped, only Consumables can be used, Currency is never equipped
    /// or used, Materials are crafting inputs.
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Consumable,
        Material,
        Currency
    }

    /// <summary>
    /// Rarity is data only in v0.1.0. It does not change stats; the UI system
    /// reads it to pick a frame colour or label.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Data definition for one kind of item. Create assets via
    /// Assets &gt; Create &gt; Sdx17 &gt; Inventory &gt; Item Definition,
    /// or Sdx17 &gt; Inventory &gt; Create Demo Items in the editor.
    /// Instances are shared data: never mutate an ItemDefinition at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Sdx17/Inventory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string id = "item";
        [SerializeField] private string displayName = "New Item";
        [SerializeField] private ItemType type = ItemType.Material;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField] private bool stackable = true;
        [SerializeField] private int maxStack = 99;
        [SerializeField] private float weight = 0.1f;
        [SerializeField] private int healAmount;
        [SerializeField] private int damage;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;
        [SerializeField, TextArea] private string description = string.Empty;

        public string Id => id;
        public string DisplayName => displayName;
        public ItemType Type => type;
        public ItemRarity Rarity => rarity;
        public bool Stackable => stackable;
        public int MaxStack => maxStack;
        public float Weight => weight;
        public int HealAmount => healAmount;
        public int Damage => damage;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public string Description => description;

        /// <summary>Weapons are the only equippable type.</summary>
        public bool IsEquippable => type == ItemType.Weapon;

        /// <summary>Consumables are the only usable type.</summary>
        public bool IsUsable => type == ItemType.Consumable;

        private void OnValidate()
        {
            // Keep data sane even when edited quickly in the inspector.
            if (string.IsNullOrWhiteSpace(id))
                id = "item";
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = "New Item";
            if (maxStack < 1)
                maxStack = 1;
            if (!stackable)
                maxStack = 1;
            if (weight < 0f)
                weight = 0f;
            if (healAmount < 0)
                healAmount = 0;
            if (damage < 0)
                damage = 0;
        }

        /// <summary>
        /// Runtime factory used by DemoItems. Editor-created .asset files are
        /// the normal path; this exists so the demo runs without any assets.
        /// </summary>
        public static ItemDefinition CreateRuntime(
            string itemId, string itemName, ItemType itemType, ItemRarity itemRarity,
            bool isStackable, int maxStackSize, float itemWeight,
            int heal, int damageValue, string itemDescription)
        {
            var definition = CreateInstance<ItemDefinition>();
            definition.id = itemId;
            definition.displayName = itemName;
            definition.type = itemType;
            definition.rarity = itemRarity;
            definition.stackable = isStackable;
            definition.maxStack = Mathf.Max(1, maxStackSize);
            if (!isStackable)
                definition.maxStack = 1;
            definition.weight = Mathf.Max(0f, itemWeight);
            definition.healAmount = Mathf.Max(0, heal);
            definition.damage = Mathf.Max(0, damageValue);
            definition.description = itemDescription ?? string.Empty;
            definition.name = itemId;
            return definition;
        }
    }
}
