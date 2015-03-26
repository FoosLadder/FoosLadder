namespace FoosLadder.Api.Models


[<CLIMutable>]
type Player =
    {   FirstName : string
        LastName : string
        TotalMatchesPlayed : int
        TotalMatchesWon : int
        TotalMatchesLost : int }

