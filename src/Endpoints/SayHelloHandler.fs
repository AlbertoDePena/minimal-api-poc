namespace WebApp.Endpoints

open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module SayHelloHandler =

    let handle: EndpointHandler =
        handleEndpoint (fun httpContext ->
            task {
                let htmlContent = "<p>Hello There!</p>" |> HtmlTemplate.render {| |}

                return Results.Html htmlContent
            })
