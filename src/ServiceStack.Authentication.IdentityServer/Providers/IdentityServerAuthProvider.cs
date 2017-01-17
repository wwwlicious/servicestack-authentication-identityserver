// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Providers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Auth;
    using Clients;
    using Enums;
    using IdentityModel;
    using Interfaces;

    public abstract class IdentityServerAuthProvider : OAuthProvider, IIdentityServerProvider
    {
        public const string Name = "IdentityServer";

        public const string DocumentDiscoveryEndpoint = ".well-known/openid-configuration";
        public const string JwksEndpoint = ".well-known/";

        protected IdentityServerAuthProvider(IIdentityServerAuthProviderSettings appSettings)
        {
            this.Provider = Name;
            this.AuthProviderSettings = appSettings;
        }

        public virtual async Task Init()
        {
            if (DocumentDiscoveryClient == null) DocumentDiscoveryClient = new DocumentDiscoveryClient(AuthProviderSettings);

            AuthProviderSettings.DiscoveryResult = await DocumentDiscoveryClient.GetAsync(DocumentDiscoveryEndpoint).ConfigureAwait(false);

            if (IntrospectionClient == null) IntrospectionClient = new IntrospectClient(AuthProviderSettings);

            if (RefreshTokenClient == null) RefreshTokenClient = new RefreshTokenClient(AuthProviderSettings);
            
            if (UserInfoClient == null) UserInfoClient = new UserInfoClient(AuthProviderSettings);
        }

        public IIdentityServerAuthProviderSettings AuthProviderSettings { get; }

        public IDocumentDiscoveryClient DocumentDiscoveryClient { get; set; }

        public IIntrospectClient IntrospectionClient { get; set; }

        public IRefreshTokenClient RefreshTokenClient { get; set; }

        public IUserInfoClient UserInfoClient { get; set; }

        /// <summary>Users the Access token to request the User Info and populates the Tokens</summary>
        /// <param name="userSession">User Session</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="authInfo">Authentication Info</param>
        protected override void LoadUserAuthInfo(AuthUserSession userSession, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            if (string.IsNullOrEmpty(tokens.AccessToken))
            {
                return;
            }

            var claims = UserInfoClient.GetClaims(tokens.AccessToken).Result;
            
            if (claims == null) return;

            var identityTokens = tokens as IdentityServerAuthTokens;

            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case JwtClaimTypes.Subject:
                        tokens.UserId = claim.Value;
                        break;
                    case JwtClaimTypes.PreferredUserName:
                        tokens.UserName = claim.Value;
                        break;
                    case JwtClaimTypes.Email:
                        tokens.Email = claim.Value;
                        break;
                    case JwtClaimTypes.PhoneNumber:
                        tokens.PhoneNumber = claim.Value;
                        break;
                    case JwtClaimTypes.GivenName:
                        tokens.FirstName = claim.Value;
                        break;
                    case JwtClaimTypes.FamilyName:
                        tokens.LastName = claim.Value;
                        break;
                }

                if (AuthProviderSettings.RoleClaimNames != null && AuthProviderSettings.RoleClaimNames.Any(x => x == claim.Type))
                {
                    if (userSession.Roles == null)
                    {
                        userSession.Roles = new List<string>();
                    }
                    userSession.Roles.Add(claim.Value);
                }

                if (AuthProviderSettings.PermissionClaimNames != null && AuthProviderSettings.PermissionClaimNames.Any(x => x == claim.Type))
                {
                    if (userSession.Permissions == null)
                    {
                        userSession.Permissions = new List<string>();
                    }
                    userSession.Permissions.Add(claim.Value);
                }

                identityTokens?.Claims.Add(claim);
            }
            LoadUserOAuthProvider(userSession, tokens);
        }

        public override void LoadUserOAuthProvider(IAuthSession authSession, IAuthTokens tokens)
        {
            var userSession = authSession as AuthUserSession;
            if (userSession == null) return;

            userSession.UserName = tokens.UserName ?? userSession.UserName;
            userSession.Email = tokens.Email ?? userSession.Email;
            userSession.PhoneNumber = tokens.PhoneNumber ?? userSession.PhoneNumber;
            userSession.FirstName = tokens.FirstName ?? userSession.FirstName;
            userSession.LastName = tokens.LastName ?? userSession.LastName;
        }

        /// <summary>Determines if the User is authorized</summary>
        /// <param name="session">Auth Session</param>
        /// <param name="tokens">Auth Tokens</param>
        /// <param name="request">Authenticate Request</param>
        /// <returns>True if Authorized</returns>
        public override bool IsAuthorized(IAuthSession session, IAuthTokens tokens, Authenticate request = null)
        {
            RefreshSettings();

            if (session == null || !session.IsAuthenticated || string.IsNullOrEmpty(tokens?.AccessToken))
            {
                return false;
            }

            session.IsAuthenticated = IsValidAccessToken(tokens).Result;
            if (!session.IsAuthenticated)
            {
                tokens.AccessToken = null;
                tokens.RefreshToken = null;
            }

            return session.IsAuthenticated;
        }

        /// <summary>
        /// Updates the settings for the Provider as these may change between requests
        /// </summary>
        protected virtual void RefreshSettings()
        {
            AuthRealm = AuthProviderSettings.AuthRealm;
            CallbackUrl = AuthProviderSettings.CallbackUrl;
            AuthorizeUrl = AuthProviderSettings.AuthorizeUrl;
            RequestTokenUrl = AuthProviderSettings.RequestTokenUrl;
            AccessTokenUrl = AuthProviderSettings.RequestTokenUrl;
        }

        /// <summary>
        /// Determines if we currently have a valid Authentication token or if we have an expired Authentication token
        /// we will attempt to refresh it
        /// </summary>
        /// <param name="authTokens">Authentication Token</param>
        /// <returns>True if we have a valid access token</returns>
        protected virtual async Task<bool> IsValidAccessToken(IAuthTokens authTokens)
        {
            if (!string.IsNullOrEmpty(authTokens.AccessToken))
            {
                // Check if it is a valid token
                var tokenResult = await IntrospectionClient.IsValidToken(authTokens.AccessToken)
                                                           .ConfigureAwait(false);

                // If it is not a valid token - has it expired?
                if (tokenResult == TokenValidationResult.Success)
                {
                    return true;
                }

                // If it has expired and we have a refresh token, refresh the access token.
                if (tokenResult == TokenValidationResult.Expired && !string.IsNullOrEmpty(authTokens.RefreshToken))
                {
                    return await RefreshTokens(authTokens);
                }
            }

            return false;
        }

        protected async Task<bool> RefreshTokens(IAuthTokens authTokens)
        {
            var refreshResult = await RefreshTokenClient.RefreshToken(authTokens.RefreshToken)
                                            .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(refreshResult.AccessToken) && !string.IsNullOrEmpty(refreshResult.RefreshToken))
            {
                authTokens.AccessToken = refreshResult.AccessToken;
                authTokens.RefreshToken = refreshResult.RefreshToken;
                authTokens.RefreshTokenExpiry = refreshResult.ExpiresAt;
                return true;
            }

            authTokens.AccessToken = null;
            authTokens.RefreshToken = null;
            authTokens.RefreshTokenExpiry = null;
            return false;
        }
    }
}
