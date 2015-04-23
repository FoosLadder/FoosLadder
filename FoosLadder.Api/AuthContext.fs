namespace FoosLadder.Api.Context
open Microsoft.AspNet.Identity.EntityFramework
open System.Data.Entity
open FoosLadder.Api.Entities

type AuthContext()= 
    inherit IdentityDbContext<IdentityUser>("AuthContext")

    [<DefaultValue>] val mutable clients : DbSet<Client>
    member this.Clients
        with get () = this.clients
        and set value = this.clients <- value

    [<DefaultValue>] val mutable refreshTokens : DbSet<RefreshToken>

    member this.RefreshTokens
        with get () = this.refreshTokens
        and set value = this.refreshTokens <- value

    static member Create() = new  AuthContext()