namespace FoosLadder.Api

open Newtonsoft.Json
open Owin
open System.Net.Http.Formatting
open System.Net.Http.Headers
open System.Threading.Tasks
open System.Web.Cors
open System.Web.Http
open System.Web.Http.Cors

type BrowserDebugJsonFormatter() as this = 
    inherit JsonMediaTypeFormatter()
    
    do 
        this.SupportedMediaTypes.Add(MediaTypeHeaderValue("text/html"))
        this.SerializerSettings.Formatting <- Formatting.Indented
    
    override this.SetDefaultContentHeaders(objectType, headers, mediaType) = 
        base.SetDefaultContentHeaders(objectType, headers, mediaType)
        headers.ContentType <- MediaTypeHeaderValue("application/json")

type ApiCorsPolicyProvider() = 
    let mutable policy = CorsPolicy(AllowAnyMethod = true, AllowAnyHeader = true)
    do policy.Origins.Add("http://localhost:50441")
    interface ICorsPolicyProvider with
        member this.GetCorsPolicyAsync(request, token) = Task.FromResult policy

type CorsPolicyFactory() = 
    let mutable provider = ApiCorsPolicyProvider() :> ICorsPolicyProvider
    interface ICorsPolicyProviderFactory with
        member this.GetCorsPolicyProvider(request) = provider

[<Sealed>]
type Startup() = 
    
    static member RegisterWebApi(config : HttpConfiguration) = 
        config.SetCorsPolicyProviderFactory(CorsPolicyFactory())
        config.EnableCors()
        // Configure routing
        config.MapHttpAttributeRoutes()
        // Configure serialization
        config.Formatters.XmlFormatter.UseXmlSerializer <- true
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.DefaultContractResolver
                                                                                   ()
#if DEBUG
        (*For pretty printing a browser (User browse only) request, I.e. attempting to navigate to: http://localhost:48210/api/players
        Calling the api from code will give the normal unformatted json. *)
        config.Formatters.Add(BrowserDebugJsonFormatter())
#else
        config.Formatters.JsonFormatter.SupportedMediaTypes.Add(MediaTypeHeaderValue("text/html"))
#endif
        
    
    // Additional Web API settings
    member __.Configuration(builder : IAppBuilder) = 
        let config = new HttpConfiguration()
        Startup.RegisterWebApi(config)
        builder.UseWebApi(config) |> ignore
