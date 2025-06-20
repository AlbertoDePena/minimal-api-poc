namespace WebApp.Endpoints

open System
open WebApp.Infrastructure.Extensions
open WebApp.Infrastructure.Telemetry
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open WebApp.Infrastructure.Constants
open WebApp.Views.UserPageView
open WebApp.EndpointFilters
open Microsoft.AspNetCore.Builder

[<RequireQualifiedAccess>]
module IndexHandler =

    [<Literal>]
    let LoggerCategoryName = "IndexPage"

    let renderPage: EndpointHandler =
        handleEndpoint (fun httpContext ->
            task {
                let logger = httpContext.GetLogger LoggerCategoryName
                let telemetry = httpContext.GetService<Telemetry>()
                let correlationId = telemetry.CreateCorrelationId()

                use activity = telemetry.ActivitySource.StartActivity LoggerCategoryName

                if activity |> isNull |> not then
                    activity
                        .AddTag(TelemetryKey.AppCorrelationId, correlationId)
                        .AddTag(TelemetryKey.AppUserName, httpContext.User.Identity.Name)
                    |> ignore

                logger.LogInformation("Requesting index view: CorrelationID {CorrelationId}", correlationId)

                return
                    httpContext.User.Identity.Name
                    |> UserPageView.render "Hello World!"
                    |> Results.Html
            })

    let configureRoutes (app: WebApplication) =
        app
            .MapGet("/", renderPage)
            .RequireAuthorization(PolicyName.All)
            .AddEndpointFilter(EndpointFilters.htmx)
        |> ignore
