namespace FoosLadder.Api

open Newtonsoft.Json
open Owin
open System.Net.Http.Formatting
open System.Net.Http.Headers
open System.Threading.Tasks
open System.Web.Cors
open System.Web.Http
open System.Web.Http.Cors
open Microsoft.Owin.Security.OAuth
open System
open FoosLadder.Api.Models
open Microsoft.Owin
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.Owin.Security
open Microsoft.Owin.Security.OAuth
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.Google
open FoosLadder.Api.Managers

type ApplicationOAuthProvider(publicClientId : string) = 
    inherit OAuthAuthorizationServerProvider()

    let mutable PublicClientId : string = ""

    do 
        if publicClientId = null then 
            raise (ArgumentNullException("publicClientId"))
        else 
            PublicClientId <- publicClientId

    override __.ValidateClientRedirectUri (context : OAuthValidateClientRedirectUriContext) : Task= 
        if context.ClientId = PublicClientId then
            let expectedRootAbsoluteUri = Uri(context.Request.Uri, "/").AbsoluteUri

            if expectedRootAbsoluteUri = context.RedirectUri then
                context.Validated() |> ignore
            else if context.ClientId = "web" then
                let  expectedUri = Uri(context.Request.Uri, "/")
                context.Validated(expectedUri.AbsoluteUri) |> ignore

        Task.Delay(0)

type BrowserDebugJsonFormatter() as this = 
    inherit JsonMediaTypeFormatter()
    
    do 
        this.SupportedMediaTypes.Add(MediaTypeHeaderValue("text/html"))
        this.SerializerSettings.Formatting <- Formatting.Indented
    
    override __.SetDefaultContentHeaders(objectType, headers, mediaType) = 
        base.SetDefaultContentHeaders(objectType, headers, mediaType)
        headers.ContentType <- MediaTypeHeaderValue("application/json")

type ApiCorsPolicyProvider() = 

    let mutable policy = CorsPolicy(AllowAnyMethod = true, AllowAnyHeader = true)

    do 
        policy.Origins.Add("https://localhost:44302")

    interface ICorsPolicyProvider with
        member __.GetCorsPolicyAsync(request, token) = Task.FromResult policy

type CorsPolicyFactory() = 
    let mutable provider = ApiCorsPolicyProvider() :> ICorsPolicyProvider

    interface ICorsPolicyProviderFactory with
        member __.GetCorsPolicyProvider(request) = provider

[<Sealed>]
type Startup() =

    let RegisterCorsPolicy(config : HttpConfiguration) = 
        config.SetCorsPolicyProviderFactory(CorsPolicyFactory())
        config.EnableCors()
        config
   
    let RegisterWebApiAttributeRoutes(config : HttpConfiguration) = 
        config.MapHttpAttributeRoutes()     
        config  

    let RegisterSerializationFormatters(config : HttpConfiguration) = 
        config.Formatters.XmlFormatter.UseXmlSerializer <- true
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
#if DEBUG
        (*For pretty printing a browser (User browse only) request, I.e. attempting to navigate to: http://localhost:48210/api/players
        Calling the api from code will give the normal unformatted json. *)
        config.Formatters.Add(BrowserDebugJsonFormatter())
#else
        config.Formatters.JsonFormatter.SupportedMediaTypes.Add(MediaTypeHeaderValue("text/html"))
#endif
        config

    let ConfigureBearerTokenAuthentication(config : HttpConfiguration) = 
        config.SuppressDefaultHostAuthentication()
        config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType))
        config

    let RegisterConfiguration (config : HttpConfiguration) = 
        config
        |> RegisterCorsPolicy 
        |> RegisterSerializationFormatters
        |> RegisterWebApiAttributeRoutes
        |> ConfigureBearerTokenAuthentication

    let createCookieAuthOptions() = 
        let cookieAuthProvider = 
            new CookieAuthenticationProvider(
                OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    TimeSpan.FromMinutes(20.0), 
                    System.Func<ApplicationUserManager, ApplicationUser, Task<Security.Claims.ClaimsIdentity>>(fun manager user -> user.GenerateUserIdentityAsync(manager))))
        new CookieAuthenticationOptions(
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie, 
            LoginPath = PathString("/Account/Login"),
            Provider = (cookieAuthProvider :> ICookieAuthenticationProvider))

    let createOAuthOptions() = 
        let publicClientId = "web"
        new OAuthAuthorizationServerOptions(
            TokenEndpointPath = PathString("/Token"),
            AuthorizeEndpointPath = new PathString("/Account/Authorize"),
            Provider = new ApplicationOAuthProvider(publicClientId),
            AccessTokenExpireTimeSpan = TimeSpan.FromDays(14.0),
            AllowInsecureHttp = true)


    let ConfigureAuthentication(app : IAppBuilder) = 
        // Configure the db context, user manager and signin manager to use a single instance per request
        app.CreatePerOwinContext(ApplicationDbContext.Create)
            .CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create)
            .CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create)
        // Enable the application to use a cookie to store information for the signed in user
            .UseCookieAuthentication(createCookieAuthOptions())
        // Use a cookie to temporarily store information about a user logging in with a third party login provider
            .UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)

        // Enable the application to use bearer tokens to authenticate users
        app.UseOAuthBearerTokens(createOAuthOptions())

        app.UseGoogleAuthentication(
            new GoogleOAuth2AuthenticationOptions(
                ClientId = "432791085130-dgufd14g956t7qqvakendiaemg05mhrl.apps.googleusercontent.com",
                ClientSecret = "8NyMdMap80wrRMKDGHUvJ2NM"
            ))

    // Additional Web API settings
    member __.Configuration(app : IAppBuilder) = 
        let temp = new HttpConfiguration()
        let configuration = RegisterConfiguration (new HttpConfiguration())
        ConfigureAuthentication app |> ignore
        app.UseWebApi configuration |> ignore

    
