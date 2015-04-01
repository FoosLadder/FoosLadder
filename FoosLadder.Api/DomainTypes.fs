namespace FoosLadder.Api.DomainTypes

module Players =

    type PlayerIdentifier = int

    [<CLIMutable>]
    type Player =
        {   Id : PlayerIdentifier
            FirstName : string
            LastName : string
            TotalMatchesPlayed : int
            TotalMatchesWon : int
            TotalMatchesLost : int }

module Actions =
    open System
    open Players

    type UserAction =
        { By : PlayerIdentifier
          At : DateTime }


module Matches =
    open System
    open Actions
    open Players

    type GameScore = int

    type GameResult = GameScore * GameScore

    type ProposedMatch = {
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime option
        Challenged : UserAction
    }

    type AcceptedMatch = {
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
    }

    type UnverifiedMatch = {
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
        GameResults : GameResult list
        Submitted : UserAction
    }

    type CompletedMatch = {
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
        GameResults : GameResult list
        Submitted : UserAction
        Verified : UserAction
        Winner : PlayerIdentifier
        Loser : PlayerIdentifier
    }

    type Match = 
        | Proposed of ProposedMatch
        | Accepted of AcceptedMatch
        | Unverified of UnverifiedMatch
        | Completed of CompletedMatch
