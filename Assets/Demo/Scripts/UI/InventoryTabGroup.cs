using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModdableInventory;
using System;
using TMPro;
using System.Collections.ObjectModel;

public class InventoryTabGroup : MonoBehaviour
{
    [SerializeField] private Color colorIdle;
    [SerializeField] private Color colorHover;
    [SerializeField] private Color colorActive;
    [SerializeField] private ItemDatabase database;
    [SerializeField] private InventoryUIManager uiManager;
    [SerializeField] private GameObject tabButtonPrefab;

    private InventoryTabButton selectedTabButton;
    private int selectedTabID;
    private List<InventoryTabButton> tabButtons = new List<InventoryTabButton>();

    public ReadOnlyCollection<InventoryTabButton> TabButtons => tabButtons.AsReadOnly();

    private void Awake() 
    {
        database.DatabaseInitialized += OnDatabaseInitialized;
    }

    private void OnDatabaseInitialized(object sender, EventArgs e)
    {
        database.DatabaseInitialized -= OnDatabaseInitialized;
        InitializeAllTabs();
    }

    private void InitializeAllTabs()
    {
        GameObject tabObj = GameObject.Instantiate(tabButtonPrefab, transform);
        InitializeTab(tabObj, "all items", -1);

        for (int i = 0; i < database.ItemCategories.Count; i++)
        {
            ItemCategory category = database.ItemCategories[i];
            
            if (category.ShowCategoryTab)
            {
                tabObj = GameObject.Instantiate(tabButtonPrefab, transform);
                InitializeTab(tabObj, category.CategoryName, i);
            }
        }

        selectedTabButton = tabButtons[0];
        selectedTabID = 0;
        for (int i = 1; i < tabButtons.Count; i++)
        {            
            tabButtons[i].Background.color = colorIdle;
        }

        OnTabSelected(selectedTabButton);
    }

    private void InitializeTab(GameObject tabObj, string text, int categoryID)
    {
        InventoryTabButton tabButton = tabObj.GetComponent<InventoryTabButton>();
        tabButton.Initialize(categoryID);

        tabButtons.Add(tabButton);

        tabObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text.ToLower();
    }

    public void OnTabEnter(InventoryTabButton tab)
    {
        ResetTabs();
        if (selectedTabButton != null && tab == selectedTabButton) return;
        tab.Background.color = colorHover;
    }

    public void OnTabExit(InventoryTabButton tab)
    {
        ResetTabs();
    }

    public void OnTabSelected(InventoryTabButton tab)
    {
        selectedTabButton = tab;

        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (selectedTabButton.Equals(tabButtons[i]))
            {
                selectedTabID = i;
                break;
            }
        }

        uiManager.DisplayItemPage(selectedTabID);

        ResetTabs();
        tab.Background.color = colorActive;
    }

    private void ResetTabs()
    {
        foreach(InventoryTabButton tab in tabButtons)
        {
            if (selectedTabButton != null && tab == selectedTabButton) { continue; }
            tab.Background.color = colorIdle;
        }
    }
}
