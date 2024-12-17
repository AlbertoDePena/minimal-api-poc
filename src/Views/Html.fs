namespace WebApp.Views.Html

open System
open System.Globalization
open System.Net

[<RequireQualifiedAccess>]
module Html =
    
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

    let antiforgery (formFieldName: string) (requestToken: string) : string =
        $"""<input name="{formFieldName}" type="hidden" value="{requestToken}">"""

    let encode (value: string) : string = WebUtility.HtmlEncode value

    let forEach<'a> (items: 'a list) (mapping: 'a -> string) (separator: string) : string =
        items |> List.map mapping |> String.concat separator

    let dateInputValue (date: DateOnly) : string =
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

    let dateTimeInputValue (dateTime: DateTime) : string =
        dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)

    let dateTimeOffsetInputValue (dateTimeOffset: DateTimeOffset) : string =
        dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)

    let formatDate (date: DateOnly) : string =
        date.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTime (dateTime: DateTime) : string =
        dateTime.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTimeOffset (dateTimeOffset: DateTimeOffset) : string =
        dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)

    
