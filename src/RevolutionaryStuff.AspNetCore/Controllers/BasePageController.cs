using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.AspNetCore.Controllers
{
    public abstract class BasePageController : Controller
    {
        public static string HomeControllerName = "Home";
        public static string HomeControllerHomeActionName = "Index";
        public static string IndexPageActionName = "Index";

        protected ActionResult RedirectToHome()
           => RedirectToAction(HomeControllerHomeActionName, HomeControllerName);

        protected virtual ActionResult RedirectToIndex()
            => RedirectToAction(IndexPageActionName);

        public override JsonResult Json(object data)
            => base.Json(data, JSO);

        public override JsonResult Json(object data, object serializerSettings)
            => base.Json(data, serializerSettings ?? JSO);

        private static JsonSerializerOptions JSO = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = null,
        };

        protected StatusCodeResult StatusCode(System.Net.HttpStatusCode code) => StatusCode((int)code);

        public static string ApplyPaginationPageSizeString = "rowsPerPage";
        protected virtual string GetApplyPaginationPageSizeStringValue()
            => Request.Cookies[ApplyPaginationPageSizeString];

        public static int ApplyPaginationDefaultRowsPerPage = 10;

        protected IQueryable<T> ApplyPagination<T>(IQueryable<T> q, int? page = null, int? pageSize = null)
        {
            if (pageSize == null)
            {
                var rowsPerPageString = GetApplyPaginationPageSizeStringValue();
                pageSize = Parse.ParseNullableInt32(rowsPerPageString);
            }
            var s = pageSize.GetValueOrDefault(ApplyPaginationDefaultRowsPerPage);
            var p = Stuff.Max(1, page.GetValueOrDefault());
            ViewBag.PaginationSupported = true;
            ViewBag.PageNum = p;
            ViewBag.PageSize = s;
            return q.Skip((p - 1) * s).Take(s);
        }

        private static readonly IDictionary<string, string> NoMappings = new Dictionary<string, string>().AsReadOnly();
        
        protected static readonly IDictionary<string, string> ApplySortDefaultMappings = new Dictionary<string, string>();

        protected IQueryable<T> ApplySort<T>(IQueryable<T> q, string sortCol, string sortDir, IDictionary<string, string> colMapper = null)
        {
            var m = new Dictionary<string, string>(colMapper ?? NoMappings);
            if (ApplySortDefaultMappings.Count > 0)
            {
                foreach (var kvp in ApplySortDefaultMappings)
                {
                    if (m.ContainsKey(kvp.Key)) continue;
                    m[kvp.Key] = kvp.Value;
                }
            }
            sortCol = StringHelpers.TrimOrNull(sortCol);
            bool isAscending = AspHelpers.IsSortDirAscending(sortDir);
            ViewBag.SortCol = sortCol;
            ViewBag.SortDir = sortDir;
            if (sortCol != null)
            {
                sortCol = m.FindOrMissing(sortCol, sortCol);
                q = q.OrderByField(sortCol, isAscending);
            }
            return q;
        }

        protected virtual int SetTotalItemCount<T>(IQueryable<T> q)
        {
            var cnt = q.Count();
            ViewBag.TotalItemCount = cnt;
            return cnt;
        }

        protected IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        {
            var cnt = SetTotalItemCount(q);
            if (cnt == 0)
            {
                q = (new T[0]).AsQueryable();
            }
            else
            {
                q = ApplySort(q, sortCol, sortDir, colMapper);
                q = ApplyPagination(q, page, pageSize);
            }
            return q;
        }
    }
}
