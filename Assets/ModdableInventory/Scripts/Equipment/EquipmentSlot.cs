using System;
using ModdableInventory.Items;
using UnityEngine;

namespace ModdableInventory
{
    public class EquipmentSlot
    {
        private string slotName;
        private string typeName;

        public EquipmentSlot(string slotName, string typeName)
        {
            this.slotName = slotName;
            this.typeName = typeName;
        }

        public string SlotName => slotName;
        public string TypeName => typeName;
        public Item Item { get; set; } = null;
        
    }
}

