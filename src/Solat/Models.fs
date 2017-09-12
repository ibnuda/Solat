namespace Solat

open System

open Aether
open NodaTime

module Models =

  open Utilities

  type TipeSolat =
    | Subuh
    | Dzuhur
    | Ashar
    | Maghrib
    | Isha
    override __.ToString () =
      match __ with
      | Subuh   -> "Subuh"
      | Dzuhur  -> "Dzuhur"
      | Ashar   -> "Ashar"
      | Maghrib -> "Maghrib"
      | Isha    -> "Isha"

  type Lokasi =
    { Id      : Guid
      Lintang : double
      Bujur   : double }
    static member Buat i l b =
      { Id      = i
        Lintang = l
        Bujur   = b}
    static member Hampa = Lokasi.Buat Guid.Empty 0. 0.
    static member L =
      (fun lo -> lo.Lintang),
      (fun l lo -> { lo with Lintang = l })
    static member B =
      (fun lo -> lo.Bujur),
      (fun b lo -> { lo with Bujur = b })

  type User =
    { Id       : Guid
      Username : string
      Password : string
      Email    : string }

  type Masjid =
    { Id       : Guid
      Nama     : string
      Lintang  : double
      Bujur    : double
      Alamat   : string
      // UTC, unix epoch
      Dibuat   : int64
      // UTC, unix epoch
      Diupdate : int64 }
    static member Buat i n l b a bu u =
      { Id       = i
        Nama     = n
        Lintang  = l
        Bujur    = b
        Alamat   = a
        Dibuat   = bu
        Diupdate = u }
    static member BuatData n l b a =
      Masjid.Buat (Guid.NewGuid ()) n l b a (now ()) (now ())
    static member Hampa = Masjid.Buat Guid.Empty "" 0. 0. "" 0L 0L
    // lenses
    static member N =
      (fun m -> m.Nama),
      (fun n m -> { m with Nama = n })
    static member A =
      (fun m -> m.Alamat),
      (fun a m -> { m with Alamat = a })
    static member B =
      (fun m -> m.Dibuat),
      (fun b m -> { m with Dibuat = b })
    static member U =
      (fun m -> m.Diupdate),
      (fun u m -> { m with Diupdate = u })

  type Jamaah =
    { Id       : Guid
      Masjid   : Guid // Masjid.Id
      Solat    : TipeSolat
      Waktu    : int64
      // UTC, unix epoch
      Dibuat   : int64
      // UTC, unix epoch
      Diupdate : int64 }
    static member Buat i m s w b u =
      { Id       = i
        Masjid   = m
        Solat    = s
        Waktu    = w
        Dibuat   = b
        Diupdate = u }
    static member Hampa = Jamaah.Buat Guid.Empty Guid.Empty Ashar 0L 0L 0L
    static member S =
      (fun j -> j.Solat),
      (fun s j -> { j with Solat = s })
    static member W =
      (fun j -> j.Waktu),
      (fun w j -> { j with Waktu = w })
    static member B =
      (fun m -> m.Dibuat),
      (fun b (m : Masjid) -> { m with Dibuat = b })
    static member U =
      (fun m -> m.Diupdate),
      (fun u (m : Masjid) -> { m with Diupdate = u })

  type UntukUser =
    { Nama      : string
      Lintang   : double
      Bujur     : double
      Alamat    : string
      TipeSolat : string
      // LocalTime, YY:MM:DD:HH:mm
      Waktu     : string }
    static member Buat n l b a t w =
      { Nama      = n
        Lintang   = l
        Bujur     = b
        Alamat    = a
        TipeSolat = t
        Waktu     = w }

[<RequireQualifiedAccessAttribute>]
module EncodingDecoding =

  open Chiron
  open Chiron.Operators

  open Models

  [<RequireQualifiedAccessAttribute>]
  module Decode =

    let masjid =
      Masjid.BuatData
      <!> Json.Decode.required Json.Decode.string "nama"
      <*> Json.Decode.required Json.Decode.float  "lintang"
      <*> Json.Decode.required Json.Decode.float  "bujur"
      <*> Json.Decode.required Json.Decode.string "alamat"

  [<RequireQualifiedAccessAttribute>]
  module Encode =
    let masjid (m : Masjid) jobj =
      jobj
      |> Json.Encode.required Json.Encode.guid   "id"       m.Id
      |> Json.Encode.required Json.Encode.string "nama"     m.Nama
      |> Json.Encode.required Json.Encode.string "alamat"   m.Alamat
      |> Json.Encode.required Json.Encode.float  "lintang"  m.Lintang
      |> Json.Encode.required Json.Encode.float  "bujur"    m.Bujur
      |> Json.Encode.required Json.Encode.int64  "dibuat"   m.Dibuat
      |> Json.Encode.required Json.Encode.int64  "diupdate" m.Diupdate
