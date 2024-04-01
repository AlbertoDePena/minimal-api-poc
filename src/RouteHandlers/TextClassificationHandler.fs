namespace WebApp.RouteHandlers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

open WebApp.Extensions
open WebApp.Domain.TextClassification
open WebApp.Views

type TextSampleDatabase() =
    let mutable dataSource = Map.empty<int, TextSample>
    let mutable currentIndex: int = -1

    member this.GetTextSamples() =
        if dataSource.IsEmpty then
            let data =
                List.init 10 (fun index ->
                    let modelId = index + 1

                    let model =
                        { Id = modelId
                          Value = sprintf "Text sample %i" modelId
                          Labels = List.empty }

                    modelId, model)

            dataSource <- data |> Map.ofList

        dataSource |> Map.toList |> List.map (fun (key, item) -> item)

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

    member this.GetTextSampleById(id: int) : TextSample option = dataSource.TryFind id

    member this.UpdateTextSample(id: int, textSample: TextSample) : unit =
        dataSource.TryFind id
        |> Option.iter (fun item -> dataSource <- dataSource.Add(id, textSample))

type LabelDatabase() =
    let mutable dataSource = Map.empty<int, Label>

    member this.GetLabels() =
        if dataSource.IsEmpty then
            let data =
                List.init 20 (fun index ->
                    let modelId = index + 1

                    let model =
                        { Id = modelId
                          Name = sprintf "Label %i" modelId }

                    modelId, model)

            dataSource <- data |> Map.ofList

        dataSource |> Map.toList |> List.map (fun (key, item) -> item)

    member this.GetLabelById(id: int) : Label option = dataSource.TryFind id

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

                let props: Index.IndexProps =
                    { Labels = labels
                      TextSample = textSample }

                let htmlContent = Index.render props

                return Results.Html htmlContent
            })

    let handleNextTextSample: RouteHandler =
        handleRoute (fun httpContext ->
            task {
                let textSampleDb = httpContext.GetService<TextSampleDatabase>()
                let textSample = textSampleDb.GetNextTextSample()

                let props: Index.TextSampleProps = { TextSample = textSample }

                let htmlContent = Index.renderTextSample props

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

                        let props: Index.TextSampleProps = { TextSample = updatedTextSample }

                        let htmlContent = Index.renderTextSample props

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

                        let props: Index.TextSampleProps = { TextSample = updatedTextSample }

                        let htmlContent = Index.renderTextSample props

                        return Results.Html htmlContent
            })
