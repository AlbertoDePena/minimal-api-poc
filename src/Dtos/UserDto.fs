namespace WebApp.Dtos.UserDto

[<CLIMutable>]
type CreateUserRequest =
    { DisplayName: string
      EmailAddress: string
      UserType: string }
