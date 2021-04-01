using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private TabGroup tabGroup;

    public Image Background { get; set; }

    public UnityEvent TabSelected;
    public UnityEvent TabDeselected;

    private void Awake() 
    {
        Background = GetComponent<Image>();
        tabGroup = transform.parent.GetComponent<TabGroup>();

        if (tabGroup == null) throw new NullReferenceException("Parent does not contain TabGroup component.");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void Select()
    {
        TabSelected?.Invoke();
    }

    public void Deselect()
    {
        TabDeselected?.Invoke();
    }
}
