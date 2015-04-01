namespace FoosLadder.Api.DomainTypes

module Players =
    [<CLIMutable>]
    type Player =
        {   FirstName : string
            LastName : string }

module Scores =
    open Players

    [<CLIMutable>]
    type Score =
        {   Player: Player
            TotalMatchesPlayed : int
            TotalMatchesWon : int
            TotalMatchesLost : int }

module Actions =
    open System
    open Players

    type UserAction =
        { By : Player
          At : DateTime }


module Matches =
    open System
    open Actions
    open Players

    type GameScore = int

    type GameResult = GameScore * GameScore

    type ProposedMatch = {
        PlayerA : Player
        PlayerB : Player
        MatchDate : DateTime option
        Challenged : UserAction
    }

    type AcceptedMatch = {
        PlayerA : Player
        PlayerB : Player
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
    }

    type UnverifiedMatch = {
        PlayerA : Player
        PlayerB : Player
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
        GameResults : GameResult list
        Submitted : UserAction
    }

    type CompletedMatch = {
        PlayerA : Player
        PlayerB : Player
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
        GameResults : GameResult list
        Submitted : UserAction
        Verified : UserAction
        Winner : Player
        Loser : Player
    }

    type Match = 
        | Proposed of ProposedMatch
        | Accepted of AcceptedMatch
        | Unverified of UnverifiedMatch
        | Completed of CompletedMatch
