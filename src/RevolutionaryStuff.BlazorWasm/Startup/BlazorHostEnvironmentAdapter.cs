using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace RevolutionaryStuff.BlazorWasm.Startup;

internal sealed class BlazorHostEnvironmentAdapter : IHostEnvironment
{
    public BlazorHostEnvironmentAdapter(IWebAssemblyHostEnvironment webAssemblyHostEnvironment)
    {
        ApplicationName = typeof(BlazorProgram).Assembly.GetName().Name;
        EnvironmentName = webAssemblyHostEnvironment.Environment;
        ContentRootPath = webAssemblyHostEnvironment.BaseAddress;
        ContentRootFileProvider = new NullFileProvider();
    }

    public string ApplicationName { get; set; }

    public string EnvironmentName { get; set; }

    public string ContentRootPath { get; set; }

    public IFileProvider ContentRootFileProvider { get; set; }
}
