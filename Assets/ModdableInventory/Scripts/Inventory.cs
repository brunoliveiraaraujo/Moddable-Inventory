﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using ModdableInventory.Utils;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Globalization;

namespace ModdableInventory
{
    [RequireComponent(typeof(ItemDatabase))]
    public class Inventory : MonoBehaviour
    {       
        private const string INVENTORY_YAML_PATH = "gamedata/config/inventory.yaml";

        private bool limitedByWeight = false;
        private float weightCapacity = 0;
        private float currentWeight = 0;
        private int money = 0;
    
        private ItemDatabase database;
        private List<ItemCategory> inventoryItems = new List<ItemCategory>();

        public bool LimitedByWeight => limitedByWeight;
        public float WeightCapacity => weightCapacity;
        public float CurrentWeight 
        { 
            get { return Mathf.Max(currentWeight, 0); } 
            set { currentWeight = value; } 
        }
        public int Money 
        { 
            get { return Mathf.Max(money, 0); } 
            set { money = value; } 
        }

        public ReadOnlyCollection<ItemCategory> InventoryItems => inventoryItems.AsReadOnly();

        public event EventHandler InventoryInitialized;

        private void Awake() 
        {
            database = GetComponent<ItemDatabase>();

            database.DatabaseInitialized += OnDatabaseInitialized;
        }

        private void OnDatabaseInitialized(object sender, EventArgs e)
        {
            database.DatabaseInitialized -= OnDatabaseInitialized;
            
            InitializeInventory();
        }

        private void InitializeInventory()
        {
            string internalInventoryYAML = Resources.Load<TextAsset>(Path.ChangeExtension(INVENTORY_YAML_PATH, null)).text;

            ParseInventory(IOUtils.ReadOrMakeYAMLFile(internalInventoryYAML, INVENTORY_YAML_PATH));

            for (int i = 0; i < database.ItemCategories.Count; i++)
            {
                inventoryItems.Add(new ItemCategory(
                    database.ItemCategories[i].CategoryName,
                    database.ItemCategories[i].ShowCategoryTab, 
                    database.ItemCategories[i].ItemType));
            }

            InventoryInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void ParseInventory(StringReader input)
        {
            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var topLevelNode in root.Children)
            {
                string keyName = topLevelNode.Key.ToString();
                string valueName = topLevelNode.Value.ToString();

                if (keyName.Equals("limitedByWeight"))
                {
                    limitedByWeight = bool.Parse(valueName.ToString());
                }
                else if (keyName.Equals("weightCapacity"))
                {
                    weightCapacity = float.Parse(valueName.ToString(), CultureInfo.InvariantCulture);
                }
            }
        }

        /// <remarks>
        /// (case and spacing are ignored)
        /// </remarks>
        public void AddItemToInventoryByName(string itemName, int addAmount = 1)
        {
            bool itemFound;
            int categoryID, itemID;
            (itemFound, categoryID, itemID) = SearchItemInDatabaseByName(itemName);

            if (itemFound) AddItemToInventoryByID(categoryID, itemID, addAmount);
            else throw new KeyNotFoundException($"Item {itemName} not found!");
        }

        public void AddItemToInventoryByID(string itemStringID, int addAmount = 1)
        {
            bool itemFound;
            int categoryID, itemID;
            (itemFound, categoryID, itemID) = SearchItemInDatabaseByID(itemStringID);

            if (itemFound) AddItemToInventoryByID(categoryID, itemID, addAmount);
            else throw new KeyNotFoundException($"Item {itemStringID} not found!");    
        }

        public void AddItemToInventoryByID(int categoryID, int itemID, int addAmount = 1)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);
            ItemSlot currSlot = GetLastSlotWithItem(categoryID, itemID);

            if (limitedByWeight && ReachedWeightLimit(item.Weight, ref addAmount))
            {
                if (addAmount == 0) return;
            }

            int amountLeftToAdd = addAmount;

