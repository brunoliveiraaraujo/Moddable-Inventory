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
        private const string ITEMS_YAML_PATH = "gamedata/config/items.yaml";

        private List<ItemCategory> items = new List<ItemCategory>();
        
        public ReadOnlyCollection<ItemCategory> Items => items.AsReadOnly();
        
        public Action onLoaded;
        
        private void Start() 
        {
            LoadDatabase();
        }
        
        private void LoadDatabase()
        {
            string itemsData = Resources.Load<TextAsset>(Path.ChangeExtension(ITEMS_YAML_PATH, null)).text;
            StringReader input = null;

            #if UNITY_EDITOR
                input = new StringReader(itemsData);
            #else
                if (File.Exists(ITEMS_YAML_PATH))
                {
                    input = new StringReader(File.ReadAllText(ITEMS_YAML_PATH));
                }
                else
                {
                    input = new StringReader(itemsData);
                    IOUtils.WriteFileToDirectory(ITEMS_YAML_PATH, itemsData);
                }
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
                            string idName = item.Key.ToString();

                            Dictionary<string, string> itemData = new Dictionary<string, string>();

                            try 
                            {
                                foreach (var parameter in ((YamlMappingNode)item.Value).Children)
                                {
                                    itemData.Add(parameter.Key.ToString(), parameter.Value.ToString());
                                }
                            }
                            catch (InvalidCastException) {}

                            Type itemType = Type.GetType(ITEMS_NAMESPACE + "." + typeName, true);
                            Item instance = (Item) Activator.CreateInstance(itemType);

                            instance.Initialize(idName, itemData);
                            
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