namespace WebApp.Endpoints

open System
open WebApp.Infrastructure.Extensions
open WebApp.Domain.Invariants
open WebApp.Infrastructure.Telemetry
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module IndexHandler =

    [<Literal>]
    let LoggerCategoryName = "Index"

    type Model =
        { Name: string
          Items: string list
          Description: Text }

    let handle: EndpointHandler =
        handleEndpoint (fun httpContext ->
            task {
                let logger = httpContext.GetLogger LoggerCategoryName
                let telemetry = httpContext.GetService<Telemetry>()

                use activity = telemetry.ActivitySource.StartActivity "IndexHandler"

                let correlationId = Guid.NewGuid() |> fun guid -> guid.ToString()

                if activity |> isNull |> not then
                    activity.AddTag("app.correlation_id", correlationId) |> ignore

                logger.LogInformation("Requesting index view: CorrelationID {CorrelationId}", correlationId)

                return Results.Html "Hello World!"
            })
