namespace FoosLadder.Api.Results

open System.Net
open System.Net.Http
open System.Web.Http
open System.Threading.Tasks


type ChallengeResult(logonProvider : string, controller : ApiController) = 
    member val LoginProvider = logonProvider with get, set
    member val Request = controller.Request with get, set

    interface IHttpActionResult with
        member this.ExecuteAsync cancellationToken = 
            this.Request.GetOwinContext().Authentication.Challenge(logonProvider)
            let response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            response.RequestMessage <- this.Request
            Task.FromResult response
