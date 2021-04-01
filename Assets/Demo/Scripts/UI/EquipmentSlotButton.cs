using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotButton : AbstractSlotButton
{
    private int equipSlotID = -1;

    public void SetData(string itemIDName, int equipSlotID = -1)
    {
        base.SetData(itemIDName);
        this.equipSlotID = equipSlotID;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (itemIDName != null)
        {
            uiManager.UnequipItem(itemIDName, equipSlotID);
            uiManager.HideItemTooltip();
        }
    }
}
