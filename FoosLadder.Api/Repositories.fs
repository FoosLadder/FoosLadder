namespace FoosLadder.Api.Repositories

open System.Collections.Generic
open FoosLadder.Api.CommonLibrary
open FoosLadder.Api.MockData

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

    let internal updateId id record = { record with Id = id }

    do
        let players = MockPlayers.generateRandomPlayers 0 10 |> List.toSeq |> Seq.zip <| Seq.initInfinite (fun index -> index)
        for (record, id) in players do
            records.Add(id, updateId id record)

    let Delete id = Helper.Delete records id
    let LoadAll () = Helper.LoadAll records
    let Load id = Helper.Load records id
    let Store record = Helper.Store records updateId record
    let Update record = Helper.Update records record record.Id

module MatchDbContext =

    open FoosLadder.Api.DomainTypes.Matches

    let internal records = new Dictionary<int, Match>()

    let internal updateId id = function
                | Proposed proposed -> Match.Proposed ({ proposed with Id = id })
                | Accepted accepted -> Match.Accepted ({ accepted with Id = id })
                | Unverified unverified -> Match.Unverified ({ unverified with Id = id })
                | Completed completed -> Match.Completed ({ completed with Id = id })

    do
        let matches = MockMatches.generateRandomMatches 0 10 |> List.toSeq |> Seq.zip <| Seq.initInfinite (fun index -> index)
        for (record, id) in matches do
            records.Add(id, updateId id record)

    let Delete id = Helper.Delete records id
    let LoadAll () = Helper.LoadAll records
    let Load id = Helper.Load records id
    let Store record = Helper.Store records updateId record
    let Update record = Helper.Update records record <| retrieveMatchIdentifier record
