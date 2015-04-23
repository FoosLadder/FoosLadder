﻿namespace FoosLadder.Api.Repositories

open FoosLadder.Api.Context
open FoosLadder.Api.Entities
open FoosLadder.Api.Models
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open System
open System.Linq
open System.Collections.Generic
open System.Threading.Tasks
open HashHelper

type AuthRepository(authContext : AuthContext) as this = 
    [<DefaultValue>]
    val mutable context : AuthContext
    [<DefaultValue>]
    val mutable userManager : UserManager<IdentityUser>

//    let buildClientList() = 
//        [ { Id = "foosLadderApp"
//            Secret = getHash("abc@foo/Time:DGoal!!!!142")
//            Name = "FoosLadder Front End"
//            ApplicationType = ApplicationTypes.JavaScript
//            Active = true
//            RefreshTokenLifeTime = 7200
//            AllowedOrigin = "http://localhost:50441" } ]
//    
//    let Seed(authContext : AuthContext) = 
//        let clientsSeq = authContext.Clients |> Seq.toList
//        let clientsCount = clientsSeq |> List.length
//        if clientsCount > 0 then ()
//        else 
//            authContext.Clients.AddRange(buildClientList()) |> ignore
//            authContext.SaveChanges() |> ignore
    
    do 
        this.context <- authContext
        //Seed(this.context)
        this.userManager <- new UserManager<IdentityUser>(new UserStore<IdentityUser>(this.context))

    new() = new AuthRepository(new AuthContext())


    
    member __.RegisterUser(userModel : UserModel) = 
        async { 
            let user = IdentityUser(UserName = userModel.UserName)
            let createResult = this.userManager.CreateAsync(user, userModel.Password) |> Async.AwaitTask
            return! createResult
        }
    
    member __.FindUser userName password = 
        async { 
            let user = this.userManager.FindAsync(userName, password) |> Async.AwaitTask
            return! user
        }
    
    //TODO If Nothing C# is calling this method, it can return  Client option
    member __.FindClient (clientId : string) = this.context.Clients.Find(clientId)

    member __.FindAsync loginInfo = 
        async {
            let user = this.userManager.FindAsync loginInfo |> Async.AwaitTask
            return! user
        }

    member __.CreateAsync user =
        async {
            let result = this.userManager.CreateAsync user |> Async.AwaitTask
            return! result
        }

    member __.AddLoginAsync userId login = 
        async {
            let result = this.userManager.AddLoginAsync(userId, login) |> Async.AwaitTask
            return! result
        }

    member __.RemoveRefreshToken (refreshToken : RefreshToken) =
        async {
            this.context.RefreshTokens.Remove(refreshToken) |> ignore
            let! result = this.context.SaveChangesAsync() |> Async.AwaitTask
            return result > 0
        }
        
    member __.RemoveRefreshToken (refreshTokenId : string) = 
        async {
            let! refreshToken = this.context.RefreshTokens.FindAsync(refreshTokenId) |> Async.AwaitTask

            if box refreshToken <> null then
                return! this.RemoveRefreshToken refreshToken
            else
                return false
        }


    let getExistingRefreshToken (tokens : RefreshToken seq) (newToken : RefreshToken) : RefreshToken option = 
        if Seq.isEmpty tokens then
            None
        else 
            let existingTokens = 
                tokens
                |> Seq.cast<RefreshToken>
                |> Seq.filter (fun t -> t.Subject = newToken.Subject && t.ClientId = newToken.ClientId)
            if Seq.isEmpty existingTokens then
                None
            else if box (Seq.head existingTokens) = null then
                None
            else                
                Some <| Seq.head existingTokens

       
    member __.AddRefreshToken(newToken : RefreshToken) = 
        async {
            let possibleExistingToken = getExistingRefreshToken (this.context.refreshTokens) newToken
            match possibleExistingToken with
            | None -> ()
            | Some existingToken -> 
                let! result = this.RemoveRefreshToken existingToken
                ()

            this.context.RefreshTokens.Add(newToken) |> ignore
            let! result = this.context.SaveChangesAsync() |> Async.AwaitTask
            return result > 0
        }

    member __.FindRefreshToken (refreshTokenId : string) = 
        async {
            return! this.context.RefreshTokens.FindAsync(refreshTokenId) |> Async.AwaitTask
        }    

    member __.GetAllRefreshTokens() = this.context.RefreshTokens |> Seq.cast<RefreshToken> |> Seq.toList
    
    member __.Dispose() = 
        this.context.Dispose()
        this.userManager.Dispose()
    
    interface IDisposable with
        member __.Dispose() = this.Dispose()
