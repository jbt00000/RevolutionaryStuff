using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

[assembly: AssemblyDescription("Local file system storage provider implementation for the RevolutionaryStuff Storage abstraction layer.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.Storage.Providers.LocalFileSystem.Tests")]
