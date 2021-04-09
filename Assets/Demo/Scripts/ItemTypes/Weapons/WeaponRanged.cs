using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRanged : Weapon
{
    public int Range { get; private set; }

    protected override void LoadProperties()
    {
        base.LoadProperties();
        Range = SetProperty<int>("range", 0);
    }

    public override string PropertiesToString(int decimalPlaces = 2)
    {
        string text = base.PropertiesToString(decimalPlaces);

        text += $"range: {Range}\n";

        return text;
    }
}
