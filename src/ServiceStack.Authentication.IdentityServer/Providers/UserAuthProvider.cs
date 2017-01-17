// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
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

            var isInitialRequest = await IsInitialAuthenticateRequest(httpRequest, tokens).ConfigureAwait(false);
            
            // We need to get the user to login as we don't have any credentials for them
            if (isInitialRequest && !IsCallbackRequest(authService, request))
            {
                return AuthenticateClient(authService, session, tokens);
            }

            // We've just returned from Identity Server so we need to get the tokens we've been given
            if (IsCallbackRequest(authService, request))
            {
                // If the tokens are not valid then redirect with an error
                var authTokens = ParseAuthenticateTokens(httpRequest);
                if (authTokens.IsEmpty || !IdTokenValidator.IsValidIdToken(tokens, authTokens.IdToken))
                {
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
        internal async Task<bool> IsInitialAuthenticateRequest(IRequest httpRequest, IAuthTokens authTokens)
        {
            if (string.IsNullOrEmpty(httpRequest.AbsoluteUri))
            {
                return false;
            }

            if (httpRequest.AbsoluteUri.IndexOf(CallbackUrl, 0, StringComparison.InvariantCultureIgnoreCase) < 0)
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

            if (httpRequest.AbsoluteUri.IndexOf(CallbackUrl, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return false;
            }

            return httpRequest.UrlReferrer != null && 
                   httpRequest.UrlReferrer.AbsoluteUri.IndexOf(AuthRealm, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        /// <summary>Authenticates the Client by Redirecting them to the Authorize Url Endpoint of Identity Server</summary>
        /// <param name="authService">Auth Service</param>
        /// <param name="session">Auth Session</param>
        /// <param name="authTokens">Auth Tokens</param>
        /// <returns>Http Redirect Result</returns>
        internal IHttpResult AuthenticateClient(IServiceBase authService, IAuthSession session, IAuthTokens authTokens)
        {
            var nonce = Guid.NewGuid().ToString("N");

            string preAuthUrl = $"{AuthorizeUrl}?client_id={AuthProviderSettings.ClientId}&scope={AuthProviderSettings.Scopes}" +
                                $"&redirect_uri={CallbackUrl.UrlEncode()}&response_type=code id_token" +
                                $"&state={Guid.NewGuid().ToString("N")}&nonce={nonce}" +
                                "&response_mode=form_post";

            var idAuthTokens = authTokens as IdentityServerAuthTokens;
            if (idAuthTokens != null)
            {
                idAuthTokens.Nonce = nonce;
            }

            authService.SaveSession(session, SessionExpiry);
            return authService.Redirect(PreAuthUrlFilter(this, preAuthUrl));
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

            if (!string.IsNullOrEmpty(error) )
            {
                Log.Error("Error response from Identity Server");
                return result;
            }
            
            var idTokenFragment = requestFragments.FirstOrDefault(x => x.Item1 == "id_token");
            result.IdToken = idTokenFragment != null ? idTokenFragment.Item2 : request.FormData["id_token"];

            var codeFragment = requestFragments.FirstOrDefault(x => x.Item1 == "code");
            result.Code = codeFragment != null ? codeFragment.Item2 : request.FormData["code"];
            
            return result;
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
                var jwtToken = new JwtSecurityToken(idAuthTokens.IdToken);
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
