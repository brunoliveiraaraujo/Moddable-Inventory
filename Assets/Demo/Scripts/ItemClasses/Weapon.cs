using System.Collections.Generic;
using ModdableInventory;
using UnityEngine;

public class Weapon : Item
{
    public int Attack { get; private set; }

    public override void LoadProperties()
    {
        base.LoadProperties();
        Attack = SetProperty<int>("attack", 0);
    }

    public override string ItemDataToString(int decimalPlaces = 2)
    {
        string text = base.ItemDataToString(decimalPlaces);

        text += $"attack: {Attack}\n";

        return text;
    }

    public override void LogItem(int decimalPlaces)
    {
        base.LogItem(decimalPlaces);
        Debug.Log($"    attack={Attack}");
    }
}