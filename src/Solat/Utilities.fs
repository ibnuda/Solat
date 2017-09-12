namespace Solat

open System

open Chessie.ErrorHandling
open Chiron

module Utilities =

  let now () =
    DateTime.Now.Ticks

  let jsonToModel (decoder : JsonObject -> JsonResult<'T>) js =
    let res =
      js
      |> Json.parse
      |> JsonResult.bind (Json.Decode.jsonObjectWith decoder)
    match res with
    | JPass res -> ok res
    | JFail _   -> fail "not a valid"

  let modelToJson (encoder : 'T -> JsonObject -> JsonObject) =
    Json.Encode.jsonObjectWith encoder >> Json.format
