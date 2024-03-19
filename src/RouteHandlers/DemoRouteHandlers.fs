namespace WebApp.RouteHandlers

open WebApp
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module DemoRouteHandlers =

    let sayHello: RouteHandler =
        handleRoute (fun httpContext -> task { return Results.Ok "Hello World!" })

    let sayHelloInHtml: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let htmlContent = Html.load "<div>Hello Html!</div>" |> Html.render
                return Results.Html htmlContent
            })

[<AutoOpen>]
module WebApplicationExtensions =
    open Microsoft.AspNetCore.Builder

    type WebApplication with
        member this.MapDemoRouteHandlers() =
             this.MapGet("/", DemoRouteHandlers.sayHello).RequireAuthorization() |> ignore
             this.MapGet("/html", DemoRouteHandlers.sayHelloInHtml).RequireAuthorization() |> ignore