namespace FoosLadder.Api

open Newtonsoft.Json
open Owin
open System.Net.Http.Formatting
open System.Net.Http.Headers
open System.Threading.Tasks
open System.Web.Cors
open System.Web.Http
open System.Web.Http.Cors
open System.Security.Claims

//TODO Rename to ConfigHelpers and move the functions into it
module private Helpers = 

    type ApiCorsPolicyProvider() = 
        let mutable policy = CorsPolicy(AllowAnyMethod = true, AllowAnyHeader = true)
        do
            match Infrastructure.GetApplicationSetting "WebRootUrl" with
            | Some value -> policy.Origins.Add(value)
            | _ -> ()
        interface ICorsPolicyProvider with
            member this.GetCorsPolicyAsync(request, token) = Task.FromResult policy

    type CorsPolicyFactory() = 
        let mutable provider = ApiCorsPolicyProvider() :> ICorsPolicyProvider
        interface ICorsPolicyProviderFactory with
            member this.GetCorsPolicyProvider(request) = provider

module private AuthenticationHelpers = 
    open Microsoft.Owin.Security.OAuth
    open FoosLadder.Api.Repositories


    
    //TODO Rename after completing tut
    type SimpleAuthorizationServerProvider() = 
        inherit OAuthAuthorizationServerProvider()

        override __.ValidateClientAuthentication (context : OAuthValidateClientAuthenticationContext) =            
            Task.Factory.StartNew(fun () -> context.Validated() |> ignore)

        override __.GrantResourceOwnerCredentials (context : OAuthGrantResourceOwnerCredentialsContext) = 
            let asyncWork = async {
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", [| "*" |])
                use repo = new AuthRepository()
                let! user = repo.FindUser context.UserName context.Password 
                if user = null then
                    context.SetError("invalid_grant", "The user name or password is incorrect.")
                else 
                    let identity = new ClaimsIdentity(context.Options.AuthenticationType)
                    identity.AddClaim(new Claim("sub", context.UserName))
                    identity.AddClaim(new Claim("role", "user"));
 
                    context.Validated(identity) |> ignore

            } 
            Task.Factory.StartNew(fun () -> asyncWork |> Async.RunSynchronously)
            
            
    

open Helpers
open AuthenticationHelpers
open Microsoft.Owin.Security.OAuth
open Microsoft.Owin
open System

type Startup() =

    let RegisterCorsPolicy(config : HttpConfiguration) =
        config.SetCorsPolicyProviderFactory(CorsPolicyFactory())
        config.EnableCors()
        config

    let RegisterWebApiAttributeRoutes(config : HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config
    
    let RegisterSerializationFormatters (config : HttpConfiguration) = 
        config.Formatters.XmlFormatter.UseXmlSerializer <- true
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()

        config.Formatters.JsonFormatter.SerializerSettings.MissingMemberHandling <- MissingMemberHandling.Error
        config.Formatters.JsonFormatter.SerializerSettings.Error <- new System.EventHandler<Serialization.ErrorEventArgs>(fun _ errorEvent ->
            let context = System.Web.HttpContext.Current
            let error = errorEvent.ErrorContext.Error
            context.AddError(error)
            errorEvent.ErrorContext.Handled <- true)
        config.Formatters.JsonFormatter.SupportedMediaTypes.Add(MediaTypeHeaderValue("text/html"))
        config

    let RegisterConfiguration (config : HttpConfiguration) =
        config
        |> RegisterCorsPolicy
        |> RegisterSerializationFormatters
        |> RegisterWebApiAttributeRoutes

    let createOAuthOptions() = 
        new OAuthAuthorizationServerOptions(
            TokenEndpointPath = PathString("/Token"),
            //AuthorizeEndpointPath = new PathString("/Account/Authorize"),
            Provider = new SimpleAuthorizationServerProvider(),
            AccessTokenExpireTimeSpan = TimeSpan.FromDays(1.0),
            AllowInsecureHttp = true)

    let ConfigureAuthentication (app: IAppBuilder) = 
        app.UseOAuthAuthorizationServer(createOAuthOptions())
            .UseOAuthBearerAuthentication(OAuthBearerAuthenticationOptions())
    member __.Configuration(app : IAppBuilder) =
        let configuration = RegisterConfiguration (new HttpConfiguration())
        ConfigureAuthentication app |> ignore
        app.UseWebApi configuration |> ignore

[<assembly: Microsoft.Owin.OwinStartup(typeof<Startup>)>]
do()