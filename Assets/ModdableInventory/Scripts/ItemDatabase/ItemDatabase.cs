using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Text;

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
            string itemsData = Resources.Load<TextAsset>("items").text;
            StringReader input = null;

            #if UNITY_EDITOR
                input = new StringReader(itemsData);
            #else
                if (!File.Exists("items.yaml"))
                {
                    File.WriteAllBytes("items.yaml", Encoding.ASCII.GetBytes(itemsData));
                }
                input = new StringReader(File.ReadAllText("items.yaml"));
            #endif

            items = ParseDatabase(input);

            onLoaded?.Invoke();
        }

        private List<ItemCategory> ParseDatabase(StringReader input)
        {
            List<ItemCategory> database = new List<ItemCategory>();

            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            int i = 0;
            foreach (var category in root.Children)
            {
                string typeName = category.Key.ToString();

                foreach (var entry in ((YamlMappingNode)category.Value).Children)
                {
                    if (entry.Key.ToString().Equals("categoryName"))
                    {
                        database.Add(new ItemCategory(typeName, entry.Value.ToString(), new List<ItemSlot>()));
                    }
                    else if (entry.Key.ToString().Equals("items"))
                    {
                        foreach (var item in ((YamlMappingNode)entry.Value).Children)
                        {
                            Dictionary<string, string> itemData = new Dictionary<string, string>();

                            foreach (var attribute in ((YamlMappingNode)item.Value).Children)
                            {
                                itemData.Add(attribute.Key.ToString(), attribute.Value.ToString());
                            }

                            Type itemType = Type.GetType(ITEMS_NAMESPACE + "." + typeName, true);
                            Item instance = (Item) Activator.CreateInstance(itemType);

                            instance.Initialize(itemData);
                            
                            if (database[i].TypeName.Equals(typeName))
                            {
                                database[i].ItemSlots.Add(new ItemSlot(instance));
                            }
                        }
                    }
                }
                i++;
            }

            return database;
        }
    }
}