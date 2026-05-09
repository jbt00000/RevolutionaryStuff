using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Dapr integration helpers including pub/sub topic publishing, configuration, and Dapr-aware API program scaffolding.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.Dapr.Tests")]
