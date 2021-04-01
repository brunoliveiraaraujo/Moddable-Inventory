using System;
using UnityEngine;

namespace ModdableInventory
{
    public class EquipmentSlot
    {
        private string slotName;
        private string itemTypeName;

        public EquipmentSlot(string slotName, string itemTypeName)
        {
            this.slotName = slotName;
            this.itemTypeName = itemTypeName;
        }

        public string SlotName => slotName;
        public string ItemTypeName => itemTypeName;
        public Item Item { get; set; } = null;
        
    }
}

