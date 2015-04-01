namespace FoosLadder.Api.Controllers

open FoosLadder.Api.DomainTypes.Players
open FoosLadder.Api.MockData
open System.Net
open System.Net.Http
open System.Web.Http


//[<EnableCorsAttribute(origins = "http://localhost:50441", headers = "*", methods = "*")>]
[<RoutePrefix("api")>]
type PlayerController() = 
    inherit ApiController()
    
    let mockPlayers = MockPlayers.generateRandomPlayers 0 100 |> List.toArray
    
    [<Route("players")>]
    member this.Get() = mockPlayers
    
    member this.Post([<FromBody>] player : Player) = mockPlayers |> Array.append [| player |]
    
    [<Route("players/{id}")>]
    member this.Get(request : HttpRequestMessage, id : int) = 
        if id >= 0 && mockPlayers.Length > id then 
            request.CreateResponse(mockPlayers.[id])
        else 
            request.CreateResponse(HttpStatusCode.NotFound)
