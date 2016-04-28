using System.Collections.Generic;
using System.Linq;
using GildedRose.Console;
using Xunit;

namespace GildedRose.Tests
{
    public class TestAssemblyTests
    {
        public IList<Item> GenerateInventory(bool includeVintageItems = true, bool includeLegendaryItems = true, bool includeBackstagePasses = true, bool includeConjuredItems = true)
        {
            IList<Item> inventory = GenerateRegularItems();
            if (includeVintageItems)
            {
                inventory.Concat(GenerateVintageItems());
            }

            if (includeLegendaryItems)
            {
                inventory.Concat(GenerateLegendaryItems());
            }

            if (includeBackstagePasses)
            {
                inventory.Concat(GenerateBackstagePassItems());
            }

            if (includeConjuredItems)
            {
                inventory.Concat(GenerateConjuredItems());
            }

            return inventory;
        }

        public IList<Item> GenerateRegularItems()
        {
            return new List<Item>
                {
                    new Item { Name = "+5 Dexterity Vest", SellIn = 10, Quality = 20 },
                    new Item { Name = "Elixir of the Mongoose", SellIn = 5, Quality = 7 },
                    new Item { Name = "Some Random Garbage Item", SellIn = 15, Quality = 4 },
                };
        }

        public IList<Item> GenerateVintageItems()
        {
            return new List<Item>
                {
                    new Item { Name = "Aged Brie", SellIn = 2, Quality = 0 },
                };
        }

        public IList<Item> GenerateLegendaryItems()
        {
            return new List<Item>
                {
                    new Item { Name = "Sulfuras, Hand of Ragnaros", SellIn = 0, Quality = 80 },
                };
        }

        public IList<Item> GenerateBackstagePassItems()
        {
            return new List<Item>
                {
                     new Item { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 15, Quality = 20 },
                };
        }

        public IList<Item> GenerateConjuredItems()
        {
            return new List<Item>
                {
                    new Item { Name = "Conjured Mana Cake", SellIn = 3, Quality = 6 }
                };
        }

        [Fact]
        public void TestTheTruth()
        {
            Assert.True(true);
        }

        //- At the end of each day our system lowers both values for every item
        [Fact]
        public void TestQualityDecreasesNormallyBeforeSellBy()
        {
            IList<Item> initialSet = GenerateRegularItems();
            IList<Item> updatedSet = GenerateRegularItems();

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality - 1, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        [Fact]
        public void TestSellInDecreasesNormally()
        {
            IList<Item> initialSet = GenerateInventory(true, false);
            IList<Item> updatedSet = GenerateInventory(true, false);

            // Ensure SellIn is positive
            initialSet.ToList().ForEach(i => i.SellIn = 5);
            updatedSet.ToList().ForEach(i => i.SellIn = 5);

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        //- Once the sell by date has passed, Quality degrades twice as fast
        [Fact]
        public void TestQualityDecreasesTwiceAsFastAfterSellBy()
        {
            IList<Item> initialSet = GenerateRegularItems();
            IList<Item> updatedSet = GenerateRegularItems();

            initialSet.ToList().ForEach(i => i.SellIn = 0);
            updatedSet.ToList().ForEach(i => i.SellIn = 0);

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality - 2, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        //- The Quality of an item is never negative
        [Fact]
        public void TestQualityNeverNegative()
        {
            IList<Item> updatedSet = GenerateRegularItems();

            for (int i = 0; i < 50; i++)
            {
                new Program().UpdateQuality(updatedSet);
            }

            foreach (Item item in updatedSet)
            {
                Assert.True(item.Quality >= 0);
            }
        }

        //- "Aged Brie" actually increases in Quality the older it gets
        [Fact]
        public void TestQualityIncreasesForAgedBrie()
        {
            IList<Item> initialSet = GenerateVintageItems();
            IList<Item> updatedSet = GenerateVintageItems();
            
            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.True(initialItem.Quality < matchingUpdatedItem.Quality);
            }
        }

        //- The Quality of an item is never more than 50
        [Fact]
        public void TestQualityMaxedAt50()
        {
            IList<Item> updatedSet = GenerateVintageItems();

            for (int i = 0; i < 60; i++)
            {
                new Program().UpdateQuality(updatedSet);
            }

            foreach (Item updatedItem in updatedSet)
            {
                Assert.True(updatedItem.Quality <= 50);
            }
        }

        //- "Sulfuras", being a legendary item, never has to be sold or decreases in Quality
        //  "Sulfuras" is a legendary item and as such its Quality is 80 and it never alters.
        [Fact]
        public void TestSulfurasNeverAges()
        {
            IList<Item> initialSet = GenerateLegendaryItems();
            IList<Item> updatedSet = GenerateLegendaryItems();

            for (int i = 0; i < 30; i++)
            {
                new Program().UpdateQuality(updatedSet);
            }

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality, matchingUpdatedItem.Quality);
            }
        }

        //- "Backstage passes", like aged brie, increases in Quality as it's SellIn value approaches; Quality increases by 2 when there are 10 days or less and by 3 when there are 5 days or less but Quality drops to 0 after the concert
        [Fact]
        public void TestBackstagePassesIncreaseCorrectlyMoreThan10Days()
        {
            IList<Item> initialSet = GenerateBackstagePassItems();
            IList<Item> updatedSet = GenerateBackstagePassItems();

            initialSet.ToList().ForEach(i => i.SellIn = 11);
            updatedSet.ToList().ForEach(i => i.SellIn = 11);

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality + 1, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        [Fact]
        public void TestBackstagePassesIncreaseCorrectlyBetween10And5Days()
        {
            IList<Item> initialSet = GenerateBackstagePassItems();
            IList<Item> updatedSet = GenerateBackstagePassItems();

            initialSet.ToList().ForEach(i => i.SellIn = 7);
            updatedSet.ToList().ForEach(i => i.SellIn = 7);

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality + 2, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        [Fact]
        public void TestBackstagePassesIncreaseCorrectlyLessThan5Days()
        {
            IList<Item> initialSet = GenerateBackstagePassItems();
            IList<Item> updatedSet = GenerateBackstagePassItems();

            initialSet.ToList().ForEach(i => i.SellIn = 3);
            updatedSet.ToList().ForEach(i => i.SellIn = 3);

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality + 3, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        [Fact]
        public void TestBackstagePassesIncreaseCorrectlyAfterSellBy()
        {
            IList<Item> initialSet = GenerateBackstagePassItems();
            IList<Item> updatedSet = GenerateBackstagePassItems();

            initialSet.ToList().ForEach(i => i.SellIn = 0);
            updatedSet.ToList().ForEach(i => i.SellIn = 0);

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(0, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }

        //NEW - "Conjured" items degrade in Quality twice as fast as normal items
        [Fact]
        public void TestQualityForConjuredItems()
        {
            IList<Item> initialSet = GenerateConjuredItems();
            IList<Item> updatedSet = GenerateConjuredItems();

            new Program().UpdateQuality(updatedSet);

            foreach (Item initialItem in initialSet)
            {
                Item matchingUpdatedItem = updatedSet.FirstOrDefault(ui => ui.Name == initialItem.Name);
                Assert.NotNull(matchingUpdatedItem);
                Assert.Equal(initialItem.Quality - 2, matchingUpdatedItem.Quality);
                Assert.Equal(initialItem.SellIn - 1, matchingUpdatedItem.SellIn);
            }
        }
    }
}