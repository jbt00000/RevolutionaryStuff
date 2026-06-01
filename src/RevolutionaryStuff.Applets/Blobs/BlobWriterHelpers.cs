using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;
using RevolutionaryStuff.Core.Services.CodeStringGenerator;
using RevolutionaryStuff.Core.Services.Tenant;
using RevolutionaryStuff.Storage;

namespace RevolutionaryStuff.Applets.Blobs;

public static class BlobWriterHelpers
{
    public record PathProviderArgs(IServiceProvider sp, string fileName, WriteBlobSettings? settings);

    public static string AppTenantDiagnosticTimestampedPathProvider(PathProviderArgs ppa)
    {
        var appName = ppa.sp.GetRequiredService<IApplicationNameFinder>().ApplicationName;
        var tenantId = ppa.sp.GetRequiredService<ITenantIdProvider>().GetTenantId();
        var codeStringGenerator = ppa.sp.GetRequiredService<ICodeStringGenerator>();
        var now = ppa.settings?.Now ?? DateTimeOffset.UtcNow;
        var ext = System.IO.Path.GetExtension(ppa.fileName);
        var machine = Environment.MachineName;
        var codeString = codeStringGenerator.CreateRomanLowerCharactersCode(4);
        var newFileName = System.IO.Path.ChangeExtension(ppa.fileName, $".{now:HHmmssfff}.{machine}.{codeString}{ext}");
        return $"{appName}/{tenantId}/diagnostics/{now:yyyy}/{now:MM}/{now:dd}/{now:HH}/{newFileName}";
    }

    public static IServiceCollection AddBlobWriter<TBlobWriter>(this IServiceCollection services, Func<PathProviderArgs, string> pathProvider)
        where TBlobWriter : class, IBlobWriter
        => AddBlobWriter<IStorageProvider, TBlobWriter>(services, pathProvider);

    public static IServiceCollection AddBlobWriter<TStorageProvider, TBlobWriter>(this IServiceCollection services, Func<PathProviderArgs, string> pathProvider)
        where TStorageProvider : IStorageProvider
        where TBlobWriter : class, IBlobWriter
    {
        services.AddScoped<TBlobWriter>(sp =>
        {
            var storageProvider = sp.GetRequiredService<TStorageProvider>();
            var inner = new MyBlobWriter(sp, storageProvider, pathProvider);
            return BlobWriterProxy<TBlobWriter>.Create(inner);
        });
        return services;
    }

    private class MyBlobWriter(IServiceProvider ServiceProvider, IStorageProvider StorageProvider, Func<PathProviderArgs, string> PathProvider) : IBlobWriter
    {
        async Task<WriteBlobResult> IBlobWriter.WriteBlobAsync(string name, Stream st, WriteBlobSettings? settings)
        {
            var path = PathProvider(new PathProviderArgs(ServiceProvider, name, settings));
            var folder = await StorageProvider.OpenRootFolderAsync();
            IFolderEntry.CreateFileArgs? cfa = null;
            if (settings != null)
            {
                cfa = new() { ContentType = settings.ContentType, Metadata = settings.Metadata };
            }
            var createFileResult = await folder.CreateFileAsync(path, st, cfa);
            return new WriteBlobResult
            {
                StorageName = createFileResult.Path,
                Name = createFileResult.Name,
                Size = createFileResult.Length
            };
        }
    }

    /// <remarks>
    /// Types deriving from dispatchProxies cannot be sealed
    /// </remarks>
    private class BlobWriterProxy<TInterface> : DispatchProxy
        where TInterface : class, IBlobWriter
    {
        private IBlobWriter Inner = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            => targetMethod!.Invoke(Inner, args);

        internal static TInterface Create(IBlobWriter inner)
        {
            // DispatchProxy.Create<T, TProxy> dynamically emits a concrete class
            // that implements TInterface and routes all calls through Invoke()
            var proxy = Create<TInterface, BlobWriterProxy<TInterface>>();
            ((BlobWriterProxy<TInterface>)(object)proxy).Inner = inner;
            return proxy;
        }
    }
}

