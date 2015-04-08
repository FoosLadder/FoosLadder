namespace FoosLadder.Api.Controllers

open System.Net
open System.Web
open System.Net.Http
open System.Web.Http

open FoosLadder.Api.CommonLibrary
open FoosLadder.Api.CommonLibrary.Rop
open FoosLadder.Api.Repositories

module Helpers =

    let validateDTOs (context : HttpContext)  =
        match context.AllErrors = null with
        | true -> Success context
        | false ->
            let dtoErrors = context.AllErrors |> Array.filter (fun error ->
                match error with 
                | :? Newtonsoft.Json.JsonSerializationException -> true
                | _ -> false ) |> Array.toSeq
            match dtoErrors |> Seq.isEmpty with
            | true -> Success context
            | false -> Failure <| ErrorMessage.DtoValidationError dtoErrors

module Players =

    open Helpers
    open FoosLadder.Api.DomainTypes.Players

    [<RoutePrefix("api/players")>]
    type PlayerController() = 
        inherit ApiController()
        
        [<Route("")>]
        member this.Get() = 
            match PlayerDbContext.LoadAll () with
            | Success records -> this.Ok(records)
            | Failure _ -> this.Ok([||])
        
        [<Route("{id}")>]
        member this.Get(id : int) = 
            match PlayerDbContext.Load id with
            | Success records -> this.Ok(records) :> IHttpActionResult
            | Failure _ -> this.NotFound() :> IHttpActionResult
        
        [<Route("")>]
        member this.Post([<FromBody>] player : Player) = 
            let result = 
                match validateDTOs HttpContext.Current with
                | Success _ -> PlayerDbContext.Store player
                | Failure failureMessage -> Failure failureMessage
            match result with
            | Success record -> this.Request.CreateResponse(record.Id)
            | Failure _ -> this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "")

module Matches =

    open Helpers
    open FoosLadder.Api.DomainTypes.Matches

    [<RoutePrefix("api/matches")>]
    type MatchController() = 
        inherit ApiController()
        
        [<Route("")>]
        member this.Get() = 
            match MatchDbContext.LoadAll () with
            | Success records -> this.Ok(records)
            | Failure _ -> this.Ok([||])
        
        [<Route("{id}")>]
        member this.Get(id : int) = 
            match MatchDbContext.Load id with
            | Success records -> this.Ok(records) :> IHttpActionResult
            | Failure _ -> this.NotFound() :> IHttpActionResult
    
        [<Route("completed")>]
        [<HttpPost>]
        member this.PostCompletedMatch([<FromBody>] completedMatch : CompletedMatch) =
            let temp = 1 
            let result = 
                match validateDTOs HttpContext.Current with
                | Success _ -> MatchDbContext.Store (Match.Completed completedMatch)
                | Failure failureMessage -> Failure failureMessage
            match result with
            | Success record -> this.Request.CreateResponse(retrieveMatchIdentifier record)
            | Failure _ -> this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "")
            
    