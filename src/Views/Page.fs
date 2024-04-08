namespace WebApp.Views

[<RequireQualifiedAccess>]
module Page =

    [<NoEquality>]
    [<NoComparison>]
    type Props =
        { PageName: string
          PageContent: string
          UserName: string }

    let render (props: Props) : string =
        $"""
            <!DOCTYPE html>
            <html lang="en" data-theme="light">
            <head>
                <meta charset="UTF-8">
                <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
                <meta http-equiv="Expires" content="0" />
                <meta http-equiv="Pragma" content="no-cache" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{props.PageName}</title>
                <link rel="stylesheet" href="/css/pico.2.0.6.css" />
                <link rel="stylesheet" href="/css/custom.css" />
            </head>

            <body hx-indicator=".loader">
                <nav class="container-fluid">
                    <ul>
                        <li><strong>HTMX POC</strong></li>
                    </ul>
                    <ul>
                        <li><a hx-boost="true" href="/PageOne">Page One</a></li>
                        <li><a hx-boost="true" href="/PageTwo">Page Two</a></li>
                        <li>{props.UserName}</li>
                        <li><button id="ThemeButton" type="button">Toggle Theme</button></li>
                    </ul>
                </nav>

                <main class="container">
                    {props.PageContent}
                </main>

                <div class="loader"></div>

                <script src="/js/htmx.1.9.11.js"></script>
                <script src="/js/main.js"></script>
            </body>
            </html>
            """
