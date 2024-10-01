// Copyright (c) RealityCollective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("1.0.9")]
[assembly: AssemblyTitle("com.realitycollective.service-framework")]
[assembly: AssemblyCompany("Reality Collective")]
[assembly: AssemblyCopyright("Copyright (c) Reality Collective. All rights reserved.")]
[assembly: InternalsVisibleTo("RealityCollective.ServiceFramework.Editor")]

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("RealityCollective.ServiceFramework.Tests")]
#endif
