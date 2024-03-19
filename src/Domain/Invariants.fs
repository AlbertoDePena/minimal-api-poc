namespace WebApp.Domain.Invariants

[<RequireQualifiedAccess>]
module String =

    /// The default value of a string is null.
    let defaultValue = null

/// <summary>
/// Represents a non null/empty/white-space string
/// </summary>
type Text =
    private
    | Text of string

    /// <summary>Unwrap the Text to it's primitive value</summary>
    member this.Value =
        let (Text value) = this
        value

    /// <summary>Apply a function to the Text's primitive value</summary>
    member this.Apply(f: string -> 'a) = this.Value |> f

    override this.ToString() = this.Value

    /// <summary>Try to convert a potentially null/empty/white-space string to a Text</summary>
    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        else
            Some(Text value)

    /// <summary>Try to convert a potentially null/empty/white-space email string to a Text</summary>
    static member OfEmailString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        elif System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(Text(value.ToLower()))
        else
            None

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64
    type EmailAddress = Text
    type Money = Decimal
    type Number = Int32
    type UniqueId = Guid
