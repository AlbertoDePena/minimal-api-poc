namespace WebApp.Domain.Invariants

/// <summary>
/// Represents a valid email address
/// </summary>
type EmailAddress =
    private
    | EmailAddress of string

    member this.Value =
        let (EmailAddress value) = this
        value

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        elif System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(EmailAddress(value.ToLower()))
        else
            None

/// <summary>
/// Represents a string that is not null, not empty, and not white-space.
/// </summary>
type Text =
    private
    | Text of string

    member this.Value =
        let (Text value) = this
        value

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
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
    type Timestamp = DateTimeOffset
