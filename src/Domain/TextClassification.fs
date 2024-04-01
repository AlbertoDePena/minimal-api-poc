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

type Label =
    { Id: int
      Name: string }

    static member Empty = { Id = 0; Name = String.Empty }

type TextSample =
    { Id: int
      Value: string
      Labels: Label list }

    static member Empty =
        { Id = 0
          Value = String.Empty
          Labels = List.empty }
