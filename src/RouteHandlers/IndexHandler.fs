namespace WebApp.RouteHandlers

open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module IndexHandler =

    let handle: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let htmlContent = Html.load "Views/Index.html" |> Html.render
                return Results.Html htmlContent
            })