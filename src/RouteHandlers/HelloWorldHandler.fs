namespace WebApp.RouteHandlers

open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module HelloWorldHandler =

    let handle: EndpointHandler =
        handleEndpoint (fun httpContext ->
            task {
                let htmlContent = Html.load "<div>Hello World!</div>" |> Html.render
                return Results.Html htmlContent
            })
