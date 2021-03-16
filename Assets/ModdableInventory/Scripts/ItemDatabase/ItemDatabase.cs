using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using IniParser;
using IniParser.Model;
using System.Linq;
using System.IO;

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

            string fileName = "items.ini";
            string editorIniPath = "./Assets/ModdableInventory/INI/";
            #pragma warning disable 0219
            string localIniPath = "./INI/";
            #pragma warning restore 0219

            #if UNITY_EDITOR
                IniData data = parser.ReadFile(editorIniPath + fileName);
            #else
                if (!File.Exists(localIniPath + fileName))
                {
                    GenerateItemsINI(localIniPath, fileName);
                }
                IniData data = parser.ReadFile(localIniPath + fileName);
            #endif

            items = ParseDatabase(parser, data);

            onLoaded?.Invoke();
        }

        private void GenerateItemsINI(string path, string fileName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (StreamWriter writer = new StreamWriter(path + fileName))
            {
                writer.WriteLine("[WeaponTypes]");
            }
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
                    string categoryName = section.Keys["categoryName"];
                    Type itemType = Type.GetType(typeName);
                    Dictionary<int, string> itemID_itemName = new Dictionary<int, string>();

                    database.Add(new ItemCategory(typeName, categoryName, new List<ItemSlot>()));

                    foreach (var key in section.Keys)
                    {
                        if (!key.KeyName.Equals("categoryName"))
                        {
                            int itemID = int.Parse(key.KeyName);

                            itemID_itemName.Add(itemID, key.Value);

                            if (itemID < 0) 
                            {
                                throw new ArgumentOutOfRangeException(
                                    $"index of \"{key.Value}\"", "cannot be negative");
                            }
                        }
                    }
                    foreach (var orderedItems in itemID_itemName.OrderBy(key => key.Key))
                    {
                        itemName_TypeName.Add(orderedItems.Value, typeName);
                    }
                }
            }

            // read item sections
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