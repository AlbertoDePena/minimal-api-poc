namespace WebApp.Infrastructure.ErrorHandlerMiddleware

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open WebApp.Infrastructure.Exceptions
open WebApp.Infrastructure.ApiMessageResponse

type ErrorHandlerMiddleware(next: RequestDelegate, logger: ILogger<ErrorHandlerMiddleware>) =

    [<Literal>]
    let AuthenticationErrorMessage = "The request is not authenticated."

    [<Literal>]
    let AuthorizationErrorMessage = "The request is not allowed."

    [<Literal>]
    let ServerErrorMessage =
        "Something really bad happened. Please contact the system administrator."

    member this.Invoke(context: HttpContext) =
        task {
            try
                do! next.Invoke(context)
            with
            | :? AuthenticationException as ex ->
                logger.LogDebug(AuthenticationException.EventId, ex, ex.Message)

                context.Response.StatusCode <- StatusCodes.Status401Unauthorized

                return!
                    context.Response.WriteAsJsonAsync(
                        ApiMessageResponse.Create(StatusCodes.Status401Unauthorized, AuthenticationErrorMessage)
                    )

            | :? AuthorizationException as ex ->
                logger.LogDebug(AuthorizationException.EventId, ex, ex.Message)

                context.Response.StatusCode <- StatusCodes.Status403Forbidden

                return!
                    context.Response.WriteAsJsonAsync(
                        ApiMessageResponse.Create(StatusCodes.Status403Forbidden, AuthorizationErrorMessage)
                    )

            | ex ->
                logger.LogError(ServerException.EventId, ex, ex.Message)

                context.Response.StatusCode <- StatusCodes.Status500InternalServerError

                return!
                    context.Response.WriteAsJsonAsync(
                        ApiMessageResponse.Create(StatusCodes.Status500InternalServerError, ServerErrorMessage)
                    )
        }
