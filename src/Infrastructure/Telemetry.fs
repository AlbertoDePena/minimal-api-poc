namespace WebApp.Infrastructure.Telemetry

open System
open System.Diagnostics
open System.Diagnostics.Metrics

type Telemetry() =

    let activitySource =
        new ActivitySource(Telemetry.ApplicationName, Telemetry.Version)

    let meter = new Meter(Telemetry.ApplicationName, Telemetry.Version)

    member this.ActivitySource = activitySource

    member this.Meter = meter

    member this.CreateCorrelationId() = Guid.NewGuid() |> fun guid -> guid.ToString()

    static member ApplicationName = "MinimalApi.WebApp"

    static member Version = typeof<Telemetry>.Assembly.GetName().Version.ToString()

    interface IDisposable with

        member this.Dispose() =
            activitySource.Dispose()
            meter.Dispose()

