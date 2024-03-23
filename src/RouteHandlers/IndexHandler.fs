namespace WebApp.RouteHandlers

open System
open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module IndexHandler =

    [<Literal>]
    let LoggerCategoryName = "Index"

    let handle: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let correlationId = Guid.NewGuid() |> fun guid -> guid.ToString()

                let logger = httpContext.GetLogger LoggerCategoryName
                logger.LogInformation("Requesting index view: CorrelationID {CorrelationId}", correlationId)

                let htmlContent = Html.load "Views/Index.html" |> Html.render
                return Results.Html htmlContent
            })