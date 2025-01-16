namespace WebApp.Domain.Invariants

/// <summary>
/// Represents a non-null email address
/// </summary>
type EmailAddress =
    private
    | EmailAddress of string

    /// <summary>Gets the value of the email address.</summary>
    member this.Value =
        let (EmailAddress value) = this
        value

    override this.ToString() = this.Value

    /// <summary>Try to convert a potentially null/empty/white-space string to an EmailAddress.</summary>
    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        elif System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(EmailAddress(value.ToLower()))
        else
            None

/// <summary>
/// Represents a non-null string.
/// </summary>
type NonNullString =
    private
    | NonNullString of string

    /// <summary>Gets the value of the non-null string.</summary>
    member this.Value =
        let (NonNullString value) = this
        value

    override this.ToString() = this.Value

    /// <summary>The default value of a non-null string is the empty string.</summary>
    static member DefaultValue = NonNullString ""

    /// <summary>Creates a non-null string from the given value.</summary>
    static member Create(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            NonNullString.DefaultValue
        else
            NonNullString value

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64
    type Money = Decimal
    type Number = Int32
    type UniqueId = Guid
    type Text = NonNullString
    type Timestamp = DateTimeOffset
