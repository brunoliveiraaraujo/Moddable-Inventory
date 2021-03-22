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
        [Min(0)][SerializeField] private bool testExampleInventory = true; // TODO remove this

        private Inventory inventory;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();

            inventory.onInventoryInitialized += InitializeInventoryLogger;
        }

        private void InitializeInventoryLogger()
        {
            inventory.onInventoryInitialized -= InitializeInventoryLogger;

            if (testExampleInventory) Test_PopulateInventory();
            if (testExampleInventory) Test_EquipItems();
        }

        private void LogInventory()
        {
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
                foreach (InventorySlot slot in inventory.InventoryItems[i].ItemSlots)
                {
                    Debug.Log($"x{slot.Amount} {slot.Item.Name}");
                }
            }
        }

        private void LogEquippedItems()
        {
            Debug.Log($"########## EQUIPMENT ##########");
            foreach (var slot in inventory.EquippedItems)
            {
                Debug.Log($"=== {slot.SlotName} ({slot.TypeName}) ===");
                if (slot.Item != null)
                {
                    Debug.Log($"    {slot.Item.Name}");
                }
                else
                {
                    Debug.Log($"    <empty>");
                }
            }
        }

        private void Test_PopulateInventory()
        {
            inventory.AddItemToInventory("shortsword", 3);
            inventory.AddItemToInventory("magic sword", 1);
            inventory.AddItemToInventory("leather tunic", 3);
            inventory.AddItemToInventory("bookofknowledge", 1);

            inventory.Sorter.SortInventoryByNameAlphabetically();
        }

        private void Test_EquipItems()
        {
            inventory.EquipItem("shortsword");

            LogEquippedItems();
            LogInventory();

            inventory.UnequipItem("shortsword");

            LogEquippedItems();
            LogInventory();
        }
    } 
}