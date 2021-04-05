using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ModdableInventory;

public class EquipmentSlotButton : AbstractSlotButton
{
    private int equipSlotID = -1;

    public void SetData(UniqueStringID stringID, int equipSlotID = -1)
    {
        base.SetData(stringID);
        this.equipSlotID = equipSlotID;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (stringID != null)
        {
            uiManager.UnequipItem(stringID, equipSlotID);
            uiManager.HideItemTooltip();
        }
    }
}
