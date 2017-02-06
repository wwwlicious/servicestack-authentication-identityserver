// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Auth;
    using Clients;
    using Interfaces;

    public class ServiceAuthProvider : IdentityServerAuthProvider
    {
        public ServiceAuthProvider(IIdentityServerAuthProviderSettings appSettings)
            : base(appSettings)
        {
        }

        public override async Task Init()
        {
            await base.Init().ConfigureAwait(false);
            if (TokenCredentialsClient == null)
                TokenCredentialsClient = new TokenCredentialsClient(AuthProviderSettings);
        }

        public ITokenCredentialsClient TokenCredentialsClient { get; set; }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            var tokens = Init(authService, ref session, request);

            var reponseToken = TokenCredentialsClient.RequestToken().Result;

            if (!string.IsNullOrWhiteSpace(reponseToken.AccessToken))
            {
                tokens.AccessToken = reponseToken.AccessToken;

                session.IsAuthenticated = this.IsValidAccessToken(tokens).Result;
            }

            return OnAuthenticated(authService, session, tokens, new Dictionary<string, string>());
        }

        /// <summary>Users the Access token to request the User Info and populates the Tokens</summary>
        /// <param name="userSession">User Session</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="authInfo">Authentication Info</param>
        protected override void LoadUserAuthInfo(AuthUserSession userSession, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
        }

        public override void LoadUserOAuthProvider(IAuthSession authSession, IAuthTokens tokens)
        {
        }
    }
}
