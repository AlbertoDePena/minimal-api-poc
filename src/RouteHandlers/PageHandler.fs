namespace WebApp.RouteHandlers

open System
open Microsoft.AspNetCore.Http

open FsToolkit.ErrorHandling

open WebApp.Extensions
open WebApp.Domain.TextClassification
open WebApp.Views
open WebApp.Views.Components

[<RequireQualifiedAccess>]
module PageHandler =

    let handlePageOne: RouteHandler =
        handleRoute (fun httpContext ->
            task {

                let mainContent = "<div>Page one</div>"

                let htmlContent =
                    Page.render
                        { PageName = "Page One"
                          UserName = httpContext.GetUserName()
                          PageContent = mainContent }

                return Results.Html htmlContent
            })

    let handlePageTwo: RouteHandler =
        handleRoute (fun httpContext ->
            task {

                let mainContent = "<div>Page Two</div>"

                let htmlContent =
                    Page.render
                        { PageName = "Page Two"
                          UserName = httpContext.GetUserName()
                          PageContent = mainContent }

                return Results.Html htmlContent
            })
