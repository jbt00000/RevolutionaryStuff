using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Represents MIME types (Media Types) for various file formats and content types.
/// Provides utilities for matching content types and file extensions, and contains
/// comprehensive definitions for common and modern media formats.
/// </summary>
/// <remarks>
/// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
/// https://www.iana.org/assignments/media-types/media-types.xhtml
/// </remarks>
public sealed partial class MimeType
{
    /// <summary>
    /// Regular expression for validating content type format (type/subtype).
    /// </summary>
    [GeneratedRegex(@"[^\s/]+/[^\s/]+")]
    private static partial Regex ContentTypeExpr { get; }

    /// <summary>
    /// Regular expression for validating file extension format (.ext).
    /// </summary>
    [GeneratedRegex(@"\.\w+")]
    private static partial Regex FileExtensionExpr { get; }

    /// <summary>
    /// Extracts the primary type from a content type string (e.g., "image" from "image/png").
    /// </summary>
    /// <param name="contentType">The full content type string.</param>
    /// <returns>The primary type portion before the slash.</returns>
    public static string GetContentTypeType(string contentType)
        => contentType.LeftOf("/");

    /// <summary>
    /// Extracts the subtype from a content type string (e.g., "png" from "image/png").
    /// </summary>
    /// <param name="contentType">The full content type string.</param>
    /// <returns>The subtype portion after the slash.</returns>
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

    /// <summary>
    /// Determines if a content type or file extension represents an image format.
    /// </summary>
    /// <param name="contentTypeOrExtension">Content type string or file extension to test.</param>
    /// <returns>True if the input matches a known image format; otherwise, false.</returns>
    public static bool IsImage(string contentTypeOrExtension)
        => Image.HasMatch(contentTypeOrExtension);

    /// <summary>
    /// Determines if a content type or file extension represents a video format.
    /// </summary>
    /// <param name="contentTypeOrExtension">Content type string or file extension to test.</param>
    /// <returns>True if the input matches a known video format; otherwise, false.</returns>
    public static bool IsVideo(string contentTypeOrExtension)
        => Video.HasMatch(contentTypeOrExtension);

    /// <summary>
    /// Application-specific MIME types including documents, archives, fonts, and data formats.
    /// </summary>
    public static class Application
    {
        /// <summary>Wildcard for any application type.</summary>
        public static readonly MimeType Any = "application/*";

        /// <summary>JSON data format.</summary>
        public static readonly MimeType Json = new("application/json", ".json");

        /// <summary>JSON-LD (Linked Data) format.</summary>
        public static readonly MimeType JsonLd = new("application/ld+json", ".jsonld");

        /// <summary>XML data format.</summary>
        public static readonly MimeType Xml = new("application/xml", ".xml");

        /// <summary>Generic binary data / unknown file type.</summary>
        public static readonly MimeType OctetStream = "application/octet-stream";

        /// <summary>SQL Server Integration Services ETL package.</summary>
        public static readonly MimeType SqlServerIntegrationServicesEtlPackage = new(OctetStream, ".dtsx");

        /// <summary>Adobe Portable Document Format.</summary>
        public static readonly MimeType Pdf = new("application/pdf", ".pdf");

        /// <summary>Amazon Kindle eBook format.</summary>
        public static readonly MimeType AmazonKindle = new("application/vnd.amazon.ebook", ".azw");

        /// <summary>EPUB eBook format.</summary>
        public static readonly MimeType Epub = new("application/epub+zip", ".epub");

        /// <summary>JavaScript code.</summary>
        public static readonly MimeType JavaScript = new("application/javascript", "text/javascript", ".js", ".mjs");

        /// <summary>TypeScript code.</summary>
        public static readonly MimeType TypeScript = new("application/typescript", ".ts");

        /// <summary>WebAssembly binary format.</summary>
        public static readonly MimeType Wasm = new("application/wasm", ".wasm");

        /// <summary>Rich Text Format.</summary>
        public static readonly MimeType Rtf = new("application/rtf", ".rtf");

