namespace FoosLadder.Api.DatabaseModels

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

module Matches =
    open System.Collections.Generic

    [<Table("GameResults")>]
    type DatabaseGameResult() =
        [<DefaultValue>]
        val mutable dbMatch : DatabaseMatch
        [<Key>] member val GameResultId = 0 with get, set
        member val MatchId = 0 with get, set
        member val Index = 0 with get, set
        member val PlayerA = 0 with get, set
        member val PlayerB = 0 with get, set
        member this.Match with get() = this.dbMatch and set v = this.dbMatch <- v

    and [<Table("Matches")>] DatabaseMatch() =
        [<DefaultValue>]
        val mutable gameResults : ICollection<DatabaseGameResult>

        [<Key>] member val MatchId = 0 with get, set
        member val PlayerA = 0 with get, set
        member val PlayerB = 0 with get, set
        member val MatchDate = Nullable<DateTime>() with get, set
        member val ChallengedBy = Nullable<int>() with get, set
        member val ChallengedAt = Nullable<DateTime>() with get, set
        member val AcceptedBy = Nullable<int>() with get, set
        member val AcceptedAt = Nullable<DateTime>() with get, set
        member val SubmittedBy = Nullable<int>() with get, set
        member val SubmittedAt = Nullable<DateTime>() with get, set
        member val VerifiedBy = Nullable<int>() with get, set
        member val VerifiedAt = Nullable<DateTime>() with get, set
        member val Winner = Nullable<int>() with get, set
        member val Loser = Nullable<int>() with get, set
        member val MatchState = 0 with get, set

        member this.GameResults with get() = this.gameResults and set v = this.gameResults <- v