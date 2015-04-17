namespace FoosLadder.Api.Models

open System.ComponentModel.DataAnnotations
    
//TODO merge with DomainTypes or rename and merge
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

[<CLIMutable>]
type ExternalLoginViewModel = {
    Name : string
    Url : string
    State : string
}

[<CLIMutable>]
type RegisterExternalBindingModel = {
    [<Required>]
    UserName : string

    [<Required>]
    Provider : string

    [<Required>]
    ExternalAccessToken : string
}

[<CLIMutable>]
type ParsedExternalAccessToken = {
    user_id : string
    app_id : string
}

    

type ApplicationTypes =
    | JavaScript = 0
    | NativeConfidential = 1

