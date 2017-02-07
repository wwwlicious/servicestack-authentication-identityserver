// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Clients
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using IdentityModel.Client;
    using Interfaces;
    using Logging;

    class ActAsUserGrantTokenClient : IActAsUserGrantTokenClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ActAsUserGrantTokenClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public ActAsUserGrantTokenClient(IIdentityServerAuthProviderSettings settings)
        {
            this.appSettings = settings;
        }

        public async Task<string> RequestCode(string accessToken, string accessTokenUrl)
        {
            var client = new TokenClient(appSettings.RequestTokenUrl, appSettings.ClientId, appSettings.ClientSecret);

            var response = await client.RequestCustomGrantAsync("act-as-user", appSettings.Scopes, new Dictionary<string, string>
            {
                { "access_token", accessToken },
                { "client_referer", accessTokenUrl }
            }).ConfigureAwait(false);

            if (response.IsError)
            {
                Log.Error($"An error occurred while validating the access token - {response.Error}");
            }

            return response.AccessToken;
        }
    }
}
