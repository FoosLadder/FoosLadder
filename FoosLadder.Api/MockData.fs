namespace FoosLadder.Api.MockData

open FsCheck

module MockPlayers = 

    open FoosLadder.Api.DomainTypes.Players

    let possibleFirstNames = [
        "Alex";
        "Ben";
        "Charles";
        "David";
        "Evan";
        "Frank";
        "George";
        "Harry";
        "Ian";
        "Jasmine";
        "Keiran";
        "Lee";
        "Mary";
        "Natalie";
        "Owen";
        "Perry";
        "Quinn";
        "Rachel";
        "Sammy";
        "Tracy";
        "Ursula";
        "Valarie";
        "William";
        "Xena";
        "Yousef";
        "Zelda"
    ]

    let possibleLastNames = [
        "Alexington";
        "Benington";
        "Charlesington";
        "Davidington";
        "Evanington";
        "Frankington";
        "Georgington";
        "Harrington";
        "Ianington";
        "Jasmington";
        "Keiranington";
        "Lington";
        "Marington";
        "Natalington";
        "Owenington";
        "Perrington";
        "Quinnington";
        "Rachelington";
        "Sammington";
        "Tracington";
        "Ursulington";
        "Valarington";
        "Williamington";
        "Xenington";
        "Yousefington";
        "Zeldington"
    ]

    let generateFirstName() = Gen.elements possibleFirstNames
    let generateLastName() = Gen.elements possibleLastNames

    let createPlayer firstName lastName = 
        {FirstName = firstName; LastName = lastName}

    let generatePlayer = 
        Gen.map2 createPlayer (generateFirstName()) (generateLastName())

    let generateRandomPlayers size count =
        let data = FsCheck.Gen.sample size count generatePlayer  
        data
    
module MockScores = 

    open FoosLadder.Api.DomainTypes.Scores

    let generateWins() = Gen.choose(0,1000000)
    let generateLosses() = Gen.choose(0,1000000)

    let createScore player wins losses = 
        {Player = player; TotalMatchesPlayed=wins+losses; TotalMatchesWon = wins; TotalMatchesLost = losses}

    let generateScore = 
        Gen.map3 createScore MockPlayers.generatePlayer (generateWins()) (generateLosses())

    let generateRandomScores size count =
        let data = FsCheck.Gen.sample size count generateScore  
        data
    
module MockMatch = 

    open FoosLadder.Api.DomainTypes.Matches
    open System

    let generateDate seconds = DateTime.Now.AddSeconds(float -seconds)
    let generateScore wins loses = wins, loses
    
    let generateDates() = Gen.map generateDate (Gen.choose(0,1000))
    let generateScores() = Gen.map2 generateScore (Gen.choose(0,1000)) (Gen.choose(0,1000))

    let createMatch gameScores playerA playerB dateA dateB = 
        Match.Completed { 
            PlayerA = playerA
            PlayerB = playerB
            MatchDate = dateA
            Challenged = { By = playerA; At = dateA } 
            Accepted = { By = playerB; At = dateA }
            GameResults = gameScores
            Submitted = { By = playerA; At = dateB } 
            Verified = { By = playerB; At = dateB } 
            Winner = playerB
            Loser = playerA }
        

    let generateMatch = 
        let gameScores = FsCheck.Gen.sample 0 5 (generateScores()) 
        let createMatchWithScores = (createMatch gameScores)
        Gen.map4 createMatchWithScores MockPlayers.generatePlayer MockPlayers.generatePlayer (generateDates()) (generateDates())

    let generateRandomMatches size count =
        let data = FsCheck.Gen.sample size count generateMatch  
        data
    
