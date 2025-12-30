using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class LinqHelpersTests
{
    public class Zaff
    {
        public string FieldA { get; set; }
        public int EnumVal { get; set; }

        public override string ToString() => $"{FieldA}-{EnumVal}";

        public Zaff(string a, Letters b)
        {
            FieldA = a;
            EnumVal = (int)b;
        }
    }

    public enum Letters
    {
        A = 1,
        B = 2,
        [Display(Order = 1)]
        C = 3,
        D = 4
    }

    [TestMethod]
    public void OrderByFieldWithValueMapping_EnumNoVals()
    {
        var items = new List<Zaff>
        { }.AsQueryable();

        var sorted = items.OrderByField(nameof(Zaff.EnumVal), typeof(Letters), true).ToList();
        Assert.HasCount(0, sorted);
    }

    [TestMethod]
    public void OrderByFieldWithValueMapping_EnumOneVal()
    {
        var items = new List<Zaff>
        {
            new("this is a", Letters.A),
        }.AsQueryable();

        var sorted = items.OrderByField(nameof(Zaff.EnumVal), typeof(Letters), true).ToList();
        Assert.HasCount(1, sorted);
        Assert.AreEqual(Letters.A, (Letters)sorted.First().EnumVal);
    }

    [TestMethod]
    public void OrderByFieldWithValueMapping_Enum()
    {
        var items = new List<Zaff>
        {
            new("this is a", Letters.A),
            new("this is c", Letters.C),
            new("this is b", Letters.B),
            new("this is d", Letters.D),
            new("this is B", Letters.B),
        }.AsQueryable();

        var sorted = items.OrderByField(nameof(Zaff.EnumVal), typeof(Letters), true).ToList();
        Assert.AreEqual(Letters.C, (Letters)sorted.First().EnumVal);
        Assert.AreEqual(Letters.D, (Letters)sorted.Last().EnumVal);

        sorted = items.OrderByField(nameof(Zaff.EnumVal), typeof(Letters), false).ToList();
        Assert.AreEqual(Letters.C, (Letters)sorted.Last().EnumVal);
        Assert.AreEqual(Letters.D, (Letters)sorted.First().EnumVal);
    }

    [TestMethod]
    public void OrderByFieldWithValueMapping_Dict()
    {
        var items = new List<Zaff>
        {
            new("this is a", Letters.A),
            new("this is c", Letters.C),
            new("this is b", Letters.B),
            new("this is d", Letters.D),
            new("this is B", Letters.B),
        }.AsQueryable();

        var map = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "canary" }, { 4, "d" } };

        var sorted = items.OrderByField(
            nameof(Zaff.EnumVal),
            map,
            true).ToList();
        Assert.AreEqual(Letters.A, (Letters)sorted.First().EnumVal);
        Assert.AreEqual(Letters.D, (Letters)sorted.Last().EnumVal);

        sorted = items.OrderByField(
            nameof(Zaff.EnumVal),
            map,
            false).ToList();
        Assert.AreEqual(Letters.A, (Letters)sorted.Last().EnumVal);
        Assert.AreEqual(Letters.D, (Letters)sorted.First().EnumVal);
    }
}
