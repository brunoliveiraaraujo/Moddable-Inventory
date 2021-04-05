using System.Collections.Generic;
using ModdableInventory;

public class Armor : ItemType
{
    public int Defense { get; private set; }

    protected override void LoadProperties()
    {
        base.LoadProperties();
        Defense = SetProperty<int>("defense", 0);
    }

    public override string PropertiesToString(int decimalPlaces = 2)
    {
        string text = base.PropertiesToString(decimalPlaces);

        text += $"defense: {Defense}\n";

        return text;
    }
}