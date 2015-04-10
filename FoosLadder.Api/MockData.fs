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

    let generateWins() = Gen.choose(0,1000000)
    let generateLosses() = Gen.choose(0,1000000)

    let createPlayer firstName lastName wins losses =
        {Id = 0; FirstName = firstName; LastName = lastName; TotalMatchesPlayed=wins+losses; TotalMatchesWon = wins; TotalMatchesLost = losses}

    let generatePlayer =
        Gen.map4 createPlayer (generateFirstName()) (generateLastName()) (generateWins()) (generateLosses())

    let generateRandomPlayers size count =
        let data = FsCheck.Gen.sample size count generatePlayer
        data

module MockMatches = 

    open FoosLadder.Api.DomainTypes.Matches
    open System

    let generateDate seconds = DateTime.Now.AddSeconds(float -seconds)
    let generateScore index wins loses : GameResult = { Index = index; PlayerA = wins; PlayerB = loses }

    let generateDates() = Gen.map generateDate (Gen.choose(0,1000))
    let generateScores() = Gen.map3 generateScore (Gen.choose(0,5)) (Gen.choose(0,5)) (Gen.choose(0,5))

    let generatePlayerIds() = Gen.choose(0,10)

    let createMatch gameScores playerA playerB dateA dateB =
        Match.Completed {
            Id = 0
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
        Gen.map4 createMatchWithScores (generatePlayerIds()) (generatePlayerIds()) (generateDates()) (generateDates())

    let generateRandomMatches size count =
        let data = FsCheck.Gen.sample size count generateMatch
        data
