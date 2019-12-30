using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RevolutionaryStuff.AspNetCore
{
    public static class AspHelpers
    {
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

        public static IList<SelectListItem> CreateSelectList<TStruct>(this IEnumerable<TStruct> itemsToConvert) where TStruct : struct
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in itemsToConvert)
            {
                var val = stringToConvert.ToString();
                stringsSelect.Add(new SelectListItem { Text = val, Value = val });
            }
            return stringsSelect;
        }

        private const string LateContentPrefix = "_late_";
        private static long LateContentId = 1;
        public static void AppendLateContent(this HttpContext context, object o)
        {
            var id = Interlocked.Increment(ref LateContentId);
            var name = $"{LateContentPrefix}{id:000000000000}";
            context.Items[name] = o;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public static HtmlString Script(this IHtmlHelper htmlHelper, Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> template)
        {
            htmlHelper.ViewContext.HttpContext.AppendLateContent(template);
            return HtmlString.Empty;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        [Obsolete("Use " + nameof(RenderLateContent), false)]
        public static HtmlString RenderPartialViewScripts(this IHtmlHelper htmlHelper)
            => htmlHelper.RenderLateContent();

        public static HtmlString RenderLateContent(this IHtmlHelper htmlHelper)
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
    }
}
