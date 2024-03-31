namespace WebApp.Domain.Invariants

[<RequireQualifiedAccess>]
module String =

    /// The default value of a string is null.
    let defaultValue = null

/// <summary>
/// Represents a non null/empty/white-space email address
/// </summary>
type EmailAddress =
    private
    | EmailAddress of string

    /// <summary>Unwrap the EmailAddress to it's primitive value</summary>
    member this.Value =
        let (EmailAddress value) = this
        value

    override this.ToString() = this.Value

    /// <summary>Try to convert a potentially null/empty/white-space string to an EmailAddress</summary>
    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        elif System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(EmailAddress(value.ToLower()))
        else
            None

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

    override this.ToString() = this.Value

    /// <summary>Try to convert a potentially null/empty/white-space string to a Text</summary>
    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        else
            Some(Text value)

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64
    type Money = Decimal
    type Number = Int32
    type UniqueId = Guid
