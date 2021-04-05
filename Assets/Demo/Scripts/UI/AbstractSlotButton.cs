using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractSlotButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected InventoryUIManager uiManager;
    protected string itemIDName = null;

    public void SetData(string itemIDName)
    {
        this.itemIDName = itemIDName;
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.ShowItemTooltip(transform.position, itemIDName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager.HideItemTooltip();
    }
}
