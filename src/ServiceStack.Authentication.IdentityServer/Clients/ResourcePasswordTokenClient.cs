// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Clients
{
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Interfaces;
    using Logging;

    public class ResourcePasswordTokenClient : ITokenCredentialsClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourcePasswordTokenClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public ResourcePasswordTokenClient(IIdentityServerAuthProviderSettings settings)
        {
            this.appSettings = settings;
        }
       
        public async Task<TokenResult> RequestToken()
        {
          var client = new TokenClient(appSettings.RequestTokenUrl, appSettings.ClientId, appSettings.ClientSecret);
          client.AuthenticationStyle = AuthenticationStyle.PostValues;
          var result = await client.RequestResourceOwnerPasswordAsync(userName: appSettings.Username, password: appSettings.Password, scope: appSettings.Scopes).ConfigureAwait(false);
 
          if (result.IsError)
          {
            Log.Error($"An error occurred while requesting the access token - {result.Error}");
            return new TokenResult();
          }

          return new TokenResult { AccessToken = result.AccessToken };
        }
    }
}
