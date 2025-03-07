﻿using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class StuffTests
{
    private void RandomBool(bool response, int attempts = 1000, double rMin = .45, double rMax = .55)
    {
        var hits = 0;
        for (var z = 0; z < attempts; ++z)
        {
            hits += Stuff.Random.NextBoolean() == response ? 1 : 0;
        }
        var r = hits / (0.0 + attempts);
        Assert.IsTrue(r >= rMin);
        Assert.IsTrue(r <= rMax);
    }

    [TestMethod]
    public void RandomBoolTrue()
        => RandomBool(true);

    [TestMethod]
    public void RandomBoolFalse()
        => RandomBool(false);

    [TestMethod]
    public void SwapTests()
    {
        int a = 1, b = 2;
        Stuff.Swap(ref a, ref b);
        Assert.AreEqual(2, a);
        Assert.AreEqual(1, b);
    }

    [TestMethod]
    public void CoalesceStringsTests()
    {
        Assert.IsNull(StringHelpers.Coalesce());
        Assert.IsNull(StringHelpers.Coalesce(""));
        Assert.AreEqual("a", StringHelpers.Coalesce("a"));
        Assert.AreEqual("a", StringHelpers.Coalesce("a", "b"));
        Assert.AreEqual("b", StringHelpers.Coalesce(null, "b"));
        Assert.AreEqual("b", StringHelpers.Coalesce("", "b"));
    }

    [TestMethod]
    public void MinTests()
    {
        Assert.AreEqual(5, Stuff.Min(12, 5));
        Assert.AreEqual(5, Stuff.Min(5, 12));
    }

    [TestMethod]
    public void MaxTests()
    {
        Assert.AreEqual(12, Stuff.Max(12, 5));
        Assert.AreEqual(12, Stuff.Max(5, 12));
    }

    [TestMethod]
    public void FileDeleteWhileClosedTest()
    {
        var fileNames = new List<string>();
        for (var z = 0; z < 10; ++z)
        {
            var fn = Path.GetTempFileName();
            fileNames.Add(fn);
        }
        for (var z = 0; z < 10; ++z)
        {
            var fn = FileSystemHelpers.GetTempFileName(".dat");
            fileNames.Add(fn);
        }
        foreach (var fn in fileNames)
        {
            Assert.IsTrue(File.Exists(fn));
        }
        FileSystemHelpers.FileTryDelete(fileNames);
        foreach (var fn in fileNames)
        {
            Assert.IsFalse(File.Exists(fn));
        }
    }

    [TestMethod]
    public void FileDeleteSkipWhileOpenTest()
    {
        var fn = FileSystemHelpers.GetTempFileName(".dat");
        using (var st = File.OpenRead(fn))
        {
            Assert.IsTrue(File.Exists(fn));
            FileSystemHelpers.FileTryDelete(fn);
            Assert.IsTrue(File.Exists(fn));
        }
        Assert.IsTrue(File.Exists(fn));
        FileSystemHelpers.FileTryDelete(fn);
        Assert.IsFalse(File.Exists(fn));
    }
}
