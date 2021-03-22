using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using ModdableInventory.Utils;
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
        private float currentWeight = 0;

        private ItemDatabase database;
        private List<ItemCategory> inventoryItems = new List<ItemCategory>();
        private List<EquipmentSlot> equippedItems = new List<EquipmentSlot>();
        private InventorySorter sorter;

        public ReadOnlyCollection<ItemCategory> InventoryItems => inventoryItems.AsReadOnly();
        public ReadOnlyCollection<EquipmentSlot> EquippedItems => equippedItems.AsReadOnly();
        public bool LimitedByWeight => limitedByWeight;
        public float WeightCapacity => weightCapacity;
        public float CurrentWeight => currentWeight;
        public InventorySorter Sorter => sorter;

        public Action onInventoryInitialized;
        public Action slotFull;
        public Action inventoryFull;

        private void Awake() 
        {
            database = GetComponent<ItemDatabase>();
            sorter = new InventorySorter(inventoryItems);

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

            ParseInventory(input);

            for (int i = 0; i < database.Items.Count; i++)
            {
                inventoryItems.Add(new ItemCategory(database.Items[i].TypeName, database.Items[i].CategoryName, new List<InventorySlot>()));
            }

            onInventoryInitialized?.Invoke();
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
                else if (keyName.Equals("equipmentSlots"))
                {
                    ParseEquipmentSlots(topLevelNode);
                }
            }
        }

        private void ParseEquipmentSlots(KeyValuePair<YamlNode, YamlNode> topLevelNode)
        {
            foreach (var slot in ((YamlMappingNode)topLevelNode.Value).Children)
            {
                string slotName = null;
                string typeName = null;

                foreach (var parameter in ((YamlMappingNode)slot.Value).Children)
                {
                    string keyName = parameter.Key.ToString();
                    string valueName = parameter.Value.ToString();

                    if (keyName.Equals("name"))
                    {
                        slotName = valueName;
                    }
                    else if (keyName.Equals("itemType"))
                    {
                        typeName = valueName;
                    }
                }

                equippedItems.Add(new EquipmentSlot(slotName, typeName));
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
            InventorySlot currSlot = GetLastSlotWithItem(categoryID, itemID);

            if (limitedByWeight && ReachedWeightLimit(item.Weight, ref addAmount))
            {
                inventoryFull?.Invoke();

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

        public void RemoveItemFromInventory(int categoryID, int itemID, int subAmount = 1)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);
            InventorySlot currSlot = GetLastSlotWithItem(categoryID, itemID);

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

        public void EquipItem(string itemName)
        {
            Item item = GetItemFromInventory(itemName);

            if (item != null)
            {
                foreach (var slot in equippedItems)
                {
                    if (item.GetType().Name.Equals(slot.TypeName) || item.GetType().BaseType.Name.Equals(slot.TypeName))
                    {
                        if (slot.Item == null)
                        {
                            slot.Item = item;
                            RemoveItemFromInventory(itemName);
                            currentWeight += item.Weight;
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }

        public void UnequipItem(string itemName)
        {
            Item item = GetItemFromEquipped(itemName);

            if (item != null)
            {
                foreach (var slot in equippedItems)
                {
                    if (item.GetType().Name.Equals(slot.TypeName) || item.GetType().BaseType.Name.Equals(slot.TypeName))
                    {
                        if (slot.Item != null)
                        {
                            slot.Item = null;
                            currentWeight -= item.Weight;
                            AddItemToInventory(itemName);
                            return;
                        }
                        else
                        {
                             continue;
                        }
                    }
                }
            }
        }

        private Item GetItemFromEquipped(string name)
        {
            foreach (var slot in equippedItems)
            {
                if (slot.Item != null)
                {
                    if (StringUtils.StringContainsName(slot.Item.Name, name))
                    {
                        return slot.Item;
                    }
                }
            }

            return null;
        }

        private Item GetItemFromInventory(string name)
        {
            foreach (var category in inventoryItems)
            {
                foreach (var slot in category.ItemSlots)
                {
                    if (StringUtils.StringContainsName(slot.Item.Name, name))
                    {
                        return slot.Item;
                    }
                }
            }

            return null;
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

        private void AddNewItemSlot(ref InventorySlot currSlot, int categoryID, int itemID, int itemAmount)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);

            inventoryItems[categoryID].ItemSlots.Add(currSlot = new InventorySlot(item, Mathf.Min(itemAmount, item.StackLimit)));
            currentWeight += currSlot.Weight;
        }

        private void UpdateItemSlot(InventorySlot currSlot, int newAmount)
        {
            currentWeight -= currSlot.Weight;
            currSlot.SetAmount(newAmount);
            currentWeight += currSlot.Weight;
        }

        private void RemoveItemSlot(InventorySlot currSlot, int categoryID)
        {
            currentWeight -= currSlot.Weight;
            inventoryItems[categoryID].ItemSlots.Remove(currSlot);
        }

        // this makes it so that items are added/removed from the last slot first.
        private InventorySlot GetLastSlotWithItem(int categoryID, int itemID)
        {
            Item item = GetItemFromDatabase(categoryID, itemID);

            sorter.SortInventoryByAmount();

            for (int i = inventoryItems[categoryID].ItemSlots.Count - 1; i >= 0; i--)
            {
                InventorySlot currSlot = inventoryItems[categoryID].ItemSlots[i];

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

                    if (StringUtils.StringContainsName(currItemName, nameToSearch))
                    {
                        return (true, i, j);
                    }
                }
            }

            return (false, -1, -1);
        }
    }
}