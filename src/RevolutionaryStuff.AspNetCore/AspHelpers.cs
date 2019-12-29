using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
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

        public static IList<SelectListItem> ConvertToSelectList<TStruct>(this IEnumerable<TStruct> itemsToConvert) where TStruct : struct
        {
            var stringsSelect = new List<SelectListItem>();

            foreach (var stringToConvert in itemsToConvert)
            {
                var val = stringToConvert.ToString();
                stringsSelect.Add(new SelectListItem { Text = val, Value = val });
            }
            return stringsSelect;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public static HtmlString Script(this IHtmlHelper htmlHelper, Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> template)
        {
            htmlHelper.ViewContext.HttpContext.Items["_script_" + Guid.NewGuid()] = template;
            return HtmlString.Empty;
        }

        /// <remarks>https://rburnham.wordpress.com/2015/03/13/asp-net-mvc-defining-scripts-in-partial-views/</remarks>
        public static HtmlString RenderPartialViewScripts(this IHtmlHelper htmlHelper)
        {
            foreach (object key in htmlHelper.ViewContext.HttpContext.Items.Keys)
            {
                if (key.ToString().StartsWith("_script_"))
                {
                    var template = htmlHelper.ViewContext.HttpContext.Items[key] as Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult>;
                    if (template != null)
                    {
                        htmlHelper.ViewContext.Writer.Write(template(null));
                    }
                }
            }
            return HtmlString.Empty;
        }
    }
}
