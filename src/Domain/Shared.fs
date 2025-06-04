namespace WebApp.Domain.Shared

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

/// <summary>
/// Represents a guid that is not empty.
/// </summary>
type UniqueId =
    private
    | UniqueId of System.Guid

    member this.Value =
        let (UniqueId value) = this
        value

    override this.ToString() =
        this.Value |> fun guid -> guid.ToString()

    static member TryCreate(value: System.Guid) =
        if value = System.Guid.Empty then
            None
        else
            Some(UniqueId value)

    static member Create() =
        RT.Comb.Provider.Sql.Create() |> UniqueId

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64
    type Money = Decimal
    type Number = Int32
    type Timestamp = DateTimeOffset
