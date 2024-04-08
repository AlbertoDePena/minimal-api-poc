namespace WebApp.Infrastructure.Telemetry

open Microsoft.ApplicationInsights.Extensibility
open Microsoft
open Microsoft.AspNetCore.Http

open WebApp.Infrastructure.ApplicationInfo

type CloudRoleNameInitializer() =

    interface ITelemetryInitializer with
        member this.Initialize(telemetry: ApplicationInsights.Channel.ITelemetry) =
            telemetry.Context.Cloud.RoleName <- ApplicationInfo.Name

type ComponentVersionInitializer() =

    interface ITelemetryInitializer with
        member this.Initialize(telemetry: ApplicationInsights.Channel.ITelemetry) =
            telemetry.Context.Component.Version <- ApplicationInfo.Version

type AuthenticatedUserInitializer(httpContextAccessor: IHttpContextAccessor) =

    interface ITelemetryInitializer with
        member this.Initialize(telemetry: ApplicationInsights.Channel.ITelemetry) =
            if
                httpContextAccessor.HttpContext <> null
                && httpContextAccessor.HttpContext.User <> null
                && httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
            then
                telemetry.Context.User.AuthenticatedUserId <- httpContextAccessor.HttpContext.User.Identity.Name
