using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class LinqHelpersSimpleTest
{
    public class TestItem
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    [TestMethod]
    public void SimpleOrderByField_Works()
    {
        var items = new List<TestItem>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 },
            new() { Name = "Bob", Value = 2 }
        }.AsQueryable();

        var sorted = items.OrderByField(nameof(TestItem.Name)).ToList();

        Assert.AreEqual("Alice", sorted[0].Name);
        Assert.AreEqual("Bob", sorted[1].Name);
        Assert.AreEqual("Charlie", sorted[2].Name);
    }

    [TestMethod]
    public void MappedOrderByField_Test()
    {
        var items = new List<TestItem>
        {
            new() { Name = "Charlie", Value = 3 },
            new() { Name = "Alice", Value = 1 },
            new() { Name = "Bob", Value = 2 }
        }.AsQueryable();

        var map = new Dictionary<int, int> { { 1, 100 }, { 2, 50 }, { 3, 75 } };

        var sorted = items.OrderByField(nameof(TestItem.Value), map).ToList();

        Assert.AreEqual(2, sorted[0].Value); // maps to 50
        Assert.AreEqual(3, sorted[1].Value); // maps to 75  
        Assert.AreEqual(1, sorted[2].Value); // maps to 100
    }

    [TestMethod]
    public void MappedOrderByField_SingleMapping()
    {
        var items = new List<TestItem>
        {
            new() { Name = "Charlie", Value = 1 }
        }.AsQueryable();

        var map = new Dictionary<int, int> { { 1, 100 } };

        var sorted = items.OrderByField(nameof(TestItem.Value), map).ToList();

        Assert.AreEqual(1, sorted[0].Value);
    }
}
