namespace WebApp.Domain.User

open System
open WebApp.Domain.Shared

[<RequireQualifiedAccess>]
type UserType =
    | Administrator
    | API
    | Client

    member this.Value =
        match this with
        | Administrator -> "Administrator"
        | API -> "API"
        | Client -> "Client"

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        match value with
        | "Administrator" -> Some Administrator
        | "API" -> Some API
        | "Client" -> Some Client
        | _ -> None

type User =
    { Id: UniqueId
      DisplayName: Text
      EmailAddress: EmailAddress
      UserType: UserType
      CreatedAt: Timestamp
      UpdatedAt: Timestamp }

[<RequireQualifiedAccess>]
type UserValidationError =
    | DisplayNameIsRequired
    | EmailAddressIsRequired
    | UserTypeIsRequired
    | UserAlreadyExists

[<RequireQualifiedAccess>]
module UserBusinessLogic =
    open WebApp.Dtos.UserDto
    open FsToolkit.ErrorHandling

    let validateCreateUserRequest
        (existingUsers: User list)
        (request: CreateUserRequest)
        : Validation<User, UserValidationError> =
        validation {
            let! displayName =
                request.DisplayName
                |> Text.TryCreate
                |> Result.requireSome UserValidationError.DisplayNameIsRequired

            and! emailAddress =
                request.EmailAddress
                |> EmailAddress.TryCreate
                |> Result.requireSome UserValidationError.EmailAddressIsRequired

            and! userType =
                request.UserType
                |> UserType.TryCreate
                |> Result.requireSome UserValidationError.UserTypeIsRequired

            and! _ =
                existingUsers
                |> List.exists (fun user -> (Some user.EmailAddress) = EmailAddress.TryCreate request.EmailAddress)
                |> Result.requireFalse UserValidationError.UserAlreadyExists

            return
                { Id = UniqueId.Create()
                  DisplayName = displayName
                  EmailAddress = emailAddress
                  UserType = userType
                  CreatedAt = DateTimeOffset.UtcNow
                  UpdatedAt = DateTimeOffset.UtcNow }
        }
