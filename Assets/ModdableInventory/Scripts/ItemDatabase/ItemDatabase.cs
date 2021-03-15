using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using IniParser;
using IniParser.Model;

namespace ModdableInventory
{
    public class ItemDatabase : MonoBehaviour
    {
        private const string ITEMS_NAMESPACE = "ModdableInventory.Items";

        private List<ItemCategory> items = new List<ItemCategory>();
        
        public ReadOnlyCollection<ItemCategory> Items => items.AsReadOnly();
        
        public Action onLoaded;
        
        private void Start() 
        {
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(".\\Assets\\ModdableInventory\\INI\\items.ini");

            items = ParseDatabase(parser, data);

            onLoaded?.Invoke();
        }

        private List<ItemCategory> ParseDatabase(FileIniDataParser parser, IniData data)
        {
            List<ItemCategory> database = new List<ItemCategory>();
            Dictionary<string, string> itemName_TypeName = new Dictionary<string, string>();
            Dictionary<string, Item> itemName_Item = new Dictionary<string, Item>();

            // read types sections
            foreach (SectionData section in data.Sections)
            {
                if (section.SectionName.EndsWith("Types"))
                {
                    string typeName = section.SectionName.Replace("Types", "");
                    Type itemType = Type.GetType(typeName);

                    string categoryName = section.Keys["categoryName"];

                    database.Add(new ItemCategory(typeName, categoryName, new List<ItemSlot>()));

                    for (int i = 0 ; i < section.Keys.Count - 1; i++)
                    {
                        var keyValue = section.Keys[i.ToString()];
                        itemName_TypeName.Add(keyValue, typeName);
                    }
                }
            }

            // read items sections
            foreach (SectionData section in data.Sections)
            {
                foreach (KeyValuePair<string, string> nameEntry in itemName_TypeName)
                {
                    var itemName = nameEntry.Key;
                    var typeName = nameEntry.Value;

                    if (section.SectionName.Equals(itemName))
                    {
                        Type itemType = Type.GetType(ITEMS_NAMESPACE + "." + typeName, true);

                        Item item = (Item) Activator.CreateInstance(itemType);
                        
                        Dictionary<string, string> itemData = new Dictionary<string, string>();

                        foreach(KeyData key in section.Keys)
                        {
                            itemData.Add(key.KeyName, key.Value);
                        }

                        item.Initialize(itemData);

                        itemName_Item.Add(section.SectionName, item);
                    }
                }
            }

            // populate database with initialized items
            foreach (KeyValuePair<string,string> nameEntry in itemName_TypeName)
            {
                var itemName = nameEntry.Key;
                var typeName = nameEntry.Value;

                foreach (KeyValuePair<string,Item> itemEntry in itemName_Item)
                {
                    if (itemName.Equals(itemEntry.Key))
                    {
                        var item = itemEntry.Value;
                        for (int i = 0; i < database.Count; i++)
                        {
                            if (database[i].TypeName.Equals(typeName))
                            {
                                database[i].ItemSlots.Add(new ItemSlot(item));
                                break;
                            }   
                        }
                        break;
                    }
                } 
            }

            return database;
        }
    }
}