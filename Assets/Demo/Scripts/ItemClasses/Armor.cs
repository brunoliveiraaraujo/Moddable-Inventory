using System.Collections.Generic;
using ModdableInventory;
using UnityEngine;

public class Armor : Item
{
    public int Defense { get; private set; }

    public override void LoadProperties()
    {
        base.LoadProperties();
        Defense = SetProperty<int>("defense", 0);
    }

    public override string ItemDataToString(int decimalPlaces = 2)
    {
        string text = base.ItemDataToString(decimalPlaces);

        text += $"defense: {Defense}\n";

        return text;
    }

    public override void LogItem(int decimalPlaces)
    {
        base.LogItem(decimalPlaces);
        Debug.Log($"    defense={Defense}");
    }
}