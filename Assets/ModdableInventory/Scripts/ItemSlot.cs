using UnityEngine;

namespace ModdableInventory
{
    public class ItemSlot
    {
        private Item item;
        private int amount;

        public ItemSlot(Item item)
        {
            this.item = item;
            this.amount = 1;
        }

        public ItemSlot(Item item, int amount = 1)
        {
            this.item = item;
            this.amount = amount;
        }

        public Item Item { get => item; }
        public int Amount 
        { 
            get => Mathf.Clamp(amount, 1, item.StackLimit); 
            set { amount = value; }
        }
        public float Weight { get => item.Weight * Amount; }

        public bool IsFull()
        {
            return Amount == item.StackLimit;
        }
    }
}

