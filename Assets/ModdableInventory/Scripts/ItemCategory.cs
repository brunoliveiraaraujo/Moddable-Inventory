using System.Collections.Generic;

namespace ModdableInventory
{
    public class ItemCategory
    {
        public ItemCategory(string itemTypeName, string categoryName, List<ItemSlot> slots)
        {
            ItemTypeName = itemTypeName;
            CategoryName = categoryName;
            ItemSlots = slots;
        }

        public string ItemTypeName { get; }
        public string CategoryName { get; }
        public List<ItemSlot> ItemSlots { get; set; }
    }
}