        /// <summary>Shell script.</summary>
        public static readonly MimeType Shell = new("application/x-sh", ".sh");

        /// <summary>RAR archive.</summary>
        public static readonly MimeType Rar = new("application/vnd.rar", "application/x-rar-compressed", ".rar");

        /// <summary>Apple disk image.</summary>
        public static readonly MimeType Dmg = new("application/x-apple-diskimage", ".dmg");

        /// <summary>Debian package.</summary>
        public static readonly MimeType Deb = new("application/vnd.debian.binary-package", ".deb");

        /// <summary>
        /// Archive and compression formats.
        /// </summary>
        public static class Container
        {
            /// <summary>7-Zip archive.</summary>
            public static readonly MimeType _7Zip = new("application/x-7z-compressed", ".7z");

            /// <summary>FreeArc archive.</summary>
            public static readonly MimeType ArchiveDocument = new("application/x-freearc", ".arc");

            /// <summary>BZip archive.</summary>
            public static readonly MimeType BZipArchive = new("application/x-bzip", ".bz");

            /// <summary>BZip2 archive.</summary>
            public static readonly MimeType BZip2Archive = new("application/x-bzip2", ".bz2");

            /// <summary>Tape Archive (tar).</summary>
            public static readonly MimeType Tar = new("application/x-tar", ".tar");

            /// <summary>ZIP archive.</summary>
            public static readonly MimeType Zip = new("application/zip", ".zip");

            /// <summary>GZip compressed file.</summary>
            public static readonly MimeType GZip = new("application/gzip", ".gz");

            /// <summary>XZ compressed file.</summary>
            public static readonly MimeType Xz = new("application/x-xz", ".xz");

            /// <summary>Zstandard compressed file.</summary>
            public static readonly MimeType Zstd = new("application/zstd", ".zst");

            /// <summary>ISO disk image.</summary>
            public static readonly MimeType Iso = new("application/x-iso9660-image", ".iso");

            internal static readonly IList<MimeType> All = new[] { ArchiveDocument, BZipArchive, BZip2Archive, _7Zip, Tar, Zip, GZip, Xz, Zstd, Iso }.ToList().AsReadOnly();
        }

        /// <summary>
        /// Font file formats.
        /// </summary>
        public static class Font
        {
            /// <summary>OpenType font.</summary>
            public static readonly MimeType OpenTypeFont = new("font/otf", ".otf");

            /// <summary>TrueType font.</summary>
            public static readonly MimeType TrueTypeFont = new("font/ttf", ".ttf");

            /// <summary>Web Open Font Format.</summary>
            public static readonly MimeType WebOpenFontFormat = new("font/woff", ".woff");

            /// <summary>Web Open Font Format 2.</summary>
            public static readonly MimeType WebOpenFontFormat2 = new("font/woff2", ".woff2");

            /// <summary>Embedded OpenType font.</summary>
            public static readonly MimeType Eot = new("application/vnd.ms-fontobject", ".eot");

            internal static readonly IList<MimeType> All = new[] { OpenTypeFont, TrueTypeFont, WebOpenFontFormat, WebOpenFontFormat2, Eot }.ToList().AsReadOnly();
        }

        /// <summary>
        /// Presentation document formats.
        /// </summary>
        public static class Presentation
        {
            /// <summary>Microsoft PowerPoint (.ppt).</summary>
            public static readonly MimeType MicrosoftPowerPoint = new("application/vnd.ms-powerpoint", ".ppt");

            /// <summary>Microsoft PowerPoint OpenXML (.pptx).</summary>
            public static readonly MimeType MicrosoftPowerPointOpenXML = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx");

            /// <summary>OpenDocument Presentation (.odp).</summary>
            public static readonly MimeType OpenDocumentPresentation = new("application/vnd.oasis.opendocument.presentation", ".odp");

            /// <summary>Apple Keynote presentation.</summary>
            public static readonly MimeType Keynote = new("application/x-iwork-keynote-sffkey", ".key");

