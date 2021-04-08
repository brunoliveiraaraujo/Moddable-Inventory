using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModdableInventory;
using UnityEngine.UI;
using System;
using TMPro;
using System.Globalization;

[RequireComponent(typeof(ItemDatabase))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Equipment))]
public class InventoryUIManager : MonoBehaviour
{
    private const string INFINITY_SYMBOL = "\u221e";
    private const int ITEM_SLOTS_PER_LINE = 9;
    private const float ITEM_GRID_TOP_OFFSET = 20;
    private const float ITEM_GRID_LINE_HEIGHT = 140;

    [Tooltip("The maximum number of stacks of each item the player starts with in this demo.")]
    [Min(0)] [SerializeField] private int startingStacksInDemo = 2;

    [SerializeField] private Transform inventoryGridUI;
    [SerializeField] private Transform equipmentSlotsUI;
    [SerializeField] private Transform itemTooltip;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private GameObject itemGridSlotPrefab;
    [SerializeField] private GameObject equipSlotPrefab;

    private ItemDatabase database;
    private Inventory inventory;
    private Equipment equipment;
    UIFollowMouse tooltipScript;
    TextMeshProUGUI tooltipHeader;
    TextMeshProUGUI tooltipBody;

    private int currentPageID = 0;

    private void Awake() 
    {
        ClearInventoryDisplay();
        HideItemTooltip();

        database = GetComponent<ItemDatabase>();
        inventory = GetComponent<Inventory>();    
        equipment = GetComponent<Equipment>();
        tooltipScript = itemTooltip.GetComponent<UIFollowMouse>();
        tooltipHeader = itemTooltip.GetChild(0).GetComponent<TextMeshProUGUI>();
        tooltipBody = itemTooltip.GetChild(1).GetComponent<TextMeshProUGUI>();

        inventory.InventoryInitialized += OnInventoryInitialized;
        equipment.EquipmentInitialized += OnEquipmentInitialized;
    }

    private void OnInventoryInitialized(object sender, EventArgs e)
    {
        inventory.InventoryInitialized -= OnInventoryInitialized;

        PopulateDemoInventory();
        InitializeInventoryUI();
    }

    private void OnEquipmentInitialized(object sender, EventArgs e)
    {
        equipment.EquipmentInitialized -= OnEquipmentInitialized;

        InitializeEquipmentUI();
    }

    private void InitializeInventoryUI()
    {
        // dynamicaly spawn item buttons
        int itemSlotCount = 0;

        foreach (ItemCategory category in inventory.InventoryItems)
        {
            foreach (ItemSlot itemSlot in category.ItemSlots)
            {
                itemSlotCount++;
            }
        }

        int itemGridLines = Mathf.CeilToInt((float)itemSlotCount / (float)ITEM_SLOTS_PER_LINE);

        for (int i = 0; i < itemGridLines; i++)
        {
            for (int j = 0; j < ITEM_SLOTS_PER_LINE; j++)
            {
                GameObject.Instantiate(itemGridSlotPrefab, inventoryGridUI);
            }
        }

        // resize viewport content window
        RectTransform content = inventoryGridUI.parent.GetComponent<RectTransform>();
        content.sizeDelta = new Vector2(content.sizeDelta.x, ITEM_GRID_TOP_OFFSET + ITEM_GRID_LINE_HEIGHT * itemGridLines);

        // initialize each button
        for (int i = 0; i < inventoryGridUI.childCount; i++)
        {
            InventoryGridSlotButton itemBtn = 
                inventoryGridUI.GetChild(i).GetChild(0).GetComponent<InventoryGridSlotButton>();

            itemBtn.Initialize(this);
        }

        DisplayItemPage(currentPageID);
    }

    public void DisplayItemPage(int pageID)
    {
        currentPageID = pageID;

        ClearInventoryDisplay();
        UpdateUIText();

        int categoryID = pageID - 1;

        if (categoryID < 0)
        {
            DisplayAllItems();
        }
        else
        {
            DisplayItemCategory(categoryID);
        }
    }

    public void EquipItem(string itemStringID)
    {
        equipment.EquipItem(itemStringID);
        DisplayEquipment();
        DisplayItemPage(currentPageID);
    }

