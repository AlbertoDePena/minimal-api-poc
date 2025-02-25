namespace WebApp.Infrastructure.Exceptions

open System
open Microsoft.Extensions.Logging

type AuthenticationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthenticationException(Exception message)

    static member EventId = EventId(10401, "AuthenticationError")

type AuthorizationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthorizationException(Exception message)

    static member EventId = EventId(10403, "AuthorizationError")

type ServerException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = ServerException(Exception message)

    static member EventId = EventId(10500, "ServerError")
