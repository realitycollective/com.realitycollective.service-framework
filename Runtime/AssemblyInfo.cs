// Copyright (c) RealityCollective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#define REALITYCOLLECTIVE_SERVICE_FRAMEWORK

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyTitle("com.realitycollective.service-framework")]
[assembly: AssemblyCompany("Reality Collective")]
[assembly: AssemblyCopyright("Copyright (c) Reality Collective. All rights reserved.")]

// Note: these are the names of the assembly definitions themselves, not necessarily the actual namespace the class is in.
[assembly: InternalsVisibleTo("RealityCollective.ServiceFramework.Editor")]
[assembly: InternalsVisibleTo("RealityCollective.ServiceFramework.Tests")]
[assembly: InternalsVisibleTo("RealityCollective.Editor")]
[assembly: InternalsVisibleTo("RealityCollective")]