    public void UnequipItem(string itemStringID, int equipSlotID)
    {
        equipment.UnequipItem(itemStringID, equipSlotID);
        DisplayEquipment();
        DisplayItemPage(currentPageID);
    }

    public void ShowItemTooltip(Vector3 pos, string itemStringID)
    {
        if (itemStringID != null)
        {
            foreach (var category in database.ItemCategories)
            {
                foreach (var slot in category.ItemSlots)
                {
                    if (slot.Item.ItemStringID.Equals(itemStringID))
                    {
                        SetItemTooltipData(pos, slot.Item, category.CategoryName);
                    }
                }
            }

            itemTooltip.gameObject.SetActive(true);
        }
    }

    public void ShowEquipSlotTooltip(Vector3 pos, int equipSlotID)
    {
        SetEquipSlotTooltipData(pos, equipment.EquipSlots[equipSlotID]);
        itemTooltip.gameObject.SetActive(true);
    }

    public void HideItemTooltip()
    {
        itemTooltip.gameObject.SetActive(false);
    }

    private void SetItemTooltipData(Vector3 pos, Item item, string categoryName)
    {
        tooltipScript.targetPos = pos;
        tooltipHeader.text = item.Name;
        tooltipBody.text = $"[{categoryName.ToLower()}]\n\n";
        tooltipBody.text += item.PropertiesToString();
    }

    private void SetEquipSlotTooltipData(Vector3 pos, EquipmentSlot equipSlot)
    {
        tooltipScript.targetPos = pos;
        tooltipHeader.text = equipSlot.SlotName;
        
        foreach (ItemCategory category in database.ItemCategories)
        {
            if (equipSlot.ItemType.Equals(category.ItemType))
            {
                tooltipBody.text = "Can equip: " + category.CategoryName;
                break;
            }
        }
    }

    private void DisplayAllItems()
    {
        int offset = 0;

        for (int i = 0; i < inventory.InventoryItems.Count; i++)
        {
            if (i > 0)
            {
                offset += inventory.InventoryItems[i-1].ItemSlots.Count;
            }

            DisplayItemCategory(i, offset, true);
        }
    }

    private void DisplayItemCategory (int categoryID, int imageSlotOffset = 0, bool forceDisplay = false)
    {
        // skip category where ShowCategoryTab is false (since there is no tab for it)
        // except when showing all items, or inside a parent tab (forceDisplay=true)
        if (!inventory.InventoryItems[categoryID].ShowCategoryTab && !forceDisplay)
        {
            if (categoryID + 1 < inventory.InventoryItems.Count)
            {
                DisplayItemCategory(categoryID + 1, imageSlotOffset);
            }
            return;
        }

        // display all inventory items in current category
        for (int i = 0; i < inventory.InventoryItems[categoryID].ItemSlots.Count; i++)
        {
            ItemSlot itemSlot = inventory.InventoryItems[categoryID].ItemSlots[i];
            Transform itemImageObj = inventoryGridUI.GetChild(i + imageSlotOffset).GetChild(0);
            Transform itemCountObj = inventoryGridUI.GetChild(i + imageSlotOffset).GetChild(1);

            itemImageObj.gameObject.SetActive(true);
            itemImageObj.GetComponent<Image>().sprite = itemSlot.Item.Sprite;

            itemCountObj.gameObject.SetActive(true);
            itemCountObj.GetComponent<TextMeshProUGUI>().text = itemSlot.Amount.ToString();

            SetInventoryGridSlotButtonData(
                itemImageObj.GetComponent<InventoryGridSlotButton>(), 
                itemSlot    
            );
        }

        // display derived categories in the same tab as the parent category, if they don't have a tab for themselves
        for (int i = 0; i < inventory.InventoryItems.Count; i++)
        {
            ItemCategory category = (ItemCategory)inventory.InventoryItems[i];
            bool isDerived = category.ItemType.IsSubclassOf(inventory.InventoryItems[categoryID].ItemType);

            if (isDerived && !category.ShowCategoryTab)
            {
                DisplayItemCategory(
                    i, 
                    imageSlotOffset + inventory.InventoryItems[categoryID].ItemSlots.Count, 
                    true);
            }
        }
    }