            internal static readonly IList<MimeType> All = new[] { MicrosoftPowerPoint, MicrosoftPowerPointOpenXML, OpenDocumentPresentation, Keynote }.ToList().AsReadOnly();
        }

        /// <summary>
        /// Word processing document formats.
        /// </summary>
        public static class WordProcessing
        {
            /// <summary>Microsoft Word (.doc).</summary>
            public static readonly MimeType MicrosoftWord = new("application/msword", ".doc");

            /// <summary>Microsoft Word OpenXML (.docx).</summary>
            public static readonly MimeType MicrosoftWordOpenXml = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");

            /// <summary>OpenDocument Text (.odt).</summary>
            public static readonly MimeType OpenDocumentTextDocument = new("application/vnd.oasis.opendocument.text", ".odt");

            /// <summary>Apple Pages document.</summary>
            public static readonly MimeType Pages = new("application/x-iwork-pages-sffpages", ".pages");

            internal static readonly IList<MimeType> All = new[] { MicrosoftWord, MicrosoftWordOpenXml, OpenDocumentTextDocument, Pages }.ToList().AsReadOnly();
        }

        /// <summary>
        /// Spreadsheet document formats.
        /// </summary>
        public static class SpreadSheet
        {
            /// <summary>OpenDocument Spreadsheet (.ods).</summary>
            public static readonly MimeType OpenDocumentSpreadsheetDocument = new("application/vnd.oasis.opendocument.spreadsheet", ".ods");

            /// <summary>Microsoft Excel (.xls).</summary>
            public static readonly MimeType MicrosoftExcel = new("application/vnd.ms-excel", ".xls");

            /// <summary>Microsoft Excel OpenXML (.xlsx).</summary>
            public static readonly MimeType MicrosoftExcelOpenXml = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");

            /// <summary>Apple Numbers spreadsheet.</summary>
            public static readonly MimeType Numbers = new("application/x-iwork-numbers-sffnumbers", ".numbers");

            internal static readonly IList<MimeType> All = new[] { OpenDocumentSpreadsheetDocument, MicrosoftExcel, MicrosoftExcelOpenXml, Numbers }.ToList().AsReadOnly();
        }

        /// <summary>
        /// Encryption and security-related formats.
        /// </summary>
        public static class Encryption
        {
            /// <summary>
            /// PGP (Pretty Good Privacy) encryption formats.
            /// </summary>
            public static class PGP
            {
                /// <summary>PGP encrypted data.</summary>
                public static readonly MimeType PgpEncrypted = new("application/pgp-encrypted", ".pgp");

                /// <summary>PGP signature.</summary>
                public static readonly MimeType PgpSignature = new("application/pgp-signature", ".sig", ".asc");

                /// <summary>PGP keys.</summary>
                public static readonly MimeType PgpKeys = new("application/pgp-keys", ".asc", ".pgp");

                internal static readonly IList<MimeType> All = new[] { PgpEncrypted, PgpSignature, PgpKeys }.ToList().AsReadOnly();
            }

            internal static readonly IList<MimeType> All = PGP.All.ToList().AsReadOnly();
        }

        internal static readonly IList<MimeType> All = new[] { Any, Json, JsonLd, Xml, OctetStream, SqlServerIntegrationServicesEtlPackage, Pdf, AmazonKindle, Epub, JavaScript, TypeScript, Wasm, Rtf, Shell, Rar, Dmg, Deb }.Union(Container.All).Union(Font.All).Union(WordProcessing.All).Union(SpreadSheet.All).Union(Presentation.All).Union(Encryption.All).ToList().AsReadOnly();
    }

    /// <summary>
    /// Image file formats including raster, vector, and modern compressed formats.
    /// </summary>
    public static class Image
    {
        /// <summary>Wildcard for any image type.</summary>
        public static readonly MimeType Any = new("image/*");

        /// <summary>Bitmap image (.bmp).</summary>
        public static readonly MimeType Bmp = new("image/bmp", ".bmp");

        /// <summary>Graphics Interchange Format (.gif).</summary>
        public static readonly MimeType Gif = new("image/gif", ".gif");