            while (amountLeftToAdd > 0)
            {
                int previousAmountInSlot = 0;

                if (currSlot == null || currSlot.IsFull)
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

        public void RemoveItemFromInventoryByName(string ItemName, int subAmount = 1)
        {
            bool itemFound;
            int categoryID, itemID;
            (itemFound, categoryID, itemID) = SearchItemInDatabaseByName(ItemName);

            if (itemFound) RemoveItemFromInventoryByID(categoryID, itemID, subAmount);
            else throw new KeyNotFoundException($"Item {ItemName} not found!");
        }

        public void RemoveItemFromInventoryByID(string itemStringID, int subAmount = 1)
        {
            bool itemFound;
            int categoryID, itemID;
            (itemFound, categoryID, itemID) = SearchItemInDatabaseByID(itemStringID);

            if (itemFound) RemoveItemFromInventoryByID(categoryID, itemID, subAmount);
            else throw new KeyNotFoundException($"Item {itemStringID} not found!");
        }

        public void RemoveItemFromInventoryByID(int categoryID, int itemID, int subAmount = 1)
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

        private bool ReachedWeightLimit(float itemWeight, ref int addAmount)
        {
            bool result = false;

            while (currentWeight + itemWeight * addAmount > weightCapacity)
            {
                result = true;

                addAmount--;

                if (addAmount == 0)
                {
                    return result;
                }
            }

            return result;
        }

        private void AddNewItemSlot(ref ItemSlot currSlot, int categoryID, int itemID, int itemAmount)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);

            inventoryItems[categoryID].AddItemSlot(currSlot = new ItemSlot(item, Mathf.Min(itemAmount, item.StackLimit)));
            currentWeight += currSlot.Weight;
        }

        private void UpdateItemSlot(ItemSlot currSlot, int newAmount)
        {
            currentWeight -= currSlot.Weight;
            currSlot.Amount = newAmount;
            currentWeight += currSlot.Weight;
        }

        private void RemoveItemSlot(ItemSlot currSlot, int categoryID)
        {
            currentWeight -= currSlot.Weight;
            inventoryItems[categoryID].RemoveItemSlot(currSlot);
        }

        /// <remarks>
        /// (this makes it so that items are added/removed from the last slot first)
        /// </remarks>
        private ItemSlot GetLastSlotWithItem(int categoryID, int itemID)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);

            for (int i = inventoryItems[categoryID].ItemSlots.Count - 1; i >= 0; i--)
            {
                ItemSlot currSlot = inventoryItems[categoryID].ItemSlots[i];

                if (currSlot.Item.Name.Equals(item.Name))
                {
                    return currSlot;
                }
            }

            return null;
        }

        private Item GetItemFromDatabase(int categoryID, int itemID)
        {
            return database.ItemCategories[categoryID].ItemSlots[itemID].Item;
        }

        private (bool itemFound, int categoryID, int itemID) SearchItemInDatabaseByName (string nameToSearch)
        {
            for (int i = 0; i < database.ItemCategories.Count; i++)
            {
                for (int j = 0; j < database.ItemCategories[i].ItemSlots.Count; j++)
                {
                    string currItemName = database.ItemCategories[i].ItemSlots[j].Item.Name;

                    if (StringUtils.StringContainsName(currItemName, nameToSearch))
                    {
                        return (true, i, j);
                    }
                }
            }

            return (false, -1, -1);
        }

        private (bool itemFound, int categoryID, int itemID) SearchItemInDatabaseByID (string itemStringID)
        {
            for (int i = 0; i < database.ItemCategories.Count; i++)
            {
                for (int j = 0; j < database.ItemCategories[i].ItemSlots.Count; j++)
                {
                    string currItemID = database.ItemCategories[i].ItemSlots[j].Item.ItemStringID;

                    if (StringUtils.StringContainsName(currItemID, itemStringID))
                    {
                        return (true, i, j);
                    }
                }
            }

            return (false, -1, -1);
        }
    }
}