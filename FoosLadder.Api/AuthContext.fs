namespace FoosLadder.Api.Context
open Microsoft.AspNet.Identity.EntityFramework
open System.Data.Entity
open FoosLadder.Api.Entities

type AuthContext() = 
    inherit IdentityDbContext<IdentityUser>("AuthContext")

    //TODO find out which of these is better practice
//    [<DefaultValue>] val mutable Clients : DbSet<Client>
//    [<DefaultValue>] val mutable RefreshTokens : DbSet<RefreshToken>
    member val Clients = Unchecked.defaultof<DbSet<Client>> with get,set
    member val RefreshTokens = Unchecked.defaultof<DbSet<RefreshToken>> with get,set
