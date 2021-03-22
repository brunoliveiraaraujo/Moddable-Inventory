using System.Collections.Generic;

namespace ModdableInventory
{
    public class ItemCategory
    {
        public ItemCategory(string typeName, string categoryName, List<InventorySlot> slots)
        {
            TypeName = typeName;
            CategoryName = categoryName;
            ItemSlots = slots;
        }

        public string TypeName { get; }
        public string CategoryName { get; }
        public List<InventorySlot> ItemSlots { get; set; }
    }
}