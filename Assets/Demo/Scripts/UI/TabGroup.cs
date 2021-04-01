using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private Color colorIdle;
    [SerializeField] private Color colorHover;
    [SerializeField] private Color colorActive;
    [SerializeField] private TabButton initialSelectedTab;

    private TabButton selectedTab;
    private List<TabButton> tabButtons = new List<TabButton>();

    private void Awake() 
    {
        selectedTab = initialSelectedTab;

        for (int i = 0; i < transform.childCount; i++)
        {
            TabButton tab = transform.GetChild(i).GetComponent<TabButton>();
            if (tab != null)
            {
                tabButtons.Add(tab);
            }
        }
    }

    private void Start() 
    {
        if (selectedTab != null)
        {
            foreach(TabButton tab in tabButtons)
            {
                if (selectedTab != null && tab == selectedTab) { continue; }
                tab.Background.color = colorIdle;
                tab.Deselect();
            }

            OnTabSelected(selectedTab);
        }
    }

    public void OnTabEnter(TabButton tab)
    {
        ResetTabs();
        if (selectedTab != null && tab == selectedTab) return;
        tab.Background.color = colorHover;
    }

    public void OnTabExit(TabButton tab)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton tab)
    {
        selectedTab.Deselect();

        selectedTab = tab;

        selectedTab.Select();

        ResetTabs();
        tab.Background.color = colorActive;
    }

    private void ResetTabs()
    {
        foreach(TabButton tab in tabButtons)
        {
            if (selectedTab != null && tab == selectedTab) { continue; }
            tab.Background.color = colorIdle;
        }
    }
}
