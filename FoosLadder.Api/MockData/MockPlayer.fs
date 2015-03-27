namespace FoosLadder.Api.MockData

open FoosLadder.Api.Models
open FsCheck

module MockPlayer = 

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
        {FirstName = firstName; LastName = lastName; TotalMatchesPlayed=wins+losses; TotalMatchesWon = wins; TotalMatchesLost = losses}

    let generatePlayer = 
        Gen.map4 createPlayer (generateFirstName()) (generateLastName()) (generateWins()) (generateLosses())

    let generateRandomPlayers size count =
        let data = FsCheck.Gen.sample size count generatePlayer  
        data
    
