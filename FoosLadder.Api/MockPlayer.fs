namespace FoosLadder.MockData

open FoosLadder.Api.Models
open FsCheck

// declare a module 
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
        "Jason";
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
        "Zara"
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
        "Jasonington";
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
        "Zarington"
    ]

    //type Stats = {TotalMatchesWon:int;TotalMatchesLost:int;TotalMatchesPlayed:int}

    let generateFirstName() = Gen.elements possibleFirstNames
    let generateLastName() = Gen.elements possibleLastNames
    let generateWins() = Gen.choose(0,1000000)
    let generateLosses() = Gen.choose(0,1000000)

    let createPlayer firstName lastName wins losses = 
        {FirstName = firstName; LastName = lastName; TotalMatchesPlayed=wins+losses; TotalMatchesWon = wins; TotalMatchesLost = losses}

    let generatePlayer = 
        Gen.map4 createPlayer (generateFirstName()) (generateLastName()) (generateWins()) (generateLosses())

    let getRandomPlayers () =
        let size = 0
        let count = 10
        let data = FsCheck.Gen.sample size count generatePlayer  
        data
    
