using UnityEngine;
using ModdableInventory.Utils;

namespace ModdableInventory
{
    [RequireComponent(typeof(ItemDatabase))]
    public class ItemDatabaseLogger : MonoBehaviour 
    {
        [Min(0)][SerializeField] private int decimalPlaces = 2;

        private ItemDatabase database;

        private void Awake() 
        {
            database = GetComponent<ItemDatabase>();

            database.onDatabaseInitialized += DebugLogDatabase;
        }

        private void DebugLogDatabase()
        {
            this.database.onDatabaseInitialized -= DebugLogDatabase;

            var items = this.database.Items;

            Debug.Log($"########## ITEM DATABASE ##########");
            foreach (var category in items)
            {
                Debug.Log($"=== {category.CategoryName} ===");
                for (int i = 0; i < category.ItemSlots.Count; i++)
                {
                    category.ItemSlots[i].Item.LogItem(decimalPlaces);
                }
            }
        }
    }
}