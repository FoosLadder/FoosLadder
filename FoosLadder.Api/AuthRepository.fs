namespace FoosLadder.Api.Repositories

open FoosLadder.Api.Context
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open System
open FoosLadder.Api.Models
open System.Threading.Tasks

type AuthRepository() as this= 

    [<DefaultValue>] val mutable context : AuthContext 
    [<DefaultValue>] val mutable userManager : UserManager<IdentityUser> 
    do
        this.context <- new AuthContext()
        this.userManager <- new UserManager<IdentityUser>(new UserStore<IdentityUser>(this.context))

    member __.RegisterUser (userModel : UserModel) = 
        async {
            let user = IdentityUser(UserName=userModel.UserName)
            let createResult = this.userManager.CreateAsync(user, userModel.Password) |> Async.AwaitTask
            return! createResult
        }


    member __.FindUser userName password = 
        async {
            let user = this.userManager.FindAsync(userName,password) |> Async.AwaitTask
            return! user
        }

    
    member __.Dispose() = 
        this.context.Dispose()
        this.userManager.Dispose()
    interface IDisposable with
        member __.Dispose() = this.Dispose()