        /// <summary>JPEG image (.jpg, .jpeg).</summary>
        public static readonly MimeType Jpg = new("image/jpeg", ".jpg", ".jpeg", ".jpe");

        /// <summary>Portable Network Graphics (.png).</summary>
        public static readonly MimeType Png = new("image/png", ".png");

        /// <summary>Scalable Vector Graphics (.svg).</summary>
        public static readonly MimeType Svg = new("image/svg+xml", ".svg");

        /// <summary>Tagged Image File Format (.tif, .tiff).</summary>
        public static readonly MimeType Tiff = new("image/tiff", ".tif", ".tiff");

        /// <summary>WebP image format (.webp).</summary>
        public static readonly MimeType WebP = new("image/webp", ".webp");

        // Modern and advanced image formats
        /// <summary>AV1 Image File Format - next-gen compressed format (.avif).</summary>
        public static readonly MimeType Avif = new("image/avif", ".avif");

        /// <summary>High Efficiency Image Container (.heic) - Apple's preferred format.</summary>
        public static readonly MimeType Heic = new("image/heic", ".heic");

        /// <summary>High Efficiency Image Format (.heif).</summary>
        public static readonly MimeType Heif = new("image/heif", ".heif");

        /// <summary>JPEG XL - next-generation JPEG format (.jxl).</summary>
        public static readonly MimeType Jxl = new("image/jxl", ".jxl");

        /// <summary>Icon file (.ico) - typically used for favicons.</summary>
        public static readonly MimeType Icon = new("image/x-icon", ".ico");

        /// <summary>Animated PNG (.apng).</summary>
        public static readonly MimeType Apng = new("image/apng", ".apng");

        /// <summary>JPEG File Interchange Format (.jfif).</summary>
        public static readonly MimeType Jfif = new("image/jpeg", ".jfif");

        /// <summary>Adobe Photoshop Document (.psd).</summary>
        public static readonly MimeType Psd = new("image/vnd.adobe.photoshop", ".psd");

        /// <summary>Camera RAW formats (.cr2, .nef, .arw, .dng).</summary>
        public static readonly MimeType Raw = new("image/x-canon-cr2", ".cr2", ".nef", ".arw", ".dng", ".orf", ".rw2");

        /// <summary>PC Paintbrush (.pcx).</summary>
        public static readonly MimeType Pcx = new("image/x-pcx", ".pcx");

        /// <summary>Truevision TGA (.tga).</summary>
        public static readonly MimeType Tga = new("image/x-targa", ".tga");

        /// <summary>X11 Bitmap (.xbm).</summary>
        public static readonly MimeType Xbm = new("image/x-xbitmap", ".xbm");

        /// <summary>X11 Pixmap (.xpm).</summary>
        public static readonly MimeType Xpm = new("image/x-xpixmap", ".xpm");

        /// <summary>JPEG 2000 (.jp2, .j2k).</summary>
        public static readonly MimeType Jpeg2000 = new("image/jp2", ".jp2", ".j2k", ".jpf", ".jpm", ".jpg2");

        /// <summary>Windows Cursor (.cur).</summary>
        public static readonly MimeType Cursor = new("image/x-icon", ".cur");

        internal static readonly IList<MimeType> All = new[] { Any, Bmp, Gif, Jpg, Png, Svg, Tiff, WebP, Avif, Heic, Heif, Jxl, Icon, Apng, Jfif, Psd, Raw, Pcx, Tga, Xbm, Xpm, Jpeg2000, Cursor }.ToList().AsReadOnly();

        /// <summary>
        /// Determines if a content type or file extension represents an image format.
        /// </summary>
        /// <param name="contentTypeOrExtension">Content type string or file extension to test.</param>
        /// <returns>True if the input matches a known image format; otherwise, false.</returns>
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    /// <summary>
    /// Text-based document formats.
    /// </summary>
    public static class Text
    {
        /// <summary>Wildcard for any text type.</summary>
        public static readonly MimeType Any = new("text/*");

        /// <summary>Comma-Separated Values (.csv).</summary>
        public static readonly MimeType Csv = new("text/csv", ".csv");

