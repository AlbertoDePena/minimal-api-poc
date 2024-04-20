namespace WebApp.Endpoints

open System
open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module IndexHandler =

    [<Literal>]
    let LoggerCategoryName = "Index"

    type Model = { Name: string }

    let handle: EndpointHandler =
        handleEndpoint (fun httpContext ->
            task {
                let correlationId = Guid.NewGuid() |> fun guid -> guid.ToString()

                let logger = httpContext.GetLogger LoggerCategoryName

                logger.LogInformation("Requesting index view: CorrelationID {CorrelationId}", correlationId)

                let result = "Templates/Index.html" |> HtmlTemplate.render { Name = "Scriban!" }

                return Results.Html result
            })
