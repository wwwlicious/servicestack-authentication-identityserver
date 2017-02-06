namespace ServiceStack.Core.Authentication.IdentityServer.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Interfaces;
    using Logging;
    using Microsoft.IdentityModel.Tokens;

    internal class JsonWebKeyClient : IJsonWebKeyClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonWebKeyClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public JsonWebKeyClient(IIdentityServerAuthProviderSettings settings)
        {
            appSettings = settings;
        }

        public async Task<IList<SecurityKey>> GetAsync()
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

            var webKeySet = new JsonWebKeySet(document);

            return webKeySet.GetSigningKeys();
        }
    }
}
