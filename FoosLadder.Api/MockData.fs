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
    
    let generatePlayerIds() = Gen.choose(0,10)
    let generateFirstName() = Gen.elements possibleFirstNames
    let generateLastName() = Gen.elements possibleLastNames
    
    let generateWins() = Gen.choose(0,1000000)
    let generateLosses() = Gen.choose(0,1000000)

    let createPlayer id firstName lastName wins losses = 
        {Id = id; FirstName = firstName; LastName = lastName; TotalMatchesPlayed=wins+losses; TotalMatchesWon = wins; TotalMatchesLost = losses}

    let generatePlayer = 
        Gen.map5 createPlayer (generatePlayerIds()) (generateFirstName()) (generateLastName()) (generateWins()) (generateLosses())

    let generateRandomPlayers size count =
        let data = FsCheck.Gen.sample size count generatePlayer  
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
        Gen.map4 createMatchWithScores (MockPlayers.generatePlayerIds()) (MockPlayers.generatePlayerIds()) (generateDates()) (generateDates())

    let generateRandomMatches size count =
        let data = FsCheck.Gen.sample size count generateMatch  
        data
    
