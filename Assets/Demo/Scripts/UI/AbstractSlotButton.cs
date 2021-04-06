using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ModdableInventory;

public abstract class AbstractSlotButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected InventoryUIManager uiManager;
    protected string itemStringID = null;

    public void SetData(string itemStringID)
    {
        this.itemStringID = itemStringID;
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.ShowItemTooltip(transform.position, itemStringID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager.HideItemTooltip();
    }
}
