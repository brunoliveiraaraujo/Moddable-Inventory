using System.Collections.Generic;
using UnityEngine;

namespace ModdableInventory.Items
{
    public class Weapon : Item
    {
        public int Attack { get; private set; }

        public override void Initialize(string idName, Dictionary<string, string> itemData)
        {
            base.Initialize(idName, itemData);
            Attack = SetProperty<int>("attack", 0);
        }

        public override void LogItem(int decimalPlaces)
        {
            base.LogItem(decimalPlaces);
            Debug.Log($"    attack={Attack}");
        }
    }
}