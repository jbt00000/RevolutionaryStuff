using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.AspNetCore
{
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

        public static string GetDisplayName(this Enum value)
            => value.GetDisplayAttribute()?.GetName() ?? value.ToString();

        public static string GetDescription(this Enum value)
            => value.GetDisplayAttribute()?.GetDescription();

        private static DisplayAttribute GetDisplayAttribute(this Enum value)
        {
            var type = value.GetType();
            if (!type.IsEnum) throw new ArgumentException(String.Format("Type '{0}' is not Enum", type));

            var members = type.GetMember(value.ToString());
            if (members.Length == 0) throw new ArgumentException(String.Format("Member '{0}' not found in type '{1}'", value, type.Name));

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            return (DisplayAttribute)attributes.FirstOrDefault();
        }

        #region SelectList

        public static IList<SelectListItem> CreateSelectList(this IEnumerable<SelectListItem> selectListItems)
        {
            var selectList = new List<SelectListItem>();

            foreach (var selectListItem in selectListItems)
            {
                selectList.Add(selectListItem);
            }
            return selectList;
        }

        public static IList<SelectListItem> CreateSelectList(this System.Collections.IEnumerable itemsToConvert)
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var o in itemsToConvert)
            {
                if (o == null) continue;
                var val = o.ToString();
                stringsSelect.Add(new SelectListItem { Text = val, Value = val });
            }
            return stringsSelect;
        }

        public static IList<SelectListItem> CreateSelectList<TEnum>(bool valAsName = true, bool sortByText = false) where TEnum : Enum
           => ((IEnumerable<TEnum>)Enum.GetValues(typeof(TEnum))).CreateSelectList(valAsName, sortByText);

        public static IList<SelectListItem> CreateSelectList<TEnum>(this IEnumerable<TEnum> enums, bool valAsName = true, bool sortByText = false) where TEnum : Enum
        {
            var items = new List<SelectListItem>();

            foreach (var e in enums)
            {
                var displayName = e.GetDisplayName();
                var desc = e.GetDescription();
                string value;
                if (valAsName)
                {
                    value = e.ToString();
                }
                else
                {
                    value = ((int)Enum.Parse(typeof(TEnum), e.ToString())).ToString();
                }
                items.Add(new ExtendedSelectListItem { Text = displayName, Value = value, Description = desc });
            }
            if (sortByText)
            {
                items.Sort((a, b) => a.Text.CompareTo(b.Text));
            }
            return items;
        }

        public static IList<SelectListItem> SelectItem(this IList<SelectListItem> items, object selectedValue, bool unselectOthers = true, bool ignoreCase=true)
            => items.SelectItems(new[] { selectedValue }, unselectOthers, ignoreCase);

        public static IList<SelectListItem> SelectItems<TVal>(this IList<SelectListItem> items, IEnumerable<TVal> selectedValues, bool unselectOthers = true, bool ignoreCase = true)
        {
            var vals = ignoreCase ? new HashSet<string>(Comparers.CaseInsensitiveStringComparer) : new HashSet<string>();
            if (selectedValues != null)
            {
                foreach (var sv in selectedValues)
                {
                    vals.Add(Stuff.ObjectToString(sv));
                }
            }
            foreach (var item in items)
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
            return items;
        }

        #endregion

        #region Later

        private const string LateContentPrefix = "_later_";
        private static long LateContentId = 1;
        public static void AppendLater(this HttpContext context, object o)
        {
            var id = Interlocked.Increment(ref LateContentId);
            var name = $"{LateContentPrefix}{id:0000000000000}";
            context.Items[name] = o;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public static HtmlString Script(this IHtmlHelper htmlHelper, Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> template)
        {
            htmlHelper.ViewContext.HttpContext.AppendLater(template);
            return HtmlString.Empty;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        [Obsolete("Use " + nameof(RenderLaterContent), false)]
        public static HtmlString RenderPartialViewScripts(this IHtmlHelper htmlHelper)
            => htmlHelper.RenderLaterContent();

        public static HtmlString RenderLaterContent(this IHtmlHelper htmlHelper)
        {
            List<string> orderedKeys = null;
            foreach (object key in htmlHelper.ViewContext.HttpContext.Items.Keys)
            {
                var sk = key as string;
                if (sk != null && sk.StartsWith(LateContentPrefix))
                {
                    orderedKeys = orderedKeys ?? new List<string>();
                    orderedKeys.Add(sk);
                }
            }
            if (orderedKeys != null)
            {
                orderedKeys.Sort();
                foreach (var sk in orderedKeys)
                {
                    var v = htmlHelper.ViewContext.HttpContext.Items[sk];
                    var template = v as Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult>;
                    if (template != null)
                    {
                        htmlHelper.ViewContext.Writer.Write(template(null));
                    }
                    else if (v is string)
                    {
                        htmlHelper.ViewContext.Writer.Write(v as string);
                    }
                }
            }
            return HtmlString.Empty;
        }

        #endregion


        public static MimeType BodyAsJsonMimeType = MimeType.Application.Json;

        public static async Task<T> BodyAsJsonObjectAsync<T>(this HttpRequest req, bool checkContentType=false)
        {
            var json = await req.BodyAsJsonAsync(checkContentType);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<string> BodyAsJsonAsync(this HttpRequest req, bool checkContentType = false)
        {
            if (checkContentType && BodyAsJsonMimeType != null)
            {
                if (!BodyAsJsonMimeType.DoesContentTypeMatch(req.ContentType))
                {
                    throw new ArgumentOutOfRangeException(nameof(req.ContentType), $"Content type does not match {nameof(BodyAsJsonMimeType)}");
                }
            }
            if (req.Body.CanSeek)
            {
                req.Body.Seek(0, SeekOrigin.Begin);
            }
            var json = await req.Body.ReadToEndAsync();
            return json;
        }
    }
}
