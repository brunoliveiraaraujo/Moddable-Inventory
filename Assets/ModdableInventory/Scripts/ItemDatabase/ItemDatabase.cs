using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Text;
using ModdableInventory.Utils;
using ModdableInventory.Items;

namespace ModdableInventory
{
    public class ItemDatabase : MonoBehaviour
    {
        private const string ITEM_DATABASE_YAML_PATH = "gamedata/config/itemDatabase.yaml";

        private List<ItemCategory> items = new List<ItemCategory>();
        
        public ReadOnlyCollection<ItemCategory> Items => items.AsReadOnly();
        
        public event EventHandler DatabaseInitialized;
        
        private void Start() 
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            string internalDatabaseYAML = Resources.Load<TextAsset>(Path.ChangeExtension(ITEM_DATABASE_YAML_PATH, null)).text;

            ParseDatabase(IOUtils.ReadOrMakeYAMLFile(internalDatabaseYAML, ITEM_DATABASE_YAML_PATH));

            DatabaseInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void ParseDatabase(StringReader input)
        {
            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            int categoryID = 0;
            foreach (var topLevelNode in root.Children)
            {
                if (topLevelNode.Key.ToString().Equals("extractSpriteData"))
                {
                    if (bool.Parse(topLevelNode.Value.ToString()))
                    {
                        DatabaseInitialized += ExtractAllItemsSprites;
                    }
                }
                else
                {
                    ParseItemCategories(topLevelNode, categoryID);
                    categoryID++;
                }
            }
        }

        private void ExtractAllItemsSprites(object sender, EventArgs e)
        {
            DatabaseInitialized -= ExtractAllItemsSprites;

            foreach (var category in items)
            {
                foreach (var slot in category.ItemSlots)
                {
                    slot.Item.ExtractItemSprite();
                }
            }
        }

        private void ParseItemCategories(KeyValuePair<YamlNode, YamlNode> topLevelNode, int categoryID)
        {
            string typeName = topLevelNode.Key.ToString();

            foreach (var entry in ((YamlMappingNode)topLevelNode.Value).Children)
            {
                if (entry.Key.ToString().Equals("categoryName"))
                {
                    items.Add(new ItemCategory(typeName, entry.Value.ToString(), new List<InventorySlot>()));
                }
                else if (entry.Key.ToString().Equals("items"))
                {
                    foreach (var item in ((YamlMappingNode)entry.Value).Children)
                    {
                        ParseItem(item, typeName, categoryID);
                    }
                }
            }
        }

        private void ParseItem(KeyValuePair<YamlNode, YamlNode> itemNode, string typeName, int categoryID)
        {
            string idName = itemNode.Key.ToString();

            Dictionary<string, string> itemData = new Dictionary<string, string>();

            try 
            {
                foreach (var parameter in ((YamlMappingNode)itemNode.Value).Children)
                {
                    itemData.Add(parameter.Key.ToString(), parameter.Value.ToString());
                }
            }
            catch (InvalidCastException) {} // loading parameters is optional, they have defaults

            string itemsNamespace = typeof(Item).Namespace;
            Type itemType = Type.GetType(itemsNamespace + "." + typeName, true);
            Item instance = (Item) Activator.CreateInstance(itemType);

            instance.Initialize(idName, itemData);
            
            items[categoryID].ItemSlots.Add(new InventorySlot(instance));
        }
    }
}