namespace FoosLadder.Api.Controllers

open FoosLadder.Api.DomainTypes.Scores
open FoosLadder.Api.MockData
open System.Net
open System.Net.Http
open System.Web.Http


//[<EnableCorsAttribute(origins = "http://localhost:50441", headers = "*", methods = "*")>]
[<RoutePrefix("api")>]
type PlayerController() = 
    inherit ApiController()
    
    let mockScores = MockScores.generateRandomScores 0 100 |> List.toArray
    
    [<Route("players")>]
    member this.Get() = mockScores
    
    member this.Post([<FromBody>] score : Score) = mockScores |> Array.append [| score |]
    
    [<Route("players/{id}")>]
    member this.Get(request : HttpRequestMessage, id : int) = 
        if id >= 0 && mockScores.Length > id then 
            request.CreateResponse(mockScores.[id])
        else 
            request.CreateResponse(HttpStatusCode.NotFound)
