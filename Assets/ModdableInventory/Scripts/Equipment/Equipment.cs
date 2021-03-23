using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ModdableInventory.Items;
using ModdableInventory.Utils;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace ModdableInventory
{
    [RequireComponent(typeof(ItemDatabase))]
    [RequireComponent(typeof(Inventory))]
    public class Equipment : MonoBehaviour
    {
        private const string EQUIPMENT_YAML_PATH = "gamedata/config/equipment.yaml";

        private ItemDatabase database;
        private Inventory inventory;
        private List<EquipmentSlot> equippedItems = new List<EquipmentSlot>();

        public ReadOnlyCollection<EquipmentSlot> EquippedItems => equippedItems.AsReadOnly();

        public event EventHandler EquipmentInitialized;

        private void Awake() 
        {
            database = GetComponent<ItemDatabase>();
            inventory = GetComponent<Inventory>();

            database.DatabaseInitialized += InitializeEquipment;
        }

        private void InitializeEquipment(object sender, EventArgs e)
        {
            database.DatabaseInitialized -= InitializeEquipment;

            string internalEquipmentYAML = Resources.Load<TextAsset>(Path.ChangeExtension(EQUIPMENT_YAML_PATH, null)).text;

            ParseEquipment(IOUtils.ReadOrMakeYAMLFile(internalEquipmentYAML, EQUIPMENT_YAML_PATH));
            
            EquipmentInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void ParseEquipment(StringReader input)
        {
            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var topLevelNode in root.Children)
            {
                string keyName = topLevelNode.Key.ToString();
                string valueName = topLevelNode.Value.ToString();

                if (keyName.Equals("equipmentSlots"))
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

        public void EquipItem(string itemName)
        {
            Item item = GetItemFromInventory(itemName, inventory.InventoryItems);

            if (item != null)
            {
                foreach (var slot in equippedItems)
                {
                    if (item.GetType().Name.Equals(slot.TypeName) || item.GetType().BaseType.Name.Equals(slot.TypeName))
                    {
                        if (slot.Item == null)
                        {
                            slot.Item = item;
                            inventory.RemoveItemFromInventory(itemName);
                            inventory.CurrentWeight += item.Weight;
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
                            inventory.CurrentWeight -= item.Weight;
                            inventory.AddItemToInventory(itemName);
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

        private Item GetItemFromInventory(string name, ReadOnlyCollection<ItemCategory> inventoryItems)
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
    }
}