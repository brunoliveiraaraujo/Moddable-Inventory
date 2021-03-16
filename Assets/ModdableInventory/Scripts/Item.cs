using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Utils;

namespace ModdableInventory
{
    public class Item
    {
        private Dictionary<string, string> itemData;

        public string Name { get; private set; }
        public int Cost { get; private set; }
        public float Weight { get; private set; }
        public int StackLimit { get; private set; }
        public bool MultiStack { get; private set; }
        public Dictionary<string, string> ItemData 
        { 
            get => itemData; private set => itemData = value; 
        }

        public virtual void Initialize(Dictionary<string, string> itemData)
        {
            this.ItemData = itemData;

            Name = SetProperty("name", "generic_item");
            Cost = SetProperty<int>("cost", 0);
            Weight = SetProperty<float>("weight", 0);
            StackLimit = SetProperty<int>("stackLimit", 99);
            MultiStack = SetProperty<bool>("multiStack", true);

            if (Cost < 0) 
                throw new ArgumentOutOfRangeException($"cost of \"{Name}\"", "cannot be negative");
            if (StackLimit < 0) 
                throw new ArgumentOutOfRangeException($"stackLimit of \"{Name}\"", "cannot be negative");
        }

        public virtual void LogItem(int decimalPlaces = 2)
        {
            Debug.Log($"{Name}");
            Debug.Log($"    cost={Cost}");
            Debug.Log($"    stackLimit={StackLimit}");
            Debug.Log($"    multiStack={MultiStack}");
            Debug.Log($"    weight={StringOperations.FloatToString(Weight, decimalPlaces)}");
        }

        protected string SetProperty(string key, string defaultValue)
        {
            string property;

            if (ItemData.ContainsKey(key))
            {
                property = ItemData[key];
            }
            else
            {
                property = defaultValue;
            }

            return property;
        }

        protected T SetProperty<T>(string key, T defaultValue) where T : struct
        {
            T property;

            if (ItemData.ContainsKey(key))
            {
                try
                {
                    property = (T) Convert.ChangeType(
                    ItemData[key], typeof(T), CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new FormatException(
                        $"value in key \"{key}\" of \"{Name}\"" 
                        + $" is not of type \"{typeof(T)}\""); 
                }
            }
            else
            {
                property = defaultValue;
            }

            return property;
        }
    }
}