using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using Utils;
using System.IO;
using System.Text;
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

        private ItemDatabase database;
        private List<ItemCategory> inventory = new List<ItemCategory>();
        private float inventoryWeight = 0;
        private InventorySorter sorter;

        public ReadOnlyCollection<ItemCategory> InventoryItems => inventory.AsReadOnly();
        public bool LimitedByWeight => limitedByWeight;
        public float WeightCapacity => weightCapacity;
        public float InventoryWeight => inventoryWeight;
        public InventorySorter Sorter => sorter;

        public Action onInventoryInitialized;
        public Action slotFull;
        public Action inventoryFull;

        private void Awake() 
        {
            database = GetComponent<ItemDatabase>();
            sorter = new InventorySorter(inventory);

            database.onDatabaseInitialized += InitializeInventory;
        }

        private void InitializeInventory()
        {
            database.onDatabaseInitialized -= InitializeInventory;

            string internalInventoryYAML = Resources.Load<TextAsset>(Path.ChangeExtension(INVENTORY_YAML_PATH, null)).text;
            StringReader input = null;

            if (EditorUtils.IsUnityEditor())
            {
                input = new StringReader(internalInventoryYAML);
            }
            else
            {
                if (File.Exists(INVENTORY_YAML_PATH))
                {
                    input = new StringReader(File.ReadAllText(INVENTORY_YAML_PATH));
                }
                else
                {
                    input = new StringReader(internalInventoryYAML);
                    IOUtils.WriteFileToDirectory(INVENTORY_YAML_PATH, Encoding.ASCII.GetBytes(internalInventoryYAML));
                }
            }

            try { LoadInventoryParameters(input); } catch {} // if can't load inventory.yaml, use defaults

            for (int i = 0; i < database.Items.Count; i++)
            {
                inventory.Add(new ItemCategory(database.Items[i].TypeName, database.Items[i].CategoryName, new List<ItemSlot>()));
            }

            onInventoryInitialized?.Invoke();
        }

        private void LoadInventoryParameters(StringReader input)
        {
            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var parameter in root.Children)
            {
                string keyName = parameter.Key.ToString();
                string valueName = parameter.Value.ToString();

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

            if (limitedByWeight && ReachedWeightLimit(item.Weight, ref addAmount))
            {
                // TODO: return the amount of items that could not add, or something similar
                inventoryFull?.Invoke();

                if (addAmount == 0) return;
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

        private bool ReachedWeightLimit(float itemWeight, ref int addAmount)
        {
            bool result = false;

            while (inventoryWeight + itemWeight * addAmount > weightCapacity)
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

                    currItemName = StringUtils.NoSpacesAndLowerCaseString(currItemName);
                    nameToSearch = StringUtils.NoSpacesAndLowerCaseString(nameToSearch);

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