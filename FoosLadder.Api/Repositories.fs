namespace FoosLadder.Api.Repositories

open System
open System.Collections.Generic
open System.Data.Entity

open FoosLadder.Api.CommonLibrary
open FoosLadder.Api.MockData

module Tools =

    let toNullable =
        function
        | Some v -> Nullable(v)
        | None -> Nullable()
        
    let toNullableValue v = toNullable <| Some v

    let fromNullable (n : _ Nullable) =
        if n.HasValue
           then Some n.Value
           else None

    type DefinedBuilder() =
        member __.Bind ((x : Nullable<'a>, property : string), (rest : 'a -> Result<'b, ErrorMessage>)) =
           if x.HasValue
              then rest x.Value
              else property |> PropertyUndefined |> Failure

        member __.Return x = Success x

    let defined = DefinedBuilder()

module DbContexts =

    open FoosLadder.Api.DatabaseModels.Matches

    type MatchContext() =
        inherit DbContext("MatchContext")

        do Database.SetInitializer(new CreateDatabaseIfNotExists<MatchContext>())
        
        [<DefaultValue()>]
        val mutable matches : IDbSet<DatabaseMatch>
        
        member x.Matches
            with get() = x.matches
            and set v = x.matches <- v

        [<DefaultValue()>]
        val mutable gameResults : IDbSet<DatabaseGameResult>
        
        member x.GameResults
            with get() = x.gameResults
            and set v = x.gameResults <- v

        override x.OnModelCreating (modelBuilder) =
            modelBuilder.Entity<DatabaseGameResult>()
                .HasRequired<DatabaseMatch>(fun y -> y.Match)
                .WithMany(fun z -> z.GameResults)
                .HasForeignKey(fun a -> a.MatchId) |> ignore

module MatchRepository =

    open FoosLadder.Api.DomainTypes.Matches
    open DbContexts
    open FoosLadder.Api.DatabaseModels.Matches
    open Tools
    
    let internal gameToDomainType (model : DatabaseGameResult) =
        {   Id = model.GameResultId
            Index = model.Index
            PlayerA = model.PlayerA
            PlayerB = model.PlayerB }
    
    let internal toDomainType (model : DatabaseMatch) =
        let state = model.MatchState;
        match state with
        | 1 ->
            defined {
                let! challengedBy = (model.ChallengedBy, "ChallengedBy")
                let! challengedAt = (model.ChallengedAt, "ChallengedAt")

                return Match.Proposed {
                        Id = model.MatchId
                        PlayerA = model.PlayerA
                        PlayerB = model.PlayerB
                        MatchDate = model.MatchDate |> fromNullable
                        Challenged = { By = challengedBy; At = challengedAt } } }
        | 2 ->
            defined {
                let! challengedBy = (model.ChallengedBy, "ChallengedBy")
                let! challengedAt = (model.ChallengedAt, "ChallengedAt")
                
                let! matchDate = (model.MatchDate, "MatchDate")
                let! acceptedBy = (model.AcceptedBy, "AcceptedBy")
                let! acceptedAt = (model.AcceptedAt, "AcceptedAt")
                
                return Match.Accepted {
                        Id = model.MatchId
                        PlayerA = model.PlayerA
                        PlayerB = model.PlayerB
                        MatchDate = matchDate
                        Challenged = { By = challengedBy; At = challengedAt }
                        Accepted = { By = acceptedBy; At = acceptedAt } } }
        | 3 ->
            defined {
                let! challengedBy = (model.ChallengedBy, "ChallengedBy")
                let! challengedAt = (model.ChallengedAt, "ChallengedAt")
                
                let! matchDate = (model.MatchDate, "MatchDate")
                let! acceptedBy = (model.AcceptedBy, "AcceptedBy")
                let! acceptedAt = (model.AcceptedAt, "AcceptedAt")
                
                let! submittedBy = (model.SubmittedBy, "SubmittedBy")
                let! submittedAt = (model.SubmittedAt, "SubmittedAt")
                
                return Match.Unverified {
                        Id = model.MatchId
                        PlayerA = model.PlayerA
                        PlayerB = model.PlayerB
                        MatchDate = matchDate
                        Challenged = { By = challengedBy; At = challengedAt }
                        Accepted = { By = acceptedBy; At = acceptedAt }
                        GameResults = model.GameResults |> Seq.map gameToDomainType |> Seq.toList
                        Submitted = { By = submittedBy; At = submittedAt } } }
        | 4 ->
            defined {
                let! challengedBy = (model.ChallengedBy, "ChallengedBy")
                let! challengedAt = (model.ChallengedAt, "ChallengedAt")
                
                let! matchDate = (model.MatchDate, "MatchDate")
                let! acceptedBy = (model.AcceptedBy, "AcceptedBy")
                let! acceptedAt = (model.AcceptedAt, "AcceptedAt")
                
                let! submittedBy = (model.SubmittedBy, "SubmittedBy")
                let! submittedAt = (model.SubmittedAt, "SubmittedAt")
                
                let! verifiedBy = (model.VerifiedBy, "VerifiedBy")
                let! verifiedAt = (model.VerifiedAt, "VerifiedAt")
                let! winner = (model.Winner, "Winner")
                let! loser = (model.Loser, "Loser")
                
                return Match.Completed {
                    Id = model.MatchId
                    PlayerA = model.PlayerA
                    PlayerB = model.PlayerB
                    MatchDate = matchDate
                    Challenged = { By = challengedBy; At = challengedAt }
                    Accepted = { By = acceptedBy; At = acceptedAt }
                    GameResults = model.GameResults |> Seq.map gameToDomainType |> Seq.toList
                    Submitted = { By = submittedBy; At = submittedAt }
                    Verified = { By = verifiedBy; At = verifiedAt }
                    Winner = winner
                    Loser = loser } }

        | value -> Failure <| InvalidateMatchType value

    let internal gameToDatabaseModel matchRecord (record : GameResult) =
         DatabaseGameResult(
             GameResultId = record.Id,
             Match = matchRecord,
             MatchId = record.Id,
             Index = record.Index,
             PlayerA = record.PlayerA,
             PlayerB = record.PlayerB)
       
    let internal toDatabaseModel record =
        let rec toProposedMatch = function
            | Proposed proposedMatch -> proposedMatch
            | record -> toProposedMatch <| previousMatchState record

        let proposedMatch = toProposedMatch record
        let initialModel = 
            DatabaseMatch(
                MatchId = proposedMatch.Id,
                PlayerA = proposedMatch.PlayerA,
                PlayerB = proposedMatch.PlayerB,
                MatchDate = (proposedMatch.MatchDate |> toNullable),
                ChallengedBy = (proposedMatch.Challenged.By |> toNullableValue),
                ChallengedAt = (proposedMatch.Challenged.At |> toNullableValue),
                MatchState = 0)

        let rec updateModel (model: DatabaseMatch) record =
            match record with
            | Match.Proposed _ ->
                model.MatchState <- 1
                model
            | Match.Accepted m -> 
                let updatedModel = previousMatchState record |> updateModel model
                model.AcceptedBy <- m.Accepted.By |> toNullableValue
                model.AcceptedAt <- m.Accepted.At |> toNullableValue
                model.MatchState <- 2
                model
            | Match.Unverified m -> 
                let updatedModel = previousMatchState record |> updateModel model
                model.GameResults <- (m.GameResults |> List.map (gameToDatabaseModel model) |> List.toArray) :> ICollection<DatabaseGameResult>
                model.SubmittedBy <- m.Submitted.By |> toNullableValue
                model.SubmittedAt <- m.Submitted.At |> toNullableValue
                model.MatchState <- 3
                model
            | Match.Completed m -> 
                let updatedModel = previousMatchState record |> updateModel model
                model.VerifiedBy <- m.Verified.By |> toNullableValue
                model.VerifiedAt <- m.Verified.At |> toNullableValue
                model.Winner <- m.Winner |> toNullableValue
                model.Loser <- m.Loser |> toNullableValue
                model.MatchState <- 4
                model
        updateModel initialModel record

    let GetAll () =
        let rec foldResults agg = function
            | (Success s)::tail -> foldResults (s::agg) tail
            | (Failure f)::_ -> Failure f
            | [] -> Success agg
        let foldResultsInit = foldResults []

        use context = new MatchContext()
        let query = context.Matches.Include<DatabaseMatch, ICollection<DatabaseGameResult>>(fun b -> b.GameResults)
        query |> Seq.map toDomainType |> Seq.toList |> foldResultsInit

    let AddNew record =
        use context = new MatchContext()
        let response = context.Matches.Add(toDatabaseModel record) |> toDomainType
        context.SaveChanges() |> ignore
        response


module internal Helper =

    let Store (records: Dictionary<int, 'b>) updateId record =
        let id = Seq.initInfinite (fun index -> index) |> Seq.filter(fun id -> not <| records.ContainsKey(id)) |> Seq.head
        let updatedRecord = updateId id record
        records.[id] <- updatedRecord
        Success updatedRecord

    let LoadAll (records: Dictionary<int, 'b>) =
        records.Values |> Seq.toArray |> Success

    let Load (records: Dictionary<int, 'b>) id =
        match records.TryGetValue(id) with
        | (false, _) -> Failure <| ErrorMessage.DbIdNotValid id
        | (true, item) -> Success item

    let Update (records: Dictionary<int, 'b>) record id =
        match records.ContainsKey(id) with
        | false -> Failure <| ErrorMessage.DbIdNotValid id
        | true ->
            records.[id] <- record
            Success record
    let Delete (records: Dictionary<int, 'b>) id =
        match records.Remove(id) with
        | false -> Failure <| ErrorMessage.DbIdNotValid id
        | true -> Success id


module PlayerDbContext =

    open FoosLadder.Api.DomainTypes.Players

    let internal records = new Dictionary<int, Player>()

    let internal updateId id (record : Player) = { record with Id = id }

    do
        let players = MockPlayers.generateRandomPlayers 0 10 |> List.toSeq |> Seq.zip <| Seq.initInfinite (fun index -> index)
        for (record, id) in players do
            records.Add(id, updateId id record)

    let Delete id = Helper.Delete records id
    let LoadAll () = Helper.LoadAll records
    let Load id = Helper.Load records id
    let Store record = Helper.Store records updateId record
    let Update record = Helper.Update records record record.Id
