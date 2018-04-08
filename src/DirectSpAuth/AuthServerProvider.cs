using System;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Server;
using AspNet.Security.OpenIdConnect.Primitives;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNetCore.Authentication;
using DirectSp.AuthServer.Exceptions;
using DirectSp.Core.Entities;

namespace DirectSp.AuthServer
{
    public class AuthServerProvider : OpenIdConnectServerProvider
    {
        private string GetUserId(AuthenticationTicket ticket)
        {
            return AuthUtil.GetUserId(ticket.Principal);
        }

        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        {
            try
            {
                //Check supported flows
                if (!context.Request.IsAuthorizationCodeFlow() &&
                    !context.Request.IsImplicitFlow())
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.UnsupportedResponseType, "Only the authorization code flow is supported by this authorization server");

                // Note: to support custom response modes, the OpenID Connect server middleware doesn't
                // reject unknown modes before the ApplyAuthorizationResponse event is invoked.
                // To ensure invalid modes are rejected early enough, a check is made here.
                if (!context.Request.IsFormPostResponseMode() &&
                    !context.Request.IsFragmentResponseMode() &&
                    !context.Request.IsQueryResponseMode())
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.InvalidRequest, "The specified response_mode is unsupported");

                //get client
                var client = await AuthDB.Application_Props(context.ClientId);

                // check redirect url
                if (client.RedirectUris == null || !client.RedirectUris.Contains(context.RedirectUri))
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.InvalidRequest, "Invalid redirect_uri");

