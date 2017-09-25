// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using ServiceStack.Configuration;

namespace ServiceStack.Authentication.IdentityServer.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Auth;
    using Clients;
    using Interfaces;

    public class ResourcePasswordFlowAuthProvider : IdentityServerAuthProvider
    {
        public ResourcePasswordFlowAuthProvider(IIdentityServerAuthProviderSettings appSettings)
            : base(appSettings)
        {
        }

        public override async Task Init()
        {
            await base.Init().ConfigureAwait(false);
            if (TokenCredentialsClient == null)
                TokenCredentialsClient = new ResourcePasswordTokenClient(AuthProviderSettings);                     
        }      

        public ITokenCredentialsClient TokenCredentialsClient { get; set; }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            RefreshSettings();

            // Get username from request if supplied, otherwise use configured one for m2m
            AuthProviderSettings.Username = request.UserName ?? AuthProviderSettings.Username;
            AuthProviderSettings.Password = request.Password ?? AuthProviderSettings.Password;
      
            var tokens = Init(authService, ref session, request);
            
            var reponseToken = TokenCredentialsClient.RequestToken().Result;

            if (!string.IsNullOrWhiteSpace(reponseToken.AccessToken))
            {
                tokens.AccessToken = reponseToken.AccessToken;
                tokens.UserName = tokens.UserName ?? request.UserName;                
                session.UserName = session.UserName ?? request.UserName;   
                session.IsAuthenticated = true;
                authService.SaveSession(session);                
            }

            return OnAuthenticated(authService, session, tokens, new Dictionary<string, string>());
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
  }
}
