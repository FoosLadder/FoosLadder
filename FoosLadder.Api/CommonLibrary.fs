namespace FoosLadder.Api.CommonLibrary

type Result<'TSuccess,'TFailure> = 
| Success of 'TSuccess
| Failure of 'TFailure

type ErrorMessage = 
    | DbIdNotValid of int
    | DtoValidationError of exn seq
    | PropertyUndefined of string
    | InvalidateMatchType of int

module Rop =
    let bind track input =
        match input with
        | Success s -> track s
        | Failure f -> Failure f

    let succeed x = Success x
