namespace WebApp.RouteHandlers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

open WebApp.Extensions
open WebApp.Domain.TextClassification
open WebApp.Views

type TextSampleDatabase() =
    let mutable textSamples = List.empty<TextSample>
    let mutable currentIndex: int option = None

    member this.GetTextSamples() =
        if textSamples.IsEmpty then
            let data =
                List.init 10 (fun index ->
                    { Id = index + 1
                      Value = sprintf "Text sample %i" (index + 1)
                      Labels = List.empty })

            textSamples <- data
            textSamples
        else
            textSamples

    member this.GetCurrentTextSample() : TextSample =
        this.GetTextSamples()
        |> List.mapi (fun index item -> if Some index = currentIndex then Some item else None)
        |> List.choose id
        |> List.tryHead
        |> Option.defaultValue TextSample.Empty

    member this.GetNextTextSample() : TextSample =
        match currentIndex with
        | Some index -> currentIndex <- Some(index + 1)
        | None -> currentIndex <- Some(0)

        this.GetCurrentTextSample()

type LabelDatabase() =
    let mutable labels = List.empty<Label>

    member this.GetLabels() =
        if labels.IsEmpty then
            let data =
                List.init 20 (fun index ->
                    { Id = index + 1
                      Name = sprintf "Label %i" (index + 1) })

            labels <- data
            labels
        else
            labels

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
                | None -> return failwith ""
                | Some labelId ->
                    let labelDb = httpContext.GetService<LabelDatabase>()


                    return Results.Html ""
            })
