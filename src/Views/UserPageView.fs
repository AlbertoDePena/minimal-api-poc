namespace WebApp.Views.UserPageView

open WebApp.Views.Html
open WebApp.Views.LayoutView

[<RequireQualifiedAccess>]
module UserPageView =

    let render (message: string) (userName: string) : string =
        let mainContent = 
            $"""
                <div>{message}</div>
            """

        LayoutView.render
            { UserName = userName
              PageTitle = "WebApp - User Details"
              MainContent = mainContent }
