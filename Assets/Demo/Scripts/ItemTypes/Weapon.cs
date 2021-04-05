using System.Collections.Generic;
using ModdableInventory;
using UnityEngine;

public class Weapon : ItemType
{
    public int Attack { get; private set; }

    protected override void LoadProperties()
    {
        base.LoadProperties();
        Attack = SetProperty<int>("attack", 0);
    }

    public override string PropertiesToString(int decimalPlaces = 2)
    {
        string text = base.PropertiesToString(decimalPlaces);

        text += $"attack: {Attack}\n";

        return text;
    }
}