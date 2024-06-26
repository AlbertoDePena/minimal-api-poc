namespace WebApp.Extensions

open System
open System.Text

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open FsToolkit.ErrorHandling

[<AutoOpen>]
module HttpContextExtensions =

    type HttpContext with

        member this.GetService<'T>() =
            this.RequestServices.GetRequiredService<'T>()

        member this.GetLogger(name: string) =
            this.GetService<ILoggerFactory>().CreateLogger name

[<AutoOpen>]
module HttpRequestExtensions =

    type HttpRequest with

        member this.TryGetHeaderValue(key: string) : string option =
            this.Headers.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetFormValue(key: string) : string option =
            match this.HasFormContentType with
            | false -> None
            | true -> this.Form.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetQueryStringValue(key: string) : string option =
            this.Query.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetBearerToken() : string option =
            this.TryGetHeaderValue "Authorization"
            |> Option.filter (fun value -> value.Contains("Bearer "))
            |> Option.map (fun value -> value.Substring("Bearer ".Length).Trim())

        /// Determines if the current HTTP Request was invoked by HTMX on the client.
        member this.IsHtmx() : bool =
            this.TryGetHeaderValue "HX-Request"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)

        /// Determines if the current HTTP Request was invoked by HTMX on the client with the "boosted" attribute.
        member this.IsHtmxBoosted() : bool =
            this.TryGetHeaderValue "HX-Boosted"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)

[<AutoOpen>]
module ResultsExtensions =

    type Results with

        static member Html(content: string) : IResult =
            Results.Content(content, "text/html; charset=utf-8", Encoding.UTF8)
