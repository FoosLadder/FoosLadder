namespace FoosLadder.Api.Controllers

open FoosLadder.Api.Repositories
open System.Web.Http
open System
open Microsoft.AspNet.Identity

//TODO reorganise modules and types
[<RoutePrefix("api/account")>]
type AccountController() as this= 
    inherit ApiController()

    [<DefaultValue>] val mutable repo : AuthRepository

    do 
        this.repo <- new AuthRepository()

    member private __.GetErrorResult (result : IdentityResult) = 
        if result = null then
            this.InternalServerError() :> IHttpActionResult
        else 
            if not result.Succeeded then
                if result.Errors <> null then
                    for error in result.Errors do
                        this.ModelState.AddModelError("", error) |> ignore
                if this.ModelState.IsValid then
                    this.BadRequest() :> IHttpActionResult
                else 
                    this.BadRequest(this.ModelState) :> IHttpActionResult 
            else
                null

    [<AllowAnonymous>]
    [<Route("register")>]
    member this.Register userModel = 
        if not this.ModelState.IsValid then
            this.BadRequest(this.ModelState : ModelBinding.ModelStateDictionary) :> IHttpActionResult
        else 
            let result = this.repo.RegisterUser(userModel) |> Async.RunSynchronously
            let errorResult = this.GetErrorResult(result) 
            if errorResult <> null then
                errorResult
            else
                this.Ok() :> IHttpActionResult
       
    override x.Dispose(disposing : bool) = 
        if disposing then
            this.repo.Dispose() |> ignore
        base.Dispose(disposing) |> ignore

 

        
            
