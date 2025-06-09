namespace WebApp.EndpointFilters

open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open WebApp.Infrastructure.Extensions

[<RequireQualifiedAccess>]
module EndpointFilters =

    let htmx (context: EndpointFilterInvocationContext) (next: EndpointFilterDelegate) : ValueTask<obj> =
        match context.HttpContext.Request.IsHtmx() with
        | false -> context.HttpContext.Response.Redirect("/")
        | _ -> ()

        next.Invoke(context)
