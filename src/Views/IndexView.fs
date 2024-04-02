namespace WebApp.Views

[<RequireQualifiedAccess>]
module IndexView =
    open WebApp.Views.Html

    [<NoEquality>]
    [<NoComparison>]
    type Props =
        { Shared: SharedProps
          MainContent: string }

    let render (props: Props) : string =

        let indexContent =
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
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@picocss/pico@2/css/pico.min.css" />
                <link rel="stylesheet" href="/css/custom.css" />
            </head>

            <body hx-indicator=".loader-container">
                <nav class="container-fluid">
                    <ul>
                        <li><strong>Text Classification</strong></li>
                    </ul>
                    <ul>
                        <li><a href="#">Text Samples</a></li>
                        <li><a href="#">Labels</a></li>
                        <li>{props.Shared.UserName}</li>
                    </ul>
                </nav>

                <main class="container">
                    {props.MainContent}
                </main>

                <div class="loader-container">
                    <div class="loader"></div>
                </div>

                <script src="https://unpkg.com/htmx.org@1.9.11"></script>
                <script src="/js/main.js"></script>
            </body>
            </html>
            """

        match props.Shared.IsHtmxBoosted with
        | true -> props.MainContent
        | false -> indexContent
