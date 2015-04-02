namespace FoosLadder.Api.Repositories

open System.Collections.Generic
open FoosLadder.Api.CommonLibrary
open FoosLadder.Api.MockData

module MatchDbContext =

    open FoosLadder.Api.DomainTypes.Matches

    let internal records = new Dictionary<int, Match>()

    do
      for record in MockMatch.generateRandomMatches 0 10 do
        let id = retrieveMatchIdentifier record
        if records.ContainsKey(id) |> not then records.Add(id, record)
    
    let Store (record: Match) =
        let id = retrieveMatchIdentifier record
        match records.ContainsKey(id) with
        | true -> Failure <| ErrorMessage.DbIdNotValid id
        | false -> 
            records.[id] <- record
            Success record
    
    let LoadAll () = 
        records.Values |> Seq.toArray |> Success
    let Load id = 
        match records.TryGetValue(id) with
        | (false, _) -> Failure <| ErrorMessage.DbIdNotValid id
        | (true, item) -> Success item

    let Update (record: Match) =
        let id = retrieveMatchIdentifier record
        match records.ContainsKey(id) with
        | false -> Failure <| ErrorMessage.DbIdNotValid id
        | true -> 
            records.[id] <- record
            Success record

    let Delete () = ()