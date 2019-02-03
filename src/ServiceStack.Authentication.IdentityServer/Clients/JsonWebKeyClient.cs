// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Interfaces;
    using Logging;

    internal class JsonWebKeyClient : IJsonWebKeyClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonWebKeyClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public JsonWebKeyClient(IIdentityServerAuthProviderSettings settings)
        {
            appSettings = settings;
        }

        public async Task<IList<Microsoft.IdentityModel.Tokens.SecurityKey>> GetAsync()
        {
            var httpClient = new JsonServiceClient();

            string document;

            try
            {
                document = await httpClient.GetAsync<string>(appSettings.JwksUrl)
                                           .ConfigureAwait(false);
            }
            catch (AggregateException exception)
            {
                foreach (var ex in exception.InnerExceptions)
                {
                    Log.Error($"Error occurred requesting json web key set from {appSettings.JwksUrl}", ex);
                }
                return null;
            }


            var webKeySet = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(document);
            return webKeySet.GetSigningKeys();                   
        }
    }
}
