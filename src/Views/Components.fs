namespace WebApp.Views.Components

open WebApp.Domain.TextClassification
open WebApp.Infrastructure.HtmlTemplate

[<RequireQualifiedAccess>]
module TextSampleComponent =

    [<NoComparison>]
    type Props =
        { ElementId: obj
          TextSample: TextSample }

    let render (props: Props) =
        Html.load "components/text-sample.html"
        |> Html.replace "ElementId" props.ElementId
        |> Html.replace "Text" props.TextSample.Text
        |> Html.replaceList "Label" props.TextSample.Labels (fun label template ->
            template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
        |> Html.render

[<RequireQualifiedAccess>]
module SelectLabelComponent =

    [<NoComparison>]
    type Props = { ElementId: obj; Labels: Label list }

    let render (props: Props) =
        Html.load "components/select-label.html"
        |> Html.replace "ElementId" props.ElementId
        |> Html.replaceList "Label" props.Labels (fun label template ->
            template |> Html.replace "Id" label.Id |> Html.replace "Name" label.Name)
        |> Html.render
