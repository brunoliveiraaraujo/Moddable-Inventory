using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ModdableInventory.Utils;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using System.Globalization;

namespace ModdableInventory
{
    [RequireComponent(typeof(ItemDatabase))]
    [RequireComponent(typeof(Inventory))]
    public class Equipment : MonoBehaviour
    {
        private const string EQUIPMENT_YAML_PATH = "gamedata/config/equipment.yaml";

        private ItemDatabase database;
        private Inventory inventory;
        private List<EquipmentSlot> equipSlots = new List<EquipmentSlot>();

        public ReadOnlyCollection<EquipmentSlot> EquipSlots => equipSlots.AsReadOnly();

        public event EventHandler EquipmentInitialized;

        private void Awake() 
        {
            database = GetComponent<ItemDatabase>();
            inventory = GetComponent<Inventory>();

            database.DatabaseInitialized += OnDatabaseInitialized;
        }

        private void OnDatabaseInitialized(object sender, EventArgs e)
        {
            database.DatabaseInitialized -= OnDatabaseInitialized;

            InitializeEquipment();
        }

        private void InitializeEquipment()
        {
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
                Vector2 deltaPos = Vector2.zero;

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
                    else if (keyName.Equals("deltaX"))
                    {
                        deltaPos.x = float.Parse(valueName, CultureInfo.InvariantCulture);
                    }
                    else if (keyName.Equals("deltaY"))
                    {
                        deltaPos.y = float.Parse(valueName, CultureInfo.InvariantCulture);
                    }
                }

                equipSlots.Add(new EquipmentSlot(
                    slotName, 
                    Type.GetType(GlobalConstants.ITEMS_NAMESPACE + "." + typeName, true), 
                    deltaPos
                ));
            }
        }

        public void EquipItem(string itemStringID)
        {
            Item item = GetItemFromInventoryByStringID(itemStringID, inventory.InventoryItems);

            if (item != null)
            {
                foreach (var slot in equipSlots)
                {
                    bool equipped = EquipItemInSlot(item, slot);
                    if (equipped) return;
                }
            }
        }

        public void UnequipItem(string itemStringID, int slotID = -1)
        {
            Item item = GetItemFromEquippedByStringID(itemStringID);

            if (item != null)
            {
                if (slotID < 0)
                {
                    foreach (var slot in equipSlots)
                    {
                        bool unequipped = UnequipItemInSlot(item, slot);
                        if (unequipped) return;
                    }
                }
                else
                {
                    UnequipItemInSlot(item, equipSlots[slotID], true);
                }
            }
        }

        private bool EquipItemInSlot(Item item, EquipmentSlot slot, bool throwOnError = false)
        {
            try
            {
                if (slot.Item == null)
                {
                    slot.Item = item;
                    inventory.RemoveItemFromInventoryByID(item.ItemStringID);
                    inventory.CurrentWeight += item.Weight;
                    return true;
                }
            }
            catch (InvalidCastException e) 
            {
                if (throwOnError) throw e;
            }
            return false;
        }

        private bool UnequipItemInSlot(Item item, EquipmentSlot slot, bool throwOnError = false)
        {
            try
            {
                if (slot.Item != null && slot.Item.ItemStringID.Equals(item.ItemStringID))
                {
                    slot.Item = null;
                    inventory.CurrentWeight -= item.Weight;
                    inventory.AddItemToInventoryByID(item.ItemStringID);
                    return true;
                }
            }
            catch (InvalidCastException e) 
            {
                if (throwOnError) throw e;
            }
            return false;
        }

        private Item GetItemFromEquippedByStringID(string itemStringID)
        {
            foreach (var slot in equipSlots)
            {
                if (slot.Item != null)
                {
                    if (StringUtils.StringContainsName(slot.Item.ItemStringID, itemStringID))
                    {
                        return slot.Item;
                    }
                }
            }

            return null;
        }

        private Item GetItemFromInventoryByStringID(string itemStringID, ReadOnlyCollection<ItemCategory> inventoryItems)
        {
            foreach (var category in inventoryItems)
            {
                foreach (var slot in category.ItemSlots)
                {
                    if (StringUtils.StringContainsName(slot.Item.ItemStringID, itemStringID))
                    {
                        return slot.Item;
                    }
                }
            }

            return null;
        }
    }
}