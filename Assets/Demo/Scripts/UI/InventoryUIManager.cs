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
    private const string infinitySymbol = "\u221e";

    [SerializeField] private Transform inventoryGridUI;
    [SerializeField] private Transform equipmentSlotsUI;
    [SerializeField] private Transform itemTooltip;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TabGroup inventoryTabs;

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

    private void Start() 
    {
        for (int i = 0; i < database.ItemCategories.Count; i++)
        {
            inventoryTabs.ChangeTabText(i+1, database.ItemCategories[i].CategoryName.ToLower());
        }
    }

    private void OnInventoryInitialized(object sender, EventArgs e)
    {
        inventory.InventoryInitialized -= OnInventoryInitialized;

        PopulateDemoInventory();
        DisplayItemPage(currentPageID);
    }

    private void OnEquipmentInitialized(object sender, EventArgs e)
    {
        equipment.EquipmentInitialized -= OnEquipmentInitialized;

        DisplayEquipment();
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

    public void EquipItem(UniqueStringID stringID)
    {
        equipment.EquipItem(stringID);
        DisplayEquipment();
        DisplayItemPage(currentPageID);
    }

    public void UnequipItem(UniqueStringID stringID, int equipSlotID)
    {
        equipment.UnequipItem(stringID, equipSlotID);
        DisplayEquipment();
        DisplayItemPage(currentPageID);
    }

    public void ShowItemTooltip(Vector3 pos, UniqueStringID stringID)
    {
        if (stringID != null)
        {
            foreach (var category in database.ItemCategories)
            {
                foreach (var slot in category.ItemSlots)
                {
                    if (slot.Item.UniqueID.Equals(stringID))
                    {
                        SetTooltipData(pos, slot.Item);
                    }
                }
            }

            itemTooltip.gameObject.SetActive(true);
        }  
    }

    public void HideItemTooltip()
    {
        itemTooltip.gameObject.SetActive(false);
    }

    private void SetTooltipData(Vector3 pos, ItemType item)
    {
        tooltipScript.targetPos = pos;
        tooltipHeader.text = item.Name;
        tooltipBody.text = item.PropertiesToString();
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

            DisplayItemCategory(i, offset);
        }
    }

    private void DisplayItemCategory (int categoryID, int imageSlotOffset = 0)
    {
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
    }

    private void ClearInventoryDisplay()
    {
        foreach(Transform slot in inventoryGridUI)
        {
            slot.GetChild(0).gameObject.SetActive(false);
            slot.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void DisplayEquipment()
    {
        UpdateUIText();
    
        for (int i = 0; i < equipment.EquippedItems.Count; i++)
        {
            EquipmentSlot equipSlot = equipment.EquippedItems[i];
            Transform equipImageSlot = equipmentSlotsUI.GetChild(i).GetChild(0);

            if (equipImageSlot.name.Equals(equipSlot.ItemTypeName))
            {
                Image equipSlotImage = equipImageSlot.GetComponent<Image>();
                
                if (equipSlot.Item != null)
                {
                    equipSlotImage.sprite = equipSlot.Item.Sprite;
                    equipSlotImage.color = Color.white;
                    SetEquipSlotButtonData(
                        equipSlotImage.GetComponent<EquipmentSlotButton>(),
                        equipSlot,
                        i
                    );
                }
                else
                {
                    bool itemCategoryFound = false;
                    foreach (ItemCategory itemCategory in database.ItemCategories)
                    {
                        if (equipImageSlot.name.Equals(itemCategory.ItemTypeName))
                        {
                            equipSlotImage.sprite = itemCategory.ItemSlots[0].Item.Sprite;
                            equipSlotImage.color = new Color(0, 0, 0, 0.5f);
                            SetEquipSlotButtonData(
                                equipSlotImage.GetComponent<EquipmentSlotButton>(),
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
                        throw new NullReferenceException($"Item Category of name {equipImageSlot.name} could not be found!");
                    }  
                }
            }
        }
    }

    private void SetInventoryGridSlotButtonData(InventoryGridSlotButton button, ItemSlot itemSlot)
    {
        button.SetData(itemSlot.Item.UniqueID);
    }

    private void SetEquipSlotButtonData(EquipmentSlotButton button, EquipmentSlot equipSlot, int equipSlotID = -1, bool unequipItem = false)
    {
        if (!unequipItem)
        {
            button.SetData(equipSlot.Item.UniqueID, equipSlotID);
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
            weightCapacity = infinitySymbol;
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
                inventory.AddItemToInventoryByName(slot.Item.Name, slot.Item.StackLimit * 2);
            }
        }
    }
}
