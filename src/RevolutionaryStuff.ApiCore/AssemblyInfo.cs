using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Base classes and startup scaffolding for ASP.NET Core Web API applications, including ApiProgram, route builder helpers, and OpenAPI utilities.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.ApiCore.Tests")]
