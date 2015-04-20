namespace FoosLadder.Api.Controllers

open FoosLadder.Api
open FoosLadder.Api.Repositories
open System.Web.Http
open System.Net.Http
open System
open System.Security.Claims
open Microsoft.AspNet.Identity
open FoosLadder.Api.Results

module private ExternalLoginHelpers = 
    type ClientValidationResult = 
        {   ValidationError : string option
            RedirectUri : string }
    
    type ExternalLoginData = 
        {   LoginProvider : string
            ProviderKey : string
            UserName : string
            ExternalAccessToken : string}

    let createExternalLoginDataFromIdentity (identity : ClaimsIdentity) = 
        if identity = null then
            None
        else 
            let providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier)

            if (providerKeyClaim = null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty(providerKeyClaim.Value)) then
                None
            else if providerKeyClaim.Issuer = ClaimsIdentity.DefaultIssuer then
                None
            else 
                Some {LoginProvider = providerKeyClaim.Issuer; ProviderKey = providerKeyClaim.Value; UserName = identity.FindFirstValue(ClaimTypes.Name); ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken")}
        
    

open ExternalLoginHelpers
open Newtonsoft.Json.Linq
open Microsoft.Owin.Security.OAuth
open Microsoft.Owin.Security
open FoosLadder.Api.Models
open Microsoft.AspNet.Identity.Owin
open Microsoft.AspNet.Identity.EntityFramework

//TODO reorganise modules and types
//TODO Make methods async
[<RoutePrefix("api/account")>]
type AccountController() as this= 
    inherit ApiController()

    [<DefaultValue>] val mutable repo : AuthRepository

    do 
        this.repo <- new AuthRepository()

    member val private Authentication = this.Request.GetOwinContext().Authentication with get

    member private __.GetErrorResult (result : IdentityResult) = 
        if result = null then
            this.InternalServerError() :> IHttpActionResult
        else 
            if not result.Succeeded then
                if result.Errors <> null then
                    for error in result.Errors do
                        this.ModelState.AddModelError("", error) |> ignore
                if this.ModelState.IsValid then
                    this.BadRequest() :> IHttpActionResult
                else 
                    this.BadRequest(this.ModelState) :> IHttpActionResult 
            else
                null

    [<AllowAnonymous>]
    [<Route("register")>]
    member this.Register userModel = 
        if not this.ModelState.IsValid then
            this.BadRequest(this.ModelState : ModelBinding.ModelStateDictionary) :> IHttpActionResult
        else 
            let result = this.repo.RegisterUser userModel |> Async.RunSynchronously
            let errorResult = this.GetErrorResult(result) 
            if errorResult <> null then
                errorResult
            else
                this.Ok() :> IHttpActionResult

    let GetQueryString (request : HttpRequestMessage) key : string option = 
        let queryStrings = request.GetQueryNameValuePairs()
        if queryStrings = null then 
            None
        else
            let queryMatch = queryStrings |> Seq.tryFind(fun keyValuePair -> String.Compare(keyValuePair.Key, key,true) = 0)
            match queryMatch with
            | None -> None
            | None when String.IsNullOrEmpty(queryMatch.Value.Value) -> None
            | Some result -> Some result.Value

    let ValidateClientAndRedirectUri (request : HttpRequestMessage) : ClientValidationResult= 
        let getRedirectUriAttempt = GetQueryString request "redirect_uri"
        match getRedirectUriAttempt with
        | None -> {ValidationError= Some "redirect_uri is required"; RedirectUri = ""}
        | Some uri -> 
            let (success,validRedirectUri) = Uri.TryCreate(uri, UriKind.Absolute)
            if not success then
                {ValidationError= Some "redirect_uri is invalid"; RedirectUri = ""}
            else
                let getClientIdAttempt = GetQueryString request "client_id"
                match getClientIdAttempt with
                | None -> {ValidationError= Some "client_Id is required"; RedirectUri = ""}
                | Some clientId ->
                    let client = this.repo.FindClient clientId
                    if box client = null then
                        {ValidationError= Some (sprintf "Client_id '%s' is not registered in the system." clientId); RedirectUri = ""}
                    else 
                        if not <| String.Equals(client.AllowedOrigin, validRedirectUri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase) then
                            {ValidationError= Some (sprintf "The given URL is not allowed by Client_id '%s' configuration." clientId); RedirectUri = ""}
                        else
                            {ValidationError= None; RedirectUri = validRedirectUri.AbsoluteUri}

    [<OverrideAuthentication>]
    [<HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)>]
    [<AllowAnonymous>]
    [<Route("externalLogin",Name="externalLogin")>]
    member this.GetExternalLogin(provider, ?error) =
        match error with 
        | Some requestError -> this.BadRequest(Uri.EscapeUriString(requestError)) :> IHttpActionResult
        | None -> 
            if not(this.User.Identity.IsAuthenticated) then
                ChallengeResult(provider,this) :> IHttpActionResult
            else
                let redirectUrlValidationResult = ValidateClientAndRedirectUri this.Request
                match redirectUrlValidationResult.ValidationError with
                | Some error -> this.BadRequest(error) :> IHttpActionResult
                | None -> 
                    let externalLoginData = createExternalLoginDataFromIdentity (this.User.Identity :?> ClaimsIdentity)
                    match externalLoginData with 
                    | None -> this.InternalServerError() :> IHttpActionResult
                    | Some externalLogin -> 
                        if externalLogin.LoginProvider <> provider then
                            this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie)
                            ChallengeResult(provider,this) :> IHttpActionResult
                        else 
                            let user = this.repo.FindAsync(UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey)) |> Async.RunSynchronously
                            let hasRegistered = (user <> null).ToString()
                            let redirectUri = 
                                sprintf "%s#external_access_token=%s&provider=%s&haslocalaccount=%s&external_user_name=%s" 
                                    redirectUrlValidationResult.RedirectUri 
                                    externalLogin.ExternalAccessToken 
                                    externalLogin.LoginProvider 
                                    hasRegistered 
                                    externalLogin.UserName

                            this.Redirect(redirectUri) :> IHttpActionResult

    [<AllowAnonymous>]
    [<Route("registerExternal")>]
    member this.RegisterExternal (model : RegisterExternalBindingModel) =
        if not this.ModelState.IsValid then
            this.BadRequest this.ModelState :> IHttpActionResult
        else 
            let verifiedAccessToken = this.VerifyExternalAccessToken model.Provider model.ExternalAccessToken |> Async.RunSynchronously
            match verifiedAccessToken with
            | None -> this.BadRequest "Invalid Provider or External Access Token" :> IHttpActionResult
            | Some verifiedToken -> 
                let findUserAttempt = this.repo.FindAsync(new UserLoginInfo(model.Provider, verifiedToken.user_id)) |> Async.RunSynchronously

                let hasRegistered = findUserAttempt <> null

                if hasRegistered then
                    this.BadRequest "External user is already registered" :> IHttpActionResult
                else
                    
                    let user = IdentityUser(UserName = model.UserName)
                    let result = this.repo.CreateAsync(user) |> Async.RunSynchronously
                    if not result.Succeeded then
                        this.GetErrorResult result
                    else
                        let info =  
                            ExternalLoginInfo(
                                DefaultUserName = model.UserName,
                                Login = new UserLoginInfo(model.Provider, verifiedToken.user_id)
                            )

                        let addLoginResult = this.repo.AddLoginAsync user.Id info.Login  |> Async.RunSynchronously
                        if not addLoginResult.Succeeded then 
                            this.GetErrorResult result 
                        else
                            //generate access token response
                            let accessTokenResponse = this.GenerateLocalAccessTokenResponse model.UserName
                            this.Ok accessTokenResponse :> IHttpActionResult

    [<AllowAnonymous>]
    [<HttpGet>]
    [<Route("obtainLocalAccessToken")>]
    member this.ObtainLocalAccessToken provider externalAccessToken = 
            if String.IsNullOrWhiteSpace(provider) || String.IsNullOrWhiteSpace(externalAccessToken) then
                this.BadRequest "Provider or external access token is not sent" :> IHttpActionResult
            else
                let verifiedAccessToken = this.VerifyExternalAccessToken provider externalAccessToken |> Async.RunSynchronously
                match verifiedAccessToken with
                | None -> this.BadRequest "Invalid Provider or External Access Token" :> IHttpActionResult
                | Some verifiedToken -> 
                    let user = this.repo.FindAsync(new UserLoginInfo(provider, verifiedToken.user_id)) |> Async.RunSynchronously

                    let hasRegistered = user <> null;

                    if not hasRegistered then
                        this.BadRequest "External user is not registered" :> IHttpActionResult
                    else 
                        //generate access token response
                        let accessTokenResponse = this.GenerateLocalAccessTokenResponse user.UserName
                        this.Ok accessTokenResponse :> IHttpActionResult

    member private this.VerifyExternalAccessToken provider accessToken : Async<ParsedExternalAccessToken option> =
        async {
            match provider with
            | "Google" -> 
                let verifyTokenEndPoint = sprintf "https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=%s" accessToken
            
                use client = new HttpClient()
                let uri = Uri(verifyTokenEndPoint)
                let! response = client.GetAsync(uri) |> Async.AwaitTask

                if response.IsSuccessStatusCode then
                    let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    //let jsonObject = JObject (Newtonsoft.Json.JsonConvert.DeserializeObject(content))
                    let jsonObject = JObject.Parse(content)

                    let userId = string( jsonObject.["user_id"])
                    let appId  = string( jsonObject.["audience"])
                    if String.Equals(Startup.GoogleAuthOptions.ClientId, appId, StringComparison.OrdinalIgnoreCase) then
                        return Some {user_id = userId; app_id = appId}
                    else 
                        return None
                else 
                    return None
            | _ -> return None
        }


    member private __.GenerateLocalAccessTokenResponse userName = 
        let tokenExpiration = TimeSpan.FromDays(1.0)
        let identity = ClaimsIdentity(OAuthDefaults.AuthenticationType)
        identity.AddClaim(Claim(ClaimTypes.Name, userName))
        identity.AddClaim(Claim("role","user"))

        let props =           AuthenticationProperties(IssuedUtc= Nullable(DateTimeOffset DateTime.UtcNow), ExpiresUtc = Nullable(DateTimeOffset (DateTime.UtcNow.Add(tokenExpiration))) )
        let ticket = new AuthenticationTicket(identity,props)
        let accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket)
        let tokenResponse = 
            JObject(
                    new JProperty("userName", userName),
                    new JProperty("access_token", accessToken),
                    new JProperty("token_type", "bearer"),
                    new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
                    new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                    new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
            )
        tokenResponse

    override __.Dispose(disposing : bool) = 
        if disposing then
            this.repo.Dispose() |> ignore
        base.Dispose(disposing) |> ignore

module TestOrders = 
    //TODO This is a test. Remove it when complete. 

    [<CLIMutable>]
    type Order = {
        OrderID : int
        CustomerName : string
        ShipperCity : string
        IsShipped : bool
    }

    let createOrders() = [
        {OrderID = 1; CustomerName = "Bob"; ShipperCity="York"; IsShipped=true}
        {OrderID = 2; CustomerName = "Henry"; ShipperCity="Dover"; IsShipped=false}
    ]

    [<RoutePrefix("api/orders")>]
    type OrdersController() = 
        inherit ApiController()

        [<Authorize>]
        [<Route("")>]
        member this.Get() = 
            this.Ok(createOrders())




        
            