        /// <summary>HyperText Markup Language (.html, .htm).</summary>
        public static readonly MimeType Html = new("text/html", ".html", ".htm");

        /// <summary>Plain text (.txt).</summary>
        public static readonly MimeType Plain = new("text/plain", ".txt", ".text");

        /// <summary>Markdown (.md).</summary>
        public static readonly MimeType Markdown = new("text/markdown", ".md", ".markdown");

        /// <summary>Cascading Style Sheets (.css).</summary>
        public static readonly MimeType Css = new("text/css", ".css");

        /// <summary>Calendar data (.ics).</summary>
        public static readonly MimeType Calendar = new("text/calendar", ".ics");

        /// <summary>vCard contact information (.vcf).</summary>
        public static readonly MimeType VCard = new("text/vcard", ".vcf");

        /// <summary>YAML data (.yaml, .yml).</summary>
        public static readonly MimeType Yaml = new("text/yaml", "application/x-yaml", ".yaml", ".yml");

        /// <summary>TOML configuration (.toml).</summary>
        public static readonly MimeType Toml = new("application/toml", ".toml");

        internal static readonly IList<MimeType> All = new[] { Any, Csv, Html, Plain, Markdown, Css, Calendar, VCard, Yaml, Toml }.ToList().AsReadOnly();

        /// <summary>
        /// Determines if a content type or file extension represents a text format.
        /// </summary>
        /// <param name="contentTypeOrExtension">Content type string or file extension to test.</param>
        /// <returns>True if the input matches a known text format; otherwise, false.</returns>
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    /// <summary>
    /// Audio file formats including lossy, lossless, and streaming formats.
    /// </summary>
    public static class Audio
    {
        /// <summary>Wildcard for any audio type.</summary>
        public static readonly MimeType Any = new("audio/*");

        /// <summary>Advanced Audio Coding (.aac).</summary>
        public static readonly MimeType Aac = new("audio/aac", ".aac");

        /// <summary>Compact Disc Audio (.cda).</summary>
        public static readonly MimeType CompactDiskAudio = new("application/x-cdf", ".cda");

        /// <summary>Musical Instrument Digital Interface (.mid, .midi).</summary>
        public static readonly MimeType Midi = new("audio/midi", "audio/x-midi", ".mid", ".midi");

        /// <summary>MP3 audio (.mp3).</summary>
        public static readonly MimeType Mp3 = new("audio/mpeg", ".mp3");

        /// <summary>Ogg Vorbis audio (.oga).</summary>
        public static readonly MimeType OggAudio = new("audio/ogg", ".oga");

        /// <summary>Opus audio codec (.opus).</summary>
        public static readonly MimeType OpusAudio = new("audio/opus", ".opus");

        /// <summary>Waveform Audio File (.wav).</summary>
        public static readonly MimeType Waveform = new("audio/wav", ".wav");

        /// <summary>WebM audio (.weba).</summary>
        public static readonly MimeType WebmAudio = new("audio/webm", ".weba");

        // Modern and lossless formats
        /// <summary>Free Lossless Audio Codec (.flac).</summary>
        public static readonly MimeType Flac = new("audio/flac", ".flac");

        /// <summary>MPEG-4 Audio (.m4a).</summary>
        public static readonly MimeType Mp4Audio = new("audio/mp4", ".m4a", ".mp4a");

        /// <summary>Apple Lossless Audio Codec (.m4a).</summary>
        public static readonly MimeType AlacAudio = new("audio/x-m4a", ".m4a");

        /// <summary>Windows Media Audio (.wma).</summary>
        public static readonly MimeType Wma = new("audio/x-ms-wma", ".wma");

        /// <summary>Audio Interchange File Format (.aif, .aiff).</summary>
        public static readonly MimeType Aiff = new("audio/x-aiff", ".aif", ".aiff", ".aifc");

        /// <summary>Sun/NeXT audio (.au, .snd).</summary>
        public static readonly MimeType Au = new("audio/basic", ".au", ".snd");

