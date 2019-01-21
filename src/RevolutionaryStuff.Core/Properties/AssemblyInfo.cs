using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RevolutionaryStuff.Core")]
[assembly: AssemblyDescription("Helpers libraries I've developed and use in all my projects dating back to .NET v1.0 beta 2.\r\n\r\n... most have been refactored over time...\r\n\r\nFeel free to use these, I'll try to keep them fairly stable, but really, they're just for projects developed by me.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Revolutionary Stuff, LLC")]
[assembly: AssemblyProduct("RevolutionaryStuff.Core")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("52469e96-d756-40cf-914b-dec5318f9107")]

//This assembly contains plugins and is eligible for dynamic exploration
[assembly: RevolutionaryStuff.Core.ApplicationParts.PluginDomain()]
