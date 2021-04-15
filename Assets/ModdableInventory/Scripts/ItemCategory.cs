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

        public ItemCategory(string categoryName, bool showCategoryTab, Type itemType)
        {
            if (!ClassUtils.IsSameOrSubclass(typeof(Item), itemType))
                throw new InvalidCastException("itemType is not a type of Item");

            CategoryName = categoryName;
            ShowCategoryTab = showCategoryTab;
            ItemType = itemType;
        }

        public string CategoryName { get; }
        public bool ShowCategoryTab { get; }
        public Type ItemType { get; }
        public ReadOnlyCollection<ItemSlot> ItemSlots => itemSlots.AsReadOnly();

        /// <summary>
        /// The ItemType of the new ItemSlot must be the type of or derived of the same type defined in this ItemCategory.
        /// </summary>
        /// <remarks>
        /// example: if this ItemCategory accepts items of type "Armor", the ItemSlot has to be of type "Armor" or a subtype of "Armor", "HeavyArmor" for example.
        /// </remarks>
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