using UnityEngine;

namespace ModdableInventory
{
    public class ItemSlot
    {
        private ItemType item;
        private int amount;

        public ItemSlot(ItemType item)
        {
            this.item = item;
            this.amount = 1;
        }

        public ItemSlot(ItemType item, int amount = 1)
        {
            this.item = item;
            this.amount = amount;
        }

        public ItemType Item => item;
        public int Amount => Mathf.Clamp(amount, 1, item.StackLimit); 
        public float Weight => item.Weight * Amount;
        public bool IsFull => Amount == item.StackLimit;
        
        public void SetAmount (int value) { amount = value; }
    }
}

