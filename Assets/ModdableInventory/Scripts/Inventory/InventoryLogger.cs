using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModdableInventory.Utils;
using System;

namespace ModdableInventory
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryLogger : MonoBehaviour
    {
        [Min(0)][SerializeField] private int decimalPlaces = 2;
        // TODO: UI: remove this, use a demo scene instead
        [Min(0)][SerializeField] private bool testExampleInventory = true; 

        private Inventory inventory;

        // TODO: UI: remove this, use a demo scene instead
        public event EventHandler InventoryPopulated;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();

            // TODO: UI: remove this, use a demo scene instead
            inventory.InventoryInitialized += InitializeInventoryLogger;
        }

        // TODO: UI: remove this, use a demo scene instead
        private void InitializeInventoryLogger(object sender, EventArgs e)
        {
            inventory.InventoryInitialized -= InitializeInventoryLogger;

            Debug.Log("######## INVENTORY LOG START ########");

            if (testExampleInventory) Test_PopulateInventory();

            LogInventory();
        }

        public void LogInventory()
        {
            var limitedByWeight = inventory.LimitedByWeight;
            var weightCapacity = inventory.WeightCapacity;
            var inventoryWeight = inventory.CurrentWeight;

            Debug.Log($"#### Inventory ####");
            Debug.Log($"Gold: {inventory.Money} ");
            if (limitedByWeight)
            {
                Debug.Log($"[Weight: {StringUtils.FloatToString(inventoryWeight, decimalPlaces)}/{StringUtils.FloatToString(weightCapacity, decimalPlaces)}]");
            }
            for (int i = 0 ; i < inventory.InventoryItems.Count; i++)
            {
                Debug.Log($"[{inventory.InventoryItems[i].CategoryName}]");
                foreach (InventorySlot slot in inventory.InventoryItems[i].ItemSlots)
                {
                    Debug.Log($"x{slot.Amount} {slot.Item.Name}");
                }
            }
        }

        private void Test_PopulateInventory()
        {
            inventory.Money += 500;

            inventory.AddItemToInventory("shortsword", 3);
            inventory.AddItemToInventory("magic sword", 1);
            inventory.AddItemToInventory("leather tunic", 3);
            inventory.AddItemToInventory("bookofknowledge", 1);

            inventory.SortCategoriesByItemName();

            InventoryPopulated?.Invoke(this, EventArgs.Empty);
        }
    } 
}