                //myaccount only accept IsImplicitFlow
                if (context.Request.ClientId == App.AppSettings.MyAccountClientId && !context.Request.IsImplicitFlow())
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.InvalidRequest, "Only ImplicitFlow allowed for myaccount");

                context.Validate(context.RedirectUri);
            }
            catch (Exception ex)
            {
                context.Reject(ex.GetType().Name, ex.ToString());
            }
        }

        public override Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            //Check supported grants
            if (!context.Request.IsPasswordGrantType() &&
                !context.Request.IsRefreshTokenGrantType() &&
                //!context.Request.IsClientCredentialsGrantType() &&
                !context.Request.IsAuthorizationCodeGrantType())
            {
                context.Reject(OpenIdConnectConstants.Errors.UnsupportedGrantType, "the grant type is not supported");
                return Task.FromResult(0);
            }

            context.Validate();
            return Task.FromResult(0);
        }

        public override async Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            try
            {
                //Create Token
                if (context.Request.IsPasswordGrantType())
                {
                    var applications = await ScopeManager.Parse(context.Request.ClientId, context.Request.GetScopes());
                    var scopes = ScopeManager.GetScopes(applications);
                    var audiences = ScopeManager.GetAudiences(applications);
                    
                    // Implement context.Request.Username/context.Request.Password validation here.
                    // Note: you can call context Reject() to indicate that authentication failed.
                    // Using password derivation and time-constant comparer is STRONGLY recommended.
                    var spInvokeParams = new SpInvokeParams { UserId = App.SpInvoker.AppUserContext.UserId, UserRemoteIp = context.HttpContext.Connection.RemoteIpAddress.ToString() };
                    spInvokeParams.InvokeOptions.CaptchaCode = context.Request.GetParameter("captchaCode")?.ToString();
                    spInvokeParams.InvokeOptions.CaptchaId = context.Request.GetParameter("captchaId")?.ToString();
                    var user = await AuthDB.User_Login(context.Request.Username, context.Request.Password, context.Request.ClientId, scopes, spInvokeParams);

                    // By default, claims are not serialized in the access/identity tokens.
                    // Use the overload taking a "destinations" parameter to make sure
                    // your claims are correctly inserted in the appropriate tokens.
                    var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                    identity.AddClaim(OpenIdConnectConstants.Claims.Subject, user.UserId.ToString());
                    if (!string.IsNullOrWhiteSpace(user.UserName))
                        identity.AddClaim(OpenIdConnectConstants.Claims.Username, user.UserName, OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);
                    //identity.AddClaim("urn:customclaim", "value",  OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);

                    // Create a new ClaimsIdentity containing the claims associated with the application.
                    // Note: setting identity.Actor is not mandatory but can be useful to access
                    // the whole delegation chain from the resource server (see ResourceController.cs).
                    identity.Actor = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                    identity.Actor.AddClaim(OpenIdConnectConstants.Claims.Subject, context.Request.ClientId);
                    //identity.Actor.AddClaim(OpenIdConnectConstants.Claims.Name, client.Name, OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);

                    //Generate Ticket
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties(), OpenIdConnectServerDefaults.AuthenticationScheme);
                    ticket.SetAudiences(audiences);
                    ticket.SetScopes(scopes);

                    context.Validate(ticket);
                }

                else if (context.Request.IsClientCredentialsGrantType())
                {
                    var applications = await ScopeManager.Parse(context.Request.ClientId, context.Request.GetScopes());
                    var scopes = ScopeManager.GetScopes(applications);
                    var audiences = ScopeManager.GetAudiences(applications);

                    // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
                    // a key derivation function like PBKDF2 to slow down the secret validation process.
                    // You SHOULD also consider using a time-constant comparer to prevent timing attacks.
                    //AuthDB.Client_OnLoging(context.Request.ClientId, client.ClientSecret, scopes);

                    // By default, claims are not serialized in the access/identity tokens.
                    // Use the overload taking a "destinations" parameter to make sure
                    // your claims are correctly inserted in the appropriate tokens.
                    var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
                    identity.AddClaim(OpenIdConnectConstants.Claims.Subject, context.Request.ClientId);

                    //Generate Ticket
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties(), OpenIdConnectServerDefaults.AuthenticationScheme);
                    ticket.SetAudiences(audiences);
                    ticket.SetScopes(scopes);

                    //context.Validate(ticket);
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.UnsupportedGrantType, "the grant type is not supported");
                }
                else if (context.Request.IsAuthorizationCodeGrantType())
                {
                }
                else if (context.Request.IsRefreshTokenGrantType())
                {
                    await AuthDB.User_OnRefreshingToken(GetUserId(context.Ticket), context.Request.ClientId, context.Request.GetScopes());
                }
                else
                {
                    throw new OpenIdConnectException(OpenIdConnectConstants.Errors.UnsupportedGrantType, "the grant type is not supported");
                }
            }
            catch (Exception ex)
            {
                context.Reject(ex.GetType().Name, ex.ToString());
            }
        }

        public override async Task ValidateLogoutRequest(ValidateLogoutRequestContext context)
        {
            try
            {
                // Skip validation if the post_logout_redirect_uri parameter was missing.
                if (string.IsNullOrEmpty(context.PostLogoutRedirectUri))
                {
                    context.Validate();
                    return;
                }

                // When provided, post_logout_redirect_uri must exactly match the address registered by the client application.
                // find return Url
                var client = await AuthDB.Application_GetByLogoutUrl(context.PostLogoutRedirectUri);
                context.Validate();

            }
            catch (Exception ex)
            {
                context.Reject(ex.GetType().Name, ex.ToString());
            }
        }

        public override async Task HandleUserinfoRequest(HandleUserinfoRequestContext context)
        {
            try
            {
                //get user id from ticket
                var userId = GetUserId(context.Ticket);

                // fix auth server error (it must has been set)
                context.Subject = userId;

                //Find user
                var user = await AuthDB.User_Props(userId);

                if (context.Ticket.HasScope(OpenIdConnectConstants.Scopes.Profile))
                {
                    context.GivenName = user.FirstName;
                    context.FamilyName = user.LastName;
                    context.BirthDate = user.Birthdate?.ToString();
                    if (!string.IsNullOrWhiteSpace(user.Gender))
                        context.Claims[OpenIdConnectConstants.Claims.Gender] = user.Gender;
                    if (!string.IsNullOrWhiteSpace(user.UserName))
                        context.Claims[OpenIdConnectConstants.Claims.Username] = user.UserName;

                    //displayname
                    string name = user.UserDisplayName;
                    //if (!string.IsNullOrEmpty(user.FirstName)) name += user.FirstName;
                    //if (!string.IsNullOrEmpty(user.LastName)) name += " " + user.LastName;
                    //if (string.IsNullOrEmpty(name)) name = user.UserName;
                    if (string.IsNullOrEmpty(name) && context.Ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
                        name = user.Email;

                    name = name.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                        context.Claims[OpenIdConnectConstants.Claims.Name] = name;
                }

                if (context.Ticket.HasScope("national_number"))
                {
                    if (!String.IsNullOrWhiteSpace(user.NationalNumber)) context.Claims["national_number"] = user.NationalNumber;
                }

                if (context.Ticket.HasScope(OpenIdConnectConstants.Scopes.Address))
                {
                    context.Address = new Newtonsoft.Json.Linq.JObject();
                    if (!String.IsNullOrWhiteSpace(user.AddressProvinceName)) context.Address[OpenIdConnectConstants.Claims.Region] = user.AddressProvinceName;
                    if (!String.IsNullOrWhiteSpace(user.AddressCityName)) context.Address[OpenIdConnectConstants.Claims.Locality] = user.AddressCityName;
                    if (!String.IsNullOrWhiteSpace(user.AddressStreet)) context.Address[OpenIdConnectConstants.Claims.StreetAddress] = user.AddressStreet;
                    if (!String.IsNullOrWhiteSpace(user.AddressPostalCode)) context.Address[OpenIdConnectConstants.Claims.PostalCode] = user.AddressPostalCode;
                }

                if (context.Ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
                    context.Email = user.Email;

                if (context.Ticket.HasScope(OpenIdConnectConstants.Scopes.Phone))
                    context.PhoneNumber = user.Mobile;
            }
            catch (Exception ex)
            {
                context.Reject(ex.GetType().Name, ex.ToString());
            }
        }
    }

}
