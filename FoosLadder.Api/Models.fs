namespace FoosLadder.Api.Models

open System.ComponentModel.DataAnnotations
    
[<CLIMutable>]
type UserModel = {
    [<Required>]
    [<Display(Name = "User name")>]
    UserName : string

    [<Required>]
    [<Display(Name = "Password")>]
    [<StringLength(100, ErrorMessage="The {0} must be at least {2} characters long.", MinimumLength = 6)>]
    Password : string

    [<Required>]
    [<Display(Name = "Confirm password")>]
    [<Compare("Password", ErrorMessage ="The password and confirmation password do not match")>]
    ConfirmPassword : string
}


