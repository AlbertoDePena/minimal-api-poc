namespace WebApp.Infrastructure.Constants

[<RequireQualifiedAccess>]
module ClaimType =

    [<Literal>]
    let Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"

[<RequireQualifiedAccess>]
module Message =

    [<Literal>]
    let AuthenticationError = "The request is not authenticated."

    [<Literal>]
    let AuthorizationError = "The request is not allowed."

    [<Literal>]
    let ServerError =
        "Something really bad happened. Please contact the system administrator."

[<RequireQualifiedAccess>]
module PolicyName =

    [<Literal>]
    let All = "All"

    [<Literal>]
    let User = "User"

    [<Literal>]
    let AccountAdministrator = "AccountAdministrator"

    [<Literal>]
    let ConfigurationAdministrator = "ConfigurationAdministrator"

[<RequireQualifiedAccess>]
module RoleClaimValue =

    [<Literal>]
    let User = "User"

    [<Literal>]
    let AccountAdministrator = "AccountAdministrator"

    [<Literal>]
    let ConfigurationAdministrator = "ConfigurationAdministrator"

[<RequireQualifiedAccess>]
module TelemetryKey =

    [<Literal>]
    let AppCorrelationId = "app.correlation_id"

    [<Literal>]
    let AppUserName = "app.user_name"
