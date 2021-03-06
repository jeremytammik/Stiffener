﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Stiffener" )]
[assembly: AssemblyDescription( "Revit add-in to create and insert a structural stiffener extrusion family into the current project." )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Autodesk Inc." )]
[assembly: AssemblyProduct( "Stiffener" )]
[assembly: AssemblyCopyright( "Copyright © 2011-2015 Jeremy Tammik Autodesk Inc." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "118f7279-630d-4661-afe5-c23c23acf46f" )]

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
//
// 2011-06-13 1.0.0.0 initial implementation for Revit 2012
// 2015-09-22 2014.0.0.0 migration to Revit 2014 including code update, .NET framework target 4.0, removal of architecture mismatch warning and obsolete API usage; img/stiffener_2014.png
// 2015-09-22 2014.0.0.1 updated to place instance from in-memory family definition with no file save, restructured logic, added using statements around transactions; img/stiffener_2014_in_memory.png
// 2015-09-22 2014.0.0.2 set family name after loading into project; img/stiffener_2014_in_memory_with_name.png
// 2015-09-22 2014.0.0.3 set 'copy local' to false on the Revit API assemblies
// 2015-09-22 2015.0.0.0 flat migration to Revit 2015
// 2015-09-22 2016.0.0.0 migration to Revit 2016, need to activate symbol, set symbol name as well as family; img/stiffener_2016_in_memory_with_both_names.png
// 2015-09-25 2016.0.0.1 replace call to deprecated PlanarFace.Normal property by PlanarFace.FaceNormal
//
[assembly: AssemblyVersion( "2016.0.0.1" )]
[assembly: AssemblyFileVersion( "2016.0.0.1" )]
