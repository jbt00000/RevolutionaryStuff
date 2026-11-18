using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RevolutionaryStuff.AspNetCore;

public static class AspHelpers
{
    public static string GetControllerName<C>() where C : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        var name = typeof(C).Name;
        return StringHelpers.Coalesce(name.LeftOf("Controller"), name);
    }

    #region Sorting

    public static string SortDirAscending = "asc";

    public static string SortDirDescending = "desc";

    public static string SortColKeyName = "sortCol";

    public static string SortDirKeyName = "sortDir";

    public static bool IsSortDirAscending(string sortDir)
        => !StringHelpers.IsSameIgnoreCase(sortDir, SortDirDescending);

    public static bool IsSortDirDescending(string sortDir)
        => !IsSortDirAscending(sortDir);

    #endregion

    extension(Enum source)
    {
        public string GetDisplayName()
                => source.GetDisplayAttribute()?.GetName() ?? source.ToString();
        public string GetDescription()
                => source.GetDisplayAttribute()?.GetDescription();
        private DisplayAttribute GetDisplayAttribute()
        {
            var type = source.GetType();
            if (!type.IsEnum) throw new ArgumentException(string.Format("Type '{0}' is not Enum", type));

            var members = type.GetMember(source.ToString());
            if (members.Length == 0) throw new ArgumentException(string.Format("Member '{0}' not found in type '{1}'", source, type.Name));

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            return (DisplayAttribute)attributes.FirstOrDefault();
        }
    }





    #region SelectList

    extension(IEnumerable<SelectListItem> sources)
    {
        public IList<SelectListItem> CreateSelectList()
        {
            var selectList = new List<SelectListItem>();

            foreach (var selectListItem in sources)
            {
                selectList.Add(selectListItem);
            }
            return selectList;
        }
    }

    extension(System.Collections.IEnumerable sources)
    {
        public IList<SelectListItem> CreateSelectList()
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var o in sources)
            {
                if (o == null) continue;
                var val = o.ToString();
                stringsSelect.Add(new SelectListItem { Text = val, Value = val });
            }
            return stringsSelect;
        }
    }

    public static IList<SelectListItem> CreateSelectList<TEnum>(bool valAsName = true, bool sortByText = false) where TEnum : Enum
       => ((IEnumerable<TEnum>)Enum.GetValues(typeof(TEnum))).CreateSelectList(valAsName, sortByText);

    extension<TEnum>(IEnumerable<TEnum> sources) where TEnum : Enum
    {
        public IList<SelectListItem> CreateSelectList(bool valAsName = true, bool sortByText = false)
        {
            var items = new List<SelectListItem>();

            foreach (var e in sources)
            {
                var displayName = e.GetDisplayName();
                var desc = e.GetDescription();
                var value = valAsName ? e.ToString() : ((int)Enum.Parse(typeof(TEnum), e.ToString())).ToString();
                items.Add(new ExtendedSelectListItem { Text = displayName, Value = value, Description = desc });
            }
            if (sortByText)
            {
                items.Sort((a, b) => a.Text.CompareTo(b.Text));
            }
            return items;
        }
    }

    extension(IList<SelectListItem> sources)
    {
        public IList<SelectListItem> SelectItem(object selectedValue, bool unselectOthers = true, bool ignoreCase = true)
                => sources.SelectItems(new[] { selectedValue }, unselectOthers, ignoreCase);
        public IList<SelectListItem> SelectItems<TVal>(IEnumerable<TVal> selectedValues, bool unselectOthers = true, bool ignoreCase = true)
        {
            var vals = ignoreCase ? new HashSet<string>(Comparers.CaseInsensitiveStringComparer) : [];
            if (selectedValues != null)
            {
                foreach (var sv in selectedValues)
                {
                    vals.Add(sv?.ToString());
                }
            }
            foreach (var item in sources)
            {
                if (vals.Contains(item.Value))
                {
                    item.Selected = true;
                }
                else if (unselectOthers)
                {
                    item.Selected = false;
                }
            }
            return sources;
        }
    }



    #endregion

    #region Later

    private const string LateContentPrefix = "_later_";
    private static long LateContentId = 1;
    extension(HttpContext source)
    {
        public void AppendLater(object o)
        {
            var id = Interlocked.Increment(ref LateContentId);
            var name = $"{LateContentPrefix}{id:0000000000000}";
            source.Items[name] = o;
        }
    }

    extension(IHtmlHelper source)
    {
        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public HtmlString Script(Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> template)
        {
            source.ViewContext.HttpContext.AppendLater(template);
            return HtmlString.Empty;
        }
        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        [Obsolete("Use " + nameof(RenderLaterContent), false)]
        public HtmlString RenderPartialViewScripts()
            => source.RenderLaterContent();
        public HtmlString RenderLaterContent()
        {
            List<string> orderedKeys = null;
            foreach (var key in source.ViewContext.HttpContext.Items.Keys)
            {
                if (key is string sk && sk.StartsWith(LateContentPrefix))
                {
                    orderedKeys ??= [];
                    orderedKeys.Add(sk);
                }
            }
            if (orderedKeys != null)
            {
                orderedKeys.Sort();
                foreach (var sk in orderedKeys)
                {
                    var v = source.ViewContext.HttpContext.Items[sk];
                    if (v is Func<object, HelperResult> template)
                    {
                        source.ViewContext.Writer.Write(template(null));
                    }
                    else if (v is string)
                    {
                        source.ViewContext.Writer.Write(v as string);
                    }
                }
            }
            return HtmlString.Empty;
        }
    }





    #endregion


    public static MimeType BodyAsJsonMimeType = MimeType.Application.Json;

    extension(HttpRequest source)
    {
        public async Task<T> BodyAsJsonObjectAsync<T>(bool checkContentType = false)
        {
            var json = await source.BodyAsJsonAsync(checkContentType);
            return JsonHelpers.FromJson<T>(json);
        }
        public async Task<string> BodyAsJsonAsync(bool checkContentType = false)
        {
            if (checkContentType && BodyAsJsonMimeType != null)
            {
                if (!BodyAsJsonMimeType.DoesContentTypeMatch(source.ContentType))
                {
                    throw new ArgumentOutOfRangeException(nameof(source.ContentType), $"Content type does not match {nameof(BodyAsJsonMimeType)}");
                }
            }
            if (source.Body.CanSeek)
            {
                source.Body.Seek(0, SeekOrigin.Begin);
            }
            var json = await source.Body.ReadToEndAsync();
            return json;
        }
    }



    extension(HttpResponse source)
    {
        public async Task<T> BodyAsJsonObjectAsync<T>(bool checkContentType = false)
        {
            var json = await source.BodyAsJsonAsync(checkContentType);
            return JsonHelpers.FromJson<T>(json);
        }
        public async Task<string> BodyAsJsonAsync(bool checkContentType = false)
        {
            if (checkContentType && BodyAsJsonMimeType != null)
            {
                if (!BodyAsJsonMimeType.DoesContentTypeMatch(source.ContentType))
                {
                    throw new ArgumentOutOfRangeException(nameof(source.ContentType), $"Content type does not match {nameof(BodyAsJsonMimeType)}");
                }
            }
            if (source.Body.CanSeek)
            {
                source.Body.Seek(0, SeekOrigin.Begin);
            }
            var json = await source.Body.ReadToEndAsync();
            return json;
        }
    }


}
