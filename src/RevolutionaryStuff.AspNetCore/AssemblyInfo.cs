using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

[assembly: AssemblyDescription("ASP.NET Core utilities including Razor Page base classes, session archiving, tag helpers, HTTP helpers, and middleware extensions.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.AspNetCore.Tests")]
