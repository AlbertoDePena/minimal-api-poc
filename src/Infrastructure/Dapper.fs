namespace WebApp.Infrastructure.Dapper

open System
open Dapper

open WebApp.Domain.Shared
open WebApp.Domain.User
open WebApp.Infrastructure.Extensions

[<RequireQualifiedAccess>]
module Dapper =

    type private StringContainerHandler<'T>(ofString: string -> 'T option, getValue: 'T -> string) =
        inherit SqlMapper.TypeHandler<'T>()

        let typeName = typeof<'T>.Name

        override _.SetValue(param, value) = param.Value <- getValue value

        override _.Parse value =
            value :?> string
            |> ofString
            |> Option.defaultWith (fun () -> failwithf "The value %O cannot be parsed into a %s" value typeName)

    type private StringContainerOptionHandler<'T>(ofString: string -> 'T option, getValue: 'T -> string) =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override _.SetValue(param, value) =
            param.Value <-
                match value with
                | Some t -> getValue t
                | None -> String.defaultValue

        override _.Parse value = value :?> string |> ofString

    type private EmptyStringHandler() =
        inherit SqlMapper.TypeHandler<string>()

        override _.SetValue(param, value) =
            param.Value <-
                if String.IsNullOrWhiteSpace value then
                    String.defaultValue
                else
                    value

        override _.Parse value =
            if isNull value || value = box DBNull.Value then
                String.defaultValue
            else
                value.ToString()

    type private OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override _.SetValue(param, value) =
            let valueOrNull =
                match value with
                | Some t -> box t
                | None -> String.defaultValue

            param.Value <- valueOrNull

        override _.Parse value =
            if isNull value || value = box DBNull.Value then
                None
            else
                Some(value :?> 'T)

    let private singleton =
        lazy
            (
             SqlMapper.AddTypeHandler(EmptyStringHandler())
             // primitive type wrapped in an option
             SqlMapper.AddTypeHandler(OptionHandler<Guid>())
             SqlMapper.AddTypeHandler(OptionHandler<byte>())
             SqlMapper.AddTypeHandler(OptionHandler<int16>())
             SqlMapper.AddTypeHandler(OptionHandler<int>())
             SqlMapper.AddTypeHandler(OptionHandler<int64>())
             SqlMapper.AddTypeHandler(OptionHandler<uint16>())
             SqlMapper.AddTypeHandler(OptionHandler<uint>())
             SqlMapper.AddTypeHandler(OptionHandler<uint64>())
             SqlMapper.AddTypeHandler(OptionHandler<float>())
             SqlMapper.AddTypeHandler(OptionHandler<decimal>())
             SqlMapper.AddTypeHandler(OptionHandler<float32>())
             SqlMapper.AddTypeHandler(OptionHandler<string>())
             SqlMapper.AddTypeHandler(OptionHandler<char>())
             SqlMapper.AddTypeHandler(OptionHandler<DateTime>())
             SqlMapper.AddTypeHandler(OptionHandler<DateTimeOffset>())
             SqlMapper.AddTypeHandler(OptionHandler<DateOnly>())
             SqlMapper.AddTypeHandler(OptionHandler<bool>())
             SqlMapper.AddTypeHandler(OptionHandler<TimeSpan>())             
             // string wrapped in a container
             SqlMapper.AddTypeHandler(StringContainerHandler(EmailAddress.TryCreate, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerHandler(Text.TryCreate, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerHandler(UserType.TryCreate, (fun x -> x.Value)))
             // string wrapped in an optional container
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(EmailAddress.TryCreate, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(Text.TryCreate, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(UserType.TryCreate, (fun x -> x.Value))))

    /// Register Dapper type handlers
    let registerTypeHandlers () = singleton.Force()
