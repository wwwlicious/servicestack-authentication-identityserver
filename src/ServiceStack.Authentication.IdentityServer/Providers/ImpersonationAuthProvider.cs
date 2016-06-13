// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Auth;
    using Clients;
    using Configuration;
    using Interfaces;
    using Web;

    public class ImpersonationAuthProvider : IdentityServerAuthProvider
    {
        public ImpersonationAuthProvider(IIdentityServerAuthProviderSettings appSettings)
            : base(appSettings)
        {
        }

        public ImpersonationAuthProvider(IAppSettings appSettings)
            : base(appSettings)
        {
        }

        public ImpersonationAuthProvider(IAppSettings appSettings, IClientSecretStore clientSecretStore)
            : base(appSettings, clientSecretStore)
        {
        }

        public override async Task Init()
        {
            await base.Init().ConfigureAwait(false);

            if (ActAsUserGrantTokenClient == null) ActAsUserGrantTokenClient = new ActAsUserGrantTokenClient(AuthProviderSettings);
        }

        public IActAsUserGrantTokenClient ActAsUserGrantTokenClient { get; set; }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            var tokens = Init(authService, ref session, request);
            var httpRequest = authService.Request;

            var accessToken = ActAsUserGrantTokenClient.RequestCode(
                                    GetAccessToken(httpRequest, request), 
                                    GetReferrer(httpRequest, request)).Result;

            tokens.AccessToken = accessToken;

            // We have the required tokens in the request so now check that they are valid.
            session.IsAuthenticated = this.IsValidAccessToken(tokens).Result;

            return OnAuthenticated(authService, session, tokens, new Dictionary<string, string>());
        }

        private string GetAccessToken(IRequest httpRequest, Authenticate request)
        {
            const string authorizationToken = "Authorization";
            const string bearerToken = "Bearer ";

            if (!string.IsNullOrWhiteSpace(request?.oauth_token))
            {
                return request.oauth_token;
            }

            var authorization = httpRequest.Headers[authorizationToken];
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                if (authorization.IndexOf(bearerToken, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return authorization.Substring(bearerToken.Length).Trim();
                }
            }

            throw HttpError.Unauthorized(ErrorMessages.NotAuthenticated);
        }

        private string GetReferrer(IRequest httpRequest, Authenticate request)
        {
            if (!string.IsNullOrWhiteSpace(request?.oauth_verifier))
            {
                return request.oauth_verifier;
            }

            if (httpRequest.UrlReferrer != null)
            {
                return httpRequest.UrlReferrer.AbsoluteUri;
            }

            throw HttpError.Unauthorized(ErrorMessages.NotAuthenticated);
        }
    }
}