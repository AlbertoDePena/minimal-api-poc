namespace WebApp.RouteHandlers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

open WebApp.Extensions
open WebApp.Domain.TextClassification
open WebApp.Views

type TextSampleDatabase() =
    let dataSource = ResizeArray<TextSample>()
    let mutable currentIndex: int = -1
    let mutable currentFilter: Filter = Filter.All

    member this.SetFilter(filter: Filter) = currentFilter <- filter

    member this.GetTextSamples() : TextSample list =
        if dataSource.Count = 0 then
            let data =
                Array.init 10 (fun index ->
                    let modelId = index + 1

                    let model =
                        { Id = modelId
                          Text = sprintf "Text sample %i" modelId
                          Labels = List.empty }

                    model)

            dataSource.AddRange data

        let data = dataSource.ToArray() |> Array.toList

        match currentFilter with
        | Filter.All -> data
        | Filter.WithLabels -> data |> List.filter (fun x -> not x.Labels.IsEmpty)
        | Filter.WithoutLabels -> data |> List.filter (fun x -> x.Labels.IsEmpty)

    member this.GetCurrentTextSample() : TextSample =
        this.GetTextSamples()
        |> List.mapi (fun index item -> if index = currentIndex then Some item else None)
        |> List.choose id
        |> List.tryHead
        |> Option.defaultValue TextSample.Empty

    member this.GetNextTextSample() : TextSample =
        currentIndex <- (currentIndex + 1)

        if this.GetTextSamples().Length = currentIndex then
            currentIndex <- 0

        this.GetCurrentTextSample()

    member this.GetTextSampleById(id: int) : TextSample option =
        this.GetTextSamples() |> List.tryFind (fun textSample -> textSample.Id = id)

    member this.UpdateTextSample(id: int, textSample: TextSample) : unit =
        this.GetTextSampleById id |> Option.iter (fun _ -> dataSource[id] <- textSample)

type LabelDatabase() =
    let dataSource = ResizeArray<Label>()

    member this.GetLabels() : Label list =
        if dataSource.Count = 0 then
            let data =
                Array.init 20 (fun index ->
                    let modelId = index + 1

                    let model =
                        { Id = modelId
                          Name = sprintf "Label %i" modelId }

                    model)

            dataSource.AddRange data

        dataSource.ToArray() |> Array.toList

    member this.GetLabelById(id: int) : Label option =
        this.GetLabels() |> List.tryFind (fun label -> label.Id = id)

[<RequireQualifiedAccess>]
module TextClassificationHandler =

    [<Literal>]
    let LoggerCategoryName = "TextClassification"

    let handle: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let labelDb = httpContext.GetService<LabelDatabase>()
                let textSampleDb = httpContext.GetService<TextSampleDatabase>()

                let labels = labelDb.GetLabels()
                let textSample = textSampleDb.GetNextTextSample()

                let htmlContent =
                    Index.render
                        { Labels = labels
                          TextSample = textSample }

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

                    let htmlContent = Index.renderTextSample { TextSample = textSample }

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

                        let htmlContent = Index.renderTextSample { TextSample = updatedTextSample }

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

                        let htmlContent = Index.renderTextSample { TextSample = updatedTextSample }

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

                    let htmlContent = Index.renderTextSample { TextSample = textSample }

                    return Results.Html htmlContent
            })
