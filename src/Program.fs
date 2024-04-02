namespace WebApp

open System
open System.Threading

open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.Extensibility

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.CookiePolicy
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Microsoft.Extensions.Configuration

open Microsoft.Identity.Web

open Serilog

open WebApp.Domain.TextClassification
open WebApp.Infrastructure.Telemetry
open WebApp.Infrastructure.Serilog
open WebApp.Infrastructure.Dapper
open WebApp.Infrastructure.Options
open WebApp.Infrastructure.ErrorHandlerMiddleware
open WebApp.RouteHandlers

[<RequireQualifiedAccess>]
module Program =

    [<Literal>]
    let SuccessExitCode = 0

    [<Literal>]
    let FailureExitCode = -1

    [<EntryPoint>]
    let main args =
        try
            try
                Dapper.registerTypeHandlers ()

                let builder = WebApplication.CreateBuilder(args)

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

                builder.Services.AddAuthorization() |> ignore

                builder.Services.AddAntiforgery() |> ignore

                builder.Services.AddApplicationInsightsTelemetry() |> ignore

                builder.Services
                    .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
                    .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()
                    .AddSingleton<ITelemetryInitializer, AuthenticatedUserInitializer>()
                |> ignore

                builder.Services.AddSingleton<TextSampleDatabase>() |> ignore
                builder.Services.AddSingleton<LabelDatabase>() |> ignore

                builder.Host.UseSerilog(
                    Action<HostBuilderContext, IServiceProvider, LoggerConfiguration>
                        (fun context services loggerConfig ->
                            Serilog.configure context.Configuration services loggerConfig)
                )
                |> ignore

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

                let telemetryClient = app.Services.GetRequiredService<TelemetryClient>()
                let lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>()
                let applicationStopped = lifetime.ApplicationStopped

                applicationStopped.Register(fun () ->
                    telemetryClient.Flush()
                    Console.WriteLine("Flushing telemetry...")
                    Thread.Sleep(5000))
                |> ignore

                if app.Environment.IsDevelopment() then
                    app.UseDeveloperExceptionPage() |> ignore
                else
                    app.UseMiddleware<ErrorHandlerMiddleware>() |> ignore
                    app.UseHsts() |> ignore

                app.UseHttpsRedirection() |> ignore
                app.UseStaticFiles() |> ignore
                app.UseSerilogRequestLogging() |> ignore
                app.UseCookiePolicy() |> ignore
                app.UseRouting() |> ignore
                app.UseAuthentication() |> ignore
                app.UseAuthorization() |> ignore
                app.UseAntiforgery() |> ignore

                app.MapGet("/", IndexHandler.handlePage).RequireAuthorization() |> ignore

                app
                    .MapGet("/NextTextSample", IndexHandler.handleNextTextSample)
                    .RequireAuthorization()
                |> ignore

                app.MapGet("/Filter", IndexHandler.handleFilter).RequireAuthorization()
                |> ignore

                app
                    .MapGet("/SearchLabels", IndexHandler.handleSearchLabels)
                    .RequireAuthorization()
                |> ignore

                app.MapPost("/AddLabel", IndexHandler.handleAddLabel).RequireAuthorization()
                |> ignore

                app
                    .MapDelete("/RemoveLabel", IndexHandler.handleRemoveLabel)
                    .RequireAuthorization()
                |> ignore

                app.Run()

                SuccessExitCode
            with ex ->
                Console.Error.WriteLine(ex)

                FailureExitCode
        finally
            Console.WriteLine("Flushing serilog...")
            Log.CloseAndFlush()
