namespace WebApp.Endpoints

open System
open WebApp.Extensions
open WebApp.Domain.Invariants
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
                let correlationId = Guid.NewGuid() |> fun guid -> guid.ToString()

                let logger = httpContext.GetLogger LoggerCategoryName

                logger.LogInformation("Requesting index view: CorrelationID {CorrelationId}", correlationId)

                return Results.Html "Hello World!"
            })