        /// <summary>Adaptive Multi-Rate audio codec (.amr).</summary>
        public static readonly MimeType Amr = new("audio/amr", ".amr");

        /// <summary>3GPP audio (.3gp).</summary>
        public static readonly MimeType _3gppAudio = new("audio/3gpp", ".3gp");

        /// <summary>3GPP2 audio (.3g2).</summary>
        public static readonly MimeType _3gpp2Audio = new("audio/3gpp2", ".3g2");

        /// <summary>Direct Stream Digital (.dsf, .dff).</summary>
        public static readonly MimeType Dsd = new("audio/dsd", ".dsf", ".dff");

        /// <summary>Monkey's Audio (.ape).</summary>
        public static readonly MimeType Ape = new("audio/x-ape", ".ape");

        /// <summary>Matroska Audio (.mka).</summary>
        public static readonly MimeType Mka = new("audio/x-matroska", ".mka");

        /// <summary>RealAudio (.ra, .ram).</summary>
        public static readonly MimeType Ra = new("audio/vnd.rn-realaudio", ".ra", ".ram");

        /// <summary>Dolby Digital AC-3 (.ac3).</summary>
        public static readonly MimeType Ac3 = new("audio/ac3", ".ac3");

        /// <summary>DTS audio (.dts).</summary>
        public static readonly MimeType Dts = new("audio/vnd.dts", ".dts");

        /// <summary>TrueHD audio (.thd).</summary>
        public static readonly MimeType TrueHd = new("audio/vnd.dolby.mlp", ".thd", ".mlp");

        /// <summary>MPEG-DASH (.mpd).</summary>
        public static readonly MimeType Dash = new("application/dash+xml", ".mpd");

        internal static readonly IList<MimeType> All = new[] { Any, Aac, Mp3, Midi, CompactDiskAudio, OggAudio, OpusAudio, Waveform, WebmAudio, Flac, Mp4Audio, AlacAudio, Wma, Aiff, Au, Amr, _3gppAudio, _3gpp2Audio, Dsd, Ape, Mka, Ra, Ac3, Dts, TrueHd, Dash }.ToList().AsReadOnly();

        /// <summary>
        /// Determines if a content type or file extension represents an audio format.
        /// </summary>
        /// <param name="contentTypeOrExtension">Content type string or file extension to test.</param>
        /// <returns>True if the input matches a known audio format; otherwise, false.</returns>
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    /// <summary>
    /// Video file formats including legacy, modern, and streaming formats.
    /// </summary>
    public static class Video
    {
        /// <summary>Wildcard for any video type.</summary>
        public static readonly MimeType Any = new("video/*");

        /// <summary>3GPP video (.3gp).</summary>
        public static readonly MimeType _3gp = new("video/3gpp", "video/3gp", ".3gp");

        /// <summary>Audio Video Interleave (.avi).</summary>
        public static readonly MimeType Avi = new("video/x-msvideo", "video/avi", ".avi");

        /// <summary>MPEG-4 video (.mp4).</summary>
        public static readonly MimeType Mp4Video = new("video/mp4", ".mp4");

        /// <summary>MPEG video (.mpeg, .mpg).</summary>
        public static readonly MimeType MpegVideo = new("video/mpeg", ".mpeg", ".mpg");

        /// <summary>Ogg Theora video (.ogv).</summary>
        public static readonly MimeType OggVideo = new("video/ogg", ".ogv");

        /// <summary>MPEG Transport Stream (.ts).</summary>
        public static readonly MimeType MpegTransportStream = new("video/mp2t", ".ts");

        /// <summary>WebM video (.webm).</summary>
        public static readonly MimeType WebmVideo = new("video/webm", ".webm");

        /// <summary>3GPP2 video (.3g2).</summary>
        public static readonly MimeType _3gp2 = new("video/3gpp2", ".3g2");

        /// <summary>Flash Video (.flv).</summary>
        public static readonly MimeType Flv = new("video/x-flv", ".flv");

        /// <summary>H.264 video (.h264).</summary>
        public static readonly MimeType H264 = new("video/h264", ".h264");

