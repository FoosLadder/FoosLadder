namespace FoosLadder.Api.Repositories

open System.Collections.Generic
open FoosLadder.Api.CommonLibrary
open FoosLadder.Api.MockData

module internal Helper =

    let Store (records: Dictionary<int, 'b>) record id =
        match records.ContainsKey(id) with
        | true -> Failure <| ErrorMessage.DbIdNotValid id
        | false -> 
            records.[id] <- record
            Success record

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

    do
        let players = MockPlayers.generateRandomPlayers 0 10 |> List.toSeq |> Seq.zip <| Seq.initInfinite (fun index -> index)
        for (record, id) in players do
            let newRecord = { record with Id = id }
            records.Add(id, newRecord)
    
    let Delete id = Helper.Delete records id
    let LoadAll () = Helper.LoadAll records
    let Load id = Helper.Load records id
    let Store record = Helper.Store records record record.Id
    let Update record = Helper.Update records record record.Id

module MatchDbContext =

    open FoosLadder.Api.DomainTypes.Matches

    let internal records = new Dictionary<int, Match>()

    do
        let matches = MockMatches.generateRandomMatches 0 10 |> List.toSeq |> Seq.zip <| Seq.initInfinite (fun index -> index)
        for (record, id) in matches do
            let recordId = retrieveMatchIdentifier record
            let newRecord =
                match record with
                | Proposed proposed -> Match.Proposed ({ proposed with Id = recordId })
                | Accepted accepted -> Match.Accepted ({ accepted with Id = recordId })
                | Unverified unverified -> Match.Unverified ({ unverified with Id = recordId })
                | Completed completed -> Match.Completed ({ completed with Id = recordId })
            records.Add(id, newRecord)

    let Delete id = Helper.Delete records id
    let LoadAll () = Helper.LoadAll records
    let Load id = Helper.Load records id
    let Store record = Helper.Store records record <| retrieveMatchIdentifier record
    let Update record = Helper.Update records record <| retrieveMatchIdentifier record