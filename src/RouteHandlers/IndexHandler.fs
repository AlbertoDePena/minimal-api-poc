namespace WebApp.RouteHandlers

open System
open Microsoft.AspNetCore.Http

open FsToolkit.ErrorHandling

open WebApp.Extensions
open WebApp.Domain.TextClassification
open WebApp.Views
open WebApp.Views.Components

[<RequireQualifiedAccess>]
module IndexHandler =

    [<Literal>]
    let LoggerCategoryName = "Index"

    let handlePage: RouteHandler =
        handleRoute (fun httpContext ->
            task {

                let htmlContent =
                    PageView.render
                        { UserName = httpContext.GetUserName()
                          PageName = "Index"
                          PageContent = "<div>Home Page<div/>" }

                return Results.Html htmlContent
            })
