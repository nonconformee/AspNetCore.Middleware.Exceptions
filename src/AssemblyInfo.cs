
using System.Reflection;

[assembly: AssemblyVersion("1.0")]

[assembly: AssemblyName("nonconformee.DotNet.Extensions")]
[assembly: AssemblyTitle("nonconformee.DotNet.Extensions")]
[assembly: AssemblyDescription("General extensions for .NET")]

[assembly: AssemplyCopyright("MIT license")]
[assembly: AssemblyTrademark("MIT license")]

[assembly: AssemblyCompany("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]

#if DEBUG
    [assembly: AssemblyConfiguration("DEBUG"),]
#elif RELEASE
    [assembly: AssemblyConfiguration("RELEASE")]
#else
    #error "DEBUG or RELEASE not specified"
#endif
