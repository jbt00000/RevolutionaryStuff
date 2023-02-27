using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class DateHelpersTests
{
    [TestMethod]
    public void TestAgeOffsetToday()
    {
        var today = DateTime.Today;
        const int expected = 45;

        var dt45 = today.AddYears(-expected);
        Assert.AreEqual(today.Year - expected, dt45.Year);
        Assert.AreEqual(today.Month, dt45.Month);
        Assert.AreEqual(today.Day, dt45.Day);
        Assert.AreEqual(expected, dt45.Age());
        Assert.AreEqual(expected, dt45.AddDays(-1).Age());
        Assert.AreEqual(expected, dt45.AddMonths(-1).Age());
        Assert.AreEqual(expected - 1, dt45.AddDays(1).Age());
        Assert.AreEqual(expected - 1, dt45.AddMonths(1).Age());
        Assert.AreEqual(expected - 1, dt45.AddMonths(1).AddDays(1+today.Day).Age());
    }

    [TestMethod]
    public void DateTimeToIsoStringTests()
    {
        var dt = DateTime.Now;
        var s = dt.ToIsoString();
        var dtViaRsllcParse = Parse.ParseNullableDateTime(s);
        Assert.IsNotNull(dtViaRsllcParse);
        Assert.AreEqual(dt, dtViaRsllcParse);
        var dtViaDotNetParse = DateTime.Parse(s);
        Assert.AreEqual(dt, dtViaDotNetParse);
    }

    [TestMethod]
    public void DateTimeOffsetToIsoStringTests()
    {
        var dto = DateTimeOffset.Now;
        var s = dto.ToIsoString();
        var dtoViaRsllcParse = Parse.ParseNullableDateTimeOffset(s);
        Assert.IsNotNull(dtoViaRsllcParse);
        Assert.AreEqual(dto, dtoViaRsllcParse);
        var dtoViaDotNetParse = DateTimeOffset.Parse(s);
        Assert.AreEqual(dto, dtoViaDotNetParse);
    }

    [TestMethod]
    public void DateTimeOffsetToIsoStringWithLegacyZTests()
    {
        var dto = DateTimeOffset.Now;
        var s = dto.ToIsoString();
        if (!s.EndsWith("Z"))
        {
            s += "Z";
        }
        var dtoViaRsllcParse = Parse.ParseNullableDateTimeOffset(s);
        Assert.IsNotNull(dtoViaRsllcParse);
        Assert.AreEqual(dto, dtoViaRsllcParse);
        try
        {
            var dtoViaDotNetParse = DateTimeOffset.Parse(s);
            Assert.AreEqual(dto, dtoViaDotNetParse);
        }
        catch (System.FormatException fex)
        { 
            //because this was BUSTED to add in the "Z"
        }
    }
}
