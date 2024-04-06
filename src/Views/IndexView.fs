namespace WebApp.Views

[<RequireQualifiedAccess>]
module IndexView =

    [<NoEquality>]
    [<NoComparison>]
    type Props =
        { PageName: string
          UserName: string
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
                <title>{props.PageName}</title>
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@picocss/pico@2/css/pico.min.css" />
                <link rel="stylesheet" href="/css/custom.css" />
            </head>

            <body hx-indicator=".loader">
                <nav class="container-fluid">
                    <ul>
                        <li><strong>Text Classification</strong></li>
                    </ul>
                    <ul>
                        <li><a hx-boost="true" href="/PageOne">Page One</a></li>
                        <li><a hx-boost="true" href="/PageTwo">Page Two</a></li>
                        <li>{props.UserName}</li>
                    </ul>
                </nav>

                <main class="container">
                    {props.MainContent}
                </main>

                <div class="loader"></div>

                <script src="https://unpkg.com/htmx.org@1.9.11"></script>
                <script src="/js/main.js"></script>
            </body>
            </html>
            """

        indexContent
