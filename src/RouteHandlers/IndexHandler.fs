namespace WebApp.RouteHandlers

open System
open Microsoft.AspNetCore.Http

open FsToolkit.ErrorHandling

open WebApp.Extensions
open WebApp.Domain.TextClassification
open WebApp.Infrastructure.HtmlTemplate

[<RequireQualifiedAccess>]
module IndexHandler =

    [<RequireQualifiedAccess>]
    type ElementId =
        | TextSample
        | Filter
        | LabelDataSource

        override this.ToString() =
            match this with
            | TextSample -> "text-sample"
            | Filter -> "filter"
            | LabelDataSource -> "label-data-source"

    [<Literal>]
    let LoggerCategoryName = "Index"

    let handle: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let textSampleDb = httpContext.GetService<TextSampleDatabase>()
                let labelDb = httpContext.GetService<LabelDatabase>()

                let textSample = textSampleDb.GetCurrentTextSample()
                let labels = labelDb.GetLabels()

                let textSampleComponent =
                    Html.load "components/text-sample.html"
                    |> Html.replace "TextSampleElementId" ElementId.TextSample
                    |> Html.replace "Text" textSample.Text
                    |> Html.replaceList "Label" textSample.Labels (fun label template ->
                        template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
                    |> Html.render

                let selectLabelComponent =
                    Html.load "components/select-label.html"
                    |> Html.replace "LabelDataSourceElementId" ElementId.LabelDataSource
                    |> Html.replaceList "Label" labels (fun label template ->
                        template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
                    |> Html.render

                let htmlContent =
                    Html.load "index.html"
                    |> Html.replace "TextSampleElementId" ElementId.TextSample
                    |> Html.replace "FilterElementId" ElementId.Filter
                    |> Html.replace "All" Filter.All
                    |> Html.replace "WithLabels" Filter.WithLabels
                    |> Html.replace "WithoutLabels" Filter.WithoutLabels
                    |> Html.replaceRaw "TextSampleComponent" textSampleComponent
                    |> Html.replaceRaw "SelectLabelComponent" selectLabelComponent
                    |> Html.render

                return Results.Html htmlContent
            })

    let handleNextTextSample: RouteHandler =
        handleRoute (fun httpContext ->
            task {
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
                        Html.load "components/text-sample.html"
                        |> Html.replace "TextSampleElementId" ElementId.TextSample
                        |> Html.replace "Text" textSample.Text
                        |> Html.replaceList "Label" textSample.Labels (fun label template ->
                            template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
                        |> Html.render

                    return Results.Html htmlContent
            })

    let handleAddLabel: RouteHandler =
        handleRoute (fun httpContext ->
            task {
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
                            Html.load "components/text-sample.html"
                            |> Html.replace "TextSampleElementId" ElementId.TextSample
                            |> Html.replace "Text" updatedTextSample.Text
                            |> Html.replaceList "Label" updatedTextSample.Labels (fun label template ->
                                template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
                            |> Html.render

                        return Results.Html htmlContent
            })

    let handleRemoveLabel: RouteHandler =
        handleRoute (fun httpContext ->
            task {
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
                            Html.load "components/text-sample.html"
                            |> Html.replace "TextSampleElementId" ElementId.TextSample
                            |> Html.replace "Text" updatedTextSample.Text
                            |> Html.replaceList "Label" updatedTextSample.Labels (fun label template ->
                                template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
                            |> Html.render

                        return Results.Html htmlContent
            })

    let handleFilter: RouteHandler =
        handleRoute (fun httpContext ->
            task {
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
                        Html.load "components/text-sample.html"
                        |> Html.replace "TextSampleElementId" ElementId.TextSample
                        |> Html.replace "Text" textSample.Text
                        |> Html.replaceList "Label" textSample.Labels (fun label template ->
                            template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
                        |> Html.render

                    return Results.Html htmlContent
            })
