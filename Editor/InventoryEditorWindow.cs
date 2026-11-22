using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sdx17.Inventory.Editor
{
    /// <summary>
    /// Editor window that creates the 8 demo ItemDefinition .asset files
    /// under Assets/Sdx17/Inventory/DemoItems, and menu items under
    /// Sdx17 &gt; Inventory. Existing assets with the same name are left
    /// untouched so re-running the window never overwrites hand edits.
    /// </summary>
    public class InventoryEditorWindow : EditorWindow
    {
        private const string DemoFolder = "Assets/Sdx17/Inventory/DemoItems";

        [MenuItem("Sdx17/Inventory/Demo Items Window")]
        public static void Open()
        {
            var window = GetWindow<InventoryEditorWindow>("Sdx17 Inventory");
            window.minSize = new Vector2(320, 220);
            window.Show();
        }

        [MenuItem("Sdx17/Inventory/Create Demo Items")]
        public static void CreateDemoItems()
        {
            EnsureFolder();
            int created = 0;
            int skipped = 0;
            foreach (var item in DemoItems.CreateAll())
            {
                string path = Path.Combine(DemoFolder, item.Id + ".asset").Replace("\\", "/");
                if (AssetDatabase.LoadAssetAtPath<ItemDefinition>(path) != null)
                {
                    skipped++;
                    Object.DestroyImmediate(item);
                    continue;
                }
                AssetDatabase.CreateAsset(item, path);
                created++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Sdx17 Inventory] Demo items: {created} created, {skipped} already existed in {DemoFolder}.");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Sdx17"))
                AssetDatabase.CreateFolder("Assets", "Sdx17");
            if (!AssetDatabase.IsValidFolder("Assets/Sdx17/Inventory"))
                AssetDatabase.CreateFolder("Assets/Sdx17", "Inventory");
            if (!AssetDatabase.IsValidFolder(DemoFolder))
                AssetDatabase.CreateFolder("Assets/Sdx17/Inventory", "DemoItems");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sdx17 Inventory Framework", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("v0.1.0 — UNTESTED IN UNITY", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Creates the 8 demo ItemDefinition assets (iron sword, hunter bow, " +
                "health potion, bread, iron ore, oak wood, magic crystal, gold coin) " +
                "under " + DemoFolder + ". Existing assets are not overwritten.",
                MessageType.Info);
            EditorGUILayout.Space();
            if (GUILayout.Button("Create Demo Items", GUILayout.Height(32)))
                CreateDemoItems();
            EditorGUILayout.Space();
            if (GUILayout.Button("Create Single Item Definition"))
            {
                EnsureFolder();
                var item = CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, DemoFolder + "/New Item.asset");
                AssetDatabase.SaveAssets();
                Selection.activeObject = item;
            }
        }
    }
}
