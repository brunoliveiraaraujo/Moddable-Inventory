using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private InventoryTabGroup tabGroup;

    public Image Background { get; private set; }

    private void Awake() 
    {
        Background = GetComponent<Image>();
        tabGroup = transform.parent.GetComponent<InventoryTabGroup>();

        if (tabGroup == null) 
            throw new NullReferenceException
            (
                "Parent does not contain InventoryTabGroup component."
            );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }
}