    private void ClearInventoryDisplay()
    {
        foreach(Transform slot in inventoryGridUI)
        {
            slot.GetChild(0).gameObject.SetActive(false);
            slot.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void InitializeEquipmentUI()
    {
        for (int i = 0; i < equipment.EquipSlots.Count; i++)
        {
            GameObject equipSlotObj = GameObject.Instantiate(equipSlotPrefab, equipmentSlotsUI);
            Transform equipSlotUI = equipmentSlotsUI.GetChild(i);

            equipSlotUI.GetChild(0).name = equipment.EquipSlots[i].ItemType.Name;

            equipSlotUI.GetComponent<RectTransform>().anchoredPosition =
                equipment.EquipSlots[i].DeltaPos;

            EquipmentSlotButton equipBtn = equipSlotUI.GetComponent<EquipmentSlotButton>();
            equipBtn.Initialize(this);
        }

        DisplayEquipment();
    }

    private void DisplayEquipment()
    {
        UpdateUIText();
    
        for (int i = 0; i < equipment.EquipSlots.Count; i++)
        {
            EquipmentSlot equipSlot = equipment.EquipSlots[i];
            Transform equipSlotUI = equipmentSlotsUI.GetChild(i);
            Transform equipSlotUIChild = equipmentSlotsUI.GetChild(i).GetChild(0);

            if (equipSlotUIChild.name.Equals(equipSlot.ItemType.Name))
            {
                Image equipSlotImage = equipSlotUIChild.GetComponent<Image>();
                
                if (equipSlot.Item != null)
                {
                    equipSlotImage.sprite = equipSlot.Item.Sprite;
                    equipSlotImage.color = Color.white;
                    SetEquipSlotButtonData(
                        equipSlotUI.GetComponent<EquipmentSlotButton>(),
                        equipSlot,
                        i
                    );
                }
                else
                {
                    bool itemCategoryFound = false;
                    foreach (ItemCategory itemCategory in database.ItemCategories)
                    {
                        if (equipSlotUIChild.name.Equals(itemCategory.ItemType.Name))
                        {
                            equipSlotImage.sprite = itemCategory.ItemSlots[0].Item.Sprite;
                            equipSlotImage.color = new Color(0, 0, 0, 0.5f);
                            SetEquipSlotButtonData(
                                equipSlotUI.GetComponent<EquipmentSlotButton>(),
                                equipSlot,
                                i,
                                true
                            );
                            itemCategoryFound = true;
                            break;
                        }   
                    }
                    if (!itemCategoryFound)
                    {
                        throw new NullReferenceException($"Item Category of name {equipSlotUIChild.name} could not be found!");
                    }  
                }
            }
        }
    }

    private void SetInventoryGridSlotButtonData(InventoryGridSlotButton button, ItemSlot itemSlot)
    {
        button.SetData(itemSlot.Item.ItemStringID);
    }

    private void SetEquipSlotButtonData(EquipmentSlotButton button, EquipmentSlot equipSlot, int equipSlotID = -1, bool unequipItem = false)
    {
        if (!unequipItem)
        {
            button.SetData(equipSlot.Item.ItemStringID, equipSlotID);
        }
        else
        {
            button.SetData(null, equipSlotID);
        }
    }

    private void UpdateUIText()
    {
        string currentWeight = inventory.CurrentWeight.ToString("F2", CultureInfo.InvariantCulture);
        string weightCapacity;
            
        if (inventory.LimitedByWeight)
        {
            weightCapacity = inventory.WeightCapacity.ToString("F2", CultureInfo.InvariantCulture);
        }
        else
        {
            weightCapacity = INFINITY_SYMBOL;
        }

        weightText.text = $"weight: {currentWeight} / {weightCapacity} kg";
        goldText.text = $"gold: {inventory.Money}";
    }

    private void PopulateDemoInventory()
    {
        inventory.Money += 999999;

        foreach (ItemCategory category in database.ItemCategories)
        {
            foreach (ItemSlot slot in category.ItemSlots)
            {
                inventory.AddItemToInventoryByName(slot.Item.Name, slot.Item.StackLimit * startingStacksInDemo);
            }
        }
    }
}
