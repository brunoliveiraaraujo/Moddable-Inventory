using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using Utils;

namespace ModdableInventory
{
    [RequireComponent(typeof(ItemDatabaseLoader))]
    public class Inventory : MonoBehaviour
    {        
        [SerializeField] private bool limitedByWeight = false;
        [Min(0)][SerializeField] private float weightCapacity;

        private ItemDatabaseLoader database;
        private List<ItemCategory> inventory = new List<ItemCategory>();
        private float inventoryWeight = 0;
        private InventorySorter sorter;

        public ReadOnlyCollection<ItemCategory> InventoryItems => inventory.AsReadOnly();
        public bool LimitedByWeight => limitedByWeight;
        public float WeightCapacity => weightCapacity;
        public float InventoryWeight => inventoryWeight;
        public InventorySorter Sorter => sorter;

        public Action onInitialized;
        public Action slotFull;
        public Action weightLimitReached;

        private void Awake() 
        {
            database = GetComponent<ItemDatabaseLoader>();
            sorter = new InventorySorter(inventory);

            database.onLoaded += InitializeInventory;
        }

        private void InitializeInventory()
        {
            database.onLoaded -= InitializeInventory;

            for (int i = 0; i < database.Items.Count; i++)
            {
                inventory.Add(new ItemCategory(database.Items[i].TypeName, database.Items[i].CategoryName, new List<ItemSlot>()));
            }

            onInitialized?.Invoke();
        }

        // adds first Item found in database which <item.Name> that contains <name>
        // (case and spacing are ignored)
        public void AddItemToInventory(string name, int addAmount = 1)
        {
            bool itemFound;
            int categoryID, itemID;
            (itemFound, categoryID, itemID) = SearchItemInDatabase(name);

            if (itemFound) AddItemToInventory(categoryID, itemID, addAmount);
        }

        public void RemoveItemFromInventory(string name, int subAmount = 1)
        {
            bool itemFound;
            int categoryID, itemID;
            (itemFound, categoryID, itemID) = SearchItemInDatabase(name);

            if (itemFound) RemoveItemFromInventory(categoryID, itemID, subAmount);
        }

        public void AddItemToInventory(int categoryID, int itemID, int addAmount = 1)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);
            ItemSlot currSlot = GetLastSlotWithItem(categoryID, itemID);

            if (limitedByWeight && WillReachWeightLimit(item.Weight, addAmount))
            {
                weightLimitReached?.Invoke();
                return;
            }

            int amountLeftToAdd = addAmount;

            while (amountLeftToAdd > 0)
            {
                int previousAmountInSlot = 0;

                if (currSlot == null || currSlot.IsFull())
                {
                    AddNewItemSlot(ref currSlot, categoryID, itemID, amountLeftToAdd);
                }
                else
                {
                    previousAmountInSlot = currSlot.Amount;
                    UpdateItemSlot(currSlot, currSlot.Amount + amountLeftToAdd);
                }
                amountLeftToAdd -= (currSlot.Amount - previousAmountInSlot);

                if (!item.MultiStack) break;
            }            
        }

        public void RemoveItemFromInventory(int categoryID, int itemID, int subAmount = 1)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);
            ItemSlot currSlot = GetLastSlotWithItem(categoryID, itemID);

            int amountLeftToSub = subAmount;

            while (amountLeftToSub > 0 && currSlot != null)
            {
                int previousAmountInSlot = currSlot.Amount;
                int remainingAmountInSlot = Mathf.Max(currSlot.Amount - amountLeftToSub, 0);
                
                if (remainingAmountInSlot > 0)
                {
                    UpdateItemSlot(currSlot, remainingAmountInSlot);
                    amountLeftToSub -= (previousAmountInSlot - remainingAmountInSlot); 
                }
                else
                {
                    RemoveItemSlot(currSlot, categoryID);
                    amountLeftToSub -= previousAmountInSlot;

                    currSlot = GetLastSlotWithItem(categoryID, itemID);
                }

                if (!item.MultiStack) break;
            }
        }

        private bool WillReachWeightLimit(float itemWeight, int addAmount)
        {
            while (inventoryWeight + itemWeight * addAmount > weightCapacity)
            {
                if (addAmount == 1)
                {
                    return true;
                }
                addAmount--;
            }

            return false;
        }

        private void AddNewItemSlot(ref ItemSlot currSlot, int categoryID, int itemID, int itemAmount)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);

            inventory[categoryID].ItemSlots.Add(currSlot = new ItemSlot(item, Mathf.Min(itemAmount, item.StackLimit)));
            inventoryWeight += currSlot.Weight;
        }

        private void UpdateItemSlot(ItemSlot currSlot, int newAmount)
        {
            inventoryWeight -= currSlot.Weight;
            currSlot.Amount = newAmount;
            inventoryWeight += currSlot.Weight;
        }

        private void RemoveItemSlot(ItemSlot currSlot, int categoryID)
        {
            inventoryWeight -= currSlot.Weight;
            inventory[categoryID].ItemSlots.Remove(currSlot);
        }

        // this makes it so that items are added/removed from the last slot first.
        private ItemSlot GetLastSlotWithItem(int categoryID, int itemID)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);

            sorter.SortInventoryByAmount();

            for (int i = inventory[categoryID].ItemSlots.Count - 1; i >= 0; i--)
            {
                ItemSlot currSlot = inventory[categoryID].ItemSlots[i];

                if (currSlot.Item.Name.Equals(item.Name))
                {
                    return currSlot;
                }
            }

            return null;
        }

        private Item GetItemFromDatabase(int categoryID, int itemID)
        {
            return database.Items[categoryID].ItemSlots[itemID].Item;
        }

        private (bool itemFound, int categoryID, int itemID) SearchItemInDatabase (string nameToSearch)
        {
            for (int i = 0; i < database.Items.Count; i++)
            {
                for (int j = 0; j < database.Items[i].ItemSlots.Count; j++)
                {
                    string currItemName = database.Items[i].ItemSlots[j].Item.Name;

                    currItemName = StringOperations.NoSpacesAndLowerCaseString(currItemName);
                    nameToSearch = StringOperations.NoSpacesAndLowerCaseString(nameToSearch);

                    if (currItemName.Contains(nameToSearch))
                    {
                        return (true, i, j);
                    }
                }
            }

            return (false, -1, -1);
        }
    }
}