using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModdableInventory.Utils;

namespace ModdableInventory
{
    /// <summary>
    /// A named list of ItemSlots that contain Items of the same ItemType.
    /// </summary>
    public class ItemCategory
    {
        private List<ItemSlot> itemSlots = new List<ItemSlot>();

        public ItemCategory(string categoryName, Type itemType)
        {
            if (!ClassUtils.IsSameOrSubclass(typeof(Item), itemType))
                throw new InvalidCastException("itemType is not a type of Item");

            CategoryName = categoryName;
            ItemType = itemType;
        }

        public string CategoryName { get; }
        public Type ItemType { get; }
        public ReadOnlyCollection<ItemSlot> ItemSlots => itemSlots.AsReadOnly();

        public void AddItemSlot(ItemSlot itemSlot)
        {
            if (!ClassUtils.IsSameOrSubclass(ItemType, itemSlot.Item.GetType()))
            {
                throw new InvalidCastException($"invalid item type, this ItemCategory only accepts items of type {ItemType.FullName} or derived");
            }

            itemSlots.Add(itemSlot);
        }

        public void RemoveItemSlot(ItemSlot itemSlot) => itemSlots.Remove(itemSlot);
    }
}