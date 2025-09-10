using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

/// <remarks>
/// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
/// </remarks>
public sealed partial class MimeType
{
    [GeneratedRegex(@"[^\s/]+/[^\s/]+")]
    private static partial Regex ContentTypeExpr { get; }

    [GeneratedRegex(@"\.\w+")]
    private static partial Regex FileExtensionExpr { get; }

    public static string GetContentTypeType(string contentType)
        => contentType.LeftOf("/");

    public static string GetContentTypeSubType(string contentType)
        => contentType.RightOf("/");

    private static bool HasMatch(string contentTypeOrExtension, IList<MimeType> items)
    {
        contentTypeOrExtension = contentTypeOrExtension.TrimOrNull();
        if (contentTypeOrExtension == null) return false;
        if (!items.NullSafeAny()) return false;
        var ext = Path.GetExtension(contentTypeOrExtension);
        if (ext != null && ext.Length > 0 && !ext.StartsWith("."))
        {
            ext = null;
        }
        foreach (var item in items)
        {
            if (item.DoesContentTypeMatch(contentTypeOrExtension) || (ext != null && item.DoesExtensionMatch(ext))) return true;
        }
        return false;
    }

    public static bool IsImage(string contentTypeOrExtension)
        => Image.HasMatch(contentTypeOrExtension);

    public static bool IsVideo(string contentTypeOrExtension)
        => Video.HasMatch(contentTypeOrExtension);

    public static class Application
    {
        public static readonly MimeType Any = "application/*";
        public static readonly MimeType Json = new("application/json", ".json");
        public static readonly MimeType Xml = new("application/xml", ".xml");
        public static readonly MimeType OctetStream = "application/octet-stream";
        public static readonly MimeType SqlServerIntegrationServicesEtlPackage = new(OctetStream, ".dtsx");
        public static readonly MimeType Pdf = new("application/pdf", ".pdf");
        public static readonly MimeType AmazonKindle = new("application/vnd.amazon.ebook", ".azw");

        public static class Container
        {
            public static readonly MimeType _7Zip = new("application/x-7z-compressed", ".7z");
            public static readonly MimeType ArchiveDocument = new("application/x-freearc", ".arc");
            public static readonly MimeType BZipArchive = new("application/x-bzip", ".bz");
            public static readonly MimeType BZip2Archive = new("application/x-bzip2", ".bz2");
            public static readonly MimeType Tar = new("application/x-tar", ".tar");
            public static readonly MimeType Zip = new("application/zip", ".zip");
            internal static readonly IList<MimeType> All = new[] { ArchiveDocument, BZipArchive, BZip2Archive, _7Zip, Tar, Zip }.ToList().AsReadOnly();
        }

        public static class Font
        {
            public static readonly MimeType OpenTypeFont = new("font/otf", ".otf");
            public static readonly MimeType TrueTypeFont = new("font/ttf", ".ttf");
            public static readonly MimeType WebOpenFontFormat = new("font/woff", ".woff");
            public static readonly MimeType WebOpenFontFormat2 = new("font/woff2", ".woff2");
            internal static readonly IList<MimeType> All = new[] { OpenTypeFont, TrueTypeFont, WebOpenFontFormat, WebOpenFontFormat2 }.ToList().AsReadOnly();
        }

        public static class Presentation
        {
            public static readonly MimeType MicrosoftPowerPoint = new("application/vnd.ms-powerpoint", ".ppt");
            public static readonly MimeType MicrosoftPowerPointOpenXML = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx");
            internal static readonly IList<MimeType> All = new[] { MicrosoftPowerPoint, MicrosoftPowerPointOpenXML }.ToList().AsReadOnly();
        }

        public static class WordProcessing
        {
            public static readonly MimeType MicrosoftWord = new("application/msword", ".doc");
            public static readonly MimeType MicrosoftWordOpenXml = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");
            public static readonly MimeType OpenDocumentTextDocument = new("application/vnd.oasis.opendocument.text", ".odt");
            internal static readonly IList<MimeType> All = new[] { MicrosoftWord, MicrosoftWordOpenXml, OpenDocumentTextDocument }.ToList().AsReadOnly();
        }

