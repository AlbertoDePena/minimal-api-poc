namespace WebApp.Infrastructure.UserDatabase

open System.Threading.Tasks
open System.Data
open Microsoft.Extensions.Options
open WebApp.Infrastructure.Options
open WebApp.Infrastructure.SqlDatabase
open WebApp.Domain.Shared
open WebApp.Domain.User
open Dapper
open FsToolkit.ErrorHandling

type UserDatabase(databaseOptions: IOptions<DatabaseOptions>) =
    inherit SqlDatabase(databaseOptions.Value.SqlConnectionString)

    member this.TryFindById(id: UniqueId) : Task<User option> =
        task {
            use connection = this.CreateConnection()

            let! result =
                connection.QueryFirstOrDefaultAsync<User>(
                    "dbo.Users_FindById",
                    {| Id = id.Value |},
                    commandType = CommandType.StoredProcedure
                )

            return result |> Option.ofNull
        }

    member this.TryFindByEmail(emailAddress: EmailAddress) : Task<User option> =
        task {
            use connection = this.CreateConnection()

            let! result =
                connection.QueryFirstOrDefaultAsync<User>(
                    "dbo.Users_FindByEmail",
                    {| EmailAddress = emailAddress.Value |},
                    commandType = CommandType.StoredProcedure
                )

            return result |> Option.ofNull
        }

    member this.Delete(id: UniqueId) : Task<unit> =
        task {
            use connection = this.CreateConnection()

            do!
                connection.QueryFirstOrDefaultAsync<User>(
                    "dbo.Users_Delete",
                    {| Id = id.Value |},
                    commandType = CommandType.StoredProcedure
                )
                |> Task.ignore
        }

    member this.Create(user: User) : Task<unit> =
        task {
            use connection = this.CreateConnection()

            do!
                connection.ExecuteAsync(
                    "dbo.Users_Create",
                    {| Id = user.Id.Value
                       DisplayName = user.DisplayName.Value
                       Email = user.EmailAddress.Value |},
                    commandType = CommandType.StoredProcedure
                )
                |> Task.ignore
        }

    member this.Update(user: User) : Task<unit> =
        task {
            use connection = this.CreateConnection()

            do!
                connection.ExecuteAsync(
                    "dbo.Users_Update",
                    {| Id = user.Id.Value
                       DisplayName = user.DisplayName.Value
                       Email = user.EmailAddress.Value |},
                    commandType = CommandType.StoredProcedure
                )
                |> Task.ignore
        }
