namespace WebApp.RouteHandlers

open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module SayHelloHandler =

    let handle: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let htmlContent = Html.load "<p>Hello There!</p>" |> Html.render
                return Results.Html htmlContent
            })
