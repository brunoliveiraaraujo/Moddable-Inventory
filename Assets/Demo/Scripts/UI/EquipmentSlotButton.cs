using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ModdableInventory;

// TODO: make EquipmentSlotButtons's GameObjects Instantiate according to the EquipmentSlots defined in ModdableInventory.Equipment through equipment.yaml
public class EquipmentSlotButton : AbstractSlotButton
{
    private int equipSlotID = -1;

    public void SetData(string itemStringID, int equipSlotID = -1)
    {
        base.SetData(itemStringID);
        this.equipSlotID = equipSlotID;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (itemStringID != null)
        {
            uiManager.UnequipItem(itemStringID, equipSlotID);
            uiManager.HideItemTooltip();
        }
    }
}
