using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace ModdableInventory
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryLogger : MonoBehaviour
    {
        [Min(0)][SerializeField] private int decimalPlaces = 2;
        [Min(0)][SerializeField] private bool TEMP_InventoryMockTest = true; // TODO remove this, use proper mock testing

        private Inventory inventory;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();

            inventory.onInitialized += DebugLogInventory;
        }

        private void DebugLogInventory()
        {
            inventory.onInitialized -= DebugLogInventory;

            if (TEMP_InventoryMockTest) Test_PopulateInventory();

            var limitedByWeight = inventory.LimitedByWeight;
            var weightCapacity = inventory.WeightCapacity;
            var inventoryWeight = inventory.InventoryWeight;

            Debug.Log($"########## INVENTORY ##########");
            if (limitedByWeight)
            {
                Debug.Log($"[Weight: {StringOperations.FloatToString(inventoryWeight, decimalPlaces)}/{StringOperations.FloatToString(weightCapacity, decimalPlaces)}]");
            }
            for (int i = 0 ; i < inventory.InventoryItems.Count; i++)
            {
                Debug.Log($"=== {inventory.InventoryItems[i].CategoryName} ===");
                foreach (ItemSlot slot in inventory.InventoryItems[i].ItemSlots)
                {
                    Debug.Log($"x{slot.Amount} {slot.Item.Name}");
                }
            }
        }

        // TODO remove this, use proper mock testing
        private void Test_PopulateInventory()
        {
            inventory.AddItemToInventory("shortsword", 300);
            inventory.AddItemToInventory("magic sword", 300);
            inventory.AddItemToInventory("LeatherTunic", 300);
            inventory.AddItemToInventory("Book of Knowledge", 300);
            inventory.AddItemToInventory("triAnglEP", 300);

            inventory.RemoveItemFromInventory("shortS", 220);
            inventory.RemoveItemFromInventory("tunic", 500);
            inventory.RemoveItemFromInventory("book", 500);

            // inventory.AddItemToInventory(0, 0, 300);
            // inventory.AddItemToInventory(0, 1, 300);
            // inventory.AddItemToInventory(1, 0, 300);
            // inventory.AddItemToInventory(2, 0, 300);
            // inventory.AddItemToInventory(2, 1, 300);

            // inventory.RemoveItemFromInventory(0, 0, 220);
            // inventory.RemoveItemFromInventory(1, 0, 500);
            // inventory.RemoveItemFromInventory(2, 0, 500);

            inventory.Sorter.SortInventoryByNameAlphabetically();
        }
    } 
}