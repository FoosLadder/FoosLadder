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

    type GameResult =
        {    Index : int
             TeamA : int
             TeamB : int }

    [<CLIMutable>]
    type ProposedMatch = {
        Id : int
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime option
        Challenged : UserAction
    }
    
    [<CLIMutable>]
    type AcceptedMatch = {
        Id : int
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
    }
    
    [<CLIMutable>]
    type UnverifiedMatch = {
        Id : int
        PlayerA : PlayerIdentifier
        PlayerB : PlayerIdentifier
        MatchDate : DateTime
        Challenged : UserAction
        Accepted : UserAction
        GameResults : GameResult list
        Submitted : UserAction
    }
    
    [<CLIMutable>]
    type CompletedMatch = {
        Id : int
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

    let retrieveMatchIdentifier = 
        function | Proposed record -> record.Id
                 | Accepted record -> record.Id
                 | Unverified record -> record.Id
                 | Completed record -> record.Id
