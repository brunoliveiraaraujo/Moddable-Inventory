using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModdableInventory
{
    [RequireComponent(typeof(Equipment))]
    [RequireComponent(typeof(InventoryLogger))]
    public class EquipmentLogger : MonoBehaviour
    {
        // TODO: UI: remove this, use a demo scene instead
        [Min(0)][SerializeField] private bool testExampleEquipment = true;

         // TODO: UI: remove this, use a demo scene instead
        private bool isEquipmentInitialized = false;
        private bool isInventoryPopulated = false;
        private bool isDoneLogging = false;

        private Equipment equipment;
        private InventoryLogger inventoryLogger;

        private void Awake()
        {
            equipment = GetComponent<Equipment>();
            inventoryLogger = GetComponent<InventoryLogger>();

             // TODO: UI: remove this, use a demo scene instead
            equipment.EquipmentInitialized += SetIsEquipmentInitialized;
            inventoryLogger.InventoryPopulated += SetIsInventoryPopulated;
        }

        // TODO: UI: remove this, use a demo scene instead
        private void Update() 
        {
            if (isEquipmentInitialized && isInventoryPopulated && !isDoneLogging)
            {
                InitializeEquipmentLogger();
            }
        }

        private void SetIsEquipmentInitialized(object sender, EventArgs e) 
        {
            equipment.EquipmentInitialized -= SetIsEquipmentInitialized;
            isEquipmentInitialized = true;
        } 

        private void SetIsInventoryPopulated(object sender, EventArgs e)
        {
            inventoryLogger.InventoryPopulated -= SetIsInventoryPopulated;
            isInventoryPopulated = true;
        }

        // TODO: UI: remove this, use a demo scene instead
        private void InitializeEquipmentLogger()
        {
            Debug.Log("######## EQUIPMENT LOG START ########");

            if (testExampleEquipment) Test_EquipItems();

            LogEquippedItems();
            inventoryLogger.LogInventory();

            isDoneLogging = true;
        }

        public void LogEquippedItems()
        {
            Debug.Log($"#### Equipment ####");
            foreach (var slot in equipment.EquippedItems)
            {
                Debug.Log($"[{slot.SlotName} ({slot.TypeName})]");
                if (slot.Item != null)
                {
                    Debug.Log($"    {slot.Item.Name}");
                }
                else
                {
                    Debug.Log($"    <empty>");
                }
            }
        }

        private void Test_EquipItems()
        {
            Debug.Log("===== Book of Knowledge Equipped =====");
            equipment.EquipItem("book of knowledge");

            Debug.Log("===== Short Sword Equipped =====");
            equipment.EquipItem("shortsword");

            LogEquippedItems();
            inventoryLogger.LogInventory();

            Debug.Log("===== Short Sword Unequiped =====");
            equipment.UnequipItem("shortsword");
        }
    }
}