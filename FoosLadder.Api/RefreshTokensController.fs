namespace FoosLadder.Api.Controllers

open System.Web.Http
open FoosLadder.Api.Repositories
open System.Threading.Tasks

//TODO make controller methods async where possible

[<RoutePrefix("api/refreshTokens")>]
type RefreshTokensController() as this = 
    inherit ApiController()

    [<DefaultValue>] val mutable repo : AuthRepository

    do 
        this.repo <- new AuthRepository()

    [<Authorize(Users="Admin")>]
    [<Route("")>]
    member __.Get() = 
        this.Ok(this.repo.GetAllRefreshTokens())



    [<Authorize(Users="Admin")>]
    [<Route("")>]
    member __.Delete (tokenId : string)= 
        //[<AllowAnonymous>]
        let result = this.repo.RemoveRefreshToken tokenId |> Async.RunSynchronously
        if result then
            this.Ok() :> IHttpActionResult
        else 
            this.BadRequest("Token Id does not exist") :> IHttpActionResult

    override __.Dispose(disposing : bool) = 
        if disposing then
            this.repo.Dispose() |> ignore
        base.Dispose(disposing) |> ignore

