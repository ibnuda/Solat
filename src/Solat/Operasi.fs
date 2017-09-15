namespace Solat

open System
open System.IO

open Chessie.ErrorHandling

module Operasi =

  open Utilities
  open Models

  let getPengaturan () =
    try
      let confName = "solat.settings.json"
      let lines = File.ReadAllLines confName |> String.Concat
      jsonToModel EncodingDecoding.Decode.pengaturanServer lines
    with
    | _ -> ok PengaturanServer.Hampa
