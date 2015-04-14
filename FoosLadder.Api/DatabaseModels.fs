namespace FoosLadder.Api.DatabaseModels

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

module Matches =
    open System.Collections.Generic
    
    [<CLIMutable>]
    [<Table("Matches")>]
    type DatabaseMatch = {
        [<Key>] MatchId : int
        PlayerA : int
        PlayerB : int
        MatchDate : Nullable<DateTime>
        ChallengedBy : Nullable<int>
        ChallengedAt : Nullable<DateTime>
        AcceptedBy : Nullable<int>
        AcceptedAt : Nullable<DateTime>
        GameResults : ICollection<DatabaseGameResult>
        SubmittedBy : Nullable<int>
        SubmittedAt : Nullable<DateTime>
        VerifiedBy : Nullable<int>
        VerifiedAt : Nullable<DateTime>
        Winner : Nullable<int>
        Loser : Nullable<int>
        MatchState : int }
    and [<CLIMutable>] [<Table("GameResults")>] DatabaseGameResult =
        {    [<Key>] GameResultId : int
             Match : DatabaseMatch
             MatchId : int
             Index : int
             PlayerA : int
             PlayerB : int }