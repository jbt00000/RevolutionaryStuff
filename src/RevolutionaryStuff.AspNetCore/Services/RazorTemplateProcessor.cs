﻿using System.Collections;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.AspNetCore.Services;

public class RazorTemplateProcessor : IFileProvider, IDirectoryContents, ITemplateProcessor
{
    private readonly IServiceProvider ServiceProvider;
    private readonly MyController Controller = new();
    public const string NoLayoutPrefix = "@{Layout=null;}";

    public bool PrependNoLayout { get; set; } = true;

    public RazorTemplateProcessor(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private readonly IDictionary<string, TemplateItem> TemplateItemByPath = new Dictionary<string, TemplateItem>();
    private readonly IDictionary<string, string> PathByTemplate = new Dictionary<string, string>();

    private class MyController : Controller
    {
        public IActionResult Go(string viewPath, object model)
            => View(viewPath, model);
    }

    private class MyRouter : IRouter
    {
        VirtualPathData IRouter.GetVirtualPath(VirtualPathContext context) => new(this, "/");

        Task IRouter.RouteAsync(RouteContext context)
            => Task.CompletedTask;
    }

    public async Task<string> ProcessAsync(string template, object model)
    {
        template = (template ?? "").Trim();
        if (PrependNoLayout)
        {
            template = NoLayoutPrefix + template;
        }
        var path = PathByTemplate.FindOrCreate(template, () => $"/Views/D{PathByTemplate.Count}.cshtml");
        TemplateItemByPath.FindOrCreate(path, () => new TemplateItem(path, template));
        var res = Controller.Go(path, model);
        using var scope = ServiceProvider.CreateScope();
        var ac = new ActionContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider },
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
            RouteData = new RouteData()
        };
        ac.RouteData.Routers.Add(new MyRouter());
        using var st = new MemoryStream();
        ac.HttpContext.Response.Body = st;
        await res.ExecuteResultAsync(ac);
        st.Position = 0;
        var sr = new StreamReader(st);
        var s = sr.ReadToEnd();
        return s;
    }

    IDirectoryContents IFileProvider.GetDirectoryContents(string subpath)
    {
        return subpath is "/Pages" or "/Views" ? this : (IDirectoryContents)null;
    }

    IFileInfo IFileProvider.GetFileInfo(string subpath)
        => TemplateItemByPath.GetValue(subpath);

    private class NullChangeToken : BaseDisposable, IChangeToken
    {
        public static readonly IChangeToken Instance = new NullChangeToken();

        private NullChangeToken() { }

        bool IChangeToken.HasChanged => false;

        bool IChangeToken.ActiveChangeCallbacks => false;

        IDisposable IChangeToken.RegisterChangeCallback(Action<object> callback, object state)
           => this;
    }

    IChangeToken IFileProvider.Watch(string filter)
        => NullChangeToken.Instance;

    bool IDirectoryContents.Exists => true;

    IEnumerator<IFileInfo> IEnumerable<IFileInfo>.GetEnumerator()
        => TemplateItemByPath.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => TemplateItemByPath.Values.GetEnumerator();

    private class TemplateItem : IFileInfo
    {
        private readonly string Template;

        public TemplateItem(string fullPath, string template)
        {
            Name = Path.GetFileNameWithoutExtension(fullPath);
            PhysicalPath = fullPath;
            Template = template;
            using var st = CreateReadStream();
            Length = st.Length;
        }

        bool IFileInfo.Exists => true;

        public long Length { get; }

        public string PhysicalPath { get; }

        public string Name { get; }

        DateTimeOffset IFileInfo.LastModified { get; } = DateTimeOffset.UtcNow;

        bool IFileInfo.IsDirectory { get; } = false;

        public Stream CreateReadStream()
            => StreamHelpers.Create(Template);
    }
}
