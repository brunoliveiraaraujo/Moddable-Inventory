using System.Collections.Generic;

namespace ModdableInventory
{
    public class ItemCategory
    {
        public ItemCategory(string typeName, string categoryName, List<ItemSlot> slots)
        {
            TypeName = typeName;
            CategoryName = categoryName;
            ItemSlots = slots;
        }

        public string TypeName { get; }
        public string CategoryName { get; }
        public List<ItemSlot> ItemSlots { get; set; }
    }
}