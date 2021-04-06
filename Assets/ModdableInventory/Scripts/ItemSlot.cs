using UnityEngine;
using System;

namespace ModdableInventory
{
    /// <summary>
    /// Holds an amount of a specific Item.
    /// </summary>
    public class ItemSlot
    {
        private int amount = 1;

        public ItemSlot(Item item, int amount = 1)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (amount <= 0)
                throw new ArgumentOutOfRangeException("amount", "must be greater than zero");

            Item = item;
            Amount = amount;
        }

        public Item Item { get; }
        public int Amount 
        { 
            get { return Mathf.Clamp(amount, 1, Item.StackLimit);} 
            set { amount = value; }
        }
        public float Weight => Item.Weight * Amount;
        public bool IsFull => Amount == Item.StackLimit;
    }
}

