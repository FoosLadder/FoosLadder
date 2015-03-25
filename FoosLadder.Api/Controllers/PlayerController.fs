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
    
    let values = 
        [| { FirstName = "George"
             LastName = "Georgie"
             TotalMatchesPlayed = 12 }
           { FirstName = "Bob"
             LastName = "Bobby"
             TotalMatchesPlayed = 33 } |]
    
    /// Gets all values.
    [<Route("players")>]
    member x.Get() = values
    
    member x.Post([<FromBody>] player : Player) = values |> Array.append [| player |]
    /// Gets a single value at the specified index.
    [<Route("players/{id}")>]
    member x.Get(request : HttpRequestMessage, id : int) = 
        if id >= 0 && values.Length > id then 
            request.CreateResponse(values.[id])
        else 
            request.CreateResponse(HttpStatusCode.NotFound)
