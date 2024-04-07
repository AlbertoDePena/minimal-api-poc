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

    override this.ToString() =
        let (EmailAddress value) = this
        value

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

    override this.ToString() =
        let (Text value) = this
        value

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
