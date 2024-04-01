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
