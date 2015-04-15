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

module private ConfigurationHelpers = 

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

    let ConfigureCorsPolicy(config : HttpConfiguration) =
        config.SetCorsPolicyProviderFactory(CorsPolicyFactory())
        config.EnableCors()
        config

    let ConfigureWebApiAttributeRoutes(config : HttpConfiguration) =
        config.MapHttpAttributeRoutes()
        config
    
    let ConfigureSerializationFormatters (config : HttpConfiguration) = 
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
        |> ConfigureCorsPolicy
        |> ConfigureSerializationFormatters
        |> ConfigureWebApiAttributeRoutes

module private AuthenticationHelpers = 
    open Microsoft.Owin.Security.OAuth
    open FoosLadder.Api.Repositories
    open Microsoft.Owin
    open System
    open FoosLadder.Api.Entities
    open System.Security.Cryptography
    open Microsoft.Owin.Security
    open System.Collections.Generic
    open Microsoft.Owin.Security.Infrastructure

    let getHash (input : string) = 
        let hashAlgorithm = new SHA256CryptoServiceProvider()
        let byteValue = System.Text.Encoding.UTF8.GetBytes(input)
        let byteHash = hashAlgorithm.ComputeHash(byteValue)
        Convert.ToBase64String byteHash

    type SimpleRefreshTokenProvider() = 

        interface IAuthenticationTokenProvider with
            member __.Create (context: AuthenticationTokenCreateContext): unit = 
                failwith "Not implemented yet"
            
            member __.Receive (context: AuthenticationTokenReceiveContext): unit = 
                failwith "Not implemented yet"
            
            member __.ReceiveAsync (context: AuthenticationTokenReceiveContext): Task = 
                let work = async {
                    let allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin")
                    context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", [| allowedOrigin |]) |> ignore
                    let hashedTokenId = getHash context.Token

                    use repo = new AuthRepository()
                    let! refreshToken = repo.FindRefreshToken hashedTokenId
                    if box refreshToken <> null then
                        context.DeserializeTicket refreshToken.ProtectedTicket
                        let! result = repo.RemoveRefreshToken hashedTokenId
                        ()
                    else
                        ()
                }
                Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)
            
            member __.CreateAsync (context : AuthenticationTokenCreateContext) = 
                let work = async {
                    let clientId = context.Ticket.Properties.Dictionary.["as:clientId"]
                    if String.IsNullOrEmpty clientId then
                        ()
                    else 
                        let refreshTokenId = Guid.NewGuid().ToString("n")
                        let refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime")
                        //TODO test that the times that this uses / creates are correct
                        let tokenIssuedUtc = DateTime.UtcNow
                        let tokenExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))

                        context.Ticket.Properties.IssuedUtc <- Nullable(DateTimeOffset tokenIssuedUtc)
                        context.Ticket.Properties.ExpiresUtc <- Nullable(DateTimeOffset tokenExpiresUtc)

                        let token = {
                            Id = getHash(refreshTokenId)
                            ClientId = clientId
                            Subject = context.Ticket.Identity.Name
                            IssuedUtc = tokenIssuedUtc
                            ExpiresUtc = tokenExpiresUtc
                            ProtectedTicket = context.SerializeTicket()
                        }
                        use repo = new AuthRepository()
                        let! result = repo.AddRefreshToken token

                        if (result) then
                            context.SetToken refreshTokenId
                            ()
                        else 
                            ()
                }
                Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

    //TODO Rename after completing tut
    type SimpleAuthorizationServerProvider() = 
        inherit OAuthAuthorizationServerProvider()

        //TODO should this be async wrapped as task i.e. let work = async {...}; Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)?
        override __.ValidateClientAuthentication (context : OAuthValidateClientAuthenticationContext) =
            let mutable clientId = ref String.Empty
            let mutable clientSecretRef = ref String.Empty
            let mutable client = Unchecked.defaultof<Client>
            
            (if not (context.TryGetBasicCredentials(clientId, clientSecretRef)) then context.TryGetFormCredentials(clientId, clientSecretRef) else true) |> ignore
            let clientSecret = clientSecretRef.Value

            if context.ClientId = null then
                context.SetError("invalid_clientId", "Client id should be sent.")
                Task.Factory.StartNew(fun () -> ())
            else 
                use repo = new AuthRepository()
                client <- repo.FindClient(context.ClientId)

                if box client = null then
                    context.SetError("invalid_clientId", sprintf "Client '%s' is not registered in the system." context.ClientId)
                    Task.Factory.StartNew(fun () -> ())
                else 
                    let clientIsNativeConfidential = client.ApplicationType = Models.ApplicationTypes.NativeConfidential
                    
                    if clientIsNativeConfidential && String.IsNullOrWhiteSpace clientSecret then
                        context.SetError("invalid_clientId", "Client secret should be sent.")
                        Task.Factory.StartNew(fun () -> ())
                    else if clientIsNativeConfidential && client.Secret <> getHash clientSecret then
                        context.SetError("invalid_clientId", "Client secret is invalid.")
                        Task.Factory.StartNew(fun () -> ())
                    else 
                        if not client.Active then
                            context.SetError("invalid_clientId", "Client is inactive.")
                            Task.Factory.StartNew(fun () -> ())
                        else 
                            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin) |> ignore
                            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString()) |> ignore
                            
                            context.Validated() |> ignore
                            Task.Factory.StartNew(fun () -> ())

        override __.GrantResourceOwnerCredentials (context : OAuthGrantResourceOwnerCredentialsContext) = 
            let work = async {
                let mutable allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin")
                allowedOrigin <- if allowedOrigin = null then "*" else allowedOrigin

                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", [| allowedOrigin |]) |> ignore

                use repo = new AuthRepository()
                let! user = repo.FindUser context.UserName context.Password
                if user = null then
                    context.SetError("invalid_grant", "The user name or password is incorrect")
                else 
                    let identity = ClaimsIdentity(context.Options.AuthenticationType)
                    identity.AddClaim(Claim(ClaimTypes.Name, context.UserName))
                    identity.AddClaim(Claim("sub", context.UserName))
                    identity.AddClaim(Claim("role", "user"));

                    //TODO See where F# readonly dictionary will work without issues. 
                    let propDict = Dictionary<string, string>()
                    let authClientId = if context.ClientId = null then String.Empty else context.ClientId

                    propDict.Add("as:client_id", authClientId)
                    propDict.Add("userName", context.UserName)

                    let props = AuthenticationProperties(propDict)

                    let ticket = AuthenticationTicket(identity, props)
                    context.Validated(ticket) |> ignore
            } 
            Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

        //TODO should this be async wrapped as task i.e. let work = async {...}; Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)?
        override __.GrantRefreshToken (context : OAuthGrantRefreshTokenContext) =
                let originalClient = context.Ticket.Properties.Dictionary.["as:client_id"]
                let currentClient = context.ClientId

                if originalClient <> currentClient then
                    context.SetError("invalid_clientId", "Refresh Token is issued to a different clientId")
                    Task.Factory.StartNew(fun () -> ())
                else
                    let newIdentity = ClaimsIdentity(context.Ticket.Identity)
                    newIdentity.AddClaim(Claim("newClaim", "newValue"))

                    let newTicket = AuthenticationTicket(newIdentity, context.Ticket.Properties)
                    context.Validated(newTicket) |> ignore
                    Task.Factory.StartNew(fun () -> ())


        //TODO should this be async wrapped as task i.e. let work = async {...}; Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)?
        override __.TokenEndpoint (context : OAuthTokenEndpointContext) = 
            for property in context.Properties.Dictionary do
                context.AdditionalResponseParameters.Add(property.Key, property.Value)
            Task.Factory.StartNew(fun () -> ())
            
    let createOAuthOptions() = 
        new OAuthAuthorizationServerOptions(
            AllowInsecureHttp = true,
            TokenEndpointPath = PathString("/api/token"),             //AuthorizeEndpointPath = new PathString("/Account/Authorize"),
            AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30.0),
            Provider = new SimpleAuthorizationServerProvider(),
            RefreshTokenProvider = new SimpleRefreshTokenProvider()
            )

    let ConfigureAuthentication (app: IAppBuilder) = 
        app.UseOAuthAuthorizationServer(createOAuthOptions())
            .UseOAuthBearerAuthentication(OAuthBearerAuthenticationOptions())
    

open ConfigurationHelpers
open AuthenticationHelpers

type Startup() =

    member __.Configuration(app : IAppBuilder) =
        let configuration = RegisterConfiguration (new HttpConfiguration())
        ConfigureAuthentication app |> ignore
        app.UseWebApi configuration |> ignore

[<assembly: Microsoft.Owin.OwinStartup(typeof<Startup>)>]
do()