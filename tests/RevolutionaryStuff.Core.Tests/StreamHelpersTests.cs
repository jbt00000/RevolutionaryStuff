using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class StreamHelpersTests
{
    private string _tempDirectory;

    [TestInitialize]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"StreamHelpersTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private string GetTempFilePath() => Path.Combine(_tempDirectory, $"test_{Guid.NewGuid():N}.tmp");

    #region Existing Tests

    [TestMethod]
    public async Task CopyToAsyncTest()
    {
        var sourceBuffer = new byte[1024 * 1024 * 4];
        Stuff.Random.NextBytes(sourceBuffer);
        var sourceStream = new MemoryStream(sourceBuffer);
        var destStream = new MemoryStream();
        var callbackCount = 0;
        long lastTotRead = 0;
        await sourceStream.CopyToAsync(destStream, (read, totRead, tot) =>
        {
            ++callbackCount;
            Trace.WriteLine($"CopyToAsync(read={read}, totRead={totRead}, tot={tot}) CallbackCount={callbackCount}");
            Assert.IsTrue(totRead >= lastTotRead);
            Assert.AreEqual(totRead, lastTotRead + read);
            lastTotRead = totRead;
        });
        Assert.IsTrue(callbackCount > 1);
        Assert.IsTrue(CompareHelpers.Compare(sourceBuffer, destStream.ToArray()));
    }

    [TestMethod]
    public async Task ReadToEndAsyncTest()
    {
        var test = $"{nameof(ReadToEndAsyncTest)} message.";
        var st = new MemoryStream();
        var sw = new StreamWriter(st, Encoding.UTF8);
        sw.Write(test);
        sw.Flush();
        st.Position = 0;
        var ans = await st.ReadToEndAsync();
        Assert.AreEqual(test, ans);
        //the below will throw an exception if readtoend closed the stream
        st.Position = 0;
    }

    [TestMethod]
    public void ReadToEndTest()
    {
        var test = $"{nameof(ReadToEndTest)} message.";
        var st = new MemoryStream();
        var sw = new StreamWriter(st, Encoding.UTF8);
        sw.Write(test);
        sw.Flush();
        st.Position = 0;
        var ans = st.ReadToEnd();
        Assert.AreEqual(test, ans);
        //the below will throw an exception if readtoend closed the stream
        st.Position = 0;
    }

    #endregion

    #region CopyFrom Tests

    [TestMethod]
    public async Task CopyFromAsync_CopiesFileContentToStream()
    {
        var filePath = GetTempFilePath();
        var expectedContent = "Test content for async copy";
        await File.WriteAllTextAsync(filePath, expectedContent);

        using var ms = new MemoryStream();
        await ms.CopyFromAsync(filePath);
        ms.Position = 0;

        var result = await new StreamReader(ms).ReadToEndAsync();
        Assert.AreEqual(expectedContent, result);
    }

    [TestMethod]
    public void CopyFrom_CopiesFileContentToStream()
    {
        var filePath = GetTempFilePath();
        var expectedContent = "Test content for sync copy";
        File.WriteAllText(filePath, expectedContent);

        using var ms = new MemoryStream();
        ms.CopyFrom(filePath);
        ms.Position = 0;

        var result = new StreamReader(ms).ReadToEnd();
        Assert.AreEqual(expectedContent, result);
    }

    #endregion

    #region CopyTo File Tests

    [TestMethod]
    public void CopyTo_CopiesStreamContentToFile()
    {
        var filePath = GetTempFilePath();
        var expectedContent = "Test content for file copy";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));
        ms.CopyTo(filePath);

        var result = File.ReadAllText(filePath);
        Assert.AreEqual(expectedContent, result);
    }

    [TestMethod]
    public async Task CopyToAsync_CopiesStreamContentToFile()
    {
        var filePath = GetTempFilePath();
        var expectedContent = "Test content for async file copy";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));
        await ms.CopyToAsync(filePath);

        var result = await File.ReadAllTextAsync(filePath);
        Assert.AreEqual(expectedContent, result);
    }

    #endregion

    #region Create Stream Tests

    [TestMethod]
    public void Create_CreatesStreamFromString()
    {
        var testString = "Hello, World!";
        using var stream = StreamHelpers.Create(testString);

        var result = new StreamReader(stream).ReadToEnd();
        Assert.AreEqual(testString, result);
    }

    [TestMethod]
    public void Create_WithEncoding_UsesSpecifiedEncoding()
    {
        var testString = "Hello, Wörld! 世界";
        using var stream = StreamHelpers.Create(testString, Encoding.Unicode);

        var result = new StreamReader(stream, Encoding.Unicode).ReadToEnd();
        Assert.AreEqual(testString, result);
    }

    [TestMethod]
    public void Create_StreamIsAtBeginning()
    {
        using var stream = StreamHelpers.Create("test");
        Assert.AreEqual(0, stream.Position);
    }

    [TestMethod]
    public void CreateUtf8WithoutPreamble_HasNoBOM()
    {
        var testString = "Test";
        using var stream = StreamHelpers.CreateUtf8WithoutPreamble(testString);

        var bytes = new byte[3];
        stream.Read(bytes, 0, 3);

        // UTF-8 BOM is 0xEF, 0xBB, 0xBF - first byte should be 'T' (0x54)
        Assert.AreEqual((byte)'T', bytes[0], "Should not have UTF-8 BOM");
    }

    #endregion

    #region Write Byte Array Tests

    [TestMethod]
    public void Write_ByteArray_WritesEntireBuffer()
    {
        var buffer = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream();

        ms.Write(buffer);

        CollectionAssert.AreEqual(buffer, ms.ToArray());
    }

    [TestMethod]
    public void Write_NullByteArray_DoesNothing()
    {
        using var ms = new MemoryStream();
        ms.Write((byte[])null);

        Assert.AreEqual(0, ms.Length);
    }

    #endregion

    #region SeekViaPos Tests

    [TestMethod]
    public void SeekViaPos_Begin_SeeksFromStart()
    {
        using var ms = new MemoryStream(new byte[100]);

        var newPos = ms.SeekViaPos(50, SeekOrigin.Begin);

        Assert.AreEqual(50, newPos);
        Assert.AreEqual(50, ms.Position);
    }

    [TestMethod]
    public void SeekViaPos_Current_SeeksFromCurrentPosition()
    {
        using var ms = new MemoryStream(new byte[100]);
        ms.Position = 30;

        var newPos = ms.SeekViaPos(20, SeekOrigin.Current);

        Assert.AreEqual(50, newPos);
        Assert.AreEqual(50, ms.Position);
    }

    [TestMethod]
    public void SeekViaPos_End_SeeksFromEnd()
    {
        using var ms = new MemoryStream(new byte[100]);

        var newPos = ms.SeekViaPos(-10, SeekOrigin.End);

        Assert.AreEqual(90, newPos);
        Assert.AreEqual(90, ms.Position);
    }

    #endregion

    #region ReadExactSize Tests

    [TestMethod]
    public void ReadExactSize_ReadsExactNumberOfBytes()
    {
        var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        using var ms = new MemoryStream(sourceData);

        var buffer = new byte[5];
        ms.ReadExactSize(buffer);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, buffer);
    }

    [TestMethod]
    public void ReadExactSize_WithOffset_ReadsAtOffset()
    {
        var sourceData = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream(sourceData);

        var buffer = new byte[10];
        ms.ReadExactSize(buffer, offset: 3, size: 5);

        Assert.AreEqual(0, buffer[0]);
        Assert.AreEqual(1, buffer[3]);
        Assert.AreEqual(5, buffer[7]);
    }

    [TestMethod]
    public void ReadExactSize_WithCustomSize_ReadsSpecifiedSize()
    {
        var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        using var ms = new MemoryStream(sourceData);

        var buffer = new byte[10];
        ms.ReadExactSize(buffer, size: 3);

        Assert.AreEqual(1, buffer[0]);
        Assert.AreEqual(2, buffer[1]);
        Assert.AreEqual(3, buffer[2]);
        Assert.AreEqual(0, buffer[3]);
    }

    [TestMethod]
    public void ReadExactSize_StreamTooSmall_ThrowsException()
    {
        var sourceData = new byte[] { 1, 2, 3 };
        using var ms = new MemoryStream(sourceData);

        var buffer = new byte[10];
        Assert.Throws<IndexOutOfRangeException>(() => ms.ReadExactSize(buffer));
    }

    #endregion

    #region ToBufferAsync Tests

    [TestMethod]
    public async Task ToBufferAsync_ConvertsStreamToByteArray()
    {
        var expected = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream(expected);

        var result = await ms.ToBufferAsync();

        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public async Task ToBufferAsync_WithNonMemoryStream_Copies()
    {
        var expected = new byte[] { 1, 2, 3, 4, 5 };
        var filePath = GetTempFilePath();
        await File.WriteAllBytesAsync(filePath, expected);

        using var fileStream = File.OpenRead(filePath);
        var result = await fileStream.ToBufferAsync();

        CollectionAssert.AreEqual(expected, result);
    }

    #endregion

    #region Write String Tests

    [TestMethod]
    public void Write_String_WritesStringAsUtf8()
    {
        var testString = "Hello, World!";
        using var ms = new MemoryStream();

        ms.Write(testString);

        var result = Encoding.UTF8.GetString(ms.ToArray());
        Assert.AreEqual(testString, result);
    }

    [TestMethod]
    public void Write_String_WithEncoding_UsesSpecifiedEncoding()
    {
        var testString = "Hello, 世界!";
        using var ms = new MemoryStream();

        ms.Write(testString, Encoding.Unicode);

        var result = Encoding.Unicode.GetString(ms.ToArray());
        Assert.AreEqual(testString, result);
    }

    [TestMethod]
    public void Write_NullString_DoesNothing()
    {
        using var ms = new MemoryStream();
        ms.Write((string)null);

        Assert.AreEqual(0, ms.Length);
    }

    #endregion

    #region Encoding Constants Tests

    [TestMethod]
    public void UTF8EncodingWithoutPreamble_HasNoBOM()
    {
        var encoding = StreamHelpers.UTF8EncodingWithoutPreamble;
        var preamble = encoding.GetPreamble();

        Assert.HasCount(0, preamble);
    }

    [TestMethod]
    public void UTF8EncodingWithPreamble_HasBOM()
    {
        var encoding = StreamHelpers.UTF8EncodingWithPreamble;
        var preamble = encoding.GetPreamble();

        Assert.IsTrue(preamble.Length > 0);
        Assert.AreEqual(0xEF, preamble[0]);
        Assert.AreEqual(0xBB, preamble[1]);
        Assert.AreEqual(0xBF, preamble[2]);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public async Task IntegrationTest_CreateReadWriteRoundTrip()
    {
        var originalText = "Round trip test with special chars: 世界 🌍";

        // Create stream from string
        using var stream1 = StreamHelpers.Create(originalText);

        // Read it back
        var readText = await stream1.ReadToEndAsync();
        Assert.AreEqual(originalText, readText);

        // Write to file
        var filePath = GetTempFilePath();
        stream1.Position = 0;
        await stream1.CopyToAsync(filePath);

        // Read from file
        using var stream2 = new MemoryStream();
        await stream2.CopyFromAsync(filePath);
        stream2.Position = 0;

        var finalText = await stream2.ReadToEndAsync();
        Assert.AreEqual(originalText, finalText);
    }

    [TestMethod]
    public async Task IntegrationTest_LargeFileWithProgress()
    {
        var filePath = GetTempFilePath();
        var largeData = new byte[1024 * 512]; // 512 KB
        Stuff.Random.NextBytes(largeData);
        await File.WriteAllBytesAsync(filePath, largeData);

        using var source = File.OpenRead(filePath);
        using var destination = new MemoryStream();

        var progressUpdates = 0;
        await source.CopyToAsync(destination, (read, total, length) =>
        {
            progressUpdates++;
            Assert.IsTrue(total <= length, "Total should not exceed length");
        }, bufferSize: 64 * 1024);

        Assert.IsTrue(progressUpdates > 0, "Should have progress updates");
        Assert.AreEqual(largeData.Length, destination.Length);
    }

    #endregion
}
