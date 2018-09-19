// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
#region Using directives

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

#endregion

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle(@"")]
[assembly: AssemblyDescription(@"")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(@"Microsoft")]
[assembly: AssemblyProduct(@"Gadgeteer")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: System.Resources.NeutralResourcesLanguage("en")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: ReliabilityContract(Consistency.MayCorruptProcess, Cer.None)]

//
// Make the Dsl project internally visible to the DslPackage assembly
//
[assembly: InternalsVisibleTo(@"Microsoft.Gadgeteer.Designer.DslPackage, PublicKey=0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GadgeteerUnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100bf05c5fc375fe3853f235847956fc890ad58815bf9879a4eea9aa68e0a8a629fe7fad96bdb0282c2011424a09889afa4222deb5d0c633f0857b012f4a54b0a4b873ee06ce0526a1f1c954a27f01b4ae9577843919a8fe87401c3887e077821eaa660bfe800201231fb72e61717252c887ec804eede317d3def479c43dfba0fe3")]

//Version information is in the shared ..\AssemblyVersionInfo.cs file