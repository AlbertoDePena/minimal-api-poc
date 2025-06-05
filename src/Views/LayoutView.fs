namespace WebApp.Views.LayoutView

open System

type LayoutViewModel =
    { PageTitle: string
      UserName: string
      MainContent: string }

[<RequireQualifiedAccess>]
module LayoutView =

    let render (model: LayoutViewModel) : string =
        $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
                <meta http-equiv="Expires" content="0" />
                <meta http-equiv="Pragma" content="no-cache" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{model.PageTitle}</title>                
                <link rel="stylesheet" href="/css/text-classification.css" />
                <link rel="stylesheet" href="/css/custom.css" />
            </head>

            <body hx-indicator=".loader-container">
                <div class="navbar-container">
                    <nav class="navbar">
                        <div class="navbar-title">{model.UserName}</div>
                        <div class="navbar-links">
                            <div class="navbar-link">Text Samples</div>
                            <div class="navbar-link">Labels</div>
                        </div>
                    </nav>
                </div>
                <div class="main-container">
                    <main>
                        {model.MainContent}
                    </main>
                </div>
                <div class="loader-container">
                    <div class="loader"></div>
                </div>

                <script src="/js/htmx.2.0.4.js"></script>
                <script src="/js/sweetalert2.11.17.2.js"></script>
                <script src="/js/main.js"></script>
            </body>
            </html>
            """
