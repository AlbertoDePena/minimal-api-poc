namespace WebApp

open System
open System.Threading
open System.Text
open System.Threading.Tasks

open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.Extensibility

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Authorization
open Microsoft.AspNetCore.CookiePolicy
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Microsoft.Extensions.Configuration

open Microsoft.Identity.Web

open Serilog

open WebApp.Infrastructure.Telemetry
open WebApp.Infrastructure.Serilog
open WebApp.Infrastructure.Dapper
open WebApp.Infrastructure.Options
open WebApp.Infrastructure.ErrorHandlerMiddleware
open WebApp.Infrastructure.HtmlTemplate

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

                if builder.Environment.IsDevelopment() then
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

                let sayHelloHandler: HttpHandler =
                    handleHttp (fun httpContext -> task { return Results.Ok "Hello World!" })

                let htmlHandler: HttpHandler =
                    handleHttp (fun httpContext ->
                        task {
                            let htmlContent = Html.load "<div>Hello Html!</div>" |> Html.render
                            return Results.Html htmlContent
                        })

                app.MapGet("/", sayHelloHandler).RequireAuthorization() |> ignore
                app.MapGet("/html", htmlHandler).RequireAuthorization() |> ignore

                app.Run()

                SuccessExitCode
            with ex ->
                Console.Error.WriteLine(ex)

                FailureExitCode
        finally
            Console.WriteLine("Flushing serilog...")
            Log.CloseAndFlush()