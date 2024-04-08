namespace WebApp.Infrastructure.ApplicationInfo

open System.Reflection

type ApplicationInfo() =
    static member Name = "htmx-poc"

    static member Version =
        Assembly
            .GetAssembly(typeof<ApplicationInfo>)
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion
