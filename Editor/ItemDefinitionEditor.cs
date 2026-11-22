using UnityEditor;
using UnityEngine;

namespace Sdx17.Inventory.Editor
{
    /// <summary>
    /// Custom inspector for ItemDefinition. Shows the fields in sections,
    /// hides Heal for non-consumables and Damage for non-weapons, and warns
    /// when the id is empty or the max stack is inconsistent with stacking.
    /// </summary>
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty id;
        private SerializedProperty displayName;
        private SerializedProperty type;
        private SerializedProperty rarity;
        private SerializedProperty stackable;
        private SerializedProperty maxStack;
        private SerializedProperty weight;
        private SerializedProperty healAmount;
        private SerializedProperty damage;
        private SerializedProperty icon;
        private SerializedProperty prefab;
        private SerializedProperty description;

        private void OnEnable()
        {
            id = serializedObject.FindProperty("id");
            displayName = serializedObject.FindProperty("displayName");
            type = serializedObject.FindProperty("type");
            rarity = serializedObject.FindProperty("rarity");
            stackable = serializedObject.FindProperty("stackable");
            maxStack = serializedObject.FindProperty("maxStack");
            weight = serializedObject.FindProperty("weight");
            healAmount = serializedObject.FindProperty("healAmount");
            damage = serializedObject.FindProperty("damage");
            icon = serializedObject.FindProperty("icon");
            prefab = serializedObject.FindProperty("prefab");
            description = serializedObject.FindProperty("description");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(displayName);
            EditorGUILayout.PropertyField(description);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Classification", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(rarity);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stacking & Weight", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(stackable);
            EditorGUILayout.PropertyField(maxStack);
            EditorGUILayout.PropertyField(weight);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
            var itemType = (ItemType)type.enumValueIndex;
            if (itemType == ItemType.Consumable)
                EditorGUILayout.PropertyField(healAmount);
            if (itemType == ItemType.Weapon)
                EditorGUILayout.PropertyField(damage);
            if (itemType == ItemType.Consumable || itemType == ItemType.Weapon)
                EditorGUILayout.HelpBox("Heal applies to Consumables, Damage to Weapons.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Presentation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(icon);
            EditorGUILayout.PropertyField(prefab);

            if (string.IsNullOrWhiteSpace(id.stringValue))
                EditorGUILayout.HelpBox("Id is empty. Give the item a unique id (lowercase, underscores) — save/load resolves items by id.", MessageType.Warning);
            if (!stackable.boolValue && maxStack.intValue != 1)
                EditorGUILayout.HelpBox("Non-stackable items always use a max stack of 1.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
