namespace WebApp

open System
open System.Text
open System.Threading.Tasks

open Microsoft.AspNetCore.Http

open FsToolkit.ErrorHandling

[<AutoOpen>]
module DelegateExtensions =

    type RouteHandler = Func<HttpContext, Task<IResult>>

    let handleRoute (handler: HttpContext -> Task<IResult>) : RouteHandler =
        Func<HttpContext, Task<IResult>>(handler)

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
