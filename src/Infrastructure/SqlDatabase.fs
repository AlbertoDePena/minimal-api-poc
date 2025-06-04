namespace WebApp.Infrastructure.SqlDatabase

open System.Data
open Microsoft.Data.SqlClient

type SqlDatabase(connectionString: string) =

    member this.CreateConnection() : IDbConnection =
        new SqlConnection(connectionString) :> IDbConnection