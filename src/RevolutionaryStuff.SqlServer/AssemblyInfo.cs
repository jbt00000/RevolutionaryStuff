using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

[assembly: AssemblyDescription("SQL Server data access helpers including connection utilities and Dapper-style query extensions.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.Data.SqlServer.Tests")]
