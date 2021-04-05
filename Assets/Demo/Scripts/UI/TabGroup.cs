using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private Color colorIdle;
    [SerializeField] private Color colorHover;
    [SerializeField] private Color colorActive;
    [SerializeField] private TabButton initialSelectedTab;

    private TabButton selectedTab;
    private List<TabButton> tabButtons = new List<TabButton>();
    private List<TextMeshProUGUI> tabTexts = new List<TextMeshProUGUI>();

    private void Awake() 
    {
        selectedTab = initialSelectedTab;

        for (int i = 0; i < transform.childCount; i++)
        {
            tabButtons.Add(transform.GetChild(i).GetComponent<TabButton>());
            tabTexts.Add(transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>());
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

    public void ChangeTabText(int tabID, string text)
    {
        tabTexts[tabID].text = text;
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
