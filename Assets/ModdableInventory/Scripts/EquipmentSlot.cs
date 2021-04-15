using System;
using ModdableInventory.Utils;
using UnityEngine;

namespace ModdableInventory
{
    /// <summary>
    /// A named slot that can have 0 or 1 item of a specific Type.
    /// </summary>
    public class EquipmentSlot
    {
        private Item item = null;

        public EquipmentSlot(string slotName, Type itemType, Vector2 deltaPos)
        {
            if (!ClassUtils.IsSameOrSubclass(typeof(Item), itemType))
                throw new InvalidCastException("itemType is not a type of Item");

            SlotName = slotName;
            ItemType = itemType;
            Item = null;
            DeltaPos = deltaPos;
        }

        public string SlotName { get; }
        public Type ItemType { get; }
        public Item Item 
        { 
            get
            {
                return item;
            }
            set
            {
                if (value != null && !ClassUtils.IsSameOrSubclass(ItemType, value.GetType()))
                {
                    throw new InvalidCastException($"invalid item type, this EquipmentSlot only accepts items of type {ItemType.FullName} or derived, or null");
                }

                item = value;
            } 
        }
        public Vector2 DeltaPos { get; }
    }
}

