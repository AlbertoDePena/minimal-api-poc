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

                let token = httpContext.GetAntiforgeryToken()

                let mainContent =
                    $"""
                        <form hx-post="/HandleForm" hx-target="#FormResult">
                            {Html.Html.csrf token.FormFieldName token.RequestToken}
                            <fieldset>
                                <label>
                                First name
                                <input
                                    name="first_name"
                                    placeholder="First name"
                                    autocomplete="given-name"
                                />
                                </label>
                                <label>
                                Email
                                <input
                                    type="email"
                                    name="email"
                                    placeholder="Email"
                                    autocomplete="email"
                                />
                                </label>
                            </fieldset>

                            <input
                                type="submit"
                                value="Subscribe"
                            />
                            </form>

                            <div id="FormResult"></div>
                    """

                let htmlContent =
                    Page.render
                        { UserName = httpContext.GetUserName()
                          PageName = "Index"
                          PageContent = mainContent }

                return Results.Html htmlContent
            })

    let handleForm: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                do! httpContext.ValidateAntiforgeryToken()

                return Results.Html "Form Submitted!"
            })
