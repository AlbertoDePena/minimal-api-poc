namespace WebApp.Program

open System
open System.Threading.Tasks

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.CookiePolicy
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Microsoft.Extensions.Configuration

open Azure.Monitor.OpenTelemetry.AspNetCore

open OpenTelemetry
open OpenTelemetry.Metrics
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open OpenTelemetry.Instrumentation.AspNetCore

open Microsoft.Identity.Web

open WebApp.Infrastructure.Telemetry
open WebApp.Infrastructure.Dapper
open WebApp.Infrastructure.Options
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.ErrorHandlerMiddleware
open WebApp.Endpoints
open WebApp.Infrastructure.Constants
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.Extensions.Diagnostics.HealthChecks
open HealthChecks.UI.Client
open Microsoft.AspNetCore.HttpOverrides

[<RequireQualifiedAccess>]
module Program =

    [<Literal>]
    let SuccessExitCode = 0

    [<Literal>]
    let FailureExitCode = -1

    [<EntryPoint>]
    let main args =
        try
            Dapper.registerTypeHandlers ()

            let builder = WebApplication.CreateBuilder args

            builder.Configuration.AddJsonFile("appsettings.local.json", optional = true)
            |> ignore

            builder.Services
                .AddOptions<DatabaseOptions>()
                .Configure<IConfiguration>(fun settings configuration ->
                    configuration.GetSection("Database").Bind settings)
            |> ignore

            builder.Services.Configure<CookiePolicyOptions>(
                Action<CookiePolicyOptions>(fun options ->
                    options.Secure <- CookieSecurePolicy.Always
                    options.HttpOnly <- HttpOnlyPolicy.Always
                    options.MinimumSameSitePolicy <- SameSiteMode.Lax
                    options.CheckConsentNeeded <- fun context -> true
                    options.HandleSameSiteCookieCompatibility() |> ignore)
            )
            |> ignore

            builder.Services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(fun options -> builder.Configuration.Bind("AzureAd", options))
            |> ignore

            builder.Services.Configure<OpenIdConnectOptions>(
                OpenIdConnectDefaults.AuthenticationScheme,
                fun (options: OpenIdConnectOptions) ->
                    let events = OpenIdConnectEvents()

                    events.OnRedirectToIdentityProvider <-
                        (fun redirectContext ->
                            redirectContext.ProtocolMessage.RedirectUri <-
                                builder.Configuration.GetValue<string> "Application:RedirectUri"

                            Task.CompletedTask)

                    options.Events <- events
                    options.AccessDeniedPath <- PathString("/Forbidden/")
            )
            |> ignore

            builder.Services.AddAuthorization(fun options ->
                options.AddPolicy(
                    PolicyName.User,
                    fun policy ->
                        policy
                            .RequireAuthenticatedUser()
                            .RequireClaim(ClaimType.Role, RoleClaimValue.User)
                        |> ignore
                )

                options.AddPolicy(
                    PolicyName.AccountAdministrator,
                    fun policy ->
                        policy
                            .RequireAuthenticatedUser()
                            .RequireClaim(ClaimType.Role, RoleClaimValue.AccountAdministrator)
                        |> ignore
                )

                options.AddPolicy(
                    PolicyName.ConfigurationAdministrator,
                    fun policy ->
                        policy
                            .RequireAuthenticatedUser()
                            .RequireClaim(ClaimType.Role, RoleClaimValue.ConfigurationAdministrator)
                        |> ignore
                )

                options.AddPolicy(
                    PolicyName.All,
                    fun policy ->
                        policy
                            .RequireAuthenticatedUser()
                            .RequireClaim(
                                ClaimType.Role,
                                [| RoleClaimValue.User
                                   RoleClaimValue.AccountAdministrator
                                   RoleClaimValue.ConfigurationAdministrator |]
                            )
                        |> ignore
                ))
            |> ignore

            builder.Services.AddSingleton<UserDatabase>() |> ignore
            builder.Services.AddSingleton<Telemetry>() |> ignore
            builder.Services.AddAuthorization() |> ignore
            builder.Services.AddAntiforgery() |> ignore

            builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>
                (fun (options: AspNetCoreTraceInstrumentationOptions) -> options.RecordException <- true)
            |> ignore

            let openTelemetryBuilder = builder.Services.AddOpenTelemetry()

            openTelemetryBuilder
                .ConfigureResource(fun resourceBuilder ->
                    resourceBuilder.AddService(
                        serviceName = Telemetry.ApplicationName,
                        serviceVersion = Telemetry.Version,
                        serviceInstanceId = Environment.MachineName
                    )
                    |> ignore)
                .WithLogging()
                .WithMetrics(fun meterBuilder ->
                    meterBuilder.AddAspNetCoreInstrumentation() |> ignore
                    meterBuilder.AddHttpClientInstrumentation() |> ignore
                    meterBuilder.AddMeter Telemetry.ApplicationName |> ignore)
                .WithTracing(fun tracerBuilder ->
                    tracerBuilder.AddAspNetCoreInstrumentation() |> ignore
                    tracerBuilder.AddHttpClientInstrumentation() |> ignore
                    tracerBuilder.AddSource Telemetry.ApplicationName |> ignore)
            |> ignore

            if
                builder.Configuration.GetValue<string> "OTEL_EXPORTER_OTLP_ENDPOINT"
                |> String.IsNullOrWhiteSpace
                |> not
            then
                openTelemetryBuilder.UseOtlpExporter() |> ignore

            elif
                builder.Configuration.GetValue<string> "APPLICATIONINSIGHTS_CONNECTION_STRING"
                |> String.IsNullOrWhiteSpace
                |> not
            then
                openTelemetryBuilder.UseAzureMonitor() |> ignore

            builder.Services
                .AddHealthChecks()
                .AddSqlServer(
                    connectionStringFactory =
                        (fun services ->
                            services
                                .GetRequiredService<IOptions<DatabaseOptions>>()
                                .Value.SqlConnectionString),
                    name = "HTMX POC Database"
                )
            |> ignore

            let app = builder.Build()

            if app.Environment.IsDevelopment() then
                app.UseDeveloperExceptionPage() |> ignore
            else
                app.UseMiddleware<ErrorHandlerMiddleware>() |> ignore
                app.UseHsts() |> ignore
                app.UseHttpsRedirection() |> ignore

            app.UseStaticFiles() |> ignore
            app.UseCookiePolicy() |> ignore
            app.UseRouting() |> ignore
            app.UseAuthentication() |> ignore
            app.UseAuthorization() |> ignore
            app.UseAntiforgery() |> ignore

            app.MapHealthChecks(
                "/api/Health",
                HealthCheckOptions(
                    ResponseWriter =
                        fun httpContext healthReport ->
                            task {
                                if healthReport.Status = HealthStatus.Unhealthy then
                                    return Results.StatusCode(statusCode = StatusCodes.Status503ServiceUnavailable)
                                else
                                    return Results.Ok()
                            }
                )
            )
            |> ignore

            app.MapHealthChecks(
                "/api/HealthReport",
                HealthCheckOptions(
                    ResponseWriter =
                        fun httpContext healthReport ->
                            UIResponseWriter.WriteHealthCheckUIResponse(httpContext, healthReport)
                )
            )
            |> ignore

            app.UseForwardedHeaders(
                ForwardedHeadersOptions(
                    ForwardedHeaders =
                        (ForwardedHeaders.XForwardedFor
                         ||| ForwardedHeaders.XForwardedProto
                         ||| ForwardedHeaders.XForwardedHost)
                )
            )
            |> ignore

            app.MapGet("/", IndexHandler.renderPage).RequireAuthorization() |> ignore

            app.Run()

            SuccessExitCode
        with ex ->
            Console.Error.WriteLine ex

            FailureExitCode
