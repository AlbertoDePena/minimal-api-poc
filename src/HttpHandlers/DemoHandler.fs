namespace WebApp.HttpHandlers

open WebApp
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module DemoHandler =

    let sayHello: HttpHandler =
        handleHttp (fun httpContext -> task { return Results.Ok "Hello World!" })

    let sayHelloInHtml: HttpHandler =
        handleHttp (fun httpContext ->
            task {
                let htmlContent = Html.load "<div>Hello Html!</div>" |> Html.render
                return Results.Html htmlContent
            })

[<AutoOpen>]
module WebApplicationExtensions =
    open Microsoft.AspNetCore.Builder

    type WebApplication with
        member this.RegisterDemoHttpHandlers() =
             this.MapGet("/", DemoHandler.sayHello).RequireAuthorization() |> ignore
             this.MapGet("/html", DemoHandler.sayHelloInHtml).RequireAuthorization() |> ignore