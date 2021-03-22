using UnityEngine;

namespace ModdableInventory
{
    public class InventorySlot
    {
        private Item item;
        private int amount;

        public InventorySlot(Item item)
        {
            this.item = item;
            this.amount = 1;
        }

        public InventorySlot(Item item, int amount = 1)
        {
            this.item = item;
            this.amount = amount;
        }

        public Item Item => item;
        public int Amount => Mathf.Clamp(amount, 1, item.StackLimit); 
        public float Weight => item.Weight * Amount;
        public bool IsFull => Amount == item.StackLimit;
        
        public void SetAmount (int value) { amount = value; }
    }
}

