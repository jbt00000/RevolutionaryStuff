using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace RevolutionaryStuff.Core
{
    public class MimeType
    {
        private static readonly Regex ContentTypeExpr = new Regex(@"[^\s/]+/[^\s/]+", RegexOptions.Compiled);
        private static readonly Regex FileExtensionExpr = new Regex(@"\.\w+", RegexOptions.Compiled);

        public static string GetContentTypeType(string contentType)
            => contentType.LeftOf("/");

        public static string GetContentTypeSubType(string contentType)
            => contentType.RightOf("/");

        public static class Application
        {
            public static readonly MimeType Any = "application/*";
            public static readonly MimeType Json = "application/json";
            public static readonly MimeType OctetStream = "application/octet-stream";
            public static readonly MimeType SqlServerIntegrationServicesEtlPackage = new MimeType(OctetStream, ".dtsx");
            public static readonly MimeType Pdf = new MimeType("application/pdf", ".pdf");
            public static class SpreadSheet
            {
                public static readonly MimeType Xlsx = new MimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
            }
        }

        public static class Image
        {
            public static readonly MimeType Any = new MimeType("image/*");
            public static readonly MimeType Bmp = new MimeType("image/bmp", ".bmp");
            public static readonly MimeType Gif = new MimeType("image/gif", ".gif");
            public static readonly MimeType Jpg = new MimeType("image/jpeg", ".jpg");
            public static readonly MimeType Png = new MimeType("image/png", ".png");
            public static readonly MimeType Tiff = new MimeType("image/tiff", ".tif", ".tiff");
        }

        public static class Text
        {
            public static readonly MimeType Any = "text/*";
            public static readonly MimeType Html = new MimeType("text/html", ".html", ".htm");
            public static readonly MimeType Plain = new MimeType("text/plain", ".txt");
        }

        public static class Video
        {
            public static readonly MimeType Any = new MimeType("video/*");
            public static readonly MimeType _3gp = new MimeType("video/3gp");
            public static readonly MimeType Avi = new MimeType("video/avi", ".avi");
            public static readonly MimeType Flv = new MimeType("video/x-flv", "video/flv", ".flv");
            public static readonly MimeType H264 = new MimeType("video/h264");
            public static readonly MimeType Quicktime = new MimeType("video/quicktime", ".mov", ".qt");
            public static readonly MimeType Wmv = new MimeType("video/x-ms-wmv", "video/wmv", ".wmv");
        }

        public readonly IList<string> ContentTypes = new List<string>();

        public readonly IList<string> FileExtensions = new List<string>();

        public string PrimaryFileExtension
            => FileExtensions.FirstOrDefault();

        public string PrimaryContentType
            => ContentTypes.FirstOrDefault();

        public override string ToString()
            => $"{this.GetType().Name} contentType={this.PrimaryContentType} fileExtension={this.PrimaryFileExtension}";

        
        public static implicit operator MediaTypeHeaderValue(MimeType ct)
            => new MediaTypeHeaderValue(ct.PrimaryContentType);

        public static implicit operator string(MimeType ct)
            => ct.PrimaryContentType;

        public static implicit operator MimeType(string contentType)
            => new MimeType(contentType);

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
                string al, ar;
                contentTypeA = contentTypeA.LeftOf(";").Trim().ToLower();
                StringHelpers.Split(contentTypeA, "/", true, out al, out ar);

                string bl, br;
                contentTypeB = contentTypeB.LeftOf(";").Trim().ToLower();
                StringHelpers.Split(contentTypeB, "/", true, out bl, out br);

                return (
                           (bl == "*" && br == "*") ||
                           (bl == al && br == "*") ||
                           (bl == al && ar == br) ||
                           (bl == al && ar.Length > br.Length + 1 && ar[br.Length] == '+' && ar.StartsWith(br)));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
