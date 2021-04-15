using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryGridSlotButton : AbstractSlotButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        uiManager.EquipItem(itemStringID);
    }
}
