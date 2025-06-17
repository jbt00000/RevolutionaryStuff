using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Core.Tests;

[TestClass]
public class MimeTypeTests
{
    private const string MyMimeTypePrimaryType = "PRIME";
    private const string MyMimeTypeSecondaryType = "SECOND";
    private const string MyMimeTypePrimaryExtension = ".hello";
    private const string MyMimeTypeSecondaryExtension = ".world";
    private static readonly MimeType MyMimeType = new($"{MyMimeTypePrimaryType}/{MyMimeTypeSecondaryType}", MyMimeTypePrimaryExtension, MyMimeTypeSecondaryExtension);

    [TestMethod]
    public void DoesExtensionMatch_True()
    {
        foreach (var e in new[] {
            "yes"+MyMimeTypePrimaryExtension,
            "yes"+MyMimeTypeSecondaryExtension,


        })
        {
            Assert.IsTrue(MyMimeType.DoesExtensionMatch(e));
            Assert.IsTrue(MyMimeType.DoesExtensionMatch(e.ToLower()));
            Assert.IsTrue(MyMimeType.DoesExtensionMatch(e.ToUpper()));
        }
    }

    [TestMethod]
    public void DoesExtensionMatch_False()
    {
        foreach (var e in new[] {
            "no"+MyMimeTypePrimaryExtension+".duh",
            "no.nah"
        })
        {
            Assert.IsFalse(MyMimeType.DoesExtensionMatch(e));
            Assert.IsFalse(MyMimeType.DoesExtensionMatch(e.ToLower()));
            Assert.IsFalse(MyMimeType.DoesExtensionMatch(e.ToUpper()));
        }
    }

    [TestMethod]
    public void FindWellKnowGlobalTypesByExtension()
    {
        Assert.AreEqual(MimeType.Text.Plain, MimeType.FindByExtension(".text"));
        Assert.AreEqual(MimeType.Text.Plain, MimeType.FindByExtension(".txt"));
        Assert.AreEqual(MimeType.Image.Jpg, MimeType.FindByExtension(".jpg"));
    }

    [TestMethod]
    public void SimpleContentTypeMatches()
    {
        Assert.IsTrue(MimeType.Text.Plain.DoesContentTypeMatch("    text/plain "));
        Assert.IsTrue(MimeType.Text.Plain.DoesContentTypeMatch("text/plain"));
        Assert.IsTrue(MimeType.Text.Plain.DoesContentTypeMatch("teXt/plain"));
        Assert.IsTrue(MimeType.Text.Plain.DoesContentTypeMatch("text/plain", true));
        Assert.IsFalse(MimeType.Text.Plain.DoesContentTypeMatch("teXt/plain", true));
    }

    [TestMethod]
    public void IgnoresCharsetEncoding()
    {
        Assert.IsTrue(MimeType.Application.Json.DoesContentTypeMatch("application/json"));
        Assert.IsTrue(MimeType.Application.Json.DoesContentTypeMatch("application/json; charset=utf-8"));
        Assert.IsTrue(MimeType.Application.Json.DoesContentTypeMatch("application/json ; charset=utf-8"));
        Assert.IsFalse(MimeType.Application.Json.DoesContentTypeMatch("application/jNONONOson"));
    }

    [DataTestMethod]
    [DataRow("image/jpeg")]
    [DataRow("image/png")]
    [DataRow("image/gif")]
    [DataRow("image/bmp")]
    [DataRow("image/svg+xml")]
    [DataRow("image/tiff")]
    [DataRow("image/webp")]
    [DataRow(".jpg")]
    [DataRow(".jpeg")]
    [DataRow(".jpe")]
    [DataRow(".png")]
    [DataRow(".gif")]
    [DataRow(".bmp")]
    [DataRow(".svg")]
    [DataRow(".tif")]
    [DataRow(".tiff")]
    [DataRow(".webp")]
    public void IsImage_PositiveCases(string input)
    {
        Assert.IsTrue(MimeType.IsImage(input));
    }

    [DataTestMethod]
    [DataRow("application/json")]
    [DataRow("text/plain")]
    [DataRow("video/mp4")]
    [DataRow("audio/mp3")]
    [DataRow(".txt")]
    [DataRow(".json")]
    [DataRow(".mp4")]
    [DataRow(".mp3")]
    [DataRow("")]
    [DataRow(null)]
    public void IsImage_NegativeCases(string input)
    {
        Assert.IsFalse(MimeType.IsImage(input));
    }
    
    [TestMethod]
    public void Image_Svg_PropertiesTest()
    {
        Assert.AreEqual("image/svg+xml", MimeType.Image.Svg.PrimaryContentType);
        Assert.AreEqual(".svg", MimeType.Image.Svg.PrimaryFileExtension);
        Assert.IsTrue(MimeType.Image.Svg.DoesContentTypeMatch("image/svg+xml"));
        Assert.IsTrue(MimeType.Image.Svg.DoesExtensionMatch(".svg"));
        Assert.IsTrue(MimeType.Image.Svg.DoesExtensionMatch("test.svg"));
    }
    
    [TestMethod]
    public void IsA_Image_Tests()
    {
        // Test specific image type is an image/*
        Assert.IsTrue(MimeType.IsA("image/svg+xml", "image/*"));
        Assert.IsTrue(MimeType.IsA("image/jpeg", "image/*"));
        
        // Test exact matches
        Assert.IsTrue(MimeType.IsA("image/svg+xml", "image/svg+xml"));
        
        // Test case insensitivity 
        Assert.IsTrue(MimeType.IsA("IMAGE/SVG+XML", "image/svg+xml"));
        Assert.IsTrue(MimeType.IsA("image/svg+xml", "IMAGE/SVG+XML"));
        
        // Test with parameters
        Assert.IsTrue(MimeType.IsA("image/svg+xml; charset=utf-8", "image/svg+xml"));
    }
    
    [TestMethod]
    public void IsA_Image_Negative_Tests()
    {
        // Test non-images aren't images
        Assert.IsFalse(MimeType.IsA("text/plain", "image/*"));
        Assert.IsFalse(MimeType.IsA("application/json", "image/*"));
        
        // Test specific type mismatches
        Assert.IsFalse(MimeType.IsA("image/png", "image/svg+xml"));
        Assert.IsFalse(MimeType.IsA("image/svg+xml", "image/png"));
    }
}
