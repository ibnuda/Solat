namespace Solat

open System

open Marten
open Npgsql

module DbHelpers =

  open Models

  let createConnectionString host username password database =
    sprintf "Host=%s;Username=%s;Password=%s;Database=%s" host username password database
    |> NpgsqlConnectionStringBuilder
    |> string

  let store h u p d =
    DocumentStore.For (fun doc ->
      doc.AutoCreateSchemaObjects <- AutoCreate.CreateOrUpdate
      doc.Connection (createConnectionString h u p d)
      doc.Logger (ConsoleMartenLogger ())
      doc.Schema.For<User>().Index(fun u -> u.Id :> obj) |> ignore
      doc.Schema.For<Masjid>().Index(fun m -> m.Id :> obj) |> ignore
      doc.Schema.For<Jamaah>().Index(fun j -> j.Id :> obj) |> ignore
    )
