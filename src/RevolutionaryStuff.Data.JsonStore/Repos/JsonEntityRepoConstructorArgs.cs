using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Repos;

public record JsonEntityRepoConstructorArgs(IJsonEntityServer Jes, ILocalCacher Cacher, IOptions<JsonEntityRepoBaseConfig> ConfigOptions)
{}
