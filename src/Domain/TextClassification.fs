namespace WebApp.Domain.TextClassification

open System

[<RequireQualifiedAccess>]
type Filter =
    | All
    | WithLabels
    | WithoutLabels

    override this.ToString() =
        match this with
        | All -> "All"
        | WithLabels -> "With Labels"
        | WithoutLabels -> "Without Labels"

    static member OfString(value: string) =
        match value with
        | "All" -> Some All
        | "With Labels" -> Some WithLabels
        | "Without Labels" -> Some WithoutLabels
        | _ -> None

type Label =
    { Id: int
      Name: string }

    static member Empty = { Id = 0; Name = String.Empty }

type TextSample =
    { Id: int
      Text: string
      Labels: Label list }

    static member Empty =
        { Id = 0
          Text = String.Empty
          Labels = List.empty }

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

    member this.SearchLabels(searchCriteria: string) : Label list =
        this.GetLabels()
        |> List.filter (fun label -> label.Name.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase))
