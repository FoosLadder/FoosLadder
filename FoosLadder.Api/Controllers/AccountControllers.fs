namespace FoosLadder.Api.Controllers

open FoosLadder.Api.Managers
open FoosLadder.Api.Models
open FoosLadder.Api.MockData
open System.Net
open System.Net.Http
open System.Web
open System.Web.Http
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin
open System.Security.Claims

[<Authorize>]
[<RoutePrefix("api/test")>]
type TestController() as this = 
    inherit ApiController()

    [<DefaultValue>] val mutable userManager : ApplicationUserManager 

    do 
        this.UserManager <- HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()

    member this.UserManager
        with get () = this.userManager
        and set value = this.userManager <- value
    
    [<Route("")>]
    member this.Get () = "IFWEOJIDFWEOFWEOIF"

[<Authorize>]
[<RoutePrefix("api/accounts")>]
type AccountController() as this = 
    inherit ApiController()

    [<DefaultValue>] val mutable userManager : ApplicationUserManager 
    [<DefaultValue>] val mutable signInManager : ApplicationSignInManager 

    do 
        this.UserManager <- HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
        this.signInManager <- HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>()

    member this.UserManager
        with get () = this.userManager
        and set value = this.userManager <- value

    member this.SignInManager
        with get () = this.signInManager
        and set value = this.signInManager <- value

//    member this.Authorize () = 
//        let claims = ClaimsPrincipal(this.User).Claims |> Seq.toArray
//        let identity = ClaimsIdentity(claims, "Bearer")


[<Authorize>]
[<RoutePrefix("api/manage")>]
type ManageController() as this = 
    inherit ApiController()

    [<DefaultValue>] val mutable userManager : ApplicationUserManager 
    [<DefaultValue>] val mutable signInManager : ApplicationSignInManager 

    do 
        this.UserManager <- HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
        this.signInManager <- HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>()

    member this.UserManager
        with get () = this.userManager
        and set value = this.userManager <- value

    member this.SignInManager
        with get () = this.signInManager
        and set value = this.signInManager <- value
