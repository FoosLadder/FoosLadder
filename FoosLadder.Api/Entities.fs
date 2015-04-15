namespace FoosLadder.Api.Entities

open FoosLadder.Api.Models 
open System.ComponentModel.DataAnnotations
open System

//TODO try make applicationtypes into DU


[<CLIMutable>]
type Client = {
    [<Key>]
    Id : string
    [<Required>]
    Secret : string
    [<Required>]
    [<MaxLength(100)>]
    Name : string
    ApplicationType : ApplicationTypes
    Active : bool
    RefreshTokenLifeTime : int
    [<MaxLength(100)>]
    AllowedOrigin : string
}

[<CLIMutable>]
type RefreshToken = {
    [<Key>]
    Id : string
    [<Required>]
    [<MaxLength(100)>]
    Subject : string
    [<Required>]
    [<MaxLength(50)>]
    ClientId : string
    IssuedUtc : DateTime
    ExpiresUtc : DateTime
    [<Required>]
    ProtectedTicket : string
}