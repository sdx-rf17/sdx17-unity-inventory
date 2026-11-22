using UnityEditor;
using UnityEngine;

namespace Sdx17.Inventory.Editor
{
    /// <summary>
    /// Menu shortcuts for the inventory framework. The window and the demo
    /// item creation live in InventoryEditorWindow; this file only adds
    /// the small one-click helpers.
    /// </summary>
    public static class InventoryMenuItems
    {
        [MenuItem("Sdx17/Inventory/Create Item Definition", false, 1)]
        public static void CreateItemDefinition()
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            string path = "Assets/New Item.asset";
            string unique = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(item, unique);
            AssetDatabase.SaveAssets();
            Selection.activeObject = item;
            EditorUtility.FocusProjectWindow();
        }

        [MenuItem("Sdx17/Inventory/Add Inventory Service to Scene", false, 2)]
        public static void AddInventoryServiceToScene()
        {
            var existing = Object.FindObjectOfType<InventoryService>();
            if (existing != null)
            {
                Selection.activeObject = existing.gameObject;
                Debug.Log("[Sdx17 Inventory] An InventoryService already exists in this scene.");
                return;
            }
            var go = new GameObject("InventoryService");
            go.AddComponent<InventoryService>();
            Undo.RegisterCreatedObjectUndo(go, "Add Inventory Service");
            Selection.activeObject = go;
        }
    }
}
