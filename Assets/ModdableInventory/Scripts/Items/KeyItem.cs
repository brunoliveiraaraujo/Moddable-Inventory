using System.Collections.Generic;

namespace ModdableInventory.Items
{
    public class KeyItem : Item
    {
        public override void Initialize(string idName, Dictionary<string, string> itemData)
        {
            base.Initialize(idName, itemData);
        }

        public override void LogItem(int decimalPlaces)
        {
            base.LogItem(decimalPlaces);
        }
    }
}