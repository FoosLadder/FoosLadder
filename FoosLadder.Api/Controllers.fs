namespace FoosLadder.Api.Controllers

open System.Net
open System.Net.Http
open System.Web.Http

open FoosLadder.Api.CommonLibrary

module Helpers =


    let validateDTOs dtos =
        let context = System.Web.HttpContext.Current
        match context.AllErrors = null with
        | true -> Success dtos
        | false ->
            let dtoErrors = context.AllErrors |> Array.filter (fun error ->
                match error with 
                | :? Newtonsoft.Json.JsonSerializationException -> true
                | _ -> false ) |> Array.toSeq
            match dtoErrors |> Seq.isEmpty with
            | true -> Success dtos
            | false -> Failure <| ErrorMessage.DtoValidationError dtoErrors

module Players =

    open FoosLadder.Api.MockData
    open FoosLadder.Api.DomainTypes.Players

    [<RoutePrefix("api/players")>]
    type PlayerController() = 
        inherit ApiController()
        
        let mockPlayers = MockPlayers.generateRandomPlayers 0 100 |> List.toArray
        
        [<Route("")>]
        member this.Get() = mockPlayers
        
        [<Route("")>]
        member this.Post([<FromBody>] player : Player) = mockPlayers |> Array.append [| player |]
        
        [<Route("{id}")>]
        member this.Get(request : HttpRequestMessage, id : int) = 
            if id >= 0 && mockPlayers.Length > id then 
                request.CreateResponse(mockPlayers.[id])
            else 
                request.CreateResponse(HttpStatusCode.NotFound)

module Matches =

    open Helpers
    open FoosLadder.Api.DomainTypes.Matches
    open FoosLadder.Api.Repositories
    open FoosLadder.Api.CommonLibrary.Rop

    [<RoutePrefix("api/matches")>]
    type MatchController() = 
        inherit ApiController()
        
        [<Route("")>]
        member this.Get() = 
            match MatchDbContext.LoadAll () with
            | Success records -> this.Ok(records)
            | Failure _ -> this.Ok([||])
        
        [<Route("{id}")>]
        member this.Get(request : HttpRequestMessage, id : int) = 
            match MatchDbContext.Load id with
            | Success records -> this.Ok(records) :> IHttpActionResult
            | Failure _ -> this.NotFound() :> IHttpActionResult
    
        [<Route("completed")>]
        [<HttpPost>]
        member this.PostCompletedMatch([<FromBody>] completedMatch : CompletedMatch) = 
            let result =
                validateDTOs
                |> bind <| succeed (Match.Completed completedMatch)
                |> bind MatchDbContext.Store
            match result with
            | Success record -> this.Request.CreateResponse(retrieveMatchIdentifier record)
            | Failure _ -> this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "")
            
    