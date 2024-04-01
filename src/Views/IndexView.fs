namespace WebApp.Views

open System

[<RequireQualifiedAccess>]
module Index =
    open WebApp.Domain.TextClassification
    open WebApp.Infrastructure.Html

    type LabelDataSourceProps = { Labels: Label list }

    let renderLabelDataSource (props: LabelDataSourceProps) =
        forEach
            props.Labels
            (fun label ->
                $"""<div class="label" id="{label.Id}" 
                        hx-post="/TextClassification/AddLabel?labelId={label.Id}">{label.Name}
                    </div>""")
            ""

    type TextSampleLabelsProps = { Labels: Label list }

    let renderTextSampleLabels (props: TextSampleLabelsProps) =
        forEach
            props.Labels
            (fun label ->
                $"""<div class="text-label" id="{label.Id}">
                        <div class="text">{label.Name}</div>
                        <button hx-delete="/TextClassification/RemoveLabel?labelId={label.Id}">X</button>
                    </div>""")
            ""

    type TextSampleProps = { TextSample: TextSample }

    let renderTextSample (props: TextSampleProps) =
        $"""
            <div id="TextSample" class="content">
                <p class="text-sample">
                    {props.TextSample.Text}
                </p>

                <div class="text-label-container">
                    {renderTextSampleLabels { Labels = props.TextSample.Labels }}
                </div>
            </div>
        """

    type IndexProps =
        { Labels: Label list
          TextSample: TextSample }

    let render (props: IndexProps) =

        let template =
            $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
                <meta http-equiv="Expires" content="0" />
                <meta http-equiv="Pragma" content="no-cache" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Text Classification</title>                
                <link rel="stylesheet" href="/css/text-classification.css" />
                <link rel="stylesheet" href="/css/custom.css" />
            </head>

            <body hx-indicator=".loader-container">
                <div class="navbar-container">
                    <nav class="navbar">
                        <div class="navbar-title">Text Classification</div>
                        <div class="navbar-links">
                            <div class="navbar-link">Text Samples</div>
                            <div class="navbar-link">Labels</div>
                        </div>
                    </nav>
                </div>
                <div class="main-container">
                    <main hx-target="#TextSample" hx-swap="innerHTML">
                        <div>
                            <div class="panel">
                                <div class="header">
                                    <select name="filter" hx-get="/TextClassification/Filter">
                                        <option value="{Filter.All}">All</option>
                                        <option value="{Filter.WithLabels}">With labels</option>
                                        <option value="{Filter.WithoutLabels}">Without Labels</option>
                                    </select>
                                    <button class="next" 
                                        hx-get="/TextClassification/NextTextSample">
                                        Next
                                    </button>
                                </div>
                                {renderTextSample { TextSample = props.TextSample }}                                
                            </div>
                        </div>

                        <div>
                            <div class="panel">
                                <div class="header">
                                    Labels
                                </div>
                                <div class="content">
                                    <input class="filter-label-input" placeholder="Filter labels..." @oninput=OnFilterLabelChanged />
                                    <div id="LabelDataSource" class="labels">
                                        {renderLabelDataSource { Labels = props.Labels }}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </main>
                </div>
                <div class="loader-container">
                    <div class="loader"></div>
                </div>

                <script src="https://unpkg.com/htmx.org@1.9.11"></script>
                <script src="/js/main.js"></script>
            </body>
            </html>
            """

        template