        /// <summary>QuickTime video (.mov, .qt).</summary>
        public static readonly MimeType Quicktime = new("video/quicktime", ".mov", ".qt");

        /// <summary>Windows Media Video (.wmv).</summary>
        public static readonly MimeType Wmv = new("video/x-ms-wmv", ".wmv");

        // Modern and advanced formats
        /// <summary>Matroska Video (.mkv) - open standard container.</summary>
        public static readonly MimeType Mkv = new("video/x-matroska", ".mkv");

        /// <summary>Advanced Systems Format (.asf).</summary>
        public static readonly MimeType Asf = new("video/x-ms-asf", ".asf");

        /// <summary>DVD Video Object (.vob).</summary>
        public static readonly MimeType Vob = new("video/dvd", ".vob");

        /// <summary>iTunes video (.m4v).</summary>
        public static readonly MimeType M4v = new("video/x-m4v", ".m4v");

        /// <summary>RealMedia (.rm).</summary>
        public static readonly MimeType Rm = new("application/vnd.rn-realmedia", ".rm");

        /// <summary>RealMedia Variable Bitrate (.rmvb).</summary>
        public static readonly MimeType Rmvb = new("application/vnd.rn-realmedia-vbr", ".rmvb");

        /// <summary>Material Exchange Format (.mxf) - professional video.</summary>
        public static readonly MimeType Mxf = new("application/mxf", ".mxf");

        /// <summary>Flash MP4 Video (.f4v).</summary>
        public static readonly MimeType F4v = new("video/x-f4v", ".f4v");

        /// <summary>DivX video (.divx).</summary>
        public static readonly MimeType Divx = new("video/divx", ".divx");

        /// <summary>Xvid video (.xvid).</summary>
        public static readonly MimeType Xvid = new("video/x-msvideo", ".xvid");

        /// <summary>H.265/HEVC video (.h265, .hevc).</summary>
        public static readonly MimeType H265 = new("video/h265", ".h265");

        /// <summary>High Efficiency Video Coding (.hevc).</summary>
        public static readonly MimeType Hevc = new("video/hevc", ".hevc");

        /// <summary>AV1 video codec (.av01) - next-gen compression.</summary>
        public static readonly MimeType Av1 = new("video/av01", ".av01");

        /// <summary>VP8 video codec (.vp8).</summary>
        public static readonly MimeType Vp8 = new("video/vp8", ".vp8");

        /// <summary>VP9 video codec (.vp9).</summary>
        public static readonly MimeType Vp9 = new("video/vp9", ".vp9");

        /// <summary>MPEG-2 Transport Stream (.m2ts, .mts) - Blu-ray format.</summary>
        public static readonly MimeType M2ts = new("video/mp2t", ".m2ts", ".mts");

        /// <summary>Digital Video (.dv).</summary>
        public static readonly MimeType Dv = new("video/dv", ".dv");

        /// <summary>MPEG-2 video (.mpg2).</summary>
        public static readonly MimeType Mpg2 = new("video/mpeg2", ".mpg2");

        /// <summary>MPEG-4 Visual (.mp4v).</summary>
        public static readonly MimeType Mpg4 = new("video/mp4v-es", ".mp4v");

        /// <summary>Apple ProRes (.prores).</summary>
        public static readonly MimeType ProRes = new("video/prores", ".prores");

        internal static readonly IList<MimeType> All = new[] { Any, _3gp, Avi, Flv, H264, Quicktime, Wmv, Mp4Video, MpegVideo, OggVideo, MpegTransportStream, WebmVideo, _3gp2, Mkv, Asf, Vob, M4v, Rm, Rmvb, Mxf, F4v, Divx, Xvid, H265, Hevc, Av1, Vp8, Vp9, M2ts, Dv, Mpg2, Mpg4, ProRes }.ToList().AsReadOnly();

