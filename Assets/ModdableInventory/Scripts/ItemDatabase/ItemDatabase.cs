using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Text;
using Utils;

namespace ModdableInventory
{
    public class ItemDatabase : MonoBehaviour
    {

        private const string ITEMS_NAMESPACE = "ModdableInventory.Items";
        private const string ITEMS_YAML_PATH = "gamedata/config/itemDatabase.yaml";

        private List<ItemCategory> items = new List<ItemCategory>();
        
        public ReadOnlyCollection<ItemCategory> Items => items.AsReadOnly();
        
        public Action onDatabaseInitialized;
        
        private void Start() 
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            string internalDatabaseYAML = Resources.Load<TextAsset>(Path.ChangeExtension(ITEMS_YAML_PATH, null)).text;
            StringReader input = null;

            if (EditorUtils.IsUnityEditor())
            {    
                input = new StringReader(internalDatabaseYAML);
            }
            else
            {    
                if (File.Exists(ITEMS_YAML_PATH))
                {
                    input = new StringReader(File.ReadAllText(ITEMS_YAML_PATH));
                }
                else
                {
                    input = new StringReader(internalDatabaseYAML);
                    IOUtils.WriteFileToDirectory(ITEMS_YAML_PATH, Encoding.ASCII.GetBytes((internalDatabaseYAML)));
                }
            }

            items = ParseDatabase(input);

            onDatabaseInitialized?.Invoke();
        }

        private List<ItemCategory> ParseDatabase(StringReader input)
        {
            List<ItemCategory> database = new List<ItemCategory>();

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
                        onDatabaseInitialized += ExtractAllItemsSprites;
                    }
                }
                else
                {
                    ParseItemCategories(topLevelNode, database, categoryID);
                    categoryID++;
                }
            }
            return database;
        }

        private void ExtractAllItemsSprites()
        {
            foreach (var category in items)
            {
                foreach (var slot in category.ItemSlots)
                {
                    slot.Item.ExtractItemSprite();
                }
            }
        }

        private void ParseItemCategories(KeyValuePair<YamlNode, YamlNode> topLevelNode, List<ItemCategory> database, int categoryID)
        {
            string typeName = topLevelNode.Key.ToString();

            foreach (var entry in ((YamlMappingNode)topLevelNode.Value).Children)
            {
                if (entry.Key.ToString().Equals("categoryName"))
                {
                    database.Add(new ItemCategory(typeName, entry.Value.ToString(), new List<ItemSlot>()));
                }
                else if (entry.Key.ToString().Equals("items"))
                {
                    foreach (var item in ((YamlMappingNode)entry.Value).Children)
                    {
                        ParseItem(item, database, typeName, categoryID);
                    }
                }
            }
        }

        private void ParseItem(KeyValuePair<YamlNode, YamlNode> itemNode, List<ItemCategory> database, string typeName, int categoryID)
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

            Type itemType = Type.GetType(ITEMS_NAMESPACE + "." + typeName, true);
            Item instance = (Item) Activator.CreateInstance(itemType);

            instance.Initialize(idName, itemData);
            
            database[categoryID].ItemSlots.Add(new ItemSlot(instance));
        }
    }
}