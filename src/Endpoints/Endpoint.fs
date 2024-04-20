namespace WebApp.Endpoints

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

[<AutoOpen>]
module Endpoint =

    type EndpointHandler = Func<HttpContext, Task<IResult>>

    let handleEndpoint (handler: HttpContext -> Task<IResult>) : EndpointHandler =
        Func<HttpContext, Task<IResult>>(handler)
