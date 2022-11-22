using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core;

public class MimeType
{
    private static readonly Regex ContentTypeExpr = new(@"[^\s/]+/[^\s/]+", RegexOptions.Compiled);
    private static readonly Regex FileExtensionExpr = new(@"\.\w+", RegexOptions.Compiled);

    public static string GetContentTypeType(string contentType)
        => contentType.LeftOf("/");

    public static string GetContentTypeSubType(string contentType)
        => contentType.RightOf("/");

    public static class Application
    {
        public static readonly MimeType Any = "application/*";
        public static readonly MimeType Json = new("application/json", ".json");
        public static readonly MimeType Xml = new("application/xml", ".xml");
        public static readonly MimeType OctetStream = "application/octet-stream";
        public static readonly MimeType SqlServerIntegrationServicesEtlPackage = new(OctetStream, ".dtsx");
        public static readonly MimeType Pdf = new("application/pdf", ".pdf");
        public static readonly MimeType Zip = new("application/zip", ".zip");

        public static class SpreadSheet
        {
            public static readonly MimeType Xlsx = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
            internal static readonly IList<MimeType> All = new[] { Xlsx }.ToList().AsReadOnly();
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

        internal static readonly IList<MimeType> All = new[] { Any, Json, Xml, OctetStream, SqlServerIntegrationServicesEtlPackage, Pdf, Zip }.Union(SpreadSheet.All).Union(Encryption.All).ToList().AsReadOnly();
    }

    public static class Image
    {
        public static readonly MimeType Any = new("image/*");
        public static readonly MimeType Bmp = new("image/bmp", ".bmp");
        public static readonly MimeType Gif = new("image/gif", ".gif");
        public static readonly MimeType Jpg = new("image/jpeg", ".jpg", ".jpeg", ".jpe");
        public static readonly MimeType Png = new("image/png", ".png");
        public static readonly MimeType Tiff = new("image/tiff", ".tif", ".tiff");
        public static readonly MimeType WebP = new("image/webp", ".webp");
        internal static readonly IList<MimeType> All = new[] { Any, Bmp, Gif, Jpg, Png, Tiff, WebP }.ToList().AsReadOnly();
    }

    public static class Text
    {
        public static readonly MimeType Any = new("text/*");
        public static readonly MimeType Html = new("text/html", ".html", ".htm");
        public static readonly MimeType Plain = new("text/plain", ".txt", ".text");
        public static readonly MimeType Markdown = new("text/markdown", ".md");
        internal static readonly IList<MimeType> All = new[] { Any, Html, Plain, Markdown }.ToList().AsReadOnly();
    }

    public static class Video
    {
        public static readonly MimeType Any = new("video/*");
        public static readonly MimeType _3gp = new("video/3gp");
        public static readonly MimeType Avi = new("video/avi", ".avi");
        public static readonly MimeType Flv = new("video/x-flv", "video/flv", ".flv");
        public static readonly MimeType H264 = new("video/h264");
        public static readonly MimeType Quicktime = new("video/quicktime", ".mov", ".qt");
        public static readonly MimeType Wmv = new("video/x-ms-wmv", "video/wmv", ".wmv");
        internal static readonly IList<MimeType> All = new[] { Any, _3gp, Avi, Flv, H264, Quicktime, Wmv }.ToList().AsReadOnly();
    }

    public static IList<MimeType> AllMimeTypes
    {
        get
        {
            if (AllMimeTypes_p == null)
            {
                lock (typeof(MimeType))
                {
                    AllMimeTypes_p ??= Video.All.Union(Text.All).Union(Image.All).Union(Application.All).ToList();
                }
            }
            return AllMimeTypes_p;
        }
    }
    private static IList<MimeType> AllMimeTypes_p = null;

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

    public readonly IList<string> ContentTypes = new List<string>();

    public readonly IList<string> FileExtensions = new List<string>();

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
