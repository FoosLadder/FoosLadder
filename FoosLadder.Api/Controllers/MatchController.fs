namespace FoosLadder.Api.Controllers

open FoosLadder.Api.DomainTypes.Matches
open FoosLadder.Api.MockData
open System.Net
open System.Net.Http
open System.Web.Http


[<RoutePrefix("api/matches")>]
type MatchController() = 
    inherit ApiController()
    
    let mockMatches = MockMatch.generateRandomMatches 0 10 |> List.toArray
    
    [<Route("")>]
    member this.Get() = mockMatches
    
    [<Route("{id}")>]
    member this.Get(request : HttpRequestMessage, id : int) = 
        if id >= 0 && mockMatches.Length > id then 
            request.CreateResponse(mockMatches.[id])
        else 
            request.CreateResponse(HttpStatusCode.NotFound)
    
    [<Route("")>]
    [<HttpPost>]
    member this.Post([<FromBody>] matchResult : Match) = 
        mockMatches |> Array.append [| matchResult |]
