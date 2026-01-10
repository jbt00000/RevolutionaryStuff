using System;
using System.Linq;
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

    #region Existing Tests

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

    #endregion

    #region Helper Methods Tests

    [TestMethod]
    public void GetContentTypeType_ReturnsMainType()
    {
        Assert.AreEqual("image", MimeType.GetContentTypeType("image/png"));
        Assert.AreEqual("video", MimeType.GetContentTypeType("video/mp4"));
        Assert.AreEqual("application", MimeType.GetContentTypeType("application/json"));
    }

    [TestMethod]
    public void GetContentTypeSubType_ReturnsSubType()
    {
        Assert.AreEqual("png", MimeType.GetContentTypeSubType("image/png"));
        Assert.AreEqual("mp4", MimeType.GetContentTypeSubType("video/mp4"));
        Assert.AreEqual("json", MimeType.GetContentTypeSubType("application/json"));
    }

    #endregion

    #region IsImage Tests

    [TestMethod]
    public void IsImage_PngContentType_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsImage("image/png"));
    }

    [TestMethod]
    public void IsImage_PngExtension_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsImage(".png"));
        Assert.IsTrue(MimeType.IsImage("file.png"));
    }

    [TestMethod]
    public void IsImage_ModernFormats_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsImage("image/avif"));
        Assert.IsTrue(MimeType.IsImage(".avif"));
        Assert.IsTrue(MimeType.IsImage("image/webp"));
        Assert.IsTrue(MimeType.IsImage(".webp"));
        Assert.IsTrue(MimeType.IsImage("image/heic"));
        Assert.IsTrue(MimeType.IsImage(".heic"));
    }

    [TestMethod]
    public void IsImage_VideoType_ReturnsFalse()
    {
        Assert.IsFalse(MimeType.IsImage("video/mp4"));
        Assert.IsFalse(MimeType.IsImage(".mp4"));
    }

    #endregion

    #region IsVideo Tests

    [TestMethod]
    public void IsVideo_Mp4ContentType_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsVideo("video/mp4"));
    }

    [TestMethod]
    public void IsVideo_Mp4Extension_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsVideo(".mp4"));
        Assert.IsTrue(MimeType.IsVideo("video.mp4"));
    }

    [TestMethod]
    public void IsVideo_ModernFormats_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsVideo("video/webm"));
        Assert.IsTrue(MimeType.IsVideo(".mkv"));
        Assert.IsTrue(MimeType.IsVideo("video/av01"));
    }

    [TestMethod]
    public void IsVideo_ImageType_ReturnsFalse()
    {
        Assert.IsFalse(MimeType.IsVideo("image/png"));
    }

    #endregion

    #region Application Modern Types Tests

    [TestMethod]
    public void Application_Wasm_HasCorrectValues()
    {
        Assert.AreEqual("application/wasm", MimeType.Application.Wasm.PrimaryContentType);
        Assert.AreEqual(".wasm", MimeType.Application.Wasm.PrimaryFileExtension);
    }

    [TestMethod]
    public void Application_Epub_HasCorrectValues()
    {
        Assert.AreEqual("application/epub+zip", MimeType.Application.Epub.PrimaryContentType);
        Assert.AreEqual(".epub", MimeType.Application.Epub.PrimaryFileExtension);
    }

    [TestMethod]
    public void Application_JsonLd_HasCorrectValues()
    {
        Assert.AreEqual("application/ld+json", MimeType.Application.JsonLd.PrimaryContentType);
        Assert.AreEqual(".jsonld", MimeType.Application.JsonLd.PrimaryFileExtension);
    }

    #endregion

    #region Container Modern Types Tests

    [TestMethod]
    public void Container_Zstd_HasCorrectValues()
    {
        Assert.AreEqual("application/zstd", MimeType.Application.Container.Zstd.PrimaryContentType);
        Assert.AreEqual(".zst", MimeType.Application.Container.Zstd.PrimaryFileExtension);
    }

    [TestMethod]
    public void Container_GZip_HasCorrectValues()
    {
        Assert.AreEqual("application/gzip", MimeType.Application.Container.GZip.PrimaryContentType);
        Assert.AreEqual(".gz", MimeType.Application.Container.GZip.PrimaryFileExtension);
    }

    #endregion

    #region Apple Office Formats Tests

    [TestMethod]
    public void WordProcessing_Pages_HasCorrectValues()
    {
        Assert.AreEqual(".pages", MimeType.Application.WordProcessing.Pages.PrimaryFileExtension);
    }

    [TestMethod]
    public void SpreadSheet_Numbers_HasCorrectValues()
    {
        Assert.AreEqual(".numbers", MimeType.Application.SpreadSheet.Numbers.PrimaryFileExtension);
    }

    [TestMethod]
    public void Presentation_Keynote_HasCorrectValues()
    {
        Assert.AreEqual(".key", MimeType.Application.Presentation.Keynote.PrimaryFileExtension);
    }

    #endregion

    #region Image Modern Formats Tests

    [TestMethod]
    public void Image_Avif_HasCorrectValues()
    {
        Assert.AreEqual("image/avif", MimeType.Image.Avif.PrimaryContentType);
        Assert.AreEqual(".avif", MimeType.Image.Avif.PrimaryFileExtension);
    }

    [TestMethod]
    public void Image_WebP_HasCorrectValues()
    {
        Assert.AreEqual("image/webp", MimeType.Image.WebP.PrimaryContentType);
        Assert.AreEqual(".webp", MimeType.Image.WebP.PrimaryFileExtension);
    }

    [TestMethod]
    public void Image_JpegXL_HasCorrectValues()
    {
        Assert.AreEqual("image/jxl", MimeType.Image.Jxl.PrimaryContentType);
        Assert.AreEqual(".jxl", MimeType.Image.Jxl.PrimaryFileExtension);
    }

    [TestMethod]
    public void Image_Heic_HasCorrectValues()
    {
        Assert.AreEqual("image/heic", MimeType.Image.Heic.PrimaryContentType);
        Assert.AreEqual(".heic", MimeType.Image.Heic.PrimaryFileExtension);
    }

    [TestMethod]
    public void Image_Jpeg2000_HasCorrectValues()
    {
        Assert.AreEqual("image/jp2", MimeType.Image.Jpeg2000.PrimaryContentType);
        Assert.IsTrue(MimeType.Image.Jpeg2000.DoesExtensionMatch(".jp2"));
        Assert.IsTrue(MimeType.Image.Jpeg2000.DoesExtensionMatch(".j2k"));
    }

    #endregion

    #region Text Modern Formats Tests

    [TestMethod]
    public void Text_Yaml_HasMultipleContentTypesAndExtensions()
    {
        Assert.IsTrue(MimeType.Text.Yaml.DoesContentTypeMatch("text/yaml"));
        Assert.IsTrue(MimeType.Text.Yaml.DoesContentTypeMatch("application/x-yaml"));
        Assert.IsTrue(MimeType.Text.Yaml.DoesExtensionMatch(".yaml"));
        Assert.IsTrue(MimeType.Text.Yaml.DoesExtensionMatch(".yml"));
    }

    [TestMethod]
    public void Text_Css_HasCorrectValues()
    {
        Assert.AreEqual("text/css", MimeType.Text.Css.PrimaryContentType);
        Assert.AreEqual(".css", MimeType.Text.Css.PrimaryFileExtension);
    }

    [TestMethod]
    public void Text_Toml_HasCorrectValues()
    {
        Assert.AreEqual("application/toml", MimeType.Text.Toml.PrimaryContentType);
        Assert.AreEqual(".toml", MimeType.Text.Toml.PrimaryFileExtension);
    }

    #endregion

    #region Audio Modern Formats Tests

    [TestMethod]
    public void Audio_Flac_HasCorrectValues()
    {
        Assert.AreEqual("audio/flac", MimeType.Audio.Flac.PrimaryContentType);
        Assert.AreEqual(".flac", MimeType.Audio.Flac.PrimaryFileExtension);
    }

    [TestMethod]
    public void Audio_Opus_HasCorrectValues()
    {
        Assert.AreEqual("audio/opus", MimeType.Audio.OpusAudio.PrimaryContentType);
        Assert.AreEqual(".opus", MimeType.Audio.OpusAudio.PrimaryFileExtension);
    }

    [TestMethod]
    public void Audio_TrueHd_HasCorrectValues()
    {
        Assert.AreEqual("audio/vnd.dolby.mlp", MimeType.Audio.TrueHd.PrimaryContentType);
        Assert.IsTrue(MimeType.Audio.TrueHd.DoesExtensionMatch(".thd"));
    }

    #endregion

    #region Video Modern Formats Tests

    [TestMethod]
    public void Video_Mkv_HasCorrectValues()
    {
        Assert.AreEqual("video/x-matroska", MimeType.Video.Mkv.PrimaryContentType);
        Assert.AreEqual(".mkv", MimeType.Video.Mkv.PrimaryFileExtension);
    }

    [TestMethod]
    public void Video_Av1_HasCorrectValues()
    {
        Assert.AreEqual("video/av01", MimeType.Video.Av1.PrimaryContentType);
        Assert.AreEqual(".av01", MimeType.Video.Av1.PrimaryFileExtension);
    }

    [TestMethod]
    public void Video_H265_HasCorrectValues()
    {
        Assert.AreEqual("video/h265", MimeType.Video.H265.PrimaryContentType);
        Assert.AreEqual(".h265", MimeType.Video.H265.PrimaryFileExtension);
    }

    [TestMethod]
    public void Video_ProRes_HasCorrectValues()
    {
        Assert.AreEqual("video/prores", MimeType.Video.ProRes.PrimaryContentType);
        Assert.AreEqual(".prores", MimeType.Video.ProRes.PrimaryFileExtension);
    }

    #endregion

    #region IsA Tests

    [TestMethod]
    public void IsA_SameType_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsA("image/png", "image/png"));
    }

    [TestMethod]
    public void IsA_TypeMatchesWildcard_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsA("image/png", "image/*"));
        Assert.IsTrue(MimeType.IsA("video/mp4", "video/*"));
        Assert.IsTrue(MimeType.IsA("application/json", "application/*"));
    }

    [TestMethod]
    public void IsA_MatchesAnyWildcard_ReturnsTrue()
    {
        Assert.IsTrue(MimeType.IsA("image/png", "*/*"));
        Assert.IsTrue(MimeType.IsA("video/mp4", "*/*"));
    }

    [TestMethod]
    public void IsA_DifferentTypes_ReturnsFalse()
    {
        Assert.IsFalse(MimeType.IsA("image/png", "video/*"));
        Assert.IsFalse(MimeType.IsA("video/mp4", "audio/*"));
    }

    #endregion

    #region FindByContentType Tests

    [TestMethod]
    public void FindByContentType_ImagePng_ReturnsMatch()
    {
        var mimes = MimeType.FindByContentType("image/png");
        Assert.AreEqual(1, mimes.Count);
        Assert.AreEqual(".png", mimes[0].PrimaryFileExtension);
    }

    [TestMethod]
    public void FindByContentType_WithParameters_IgnoresParameters()
    {
        var mimes = MimeType.FindByContentType("text/html; charset=utf-8");
        Assert.AreEqual(1, mimes.Count);
    }

    [TestMethod]
    public void FindByContentType_CaseInsensitive_ReturnsMatch()
    {
        var mimes = MimeType.FindByContentType("IMAGE/PNG");
        Assert.AreEqual(1, mimes.Count);
    }

    #endregion

    #region AllMimeTypes Tests

    [TestMethod]
    public void AllMimeTypes_ContainsModernTypes()
    {
        var all = MimeType.AllMimeTypes;
        Assert.IsTrue(all.Any(m => m.PrimaryContentType == "image/avif"));
        Assert.IsTrue(all.Any(m => m.PrimaryContentType == "image/webp"));
        Assert.IsTrue(all.Any(m => m.PrimaryContentType == "application/wasm"));
        Assert.IsTrue(all.Any(m => m.PrimaryContentType == "video/av01"));
        Assert.IsTrue(all.Any(m => m.PrimaryContentType == "audio/flac"));
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_InvalidContentType_ThrowsException()
    {
        try
        {
            var mime = new MimeType("invalid");
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Constructor_WithMultipleExtensions_AddsAll()
    {
        var mime = new MimeType("custom/type", ".ext1", ".ext2");
        Assert.IsTrue(mime.DoesExtensionMatch(".ext1"));
        Assert.IsTrue(mime.DoesExtensionMatch(".ext2"));
    }

    #endregion
}
