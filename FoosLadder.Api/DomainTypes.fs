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

    [<CLIMutable>]
    type GameResult =
        {    Id : int
             Index : int
             PlayerA : int
             PlayerB : int }

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

    let previousMatchState =
        function | Proposed record -> Proposed record
                 | Accepted record ->
                     Proposed {
                         Id = record.Id
                         PlayerA = record.PlayerA
                         PlayerB = record.PlayerB
                         MatchDate = Some record.MatchDate
                         Challenged = { By = record.Challenged.By; At = record.Challenged.At } }
                 | Unverified record ->
                     Accepted {
                         Id = record.Id
                         PlayerA = record.PlayerA
                         PlayerB = record.PlayerB
                         MatchDate = record.MatchDate
                         Challenged = { By = record.Challenged.By; At = record.Challenged.At }
                         Accepted = { By = record.Challenged.By; At = record.Challenged.At } }
                 | Completed record ->
                     Unverified {
                         Id = record.Id
                         PlayerA = record.PlayerA
                         PlayerB = record.PlayerB
                         MatchDate = record.MatchDate
                         Challenged = { By = record.Challenged.By; At = record.Challenged.At }
                         Accepted = { By = record.Challenged.By; At = record.Challenged.At }
                         GameResults = record.GameResults
                         Submitted = { By = record.Challenged.By; At = record.Challenged.At } }