        /// <summary>
        /// Determines if a content type or file extension represents a video format.
        /// </summary>
        /// <param name="contentTypeOrExtension">Content type string or file extension to test.</param>
        /// <returns>True if the input matches a known video format; otherwise, false.</returns>
        public static bool HasMatch(string contentTypeOrExtension)
            => MimeType.HasMatch(contentTypeOrExtension, All);
    }

    /// <summary>
    /// Gets a comprehensive list of all defined MIME types.
    /// Lazily initialized and thread-safe.
    /// </summary>
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

    /// <summary>
    /// Finds a MIME type by file extension.
    /// </summary>
    /// <param name="extension">The file extension (with or without leading period).</param>
    /// <returns>The matching MimeType if found; otherwise, null.</returns>
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

    /// <summary>
    /// Finds all MIME types that match a given content type.
    /// </summary>
    /// <param name="contentType">The content type to search for.</param>
    /// <param name="caseSensitive">Whether to perform case-sensitive matching.</param>
    /// <returns>A list of matching MimeType instances.</returns>
    public static IList<MimeType> FindByContentType(string contentType, bool caseSensitive = false)
        => AllMimeTypes.Where(m => m.DoesContentTypeMatch(contentType, caseSensitive)).ToList();

    /// <summary>
    /// List of content type strings associated with this MIME type.
    /// </summary>
    public readonly IList<string> ContentTypes = [];

    /// <summary>
    /// List of file extensions associated with this MIME type.
    /// </summary>
    public readonly IList<string> FileExtensions = [];

    /// <summary>
    /// Gets the primary (first) file extension for this MIME type.
    /// </summary>
    public string PrimaryFileExtension
        => FileExtensions.FirstOrDefault();

    /// <summary>
    /// Gets the primary (first) content type string for this MIME type.
    /// </summary>
    public string PrimaryContentType
        => ContentTypes.FirstOrDefault();

    /// <summary>
    /// Returns a string representation of this MIME type.
    /// </summary>
    public override string ToString()
        => $"{GetType().Name} contentType={PrimaryContentType} fileExtension={PrimaryFileExtension}";

    /// <summary>
    /// Implicitly converts a MimeType to a MediaTypeHeaderValue.
    /// </summary>
    public static implicit operator MediaTypeHeaderValue(MimeType ct)
        => new(ct.PrimaryContentType);

    /// <summary>
    /// Implicitly converts a MimeType to its primary content type string.
    /// </summary>
    public static implicit operator string(MimeType ct)
        => ct.PrimaryContentType;

    /// <summary>
    /// Implicitly converts a content type string to a MimeType.
    /// </summary>
    public static implicit operator MimeType(string contentType)
        => new(contentType);

    /// <summary>
    /// Creates a new MimeType instance.
    /// </summary>
    /// <param name="contentType">The primary content type (e.g., "image/png").</param>
    /// <param name="contentTypeOrFileExtensions">Additional content types or file extensions.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when contentType is invalid or when contentTypeOrFileExtensions contains invalid values.</exception>
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

    /// <summary>
    /// Determines if this MIME type matches a given content type string.
    /// </summary>
    /// <param name="contentType">The content type to test.</param>
    /// <param name="caseSensitive">Whether to perform case-sensitive comparison.</param>
    /// <returns>True if the content type matches; otherwise, false.</returns>
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

    /// <summary>
    /// Determines if this MIME type matches a given file extension.
    /// </summary>
    /// <param name="filename">The filename or extension to test.</param>
    /// <param name="caseSensitive">Whether to perform case-sensitive comparison.</param>
    /// <returns>True if the extension matches; otherwise, false.</returns>
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
    /// Determines if one content type "is a" subtype of another.
    /// Both content types may contain wildcards (*) in their primary or secondary parts.
    /// </summary>
    /// <param name="contentTypeA">The first content type to test.</param>
    /// <param name="contentTypeB">The second content type to test against.</param>
    /// <returns>True if contentTypeA is a subtype of contentTypeB; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// IsA("image/png", "image/*")      // returns true
    /// IsA("image/png", "*/*")          // returns true
    /// IsA("application/json", "text/*") // returns false
    /// </code>
    /// </example>
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
