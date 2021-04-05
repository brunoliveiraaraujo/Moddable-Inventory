using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ModdableInventory;

public abstract class AbstractSlotButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected InventoryUIManager uiManager;
    protected UniqueStringID stringID = null;

    public void SetData(UniqueStringID stringID)
    {
        this.stringID = stringID;
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.ShowItemTooltip(transform.position, stringID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager.HideItemTooltip();
    }
}
