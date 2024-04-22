namespace WebApp.Infrastructure.Html

open System
open System.Globalization
open System.Net

[<AutoOpen>]
module Html =

    let csrf (formFieldName: string) (requestToken: string) : string =
        $"""<input name="{formFieldName}" type="hidden" value="{requestToken}">"""

    let encode (value: string) : string = WebUtility.HtmlEncode value

    let forEach<'a> (items: 'a list) (mapping: 'a -> string) (separator: string) : string =
        items |> List.map mapping |> String.concat separator

    let formatDateTime (dataTime: DateTime) : string =
        dataTime.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTimeOffset (dateTimeOffset: DateTimeOffset) : string =
        dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)

[<AutoOpen>]
module HtmlAttribute =

    let disabled (value: bool) : string =
        match value with
        | true -> "disabled"
        | false -> ""

    let readonly (value: bool) : string =
        match value with
        | true -> "readonly"
        | false -> ""

    let required (value: bool) : string =
        match value with
        | true -> "required"
        | false -> ""

[<RequireQualifiedAccess>]
module HtmlTemplate =
    open System
    open System.IO
    open Scriban
    open Scriban.Runtime

    let private currentDirectory =
        Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

    let render (model: obj) (fileOrContent: string) : string =

        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        let htmlContent =
            if fileOrContent.EndsWith(".html") then
                Path.Combine(currentDirectory, fileOrContent) |> File.ReadAllText
            else
                fileOrContent

        let scriptObject = ScriptObject()
        scriptObject.Import(obj = model, renamer = (fun x -> x.Name))

        let context = TemplateContext(StrictVariables = true)
        context.PushGlobal(scriptObject)

        let template = Template.Parse(htmlContent)

        if template.HasErrors then
            failwithf "%A" template.Messages

        template.Render(context)
