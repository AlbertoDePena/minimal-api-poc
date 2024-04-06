namespace WebApp.Views.Components

open WebApp.Domain.TextClassification

[<RequireQualifiedAccess>]
module TextSampleComponent =
    open WebApp.Views.Html

    [<NoComparison>]
    type Props =
        { ElementId: obj
          TextSample: TextSample }

    let render (props: Props) =

        let renderLabel (label: Label) =
            $"""
            <div id="{label.Id}">
                <div>{encode label.Name}</div>
                <button hx-delete="/RemoveLabel?labelId={label.Id}">X</button>
            </div>
            """

        $"""
        <article id="{props.ElementId}">
            <header>Text Sample</header>
            <div>
                <p>{encode props.TextSample.Text}</p>
                <div>
                    {forEach props.TextSample.Labels renderLabel ""}
                </div>
            </div>
        </article>
        """

[<RequireQualifiedAccess>]
module SelectLabelComponent =
    open WebApp.Views.Html

    [<NoComparison>]
    type Props = { ElementId: obj; Labels: Label list }

    let render (props: Props) =

        let renderLabel (label: Label) =
            $"""
            <div id="{label.Id}"
                 hx-target="#text-sample"
                 hx-post="/AddLabel?labelId={label.Id}">
                {encode label.Name}
            </div>
            """

        $"""
        <article id="{props.ElementId}" hx-target="this" hx-swap="outerHTML">
            <header>Labels</header>
            <div>
                <input name="label-filter" placeholder="Filter labels..."                              
                       hx-get="/SearchLabels" />
                <div>
                    {forEach props.Labels renderLabel ""}                   
                </div>
            </div>
        </article>
        """