        public static class SpreadSheet
        {
            public static readonly MimeType OpenDocumentSpreadsheetDocument = new("application/vnd.oasis.opendocument.spreadsheet", ".ods");
            public static readonly MimeType MicrosoftExcel = new("application/vnd.ms-excel", ".xls");
            public static readonly MimeType MicrosoftExcelOpenXml = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
            internal static readonly IList<MimeType> All = new[] { OpenDocumentSpreadsheetDocument, MicrosoftExcel, MicrosoftExcelOpenXml }.ToList().AsReadOnly();
        }

        public static class Encryption
        {
            public static class PGP
            {
                public static readonly MimeType PgpEncrypted = new("application/pgp-encrypted", ".pgp");
                internal static readonly IList<MimeType> All = new[] { PgpEncrypted }.ToList().AsReadOnly();
            }

            internal static readonly IList<MimeType> All = PGP.All.ToList().AsReadOnly();
        }

        internal static readonly IList<MimeType> All = new[] { Any, Json, Xml, OctetStream, SqlServerIntegrationServicesEtlPackage, Pdf, AmazonKindle }.Union(Container.All).Union(Font.All).Union(WordProcessing.All).Union(SpreadSheet.All).Union(Presentation.All).Union(Encryption.All).ToList().AsReadOnly();
    }

    public static class Image
    {
        public static readonly MimeType Any = new("image/*");
        public static readonly MimeType Bmp = new("image/bmp", ".bmp");
        public static readonly MimeType Gif = new("image/gif", ".gif");
        public static readonly MimeType Jpg = new("image/jpeg", ".jpg", ".jpeg", ".jpe");
        public static readonly MimeType Png = new("image/png", ".png");
        public static readonly MimeType Svg = new("image/svg+xml", ".svg");
        public static readonly MimeType Tiff = new("image/tiff", ".tif", ".tiff");
        public static readonly MimeType WebP = new("image/webp", ".webp");

        // Modern and missing image formats
        public static readonly MimeType Avif = new("image/avif", ".avif");
        public static readonly MimeType Heic = new("image/heic", ".heic");
        public static readonly MimeType Heif = new("image/heif", ".heif");
        public static readonly MimeType Jxl = new("image/jxl", ".jxl");
        public static readonly MimeType Icon = new("image/x-icon", ".ico");
        public static readonly MimeType Apng = new("image/apng", ".apng");
        public static readonly MimeType Jfif = new("image/jpeg", ".jfif");
        public static readonly MimeType Psd = new("image/vnd.adobe.photoshop", ".psd");
        public static readonly MimeType Raw = new("image/x-canon-cr2", ".cr2", ".nef", ".arw", ".dng");
        public static readonly MimeType Pcx = new("image/x-pcx", ".pcx");
        public static readonly MimeType Tga = new("image/x-targa", ".tga");
        public static readonly MimeType Xbm = new("image/x-xbitmap", ".xbm");
        public static readonly MimeType Xpm = new("image/x-xpixmap", ".xpm");

