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
        [Min(0)][SerializeField] private bool TEMP_InventoryMockTest = true; // TODO remove this

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
                Debug.Log($"[Weight: {StringUtils.FloatToString(inventoryWeight, decimalPlaces)}/{StringUtils.FloatToString(weightCapacity, decimalPlaces)}]");
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

        private void Test_PopulateInventory()
        {
            inventory.AddItemToInventory("shortsword", 300);
            inventory.AddItemToInventory("leather tunic", 1);

            inventory.Sorter.SortInventoryByNameAlphabetically();
        }
    } 
}