// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using Interfaces;
    using Logging;

    internal class UserInfoClient : IUserInfoClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserInfoClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public UserInfoClient(IIdentityServerAuthProviderSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public async Task<IEnumerable<Claim>> GetClaims(string accessToken)
        {
#if NETSTANDARD1_6
            var client = new IdentityModel.Client.UserInfoClient(appSettings.UserInfoUrl);
            var response = await client.GetAsync(accessToken).ConfigureAwait(false);
            if (response.IsError)
            {
                Log.Error($"Error calling endpoint {appSettings.UserInfoUrl} - {response.Error}");
            }

            return response.Claims.Select(x => new Claim(x.Type, x.Value));
#elif NET45
            var client = new IdentityModel.Client.UserInfoClient(new Uri(appSettings.UserInfoUrl), accessToken);
            var response = await client.GetAsync().ConfigureAwait(false);
            if (response.IsError)
            {
                Log.Error($"Error calling endpoint {appSettings.UserInfoUrl} - {response.ErrorMessage}");
            }
            return response.Claims.Select(x => new Claim(x.Item1, x.Item2));
#endif
        }
    }
}
