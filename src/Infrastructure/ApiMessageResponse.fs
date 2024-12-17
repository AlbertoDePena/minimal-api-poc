namespace WebApp.Infrastructure.ApiMessageResponse

[<CLIMutable>]
type ApiMessageResponse =
    { StatusCode: int
      Messages: string array }

    static member Create(statusCode: int, messages: string list) : ApiMessageResponse =
        { StatusCode = statusCode
          Messages = messages |> List.distinct |> List.toArray }

    static member Create(statusCode: int, message: string) : ApiMessageResponse =
        { StatusCode = statusCode
          Messages = [| message |] }
