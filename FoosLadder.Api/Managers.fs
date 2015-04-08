namespace FoosLadder.Api.Managers

open FoosLadder.Api.Models
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin
open Microsoft.Owin.Security
open System

type ApplicationUserManager(store : IUserStore<ApplicationUser>)=
    inherit UserManager<ApplicationUser>(store)
    
    static member private createUserValidator manager : IIdentityValidator<ApplicationUser> = 
        let userValidator = new UserValidator<ApplicationUser>(manager)
        userValidator.AllowOnlyAlphanumericUserNames <- false
        userValidator.RequireUniqueEmail <- true
        upcast userValidator

    static member Create (options : IdentityFactoryOptions<ApplicationUserManager>) (context : IOwinContext)=
        let manager =
            new ApplicationUserManager(
                store = new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()), 
                UserLockoutEnabledByDefault = true, 
                DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes 5.0,
                MaxFailedAccessAttemptsBeforeLockout = 5)

        manager.UserValidator <- ApplicationUserManager.createUserValidator manager
        let dataProtectionProvider = options.DataProtectionProvider
        if dataProtectionProvider <> null then
            manager.UserTokenProvider <- new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
        manager

type ApplicationSignInManager (userManager : ApplicationUserManager, authenticationManager : IAuthenticationManager)=
    inherit SignInManager<ApplicationUser, string>(userManager, authenticationManager)

    override __.CreateUserIdentityAsync (user : ApplicationUser) = 
        user.GenerateUserIdentityAsync (base.UserManager :?> ApplicationUserManager)

    static member Create(options : IdentityFactoryOptions<ApplicationSignInManager>) (context : IOwinContext)=
        new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication)