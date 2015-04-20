namespace FoosLadder.Api.Repositories

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

type AuthRepository() as this = 
    [<DefaultValue>]
    val mutable context : AuthContext
    [<DefaultValue>]
    val mutable userManager : UserManager<IdentityUser>

    let buildClientList() = 
        [ { Id = "foosLadderApp"
            Secret = getHash("abc@foo/Time:DGoal!!!!142")
            Name = "FoosLadder Front End"
            ApplicationType = ApplicationTypes.JavaScript
            Active = true
            RefreshTokenLifeTime = 7200
            AllowedOrigin = "http://localhost:50441" } ]
    
    let Seed(authContext : AuthContext) = 
        if authContext.Clients.Count() > 0 then ()
        else 
            authContext.Clients.AddRange(buildClientList()) |> ignore
            authContext.SaveChanges() |> ignore
    
    do 
        this.context <- new AuthContext()
        //Seed(this.context)
        this.userManager <- new UserManager<IdentityUser>(new UserStore<IdentityUser>(this.context))


    
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
       
    member __.AddRefreshToken(newToken : RefreshToken) = 
        async {
            let existingToken = 
                this.context.RefreshTokens 
                |> Seq.cast<RefreshToken>
                |> Seq.filter (fun t -> t.Subject = newToken.Subject && t.ClientId = newToken.ClientId)
                |> Seq.head
            if box existingToken <> null then
                let! result = this.RemoveRefreshToken existingToken 
                ()
            else 
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
