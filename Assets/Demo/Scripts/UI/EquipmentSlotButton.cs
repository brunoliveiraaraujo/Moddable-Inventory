using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ModdableInventory;

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
            
            itemStringID = null;
            OnPointerEnter(eventData);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (itemStringID == null)
            uiManager.ShowEquipSlotTooltip(transform.position, equipSlotID);
        else
            base.OnPointerEnter(eventData);
    }
}
