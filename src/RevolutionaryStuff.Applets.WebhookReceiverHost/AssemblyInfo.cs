using System.Reflection;
using System.Runtime.CompilerServices;
using RevolutionaryStuff.Core.ApplicationParts;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Applet for hosting webhook auto-responders with configurable route mapping, request archiving, Service Bus forwarding, and diagnostic blob storage.")]

[assembly: AssemblySettingsResourceAutoDiscovery]

//For the unit tests
[assembly: InternalsVisibleTo("RevolutionaryStuff.Applets.Tests")]
[assembly: InternalsVisibleTo("RevolutionaryStuff.Applets.WebhookReceiverHost.Tests")]
