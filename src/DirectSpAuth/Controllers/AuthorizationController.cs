using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using DirectSp.AuthServer.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DirectSp.AuthServer.Controllers
{
    public class AuthorizationController : Controller
    {
        public AuthorizationController()
        {
        }

        [HttpGet("~/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            try
            {
                // Note: when a fatal error occurs during the request processing, an OpenID Connect response
                // is prematurely forged and added to the ASP.NET context by OpenIdConnectServerHandler.
                // You can safely remove this part and let ASOS automatically handle the unrecoverable errors
                // by switching ApplicationCanDisplayErrors to false in Startup.cs.
                var response = HttpContext.GetOpenIdConnectResponse();
                if (response != null)
                    throw new OpenIdConnectException(response.Error, response.ErrorDescription);

                // Extract the authorization request from the ASP.NET environment.
                var request = HttpContext.GetOpenIdConnectRequest();
                if (request == null)
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.ServerError, "An internal error has occurred");

                // Note: ASOS automatically ensures that an application corresponds to the client_id specified
                // in the authorization request by calling OpenIdConnectServerProvider.ValidateAuthorizationRequest.
                // In theory, this null check shouldn't be needed, but a race condition could occur if you
                // manually removed the application details from the database after the initial check made by ASOS.
                var client = await AuthDB.Application_Props(request.ClientId);

                //filter scope to remove unknown scopes
                var parameters = GetRequestQueryParams();

                // redirect to login page
                return RedirectToPage("login", parameters);
            }
            catch (Exception ex)
            {
                return RedirectToError(ex);
            }
        }

        //[Authorize] is checked by code
        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Access()
        {
            try
            {
                //OpenIdConnect validation
                var response = HttpContext.GetOpenIdConnectResponse();
                if (response != null)
                    throw new OpenIdConnectException(response.Error, response.ErrorDescription);

                var request = HttpContext.GetOpenIdConnectRequest();
                if (request == null)
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.ServerError, "An internal error has occurred in processing request");

                //Check Authentication
                if (!User.Identity.IsAuthenticated)
                    throw new OpenIdConnectException("unauthorized", "unauthorized or unauthorized_clientId");

                //process deny mode
                if (Request.Form != null && Request.Form["permission"] == "deny")
                    return Challenge(OpenIdConnectServerDefaults.AuthenticationScheme);

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var applications = await ScopeManager.Parse(request.ClientId, request.GetScopes());
                var scopes = ScopeManager.GetScopes(applications);
                var audiences = ScopeManager.GetAudiences(applications);
                var user = await AuthDB.User_Props(AuthUtil.GetUserId(User));

                //match password again for myaccount
                var password = Request.Form?["password"];
                var matchPasswordType = await AuthDB.User_MatchPassword(user.UserId, password);

                //Validate granting
                await AuthDB.User_OnGranting(user.UserId, request.ClientId, scopes, matchPasswordType);

                // Add claims to identity
                var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                identity.AddClaim(OpenIdConnectConstants.Claims.Subject, user.UserId.ToString(), OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);
                if (request.HasScope(OpenIdConnectConstants.Scopes.Profile) && !string.IsNullOrWhiteSpace(user.UserName))
                    identity.AddClaim(OpenIdConnectConstants.Claims.Username, user.UserName, OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);

                // Create a new ClaimsIdentity containing the claims associated with the application.
                // Note: setting identity.Actor is not mandatory but can be useful to access
                // the whole delegation chain from the resource server (see ResourceController.cs).
                identity.Actor = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                identity.Actor.AddClaim(OpenIdConnectConstants.Claims.Subject, request.ClientId);
                //identity.Actor.AddClaim(OpenIdConnectConstants.Claims.Name, client.Name, OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);

                //Generate Ticket
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties(), OpenIdConnectServerDefaults.AuthenticationScheme);
                ticket.SetAudiences(audiences);
                ticket.SetScopes(scopes.ToArray());

                // Returning a SignInResult will ask ASOS to serialize the specified identity to build appropriate tokens.
                // Note: you should always make sure the identities you return contain OpenIdConnectConstants.Claims.Subject claim.
                // In this sample, the identity always contains the name identifier returned by the external provider.
                var res = SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                return res;
            }
            catch (Exception ex)
            {
                return RedirectToError(ex);
            }
        }

        [HttpGet("~/connect/logout")]
        public IActionResult Logout()
        {
            try
            {
                var parameters = GetRequestQueryParams();
                return RedirectToPage("logout", parameters);
            }
            catch (Exception ex)
            {
                return RedirectToError(ex);
            }
        }

        [Authorize]
        [HttpPost("~/connect/logout")]
        public IActionResult LogoutPost()
        {
            try
            {
                // Returning a SignOutResult will ask the cookies middleware to delete the local cookie created when
                // the user agent is redirected from the external identity provider after a successful authentication flow
                // and will redirect the user agent to the post_logout_redirect_uri specified by the client application.
                // return SignOut("ServerCookie", OpenIdConnectServerDefaults.AuthenticationScheme);
                return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                return RedirectToError(ex);
            }
        }

        private IActionResult RedirectToError(Exception ex)
        {
            return RedirectToError(ex.GetType().Name, ex.ToString());
        }

        private IActionResult RedirectToError(string error, string errorDescription)
        {
            var param = GetRequestQueryParams();
            if (param.ContainsKey("error")) param.Remove("error");
            if (param.ContainsKey("error_description")) param.Remove("error_description");
            param.Add("error", error);
            param.Add("error_description", errorDescription);
            //param.Add("request_uri", org_query);
            var a = UriHelper.BuildAbsolute(scheme: Request.Scheme, host: Request.Host, path: Request.Path, query: Request.QueryString);

            return RedirectToPage("login", param, false);
        }

        private IActionResult RedirectToPage(string pathName, IDictionary<string, string> parameters, bool clearError = true)
        {
            if (clearError)
            {
                if (parameters.ContainsKey("error")) parameters.Remove("error");
                if (parameters.ContainsKey("error_description")) parameters.Remove("error_description");
            }
            var baseUrl = new Uri(new Uri(App.AppSettings.SignInUri), pathName).AbsoluteUri;
            var uriString = QueryHelpers.AddQueryString(baseUrl, parameters);
            return Redirect(uriString);
        }

        private IDictionary<string, string> GetRequestQueryParams()
        {
            return Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        //Not used now
        private IDictionary<string, string> GetRequestQueryParams2()
        {
            var query = Request.Query;
            var queryParams = query.ToDictionary(x => x.Key, x => x.Value.ToString());

            var orgQueryString = Request.Query["org_query"].ToString();
            if (orgQueryString != null)
            {
                orgQueryString = Uri.UnescapeDataString(orgQueryString);
                var orgQueryParams = HttpUtility.ParseQueryString(orgQueryString);
                queryParams.Remove("org_query");
                foreach (string key in orgQueryParams)
                    queryParams.Add(key, orgQueryParams[key]);
            }
            return queryParams;
        }
    }
}
