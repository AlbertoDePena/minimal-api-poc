namespace WebApp.Endpoints

open WebApp.Extensions
open WebApp.Infrastructure.HtmlTemplate
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module HelloWorldHandler =

    let handle: EndpointHandler =
        handleEndpoint (fun httpContext ->
            task {
                let htmlContent = "<div>Hello World!</div>" |> HtmlTemplate.render {| |}

                return Results.Html htmlContent
            })
