﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Text;
using ModdableInventory.Utils;

namespace ModdableInventory
{
    /// <summary>
    /// Loads and manages all items defined "itemDatabase.yaml".
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        private const string ITEM_DATABASE_YAML_PATH = "gamedata/config/itemDatabase.yaml";

        private List<ItemCategory> itemCategories = new List<ItemCategory>();
        private bool extractSpriteData = false;
        
        public ReadOnlyCollection<ItemCategory> ItemCategories => itemCategories.AsReadOnly();
        
        public event EventHandler DatabaseInitialized;
        
        private void Awake() 
        {
            DatabaseInitialized += OnDatabaseInitialized;
        }

        private void Start() 
        {
            InitializeDatabase();
        }

        private void OnDatabaseInitialized(object sender, EventArgs e)
        {
            DatabaseInitialized -= OnDatabaseInitialized;

            if (extractSpriteData) ExtractAllItemsSprites();
        }

        private void InitializeDatabase()
        {
            string internalDatabaseYAML = Resources.Load<TextAsset>(Path.ChangeExtension(ITEM_DATABASE_YAML_PATH, null)).text;

            ParseDatabase(IOUtils.ReadOrMakeYAMLFile(internalDatabaseYAML, ITEM_DATABASE_YAML_PATH));

            DatabaseInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void ParseDatabase(StringReader yamlFile)
        {
            var yaml = new YamlStream();
            yaml.Load(yamlFile);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            int categoryID = 0;
            foreach (var categoryNode in root.Children)
            {
                if (categoryNode.Key.ToString().Equals("extractSpriteData"))
                {
                    extractSpriteData = bool.Parse(categoryNode.Value.ToString());
                }
                else
                {
                    ParseItemCategories(categoryNode, categoryID);
                    categoryID++;
                }
            }
        }

        private void ParseItemCategories(KeyValuePair<YamlNode, YamlNode> categoryNode, int categoryID)
        {
            string typeName = categoryNode.Key.ToString();
            Type itemType = Type.GetType(GlobalConstants.ITEMS_NAMESPACE + "." + typeName, true);

            foreach (var entry in ((YamlMappingNode)categoryNode.Value).Children)
            {
                if (entry.Key.ToString().Equals("categoryName"))
                {
                    string categoryName = entry.Value.ToString();

                    itemCategories.Add(new ItemCategory(categoryName, itemType));
                }
                else if (entry.Key.ToString().Equals("items"))
                {
                    foreach (var itemNode in ((YamlMappingNode)entry.Value).Children)
                    {
                        ParseItem(itemNode, (Item) Activator.CreateInstance(itemType), categoryID);
                    }
                }
            }
        }

        private void ParseItem(KeyValuePair<YamlNode, YamlNode> itemNode, Item item, int categoryID)
        {
            Dictionary<string, string> itemData = new Dictionary<string, string>();

            try 
            {
                foreach (var parameter in ((YamlMappingNode)itemNode.Value).Children)
                {
                    itemData.Add(parameter.Key.ToString(), parameter.Value.ToString());
                }
            }
            catch (InvalidCastException) {} // loading parameters is optional, they have defaults

            item.Initialize(itemNode.Key.ToString(), itemData);
            
            itemCategories[categoryID].AddItemSlot(new ItemSlot(item));
        }

        private void ExtractAllItemsSprites()
        {
            foreach (var category in itemCategories)
            {
                foreach (var slot in category.ItemSlots)
                {
                    slot.Item.ExtractItemSprite();
                }
            }
        }
    }
}