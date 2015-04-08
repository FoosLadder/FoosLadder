namespace FoosLadder.Api.Models

open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open System.ComponentModel.DataAnnotations
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies

//AccountViewModels - Models returned by AccountController actions.

[<CLIMutable>]
type ExternalLoginConfirmationViewModel = {
    [<Required>]
    [<Display(Name = "Email")>]
    Email : string
}

[<CLIMutable>]
type ExternalLoginListViewModel = {
    ReturnUrl : string
}

[<CLIMutable>]
type SendCodeViewModel = {
    SelectedProvider : string
    Providers : System.Web.Mvc.SelectListItem seq
    ReturnUrl : string
    RememberMe : bool
}

[<CLIMutable>]
type VerifyCodeViewModel = {
    [<Required>]
    Provider : string

    [<Required>]
    [<Display(Name = "Code")>]
    Code : string
    ReturnUrl : string

    [<Display(Name = "Remember this browser?")>]
    RememberBrowser : bool

    RememberMe : bool
}

[<CLIMutable>]
type ForgotViewModel = {
    [<Required>]
    [<Display(Name = "Email")>]
    Email : string
}

[<CLIMutable>]
type LoginViewModel = {
    [<Required>]
    [<Display(Name = "Email")>]
    [<EmailAddress>]
    Email : string

    [<Display(Name = "Remember me?")>]
    RememberMe : bool
}

[<CLIMutable>]
type RegisterViewModel = {
    [<Required>]
    [<EmailAddress>]
    [<Display(Name = "Email")>]
    Email : string
}

[<CLIMutable>]
type ResetPasswordViewModel = {
    [<Required>]
    [<EmailAddress>]
    [<Display(Name = "Email")>]
    Email : string
    Code : string
}

[<CLIMutable>]
type ForgotPasswordViewModel = {
    [<Required>]
    [<EmailAddress>]
    [<Display(Name = "Email")>]
    Email : string
}


//IdentityModels

type ApplicationUser() = 
    inherit IdentityUser()

    member this.GenerateUserIdentityAsync (manager : UserManager<ApplicationUser>) = 
        manager.CreateIdentityAsync(this,CookieAuthenticationDefaults.AuthenticationType) 

type ApplicationDbContext() = 
    inherit IdentityDbContext<ApplicationUser>("DefaultConnection", false)

    static member Create() = new ApplicationDbContext()

// ManageViewModles

[<CLIMutable>]
type IndexViewModel={
    HasPassword : bool
    Logins : UserLoginInfo list
    BrowserRemembered : bool
}

[<CLIMutable>]
type ManageLoginsViewModel={
    CurrentLogins : UserLoginInfo list
    OtherLogins : AuthenticationDescription list
}

[<CLIMutable>]
type FactorViewModel= {
    Purpose : string
}

[<CLIMutable>]
type ConfigureTwoFactorViewModel ={
    SelectedProvider : string
    Providers : System.Web.Mvc.SelectListItem list
}

//TestViewModel
[<CLIMutable>]
type TestViewModel = {
    TestString : string
}


////AccountBindingModels  Models used as parameters to AccountController actions.
//
//[<CLIMutable>]
//type AddExternalLoginBindingModel = {
//    [<Required>]
//    [<Display(Name= "External access token")>]
//    ExternalAccessToken : string
//}
//
////type AddExternalLoginBindingModel() = 
////
////    [<Required>]
////    [<Display(Name= "External access token")>]
////    member val ExternalAccessToken = "" with get,set 
//
//
//[<CLIMutable>]
//type RegisterBindingModel = {
//    [<Required>]
//    [<Display(Name= "Email")>]
//    Email : string
//}
//
//[<CLIMutable>]
//type RegisterExternalBindingModel = {
//    [<Required>]
//    [<Display(Name= "Email")>]
//    Email : string
//}
//
//[<CLIMutable>]
//type RemoveLoginBindingModel = {
//    [<Required>]
//    [<Display(Name= "Login provider")>]
//    LoginProvider : string
//
//    [<Required>]
//    [<Display(Name= "Provider key")>]
//    ProviderKey : string
//}
//
////AccountViewModels - Models returned by AccountController actions.
//
//[<CLIMutable>]
//type ExternalLoginViewModel = {
//    Name  : string
//    Url   : string
//    State : string
//}
//
//[<CLIMutable>]
//type UserInfoViewModel = {
//    Email : string
//    HasRegistered : bool
//    LoginProvider : string
//}
//
//[<CLIMutable>]
//type UserLoginInfoViewModel = {
//    LoginProvider : string
//    ProviderKey : string
//}
//
//[<CLIMutable>]
//type ManageInfoViewModel = {
//    LocalLoginProvider : string
//    Email : string
//    Logins : UserLoginInfoViewModel seq
//    ExternalLoginProviders : ExternalLoginViewModel seq
//}


