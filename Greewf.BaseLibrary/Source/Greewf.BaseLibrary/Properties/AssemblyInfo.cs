using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Greewf")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Mahmoud Moravej (moravej@gmail.com)")]
[assembly: AssemblyProduct("Greewf")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2015-2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("beccc21e-a123-45ff-93b3-b91518ec6a44")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.4")]//we ignore * because we change this dll regularly. But we don't update all reference assemblies at same time. So we may get this error "Could not load file or assembly 'Greewf.BaseLibrary, Version..." because of old versions.
[assembly: AssemblyFileVersion("3.4")]

