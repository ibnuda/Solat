namespace Solat

open System

open Aether
open Chessie.ErrorHandling
open NodaTime

module Models =

  open Utilities

  type TipeSolat =
    | Subuh
    | Dzuhur
    | Ashar
    | Maghrib
    | Isha
    | Sunnah
    override __.ToString () =
      match __ with
      | Subuh   -> "Subuh"
      | Dzuhur  -> "Dzuhur"
      | Ashar   -> "Ashar"
      | Maghrib -> "Maghrib"
      | Isha    -> "Isha"
      | Sunnah  -> "Sunnah"
    static member FromString =
      function
      | "Subuh"   -> Subuh
      | "Dzuhur"  -> Dzuhur
      | "Ashar"   -> Ashar
      | "Maghrib" -> Maghrib
      | "Isha"    -> Isha
      | _         -> Sunnah

  type User =
    { Id       : Guid
      Username : string
      Password : string
      Email    : string }
    static member Buat i u p e =
      { Id       = i
        Username = u
        Password = p
        Email    = e }
    static member Daftar u p e =
      User.Buat (Guid.NewGuid()) u p e

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
      MasjidId : Guid // Masjid.Id
      Solat    : TipeSolat
      Waktu    : int64
      // UTC, unix epoch
      Dibuat   : int64
      // UTC, unix epoch
      Diupdate : int64 }
    static member Buat i m s w b u =
      { Id       = i
        MasjidId = m
        Solat    = s
        Waktu    = w
        Dibuat   = b
        Diupdate = u }
    static member DariUser m s w =
      Jamaah.Buat (Guid.NewGuid ()) m s w (now ()) (now ())
    static member Hampa =
      Jamaah.Buat Guid.Empty Guid.Empty Ashar 0L 0L 0L
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

  type PengaturanServer =
    { IdKlien                : string
      GoogleMapsKey          : string
      JarakMinMasjidNamaSama : float
      JarakMinMasjidNamaBeda : float }
    static member Buat i k s b =
      { IdKlien                = i
        GoogleMapsKey          = k
        JarakMinMasjidNamaSama = s
        JarakMinMasjidNamaBeda = b }
    static member Hampa = PengaturanServer.Buat "" "" 0. 0.
    static member I =
      (fun s -> s.IdKlien),
      (fun i s -> { s with IdKlien = i })
    static member K =
      (fun s -> s.GoogleMapsKey),
      (fun k s -> { s with GoogleMapsKey = k })
    static member S =
      (fun s -> s.JarakMinMasjidNamaSama),
      (fun m s -> { s with JarakMinMasjidNamaSama = m })
    static member B =
      (fun s -> s.JarakMinMasjidNamaBeda),
      (fun b s -> { s with JarakMinMasjidNamaBeda = b })

[<RequireQualifiedAccessAttribute>]
module EncodingDecoding =

  open Chiron
  open Chiron.Operators

  open Models

  [<RequireQualifiedAccessAttribute>]
  module Decode =

    let tipeSolat =
      TipeSolat.FromString
      <!> Json.Decode.required Json.Decode.string "tipesolat"

    let user =
      User.Daftar
      <!> Json.Decode.required Json.Decode.string "username"
      <*> Json.Decode.required Json.Decode.string "password"
      <*> Json.Decode.required Json.Decode.string "email"

    let masjid =
      Masjid.BuatData
      <!> Json.Decode.required Json.Decode.string "nama"
      <*> Json.Decode.required Json.Decode.float  "lintang"
      <*> Json.Decode.required Json.Decode.float  "bujur"
      <*> Json.Decode.required Json.Decode.string "alamat"

    let jamaah =
      Jamaah.DariUser
      <!> Json.Decode.required Json.Decode.guid "masjidid"
      <*> Json.Decode.required (Json.Decode.jsonObjectWith tipeSolat) "tipesolat"
      <*> Json.Decode.required Json.Decode.int64 "waktulokal"

    let pengaturanServer =
      PengaturanServer.Buat
      <!> Json.Decode.required Json.Decode.string "idklien"
      <*> Json.Decode.required Json.Decode.string "googlemapskey"
      <*> Json.Decode.required Json.Decode.float  "jarakminmasjidnamasama"
      <*> Json.Decode.required Json.Decode.float  "jarakminmasjidnamabeda"

  [<RequireQualifiedAccessAttribute>]
  module Encode =

    let tipeSolat (t : TipeSolat) =
      Json.Encode.required Json.Encode.string "tipesolat" (t.ToString())

    let masjid (m : Masjid) =
         Json.Encode.required Json.Encode.guid   "id"       m.Id
      >> Json.Encode.required Json.Encode.string "nama"     m.Nama
      >> Json.Encode.required Json.Encode.string "alamat"   m.Alamat
      >> Json.Encode.required Json.Encode.float  "lintang"  m.Lintang
      >> Json.Encode.required Json.Encode.float  "bujur"    m.Bujur
      >> Json.Encode.required Json.Encode.int64  "dibuat"   m.Dibuat
      >> Json.Encode.required Json.Encode.int64  "diupdate" m.Diupdate

    let jamaah (j : Jamaah) =
         Json.Encode.required Json.Encode.guid  "id"       j.Id
      >> Json.Encode.required Json.Encode.guid  "masjidid" j.MasjidId
      >> Json.Encode.required (Json.Encode.jsonObjectWith tipeSolat) "solat" j.Solat
      >> Json.Encode.required Json.Encode.int64 "waktu"    j.Waktu
      >> Json.Encode.required Json.Encode.int64 "dibuat"   j.Dibuat
      >> Json.Encode.required Json.Encode.int64 "diupdate" j.Diupdate
