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
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver
                                                                                   ()                                                                        
        config.Formatters.JsonFormatter.SerializerSettings.MissingMemberHandling <- MissingMemberHandling.Error                                         
        config.Formatters.JsonFormatter.SerializerSettings.Error <- new System.EventHandler<Serialization.ErrorEventArgs>(fun _ errorEvent ->
            let context = System.Web.HttpContext.Current
            let error = errorEvent.ErrorContext.Error
            context.AddError(error)
            errorEvent.ErrorContext.Handled <- true)
#if DEBUG
        (*For pretty printing a browser (User browse only) request, I.e. attempting to navigate to: http://localhost:48210/api/players
        Calling the api from code will give the normal unformatted json. *)
        config.Formatters.Add(BrowserDebugJsonFormatter())
#else
        config.Formatters.JsonFormatter.SupportedMediaTypes.Add(MediaTypeHeaderValue("text/html"))
#endif
        config

    let RegisterConfiguration (config : HttpConfiguration) = 
        config
        |> RegisterCorsPolicy 
        |> RegisterSerializationFormatters
        |> RegisterWebApiAttributeRoutes
        
    
    // Additional Web API settings
    member __.Configuration(app : IAppBuilder) = 
        let configuration = RegisterConfiguration (new HttpConfiguration())
        app.UseWebApi configuration |> ignore
