using System;
using System.Collections;
using System.Collections.Generic;
using ModdableInventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ItemTests
    {
        [Test]
        public void Item_NegativeCost_Exception()
        {
            var item = new Item();
            Dictionary<string, string> itemData = new Dictionary<string, string>();

            itemData.Add("cost", "-1");

            Assert.Throws<ArgumentOutOfRangeException>(() => item.Initialize(itemData));
        }

        [Test]
        public void Item_NegativeStackLimit_Exception()
        {
            var item = new Item();
            Dictionary<string, string> itemData = new Dictionary<string, string>();

            itemData.Add("stackLimit", "-1");

            Assert.Throws<ArgumentOutOfRangeException>(() => item.Initialize(itemData));
        }

        [Test]
        public void Item_InvalidPropertyFormat_Exception()
        {
            var item = new Item();
            Dictionary<string, string> itemData = new Dictionary<string, string>();

            itemData.Add("cost", "5f");

            Assert.Throws<FormatException>(() => item.Initialize(itemData));
        }
    }
}