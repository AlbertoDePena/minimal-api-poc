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
                let textSampleDb = httpContext.GetService<TextSampleDatabase>()
                let labelDb = httpContext.GetService<LabelDatabase>()

                let textSample = textSampleDb.GetCurrentTextSample()
                let labels = labelDb.GetLabels()

                let htmlContent =
                    PageView.render
                        { IsHtmxBoosted = httpContext.Request.IsHtmxBoosted()
                          UserName = httpContext.GetUserName()
                          TextSample = textSample
                          Labels = labels }

                return Results.Html htmlContent
            })

    let handleNextTextSample: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                if httpContext.Request.IsHtmx() then
                    let filterOption =
                        httpContext.Request.TryGetQueryStringValue "filter"
                        |> Option.bind Filter.OfString

                    match filterOption with
                    | None -> return failwith "filter is required"
                    | Some filter ->
                        let textSampleDb = httpContext.GetService<TextSampleDatabase>()

                        textSampleDb.SetFilter filter

                        let textSample = textSampleDb.GetNextTextSample()

                        let htmlContent =
                            TextSampleComponent.render
                                { ElementId = PageView.ElementId.TextSample
                                  TextSample = textSample }

                        return Results.Html htmlContent
                else
                    return Results.Redirect("/")
            })

    let handleAddLabel: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                if httpContext.Request.IsHtmx() then
                    let labelIdOption =
                        httpContext.Request.TryGetQueryStringValue "labelId"
                        |> Option.map Int32.TryParse
                        |> Option.bind Option.ofPair

                    match labelIdOption with
                    | None -> return failwith "labelId is required"
                    | Some labelId ->
                        let textSampleDb = httpContext.GetService<TextSampleDatabase>()
                        let labelDb = httpContext.GetService<LabelDatabase>()
                        let labelOption = labelDb.GetLabelById labelId

                        match labelOption with
                        | None -> return failwith "Label not found"
                        | Some label ->
                            let textSample = textSampleDb.GetCurrentTextSample()

                            let updatedTextSample =
                                { textSample with
                                    Labels = textSample.Labels @ [ label ] |> List.distinct }

                            textSampleDb.UpdateTextSample(updatedTextSample.Id, updatedTextSample)

                            let htmlContent =
                                TextSampleComponent.render
                                    { ElementId = PageView.ElementId.TextSample
                                      TextSample = updatedTextSample }

                            return Results.Html htmlContent
                else
                    return Results.Redirect("/")
            })

    let handleRemoveLabel: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                if httpContext.Request.IsHtmx() then
                    let labelIdOption =
                        httpContext.Request.TryGetQueryStringValue "labelId"
                        |> Option.map Int32.TryParse
                        |> Option.bind Option.ofPair

                    match labelIdOption with
                    | None -> return failwith "labelId is required"
                    | Some labelId ->
                        let textSampleDb = httpContext.GetService<TextSampleDatabase>()
                        let labelDb = httpContext.GetService<LabelDatabase>()
                        let labelOption = labelDb.GetLabelById labelId

                        match labelOption with
                        | None -> return failwith "Label not found"
                        | Some label ->
                            let textSample = textSampleDb.GetCurrentTextSample()

                            let updatedTextSample =
                                { textSample with
                                    Labels = textSample.Labels |> List.filter (fun x -> x.Id <> label.Id) }

                            textSampleDb.UpdateTextSample(updatedTextSample.Id, updatedTextSample)

                            let htmlContent =
                                TextSampleComponent.render
                                    { ElementId = PageView.ElementId.TextSample
                                      TextSample = updatedTextSample }

                            return Results.Html htmlContent
                else
                    return Results.Redirect("/")
            })

    let handleFilter: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                if httpContext.Request.IsHtmx() then
                    let filterOption =
                        httpContext.Request.TryGetQueryStringValue "filter"
                        |> Option.bind Filter.OfString

                    match filterOption with
                    | None -> return failwith "filter is required"
                    | Some filter ->
                        let textSampleDb = httpContext.GetService<TextSampleDatabase>()

                        textSampleDb.SetFilter filter

                        let textSample = textSampleDb.GetCurrentTextSample()

                        let htmlContent =
                            TextSampleComponent.render
                                { ElementId = PageView.ElementId.TextSample
                                  TextSample = textSample }

                        return Results.Html htmlContent
                else
                    return Results.Redirect("/")
            })

    let handleSearchLabels: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                if httpContext.Request.IsHtmx() then
                    let filterOption = httpContext.Request.TryGetQueryStringValue "label-filter"

                    match filterOption with
                    | None -> return failwith "filter is required"
                    | Some filter ->
                        let labelDb = httpContext.GetService<LabelDatabase>()

                        let labels = labelDb.SearchLabels(filter)

                        let htmlContent =
                            SelectLabelComponent.render
                                { ElementId = PageView.ElementId.LabelDataSource
                                  Labels = labels }

                        return Results.Html htmlContent
                else
                    return Results.Redirect("/")
            })
