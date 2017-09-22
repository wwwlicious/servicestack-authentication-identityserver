// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using ServiceStack.Authentication.IdentityServer.Enums;

namespace ServiceStack.Authentication.IdentityServer.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Auth;
    using Clients;
    using Extensions;
    using IdentityModel;
    using Interfaces;
    using Web;

    /// <summary>
    /// Service Stack Authentication Provider - if a User isn't Authenticated, it redirects them to an Identity Server instance
    /// for them to login.
    /// </summary>
    public class UserAuthProvider : IdentityServerAuthProvider
    {
        public UserAuthProvider(IIdentityServerAuthProviderSettings appSettings)
            : base(appSettings)
        {
        }

        public override async Task Init()
        {
            await base.Init().ConfigureAwait(false);

            if (AuthCodeClient == null) AuthCodeClient = new AuthCodeClient(AuthProviderSettings);

            if (IdTokenValidator == null)
            {
                IdTokenValidator = new IdentityServerIdTokenValidator(AuthProviderSettings);
                await IdTokenValidator.Init().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Access Token Endpoint Client - Factory property to support Unit Testing
        /// </summary>
        internal IAuthCodeClient AuthCodeClient { get; set; }

        /// <summary>
        /// Json Web Id Token Validator - Factory property to support Unit Testing
        /// </summary>
        internal IIdentityServerIdTokenValidator IdTokenValidator { get; set; }

        /// <summary>Authenticates the User</summary>
        /// <param name="authService">Authenticating Service</param>
        /// <param name="session">Authenticating Session</param>
        /// <param name="request">Authenticate Request</param>
        /// <returns></returns>
        public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            return AuthenticateAsync(authService, session, request).Result;
        }

        /// <summary>Underlying Async Authenticate method - might require to make external Http calls, hence making it Async</summary>
        /// <param name="authService">Authenticating Service</param>
        /// <param name="session">Authentication Session</param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<IHttpResult> AuthenticateAsync(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            var tokens = Init(authService, ref session, request);
            var httpRequest = authService.Request;

            var isInitialRequest = await IsInitialAuthenticateRequest(httpRequest, tokens, authService, request).ConfigureAwait(false);

            // We need to get the user to login as we don't have any credentials for them
            if (isInitialRequest)
            {
                return AuthenticateClient(authService, session, tokens, request.State, request.nonce);
            }

            // We've just returned from Identity Server so we need to get the tokens we've been given
            if (IsCallbackRequest(authService, request))
            {
                // If the tokens are not valid then redirect with an error
                var authTokens = ParseAuthenticateTokens(httpRequest);
                var invalidTokens = AuthProviderSettings.AuthorizationFlow ==
                                    IdentityServerOpenIdAuthorizationFlowType.CodeFlow
                  ? authTokens.Code.IsNullOrEmpty()
                  : authTokens.IsEmpty || !IdTokenValidator.IsValidIdToken(tokens, authTokens.IdToken);

              
                if (invalidTokens)
                {
                    Log.Warn($"Unauthorized request due to invalid tokens in callback request {request.ToJson()}");
                    throw HttpError.Unauthorized(ErrorMessages.NotAuthenticated);
                }

                // Assign the Id token
                var idTokens = tokens as IdentityServerAuthTokens;
                if (idTokens != null)
                {
                    idTokens.IdToken = authTokens.IdToken;
                    idTokens.Code = authTokens.Code;
                }

                var accessTokens = await AuthCodeClient.RequestCode(authTokens.Code, CallbackUrl)
                                                       .ConfigureAwait(false);
                if (accessTokens != null)
                {
                    // Now we have the access token and the refresh token
                    tokens.AccessToken = accessTokens.AccessToken;
                    tokens.RefreshToken = accessTokens.RefreshToken;
                }
            }

            session.IsAuthenticated = await IsValidAccessToken(tokens).ConfigureAwait(false);

            if (!session.IsAuthenticated)
            {
              Log.Warn($"Unauthorized request due to invalid access token in callback request {request.ToJson()}");
              throw HttpError.Unauthorized(ErrorMessages.NotAuthenticated);
            }
            return OnAuthenticated(authService, session, tokens, new Dictionary<string, string>());
        }

        /// <summary>Determines if the User is authorized</summary>
        /// <param name="session">Auth Session</param>
        /// <param name="tokens">Auth Tokens</param>
        /// <param name="request">Authenticate Request</param>
        /// <returns>True if Authorized</returns>
        public override bool IsAuthorized(IAuthSession session, IAuthTokens tokens, Authenticate request = null)
        {
            if (request != null)
            {
                if (!LoginMatchesSession(session, request.UserName))
                {
                    return false;
                }
            }

            return base.IsAuthorized(session, tokens, request);
        }

        /// <summary>
        /// Gets the Referral URL from the Request
        /// </summary>
        /// <param name="authService">Auth Service</param>
        /// <param name="session">Auth Session</param>
        /// <param name="request">Authenticate Request</param>
        /// <returns></returns>
        protected override string GetReferrerUrl(IServiceBase authService, IAuthSession session, Authenticate request = null)
        {
            string referralUrl = base.GetReferrerUrl(authService, session, request);

            // Special case as the redirect url cannot be sent to identity server as it uses the redirect
            // url to authenticate where the request came from.
            if (!string.IsNullOrEmpty(authService.Request.QueryString["redirect"]))
            {
                referralUrl = authService.Request.QueryString["redirect"];
            }

            return referralUrl;
        }

        /// <summary>
        /// Determines whether we have received an initial authentication request
        /// </summary>
        /// <param name="httpRequest">Http Request</param>
        /// <param name="authTokens">Auth Tokens</param>
        /// <returns></returns>
        public async Task<bool> IsInitialAuthenticateRequest(IRequest httpRequest, IAuthTokens authTokens, IServiceBase authService, Authenticate request)
        {
            if (string.IsNullOrEmpty(httpRequest.AbsoluteUri))
            {
                return false;
            }

            if (IsCallbackRequest(authService, request))
            {
                return false;
            }

            return !await IsValidAccessToken(authTokens).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines if we have been redirected back from the Identity Server Instance
        /// </summary>
        /// <param name="authService">Authenticating Service</param>
        /// <param name="request">Authenticate Request</param>
        /// <returns></returns>
        internal bool IsCallbackRequest(IServiceBase authService, Authenticate request)
        {
            var httpRequest = authService.Request;
            if (string.IsNullOrEmpty(httpRequest.AbsoluteUri))
            {
                return false;
            }

            if (!httpRequest.QueryString["code"].IsNullOrEmpty())
            {
              return true;
            }

            if (httpRequest.AbsoluteUri.IndexOf(CallbackUrl, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

#if NETSTANDARD1_6

            if (httpRequest.UrlReferrer == null) return false;

            var referrer = new Uri(httpRequest.UrlReferrer.GetLeftAuthority());
            var authRealm = new Uri(AuthRealm);

            return referrer.AbsoluteUri.IndexOf(authRealm.AbsoluteUri, StringComparison.OrdinalIgnoreCase) == 0;

#elif NET45
            return httpRequest.UrlReferrer != null && 
                   httpRequest.UrlReferrer.AbsoluteUri.IndexOf(AuthRealm, StringComparison.InvariantCultureIgnoreCase) == 0;
#endif
        }

        /// <summary>Authenticates the Client by Redirecting them to the Authorize Url Endpoint of Identity Server</summary>
        /// <param name="authService">Auth Service</param>
        /// <param name="session">Auth Session</param>
        /// <param name="authTokens">Auth Tokens</param>
        /// <param name="state">State</param>
        /// <param name="nonce">The nonce</param>
        /// <returns>Http Redirect Result</returns>
        internal IHttpResult AuthenticateClient(IServiceBase authService, IAuthSession session, IAuthTokens authTokens, string state, string nonce)
        {
            const string preAuthUrl = "{0}?client_id={1}&scope={2}&redirect_uri={3}&response_type={4}&state={5}&nonce={6}&response_mode=form_post";
            const string codeFlow = "code";
            const string hybridFlow = "code id_token";

            var responseType = AuthProviderSettings.AuthorizationFlow == IdentityServerOpenIdAuthorizationFlowType.CodeFlow ? codeFlow : hybridFlow;

            if (string.IsNullOrWhiteSpace(state))
            {
                state = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(nonce))
            {
                nonce = Guid.NewGuid().ToString("N");
            }

            var requestUrl = string.Format(
                preAuthUrl, 
                AuthorizeUrl, 
                AuthProviderSettings.ClientId, 
                AuthProviderSettings.Scopes,
                CallbackUrl,
                responseType,
                state,
                nonce
                );

            var idAuthTokens = authTokens as IdentityServerAuthTokens;
            if (idAuthTokens != null)
            {                
                idAuthTokens.Nonce = nonce;
            }

            authService.SaveSession(session, SessionExpiry);
            return authService.Redirect(PreAuthUrlFilter(this, requestUrl));
        }

        /// <summary>
        /// Retrieves the request tokens from either the request url fragment or the body of the request
        /// </summary>
        /// <param name="request">Http Request</param>
        /// <returns>Authenticate Result</returns>
        internal AuthenticateResult ParseAuthenticateTokens(IRequest request)
        {
            var result = new AuthenticateResult();

            var requestFragments = request.GetFragments().ToList();

            // Check for errors first.
            var errorFragment = requestFragments.FirstOrDefault(x => x.Item1 == "error");
            var error = errorFragment != null ? errorFragment.Item2 : request.FormData["error"];

            if (!string.IsNullOrEmpty(error))
            {
                Log.Error("Error response from Identity Server");
                return result;
            }

          result.IdToken = GetRequestValue("id_token", request);
          result.Code = GetRequestValue("code", request);
           
          return result;
        }

        private string GetRequestValue(string key, IRequest request)
        {
          var requestFragments = request.GetFragments().ToList();
          var fragment = requestFragments.FirstOrDefault(x => x.Item1 == key);
          var result= fragment != null ? fragment.Item2 : request.FormData[key];
          return result ?? request.QueryString[key];
        }

        /// <summary>Initialise the Auth Token</summary>
        /// <param name="authService"></param>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected new IAuthTokens Init(IServiceBase authService, ref IAuthSession session, Authenticate request)
        {
            if (CallbackUrl.IsNullOrEmpty())
                CallbackUrl = authService.Request.AbsoluteUri;

            session.ReferrerUrl = GetReferrerUrl(authService, session, request);

            var tokens = session.ProviderOAuthAccess.FirstOrDefault(x => x.Provider == Provider);
            if (tokens == null)
                session.ProviderOAuthAccess.Add(tokens = new IdentityServerAuthTokens { Provider = Provider });

            return tokens;
        }

        protected override async Task<bool> IsValidAccessToken(IAuthTokens authTokens)
        {
            if (string.IsNullOrWhiteSpace(authTokens.AccessToken))
            {
                return false;
            }

            if (authTokens.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return await RefreshTokens(authTokens);
            }

            return true;
        }

        protected override void LoadUserAuthInfo(AuthUserSession userSession, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            var idAuthTokens = tokens as IdentityServerAuthTokens;
            if (!string.IsNullOrWhiteSpace(idAuthTokens?.IdToken))
            {
#if NETSTANDARD1_6

                var jwtToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(idAuthTokens.IdToken);
#elif NET45
                var jwtToken = new System.IdentityModel.Tokens.JwtSecurityToken(idAuthTokens.IdToken);
#endif
                idAuthTokens.Issuer = jwtToken.Issuer;
                idAuthTokens.Subject = jwtToken.Subject;

                foreach (var claim in jwtToken.Claims)
                {
                    switch (claim.Type)
                    {
                        case JwtClaimTypes.Expiration:
                            idAuthTokens.Expiration = claim.Value;
                            break;
                        case JwtClaimTypes.Audience:
                            idAuthTokens.Audience = claim.Value;
                            break;
                        case JwtClaimTypes.IssuedAt:
                            idAuthTokens.IssuedAt = claim.Value;
                            break;
                        case JwtClaimTypes.AuthenticationTime:
                            idAuthTokens.AuthenticationTime = claim.Value;
                            break;
                        case JwtClaimTypes.Nonce:
                            idAuthTokens.Nonce = claim.Value;
                            break;
                    }
                }
            }

            base.LoadUserAuthInfo(userSession, tokens, authInfo);
        }
    }
}
