namespace FoosLadder.Api.Controllers

open FoosLadder.Api.Models
open System.Net
open System.Net.Http
open System.Web.Http
open System.Web.Http.Cors

//Example
/// Retrieves values.
[<EnableCorsAttribute(origins = "http://localhost:50441", headers = "*", methods = "*")>]
[<RoutePrefix("api")>]
type PlayerController() = 
    inherit ApiController()
    
    let mockPlayers = 
        [| { FirstName = "George"
             LastName = "Georgie"
             TotalMatchesPlayed = 12 
             TotalMatchesWon = 6
             TotalMatchesLost = 6}
           { FirstName = "Bob"
             LastName = "Bobby"
             TotalMatchesPlayed = 30
             TotalMatchesWon = 15
             TotalMatchesLost = 15} |]
    
    /// Gets all values.
    [<Route("players")>]
    member x.Get() = mockPlayers
    
    member x.Post([<FromBody>] player : Player) = mockPlayers |> Array.append [| player |]
    /// Gets a single value at the specified index.
    [<Route("players/{id}")>]
    member x.Get(request : HttpRequestMessage, id : int) = 
        if id >= 0 && mockPlayers.Length > id then 
            request.CreateResponse(mockPlayers.[id])
        else 
            request.CreateResponse(HttpStatusCode.NotFound)
