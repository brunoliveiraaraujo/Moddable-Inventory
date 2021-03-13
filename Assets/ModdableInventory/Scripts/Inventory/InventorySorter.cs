using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ModdableInventory
{
    public class InventorySorter
    {
        List<ItemCategory> inventoryItems;

        public InventorySorter(List<ItemCategory> inventoryItems)
        {
            this.inventoryItems = inventoryItems;
        }

        public void SortInventoryByNameAlphabetically()
        {
            SortInventoryByAmount();
            foreach (var category in inventoryItems)
            {
                category.ItemSlots = category.ItemSlots
                    .OrderBy(slot => slot.Item.Name)
                    .ToList();
            }
        }

        public void SortInventoryByCostDescending()
        {
            SortInventoryByAmount();
            foreach (var category in inventoryItems)
            {
                category.ItemSlots = category.ItemSlots
                    .OrderByDescending(slot => slot.Item.Cost)
                    .ToList();
            }     
        }

        public void SortInventoryByAmount()
        {
            foreach (var category in inventoryItems)
            {
                category.ItemSlots = category.ItemSlots
                    .OrderByDescending(slot => slot.Amount)
                    .ToList();
            }
        }
    }
}