        internal static readonly IList<MimeType> All = new[] { Any, Bmp, Gif, Jpg, Png, Svg, Tiff, WebP, Avif, Heic, Heif, Jxl, Icon, Apng, Jfif, Psd, Raw, Pcx, Tga, Xbm, Xpm }.ToList().AsReadOnly();
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    public static class Text
    {
        public static readonly MimeType Any = new("text/*");
        public static readonly MimeType Csv = new("text/csv", ".csv");
        public static readonly MimeType Html = new("text/html", ".html", ".htm");
        public static readonly MimeType Plain = new("text/plain", ".txt", ".text");
        public static readonly MimeType Markdown = new("text/markdown", ".md");
        internal static readonly IList<MimeType> All = new[] { Any, Csv, Html, Plain, Markdown }.ToList().AsReadOnly();
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    public static class Audio
    {
        public static readonly MimeType Any = new("audio/*");
        public static readonly MimeType Aac = new("audio/aac", ".aac");
        public static readonly MimeType CompactDiskAudio = new("application/x-cdf", ".cda");
        public static readonly MimeType Midi = new("audio/midi", "audio/x-midi", ".mid", ".midi");
        public static readonly MimeType Mp3 = new("audio/mpeg", ".mp3");
        public static readonly MimeType OggAudio = new("audio/ogg", ".oga");
        public static readonly MimeType OpusAudio = new("audio/opus", ".opus");
        public static readonly MimeType Waveform = new("audio/wav", ".wav");
        public static readonly MimeType WebmAudio = new("audio/webm", ".weba");

        // Modern and missing audio formats
        public static readonly MimeType Flac = new("audio/flac", ".flac");
        public static readonly MimeType Mp4Audio = new("audio/mp4", ".m4a", ".mp4a");
        public static readonly MimeType AlacAudio = new("audio/x-m4a", ".m4a");
        public static readonly MimeType Wma = new("audio/x-ms-wma", ".wma");
        public static readonly MimeType Aiff = new("audio/x-aiff", ".aif", ".aiff", ".aifc");
        public static readonly MimeType Au = new("audio/basic", ".au", ".snd");
        public static readonly MimeType Amr = new("audio/amr", ".amr");
        public static readonly MimeType _3gppAudio = new("audio/3gpp", ".3gp");
        public static readonly MimeType _3gpp2Audio = new("audio/3gpp2", ".3g2");
        public static readonly MimeType Dsd = new("audio/dsd", ".dsf", ".dff");
        public static readonly MimeType Ape = new("audio/x-ape", ".ape");
        public static readonly MimeType Mka = new("audio/x-matroska", ".mka");
        public static readonly MimeType Ra = new("audio/vnd.rn-realaudio", ".ra", ".ram");
        public static readonly MimeType Ac3 = new("audio/ac3", ".ac3");
        public static readonly MimeType Dts = new("audio/vnd.dts", ".dts");

        internal static readonly IList<MimeType> All = new[] { Any, Aac, Mp3, Midi, CompactDiskAudio, OggAudio, OpusAudio, Waveform, WebmAudio, Flac, Mp4Audio, AlacAudio, Wma, Aiff, Au, Amr, _3gppAudio, _3gpp2Audio, Dsd, Ape, Mka, Ra, Ac3, Dts }.ToList().AsReadOnly();
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    public static class Video
    {
        public static readonly MimeType Any = new("video/*");
        public static readonly MimeType _3gp = new("video/3gpp", "video/3gp", ".3gp");
        public static readonly MimeType Avi = new("video/x-msvideo", "video/avi", ".avi");
        public static readonly MimeType Mp4Video = new("video/mp4", ".mp4");
        public static readonly MimeType MpegVideo = new("video/mpeg", ".mpeg", ".mpg");
        public static readonly MimeType OggVideo = new("video/ogg", ".ogv");
        public static readonly MimeType MpegTransportStream = new("video/mp2t", ".ts");
        public static readonly MimeType WebmVideo = new("video/webm", ".webm");
        public static readonly MimeType _3gp2 = new("video/3gpp2", ".3g2");
        public static readonly MimeType Flv = new("video/x-flv", ".flv");
        public static readonly MimeType H264 = new("video/h264", ".h264");
        public static readonly MimeType Quicktime = new("video/quicktime", ".mov", ".qt");
        public static readonly MimeType Wmv = new("video/x-ms-wmv", ".wmv");

        // Modern and missing video formats
        public static readonly MimeType Mkv = new("video/x-matroska", ".mkv");
        public static readonly MimeType Asf = new("video/x-ms-asf", ".asf");
        public static readonly MimeType Vob = new("video/dvd", ".vob");
        public static readonly MimeType M4v = new("video/x-m4v", ".m4v");
        public static readonly MimeType Rm = new("application/vnd.rn-realmedia", ".rm");
        public static readonly MimeType Rmvb = new("application/vnd.rn-realmedia-vbr", ".rmvb");
        public static readonly MimeType Mxf = new("application/mxf", ".mxf");
        public static readonly MimeType F4v = new("video/x-f4v", ".f4v");
        public static readonly MimeType Divx = new("video/divx", ".divx");
        public static readonly MimeType Xvid = new("video/x-msvideo", ".xvid");
        public static readonly MimeType H265 = new("video/h265", ".h265");
        public static readonly MimeType Hevc = new("video/hevc", ".hevc");
        public static readonly MimeType Av1 = new("video/av01", ".av01");
        public static readonly MimeType Vp8 = new("video/vp8", ".vp8");
        public static readonly MimeType Vp9 = new("video/vp9", ".vp9");
        public static readonly MimeType M2ts = new("video/mp2t", ".m2ts", ".mts");
        public static readonly MimeType Dv = new("video/dv", ".dv");
        public static readonly MimeType Mpg2 = new("video/mpeg2", ".mpg2");
        public static readonly MimeType Mpg4 = new("video/mp4v-es", ".mp4v");

        internal static readonly IList<MimeType> All = new[] { Any, _3gp, Avi, Flv, H264, Quicktime, Wmv, Mp4Video, MpegVideo, OggVideo, MpegTransportStream, WebmVideo, _3gp2, Mkv, Asf, Vob, M4v, Rm, Rmvb, Mxf, F4v, Divx, Xvid, H265, Hevc, Av1, Vp8, Vp9, M2ts, Dv, Mpg2, Mpg4 }.ToList().AsReadOnly();
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    public static IList<MimeType> AllMimeTypes
    {
        get
        {
            if (field == null)
            {
                lock (typeof(MimeType))
                {
                    field ??= Audio.All.Union(Video.All).Union(Text.All).Union(Image.All).Union(Application.All).ToList();
                }
            }
            return field;
        }
    }

    public static MimeType FindByExtension(string extension)
    {
        extension = Path.GetExtension(extension) ?? "";
        foreach (var m in AllMimeTypes)
        {
            if (0 == string.Compare(m.PrimaryFileExtension, extension, true)) return m;
        }
        foreach (var m in AllMimeTypes)
        {
            if (m.DoesExtensionMatch(extension)) return m;
        }
        return null;
    }

    public static IList<MimeType> FindByContentType(string contentType, bool caseSensitive = false)
        => AllMimeTypes.Where(m => m.DoesContentTypeMatch(contentType, caseSensitive)).ToList();

    public readonly IList<string> ContentTypes = [];

    public readonly IList<string> FileExtensions = [];

    public string PrimaryFileExtension
        => FileExtensions.FirstOrDefault();

    public string PrimaryContentType
        => ContentTypes.FirstOrDefault();

    public override string ToString()
        => $"{GetType().Name} contentType={PrimaryContentType} fileExtension={PrimaryFileExtension}";


    public static implicit operator MediaTypeHeaderValue(MimeType ct)
        => new(ct.PrimaryContentType);

    public static implicit operator string(MimeType ct)
        => ct.PrimaryContentType;

    public static implicit operator MimeType(string contentType)
        => new(contentType);

    public MimeType(string contentType, params string[] contentTypeOrFileExtensions)
    {
        Requires.Match(ContentTypeExpr, contentType, nameof(contentType));
        ContentTypes.Add(contentType);
        if (contentTypeOrFileExtensions != null)
        {
            foreach (var item in contentTypeOrFileExtensions)
            {
                if (ContentTypeExpr.IsMatch(item))
                {
                    ContentTypes.Add(item);
                }
                else if (FileExtensionExpr.IsMatch(item))
                {
                    FileExtensions.Add(item);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(contentTypeOrFileExtensions), $"[{item}] must be either a contentType or a file Extension");
                }
            }
        }
    }

    public bool DoesContentTypeMatch(string contentType, bool caseSensitive = false)
    {
        if (contentType == null) return false;
        contentType = contentType.LeftOf(";").TrimOrNull();
        foreach (var ct in ContentTypes)
        {
            if (0 == string.Compare(ct, contentType, !caseSensitive)) return true;
        }
        return false;
    }

    public bool DoesExtensionMatch(string filename, bool caseSensitive = false)
    {
        if (filename == null) return false;
        var ext = Path.GetExtension(filename);
        foreach (var e in FileExtensions)
        {
            if (0 == string.Compare(e, ext, !caseSensitive)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns if contentTypeA IsA ContentTypeB
    /// Both A and B may contain '*' in either their primary or secondary parts
    /// </summary>
    /// <param name="contentTypeA">The first contentType</param>
    /// <param name="contentTypeB">The second contentType.</param>
    /// <returns>true if they contentTypeA IsA ContentTypeB</returns>
    public static bool IsA(string contentTypeA, string contentTypeB)
    {
        if (string.IsNullOrEmpty(contentTypeA) || string.IsNullOrEmpty(contentTypeB)) return false;
        try
        {
            contentTypeA = contentTypeA.LeftOf(";").Trim().ToLower();
            contentTypeA.Split("/", true, out var al, out var ar);

            contentTypeB = contentTypeB.LeftOf(";").Trim().ToLower();
            contentTypeB.Split("/", true, out var bl, out var br);

            return
                       (bl == "*" && br == "*") ||
                       (bl == al && br == "*") ||
                       (bl == al && ar == br) ||
                       (bl == al && ar.Length > br.Length + 1 && ar[br.Length] == '+' && ar.StartsWith(br));
        }
        catch (Exception)
        {
            return false;
        }
    }
}
