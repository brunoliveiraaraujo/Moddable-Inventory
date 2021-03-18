using System.Collections.Generic;
using UnityEngine;

namespace ModdableInventory.Items
{
    public class Armor : Item
    {
        public int Defense { get; private set; }

        public override void Initialize(string idName, Dictionary<string, string> itemData)
        {
            base.Initialize(idName, itemData);
            Defense = SetProperty<int>("defense", 0);
        }

        public override void LogItem(int decimalPlaces)
        {
            base.LogItem(decimalPlaces);
            Debug.Log($"    defense={Defense}");
        }

    }
}