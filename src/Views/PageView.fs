namespace WebApp.Views

open WebApp.Domain.TextClassification
open WebApp.Views.Html
open WebApp.Views.Components

[<RequireQualifiedAccess>]
module PageView =

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

    [<NoEquality>]
    [<NoComparison>]
    type Props =
        { IsHtmxBoosted: bool
          UserName: string
          TextSample: TextSample
          Labels: Label list }

    let render (props: Props) : string =
        let mainContent =
            $"""
            <div class="grid" hx-target="#{ElementId.TextSample}" hx-swap="outerHTML">
                <div>
                    <fieldset role="group">
                        <select id="{ElementId.Filter}" name="{ElementId.Filter}" title="Select a filter" hx-get="/Filter">
                            <option value="{Filter.All}">All</option>
                            <option value="{Filter.WithLabels}">With Labels</option>
                            <option value="{Filter.WithoutLabels}">Without Labels</option>
                        </select>
                        <button class="next"
                                hx-get="/NextTextSample"
                                hx-include="#{ElementId.Filter}">
                            Next
                        </button>
                    </fieldset>

                    {TextSampleComponent.render
                         { ElementId = ElementId.TextSample
                           TextSample = props.TextSample }}
                </div>

                {SelectLabelComponent.render
                     { ElementId = ElementId.LabelDataSource
                       Labels = props.Labels }}
            </div>        
            """

        IndexView.render
            { PageName = "Some Page"
              UserName = props.UserName
              MainContent = mainContent }
