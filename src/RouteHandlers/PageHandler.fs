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
                    IndexView.render
                        { PageName = "Page One"
                          UserName = "Demo User"
                          MainContent = mainContent }

                return Results.Html htmlContent
            })

    let handlePageTwo: RouteHandler =
        handleRoute (fun httpContext ->
            task {

                let mainContent = "<div>Page Two</div>"

                let htmlContent =
                    IndexView.render
                        { PageName = "Page Two"
                          UserName = "Demo User"
                          MainContent = mainContent }

                return Results.Html htmlContent
            })
