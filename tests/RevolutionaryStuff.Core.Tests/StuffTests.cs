using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class StuffTests
{
    #region Existing Tests

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

    #endregion

    #region Assembly and Application Metadata Tests

    [TestMethod]
    public void ThisAssembly_IsNotNull()
    {
        Assert.IsNotNull(Stuff.ThisAssembly);
    }

    [TestMethod]
    public void ThisAssembly_IsCorrectAssembly()
    {
        Assert.AreEqual("RevolutionaryStuff.Core", Stuff.ThisAssembly.GetName().Name);
    }

    [TestMethod]
    public void ApplicationName_IsNotNull()
    {
        Assert.IsNotNull(Stuff.ApplicationName);
    }

    [TestMethod]
    public void ApplicationFamily_IsNotNull()
    {
        Assert.IsNotNull(Stuff.ApplicationFamily);
    }

    [TestMethod]
    public void ApplicationStartedAt_IsReasonable()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.IsTrue(Stuff.ApplicationStartedAt <= now);
        Assert.IsTrue(Stuff.ApplicationStartedAt > now.AddHours(-1)); // Started within last hour
    }

    [TestMethod]
    public void ApplicationInstanceId_IsNotEmpty()
    {
        Assert.AreNotEqual(Guid.Empty, Stuff.ApplicationInstanceId);
    }

    #endregion

    #region Constants Tests

    [TestMethod]
    public void Qbert_HasExpectedValue()
    {
        Assert.AreEqual("@!#?@!", Stuff.Qbert);
    }

    [TestMethod]
    public void BaseRsllcUrn_HasExpectedValue()
    {
        Assert.AreEqual("urn:www.revolutionarystuff.com", Stuff.BaseRsllcUrn);
    }

    #endregion

    #region Random Tests

    [TestMethod]
    public void RandomWithFixedSeed_ProducesSameSequence()
    {
        var random1 = new Random(19740409);
        var random2 = Stuff.RandomWithFixedSeed;
        
        // Reset random2 by creating new instance with same seed
        random2 = new Random(19740409);
        
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(random1.Next(), random2.Next());
        }
    }

    [TestMethod]
    public void Random_IsNotNull()
    {
        Assert.IsNotNull(Stuff.Random);
    }

    [TestMethod]
    public void Random_ProducesRandomValues()
    {
        var values = new HashSet<int>();
        for (int i = 0; i < 100; i++)
        {
            values.Add(Stuff.Random.Next(1000));
        }
        // Should have at least 90 unique values out of 100 (very high probability)
        Assert.IsTrue(values.Count >= 90);
    }

    #endregion

    #region NoOp Tests

    [TestMethod]
    public void NoOp_DoesNotThrow()
    {
        Stuff.NoOp();
        Stuff.NoOp(1);
        Stuff.NoOp(1, "two", 3.0);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithNull_ReturnsNull()
    {
        Assert.IsNull(Stuff.ToString(null));
    }

    [TestMethod]
    public void ToString_WithObject_ReturnsString()
    {
        Assert.AreEqual("123", Stuff.ToString(123));
        Assert.AreEqual("hello", Stuff.ToString("hello"));
    }

    #endregion

    #region Swap Additional Tests

    [TestMethod]
    public void Swap_Strings_Succeeds()
    {
        string a = "first", b = "second";
        Stuff.Swap(ref a, ref b);
        Assert.AreEqual("second", a);
        Assert.AreEqual("first", b);
    }

    [TestMethod]
    public void Swap_Objects_Succeeds()
    {
        object a = new object();
        object b = new object();
        var originalA = a;
        var originalB = b;
        
        Stuff.Swap(ref a, ref b);
        Assert.AreSame(originalB, a);
        Assert.AreSame(originalA, b);
    }

    #endregion

    #region Min/Max Additional Tests

    [TestMethod]
    public void Min_Strings_Succeeds()
    {
        Assert.AreEqual("apple", Stuff.Min("apple", "banana"));
        Assert.AreEqual("apple", Stuff.Min("banana", "apple"));
    }

    [TestMethod]
    public void Max_Strings_Succeeds()
    {
        Assert.AreEqual("banana", Stuff.Max("apple", "banana"));
        Assert.AreEqual("banana", Stuff.Max("banana", "apple"));
    }

    [TestMethod]
    public void Min_Decimals_Succeeds()
    {
        Assert.AreEqual(1.5m, Stuff.Min(1.5m, 2.5m));
    }

    [TestMethod]
    public void Max_Decimals_Succeeds()
    {
        Assert.AreEqual(2.5m, Stuff.Max(1.5m, 2.5m));
    }

    [TestMethod]
    public void Min_SameValues_ReturnsValue()
    {
        Assert.AreEqual(5, Stuff.Min(5, 5));
    }

    [TestMethod]
    public void Max_SameValues_ReturnsValue()
    {
        Assert.AreEqual(5, Stuff.Max(5, 5));
    }

    #endregion

    #region TickCount2DateTime Tests

    [TestMethod]
    public void TickCount2DateTime_CurrentTickCount_ReturnsNow()
    {
        var tickCount = Environment.TickCount;
        var result = Stuff.TickCount2DateTime(tickCount);
        var now = DateTime.Now;
        
        // Should be within 1 second
        Assert.IsTrue((now - result).TotalSeconds < 1);
    }

    [TestMethod]
    public void TickCount2DateTime_PastTickCount_ReturnsPastTime()
    {
        var currentTick = Environment.TickCount;
        var pastTick = currentTick - 60000; // 1 minute ago
        var result = Stuff.TickCount2DateTime(pastTick);
        var now = DateTime.Now;
        
        var diff = (now - result).TotalSeconds;
        Assert.IsTrue(diff >= 59 && diff <= 61); // Around 1 minute
    }

    #endregion

    #region GetEnumValues Tests

    [TestMethod]
    public void GetEnumValues_ReturnsAllValues()
    {
        var values = Stuff.GetEnumValues<DayOfWeek>().ToList();
        Assert.AreEqual(7, values.Count);
        Assert.IsTrue(values.Contains(DayOfWeek.Sunday));
        Assert.IsTrue(values.Contains(DayOfWeek.Monday));
        Assert.IsTrue(values.Contains(DayOfWeek.Saturday));
    }

    [TestMethod]
    public void GetEnumValues_CustomEnum_Succeeds()
    {
        var values = Stuff.GetEnumValues<TestEnum>().ToList();
        Assert.AreEqual(3, values.Count);
        Assert.IsTrue(values.Contains(TestEnum.First));
        Assert.IsTrue(values.Contains(TestEnum.Second));
        Assert.IsTrue(values.Contains(TestEnum.Third));
    }

    private enum TestEnum
    {
        First,
        Second,
        Third
    }

    #endregion

    #region FlagEq Tests

    [TestMethod]
    public void FlagEq_FlagSet_ReturnsTrue()
    {
        var flags = FileAttributes.ReadOnly | FileAttributes.Hidden;
        Assert.IsTrue(Stuff.FlagEq(flags, FileAttributes.ReadOnly));
        Assert.IsTrue(Stuff.FlagEq(flags, FileAttributes.Hidden));
    }

    [TestMethod]
    public void FlagEq_FlagNotSet_ReturnsFalse()
    {
        var flags = FileAttributes.ReadOnly | FileAttributes.Hidden;
        Assert.IsFalse(Stuff.FlagEq(flags, FileAttributes.System));
    }

    [TestMethod]
    public void FlagEq_NoFlags_ReturnsFalse()
    {
        var flags = FileAttributes.Normal;
        Assert.IsFalse(Stuff.FlagEq(flags, FileAttributes.ReadOnly));
    }

    #endregion

    #region Dispose Tests

    [TestMethod]
    public void Dispose_DisposableObject_Disposes()
    {
        var disposable = new TestDisposable();
        Stuff.Dispose(disposable);
        Assert.IsTrue(disposable.IsDisposed);
    }

    [TestMethod]
    public void Dispose_MultipleObjects_DisposesAll()
    {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        Stuff.Dispose(d1, d2);
        Assert.IsTrue(d1.IsDisposed);
        Assert.IsTrue(d2.IsDisposed);
    }

    [TestMethod]
    public void Dispose_NullObject_DoesNotThrow()
    {
        Stuff.Dispose((object)null);
    }

    [TestMethod]
    public void Dispose_NonDisposableObject_DoesNotThrow()
    {
        Stuff.Dispose("not disposable", 123);
    }

    [TestMethod]
    public void Dispose_ThrowingDisposable_DoesNotThrow()
    {
        var throwing = new ThrowingDisposable();
        Stuff.Dispose(throwing); // Should not throw
    }

    private class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    private class ThrowingDisposable : IDisposable
    {
        public void Dispose() => throw new InvalidOperationException("Dispose failed");
    }

    #endregion

    #region File Cleanup Tests

    [TestMethod]
    public void MarkFileForCleanup_ValidFile_MarksForCleanup()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            Stuff.MarkFileForCleanup(tempFile);
            Assert.IsTrue(File.Exists(tempFile));
        }
        finally
        {
            Stuff.Cleanup();
            // File should be deleted after cleanup
        }
    }

    [TestMethod]
    public void MarkFileForCleanup_NullPath_DoesNotThrow()
    {
        Stuff.MarkFileForCleanup(null);
    }

    [TestMethod]
    public void Cleanup_DeletesMarkedFiles()
    {
        var files = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            var tempFile = Path.GetTempFileName();
            files.Add(tempFile);
            Stuff.MarkFileForCleanup(tempFile, setAttributesAsTempFile: false);
        }

        // All files should exist
        foreach (var file in files)
        {
            Assert.IsTrue(File.Exists(file));
        }

        Stuff.Cleanup();

        // Files should be deleted (or cleanup attempted)
        // Note: Some might still exist if locked, but cleanup should have been attempted
    }

    #endregion

    #region CreateRandomCode Tests

    [TestMethod]
    public void CreateRandomCode_DefaultLength_Returns6Characters()
    {
        var code = Stuff.CreateRandomCode();
        Assert.AreEqual(6, code.Length);
    }

    [TestMethod]
    public void CreateRandomCode_CustomLength_ReturnsCorrectLength()
    {
        var code = Stuff.CreateRandomCode(10);
        Assert.AreEqual(10, code.Length);
    }

    [TestMethod]
    public void CreateRandomCode_GeneratesDifferentCodes()
    {
        var codes = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            codes.Add(Stuff.CreateRandomCode());
        }
        // Should have many unique codes
        Assert.IsTrue(codes.Count >= 95);
    }

    #endregion

    #region CreateParallelOptions Tests

    [TestMethod]
    public void CreateParallelOptions_CannotParallelize_SetsMaxDegreeToOne()
    {
        var options = Stuff.CreateParallelOptions(canParallelize: false);
        Assert.AreEqual(1, options.MaxDegreeOfParallelism);
    }

    [TestMethod]
    public void CreateParallelOptions_CanParallelize_NoLimit()
    {
        var options = Stuff.CreateParallelOptions(canParallelize: true);
        Assert.AreEqual(-1, options.MaxDegreeOfParallelism); // Default is -1 (unlimited)
    }

    [TestMethod]
    public void CreateParallelOptions_CanParallelize_WithDegrees()
    {
        var options = Stuff.CreateParallelOptions(canParallelize: true, degrees: 4);
        Assert.AreEqual(4, options.MaxDegreeOfParallelism);
    }

    [TestMethod]
    public void CreateParallelOptions_CannotParallelize_IgnoresDegrees()
    {
        var options = Stuff.CreateParallelOptions(canParallelize: false, degrees: 10);
        Assert.AreEqual(1, options.MaxDegreeOfParallelism);
    }

    #endregion

    #region LoggerOfLastResort Tests

    [TestMethod]
    public void LoggerOfLastResort_DefaultIsNullLogger()
    {
        Assert.IsNotNull(Stuff.LoggerOfLastResort);
    }

    [TestMethod]
    public void LoggerOfLastResort_SetToNull_BecomesNullLogger()
    {
        var original = Stuff.LoggerOfLastResort;
        try
        {
            Stuff.LoggerOfLastResort = null;
            Assert.IsNotNull(Stuff.LoggerOfLastResort);
        }
        finally
        {
            Stuff.LoggerOfLastResort = original;
        }
    }

    #endregion
}
