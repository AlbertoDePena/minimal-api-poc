namespace WebApp.Infrastructure.HtmlTemplate

open System
open System.IO
open Scriban

[<RequireQualifiedAccess>]
module HtmlTemplate =

    let private currentDirectory =
        Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

    let render (model: 'a) (fileOrContent: string) =

        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        let htmlContent =
            if fileOrContent.EndsWith(".html") then
                Path.Combine(currentDirectory, fileOrContent) |> File.ReadAllText
            else
                fileOrContent

        let template = Template.Parse(htmlContent)

        if template.HasErrors then
            failwithf "%A" template.Messages

        template.Render(model, memberRenamer = (fun m -> m.Name))
