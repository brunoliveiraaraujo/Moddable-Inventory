using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ModdableInventory;
using System;

public abstract class AbstractSlotButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected InventoryUIManager uiManager = null;
    protected string itemStringID = null;

    private bool initialized = false;

    public void Initialize(InventoryUIManager uiManager)
    {
        if (!initialized) this.uiManager = uiManager;
        initialized = true;
    }

    public void SetData(string itemStringID)
    {
        this.itemStringID = itemStringID;
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.ShowItemTooltip(transform.position, itemStringID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager.HideItemTooltip();
    }
}
