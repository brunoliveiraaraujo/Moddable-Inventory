using System.Collections;
using System.Collections.Generic;
using ModdableInventory;
using UnityEngine;

public class Ammo : OtherItem
{
    public string WeaponName { get; private set; }
    public int AttackModifier { get; private set; }

    protected override void LoadProperties()
    {
        base.LoadProperties();
        WeaponName = SetStringProperty("weaponName", null);
        AttackModifier = SetProperty<int>("attackModifier", 0);
    }

    public override string PropertiesToString(int decimalPlaces = 2)
    {
        string text = base.PropertiesToString(decimalPlaces);

        text += $"attack bonus: {AttackModifier}\n";
        text += $"ammo for: {WeaponName}\n";

        return text;
    }
}
