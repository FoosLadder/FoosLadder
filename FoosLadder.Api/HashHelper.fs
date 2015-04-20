module HashHelper

open System.Security.Cryptography
open System

let getHash (input : string) = 
    let hashAlgorithm = new SHA256CryptoServiceProvider()
    let byteValue = System.Text.Encoding.UTF8.GetBytes(input)
    let byteHash = hashAlgorithm.ComputeHash(byteValue)
    Convert.ToBase64String byteHash

