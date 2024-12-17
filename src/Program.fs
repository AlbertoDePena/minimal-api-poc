namespace WebApp.Program

open System
open System.Threading

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
open OpenTelemetry.Exporter
open OpenTelemetry.Logs
open OpenTelemetry.Metrics
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open OpenTelemetry.Instrumentation.AspNetCore

open Microsoft.Identity.Web

open WebApp.Infrastructure.Telemetry
open WebApp.Infrastructure.Dapper
open WebApp.Infrastructure.Options
open WebApp.Infrastructure.ErrorHandlerMiddleware
open WebApp.Endpoints
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.Extensions.Diagnostics.HealthChecks
open HealthChecks.UI.Client

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

            let builder = WebApplication.CreateBuilder(args)

            let isDevelopment = builder.Environment.IsDevelopment()

            builder.Services
                .AddOptions<DatabaseOptions>()
                .Configure<IConfiguration>(fun settings configuration ->
                    configuration.GetSection("Database").Bind(settings))
            |> ignore

            builder.Services.Configure<CookiePolicyOptions>(
                Action<CookiePolicyOptions>(fun options ->
                    options.Secure <- CookieSecurePolicy.Always
                    options.HttpOnly <- HttpOnlyPolicy.Always
                    options.MinimumSameSitePolicy <- SameSiteMode.Lax
                    options.CheckConsentNeeded <- (fun context -> true)
                    options.HandleSameSiteCookieCompatibility() |> ignore)
            )
            |> ignore

            builder.Services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(fun options -> builder.Configuration.Bind("AzureAd", options))
            |> ignore

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
                .WithLogging(fun loggerBuilder ->
                    if isDevelopment then
                        loggerBuilder.AddConsoleExporter() |> ignore)
                .WithMetrics(fun meterBuilder ->
                    meterBuilder.AddAspNetCoreInstrumentation() |> ignore
                    meterBuilder.AddHttpClientInstrumentation() |> ignore
                    meterBuilder.AddMeter(Telemetry.ApplicationName) |> ignore

                    if isDevelopment then
                        meterBuilder.AddConsoleExporter() |> ignore)
                .WithTracing(fun tracerBuilder ->
                    tracerBuilder.AddAspNetCoreInstrumentation() |> ignore
                    tracerBuilder.AddHttpClientInstrumentation() |> ignore
                    tracerBuilder.AddSource(Telemetry.ApplicationName) |> ignore

                    if isDevelopment then
                        tracerBuilder.AddConsoleExporter() |> ignore)
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
                            services.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString),
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
                        (fun httpContext healthReport ->
                            task {
                                if healthReport.Status = HealthStatus.Unhealthy then
                                    return Results.StatusCode(statusCode = StatusCodes.Status503ServiceUnavailable)
                                else
                                    return Results.Ok()
                            })
                )
            )
            |> ignore

            app.MapHealthChecks(
                "/api/HealthReport",
                HealthCheckOptions(
                    ResponseWriter =
                        (fun httpContext healthReport ->
                            UIResponseWriter.WriteHealthCheckUIResponse(httpContext, healthReport))
                )
            )
            |> ignore

            app.MapGet("/", IndexHandler.handle).RequireAuthorization() |> ignore

            app.Run()

            SuccessExitCode
        with ex ->
            Console.Error.WriteLine(ex)

            FailureExitCode
