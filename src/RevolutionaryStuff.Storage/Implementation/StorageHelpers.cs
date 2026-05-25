using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Storage.Implementation;

public static class StorageHelpers
{
    public const char ExternalFolderSeparatorChar = '/';

    public static readonly string ExternalFolderSeparator = ExternalFolderSeparatorChar.ToString();

    public static readonly string RootPath = ExternalFolderSeparator;

    public static DateTimeOffset EarliestFileDate = new(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly IList<IEntry> NoEntries = new List<IEntry>().AsReadOnly();

    public static readonly IFindResults NoFindResults = new NoFindResultsClass();

    private class NoFindResultsClass : IFindResults
    {
        IList<IEntry> IFindResults.Entries => NoEntries;

        Task<IFindResults> IFindResults.NextAsync()
            => Task.FromResult((IFindResults)this);
    }

    public static string[] GetPathSegments(string path, bool throwOnInvalidParts = true)
    {
        var parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        for (var z = 0; z < parts.Length; ++z)
        {
            var part = parts[z].TrimOrNull();
            parts[z] = part;
            if (throwOnInvalidParts)
            {
                switch (part)
                {
                    case null:
                    case "":
                    case ".":
                    case "..":
                        throw new StorageProviderException(StorageProviderExceptionCodes.NotWithinTree);
                }
                if (RegexHelpers.Common.InvalidPathChars.IsMatch(part))
                    throw new StorageProviderException(StorageProviderExceptionCodes.NotWithinTree);
            }
        }
        return parts;
    }

    public static IServiceCollection UseStorageProviderConfigTypeNameSelector(this IServiceCollection services, string keyName = null)
    {
        services.ConfigureOptions<StorageProviderTypeNameSelectorConfig>(keyName ?? StorageProviderTypeNameSelectorConfig.ConfigSectionName);
        services.AddScoped(sp =>
        {
            var config = sp.GetRequiredService<IOptions<StorageProviderTypeNameSelectorConfig>>().Value;
            var serviceDescriptors = services.Where(s => s.ServiceType.IsA<IStorageProvider>()).ToList();
            var serviceDescriptor = serviceDescriptors.FirstOrDefault(sd => sd.ServiceType.Name == config.StorageProviderTypeName);
            return (IStorageProvider)sp.GetService(serviceDescriptor.ServiceType);
        });
        return services;
    }

    public class StorageProviderTypeNameSelectorConfig
    {
        public const string ConfigSectionName = "StorageProviderTypeNameSelectorConfig";
        public string StorageProviderTypeName { get; set; }
    }

    public static void RequiresFilePath(string filePath, string argName)
    {
        Requires.Text(filePath, argName, false, 2);
        if (filePath[0] != ExternalFolderSeparatorChar)
            throw new ArgumentOutOfRangeException(argName, $"Must start with {ExternalFolderSeparatorChar}");
    }

    public static IServiceCollection AddTypedStorageProvider<TInterface, TImplementation, TConfig>(
        this IServiceCollection services,
        string configSectionName,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class, IStorageProvider
        where TImplementation : class, IStorageProvider
        where TConfig : class, new()
    {
        services.ConfigureOptions<TConfig>(configSectionName);

        services.Add(new ServiceDescriptor(typeof(TInterface), sp =>
        {
            var config = sp.GetRequiredService<IOptions<TConfig>>().Value;
            var typedOptions = Options.Create(config);
            var impl = ActivatorUtilities.CreateInstance<TImplementation>(sp, typedOptions);
            return StorageProviderProxy<TInterface>.Create(impl);
        }, lifetime));

        return services;
    }

    private sealed class StorageProviderProxy<TInterface> : DispatchProxy
        where TInterface : class, IStorageProvider
    {
        private IStorageProvider Inner = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            => targetMethod!.Invoke(Inner, args);

        internal static TInterface Create(IStorageProvider inner)
        {
            var proxy = Create<TInterface, StorageProviderProxy<TInterface>>();
            ((StorageProviderProxy<TInterface>)(object)proxy).Inner = inner;
            return proxy;
        }
    }
}
