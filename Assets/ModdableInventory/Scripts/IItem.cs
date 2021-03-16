using System.Collections.Generic;

namespace ModdableInventory
{
    public interface IITem
    {
        string Name { get; }
        int Cost { get; }
        float Weight { get; }
        int StackLimit { get; }
        bool MultiStack { get; }
        Dictionary<string, string> ItemData { get; }

        void Initialize(Dictionary<string, string> itemData);
        void LogItem(int decimalPlaces);
    }
}