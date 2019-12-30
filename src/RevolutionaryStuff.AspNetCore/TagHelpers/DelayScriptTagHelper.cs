using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RevolutionaryStuff.AspNetCore.TagHelpers
{
    [HtmlTargetElement("script", Attributes = IndicatorAttributeName)]
    public class DelayScriptTagHelper : TagHelper
    {
        public const string IndicatorAttributeName = "later";
        private readonly IHtmlGenerator Generator;
        private readonly HtmlEncoder HtmlEncoder;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public DelayScriptTagHelper(IHtmlGenerator generator, HtmlEncoder htmlEncoder)
        {
            Generator = generator;
            HtmlEncoder = htmlEncoder;
        }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var sb = new StringBuilder();
            var tw = new StringWriter(sb);
            var inner = await output.GetChildContentAsync();
            output.Attributes.Remove(output.Attributes[IndicatorAttributeName]);
            output.Content = inner;
            output.WriteTo(tw, HtmlEncoder);
            var s = sb.ToString();
            ViewContext.HttpContext.AppendLater(s);
            output.SuppressOutput();
        }
    }
}
