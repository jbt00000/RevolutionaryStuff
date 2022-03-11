using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.AspNetCore.Controllers;

public abstract class BasePageController : Controller
{
    public static string HomeControllerName = "Home";
    public static string HomeControllerHomeActionName = "Index";
    public static string IndexPageActionName = "Index";

    protected BasePageController(ILogger logger)
    {
        Requires.NonNull(logger, nameof(logger));

        Logger = logger;
    }

    #region Logging

    protected readonly ILogger Logger;

    protected void LogWarning(string message, params object[] args)
        => Logger.LogWarning(message, args);

    protected void LogInformation(string message, params object[] args)
        => Logger.LogInformation(message, args);

    protected void LogError(string message, params object[] args)
        => Logger.LogError(message, args);

    protected void LogError(Exception ex, string message, params object[] args)
        => Logger.LogError(ex, message, args);

    protected void LogException(Exception ex, [CallerMemberName] string caller = null)
        => Logger.LogError(ex, "Invoked from {caller}", caller);

    protected void LogDebug(string message, params object[] args)
        => Logger.LogDebug(message, args);

    protected void LogTrace(string message, params object[] args)
        => Logger.LogTrace(message, args);

    #endregion

    protected ActionResult RedirectToHome(object routeValues = null)
       => RedirectToAction(HomeControllerHomeActionName, HomeControllerName, routeValues);

    protected virtual ActionResult RedirectToIndex(object routeValues = null)
        => RedirectToAction(IndexPageActionName, routeValues);

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

    protected static readonly IDictionary<string, string> NoMappings = new Dictionary<string, string>().AsReadOnly();

    protected static readonly IDictionary<string, string> ApplySortDefaultMappings = new Dictionary<string, string>(Comparers.CaseInsensitiveStringComparer);

    protected virtual IQueryable<T> ApplySort<T>(IQueryable<T> q, Type sortColEnumType, string sortCol, string sortDir, IDictionary<string, string> colMapper = null, IEnumerable<string> orderedValues = null)
    {
        var m = new Dictionary<string, string>(colMapper ?? NoMappings, Comparers.CaseInsensitiveStringComparer);
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
            if (sortColEnumType == null)
            {
                q = q.OrderByField(sortCol, orderedValues, isAscending);
            }
            else
            {
                q = q.OrderByField(sortCol, sortColEnumType, isAscending);
            }
        }
        return q;
    }

    protected virtual int SetTotalItemCount<T>(IQueryable<T> q)
    {
        var cnt = q.Count();
        ViewBag.TotalItemCount = cnt;
        return cnt;
    }

    protected virtual IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, string sortCol, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null, IEnumerable<string> orderedValues = null)
        => ApplyBrowse(q, null, sortCol, sortDir, page, pageSize, colMapper, orderedValues);

    protected virtual IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, Type sortColEnumType, string sortCol, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null)
        => ApplyBrowse(q, sortColEnumType, sortCol, sortDir, page, pageSize, colMapper, null);

    private IQueryable<T> ApplyBrowse<T>(IQueryable<T> q, Type sortColEnumType, string sortCol, string sortDir, int? page, int? pageSize, IDictionary<string, string> colMapper = null, IEnumerable<string> orderedValues = null)
    {
        var cnt = SetTotalItemCount(q);
        if (cnt == 0)
        {
            q = (new T[0]).AsQueryable();
        }
        else
        {
            q = ApplySort(q, sortColEnumType, sortCol, sortDir, colMapper, orderedValues);
            q = ApplyPagination(q, page, pageSize);
        }
        return q;

    }

    protected void AddPageAlert(string toastMessage, bool autoDismiss = false, PageAlert.AlertTypes pageAlertType = PageAlert.AlertTypes.Info)
       => AddPageAlert(new PageAlert(toastMessage, autoDismiss, pageAlertType));

    protected void AddPageAlert(PageAlert pa)
        => TempData.AddPageAlert(pa);

    protected async Task ActAsync(Func<Task> executeAsync, [CallerMemberName] string caller = null)
    {
        try
        {
            LogInformation("{caller} function started processing", caller);
            Requires.NonNull(executeAsync, nameof(executeAsync));
            await executeAsync();
            LogInformation("{caller} function completed", caller);
        }
        catch (Exception ex)
        {
            LogException(ex);
            throw;
        }
    }
}
