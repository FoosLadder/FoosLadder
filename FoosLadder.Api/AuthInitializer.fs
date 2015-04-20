namespace FoosLadder.Api.Initializer

open FoosLadder.Api.Context
open FoosLadder.Api.Entities
open FoosLadder.Api.Models
open HashHelper
open System.Linq
open FoosLadder.Api

type AuthInitializer() = 
    inherit System.Data.Entity.CreateDatabaseIfNotExists<AuthContext>()
    
    let buildClientList() = 
        let webRootUrl = 
            match Infrastructure.GetApplicationSetting "WebRootUrl" with
            | Some value -> value
            | None -> raise (System.ArgumentException("WebRootUrl setting is missing. Check that it is set correctly in the web config"))
        [ { Id = "foosLadderApp"
            Secret = getHash("abc@foo/Time:DGoal!!!!142")
            Name = "FoosLadder Front End"
            ApplicationType = ApplicationTypes.JavaScript
            Active = true
            RefreshTokenLifeTime = 7200
            AllowedOrigin = webRootUrl } ]
    
    override __.Seed(context : AuthContext) = 
        if context.Clients.Count() > 0 then ()
        else 
            context.Clients.AddRange(buildClientList()) |> ignore
            context.SaveChanges() |> ignore
