/// Provides a nicer interop for configuring and starting the Kestrel server.
module KestrelInterop

open Freya.Core
open Freya.Core.Operators
open Freya.Polyfills.Kestrel
open Freya.Routers.Uri.Template
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

module ApplicationBuilder =
  let inline useFreya f (app:IApplicationBuilder)=
    let routes = UriTemplateRouter.Freya f
    let pipeline = Polyfill.kestrel >?= routes
    let midfunc : OwinMidFunc = OwinMidFunc.ofFreya pipeline
    app.UseOwin(fun p -> p.Invoke midfunc)

module WebHost =
  let create () = WebHostBuilder().UseKestrel()
  let bindTo urls (b:IWebHostBuilder) = b.UseUrls urls
  let configure (f : IApplicationBuilder -> IApplicationBuilder) (b:IWebHostBuilder) =
      b.Configure (System.Action<_> (f >> ignore))
  let build (b:IWebHostBuilder) = b.Build()
  let run (wh:IWebHost) = wh.Run()
  let buildAndRun : IWebHostBuilder -> unit = build >> run
