using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Utils;

namespace ModdableInventory
{
    public abstract class Item
    {
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public float Weight { get; private set; }
        public int StackLimit { get; private set; }
        public bool MultiStack { get; private set; }

        Dictionary<string, string> itemData;

        public virtual void Initialize(Dictionary<string, string> itemData)
        {
            this.itemData = itemData;

            Name = SetProperty("name", "generic_item");
            Cost = SetProperty<int>("cost", 0);
            Weight = SetProperty<float>("weight", 0);
            StackLimit = SetProperty<int>("stackLimit", 99);
            MultiStack = SetProperty<bool>("multiStack", true);
        }

        public virtual void LogItem(int decimalPlaces)
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

            try { property = itemData[key]; }
            catch { property = defaultValue;}

            return property;
        }

        protected T SetProperty<T>(string key, T defaultValue) where T : struct
        {
            T property;

            try 
            { 
                property = (T)Convert.ChangeType(
                itemData[key], typeof(T), CultureInfo.InvariantCulture); 
            }
            catch { property = defaultValue; }
            return property;
        }
    }
}