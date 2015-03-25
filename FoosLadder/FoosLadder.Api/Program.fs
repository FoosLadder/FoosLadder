module Program

open Microsoft.Owin.Hosting
open System
open System.Net.Http
open FsKatanaStart
 
[<EntryPoint>]
let main argv =
    let baseAddress = "http://localhost:8000"
    let server = WebApp.Start<Startup>(baseAddress)
    //really dumb way to lock console,
    //server should be wraped in windows service
    Console.WriteLine("Press enter to exit ... ")
    Console.ReadLine() |> ignore
    server.Dispose()
    0 