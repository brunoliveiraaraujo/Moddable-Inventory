using UnityEngine;
using Utils;

namespace ModdableInventory
{
    [RequireComponent(typeof(ItemDatabaseLoader))]
    public class ItemDatabaseLogger : MonoBehaviour 
    {
        [Min(0)][SerializeField] private int decimalPlaces = 2;

        private ItemDatabaseLoader database;

        private void Awake() 
        {
            database = GetComponent<ItemDatabaseLoader>();

            database.onLoaded += DebugLogDatabase;
        }

        private void DebugLogDatabase()
        {
            this.database.onLoaded -= DebugLogDatabase;

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