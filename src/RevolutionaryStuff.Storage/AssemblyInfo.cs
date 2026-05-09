using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

[assembly: AssemblyDescription("Provider-agnostic file and blob storage abstractions with shared interfaces for storage providers, file entries, and folder entries.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.Storage.Tests")]
