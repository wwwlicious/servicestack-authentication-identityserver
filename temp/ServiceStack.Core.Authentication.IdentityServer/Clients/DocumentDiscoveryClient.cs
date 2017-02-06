namespace ServiceStack.Core.Authentication.IdentityServer.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Interfaces;
    using Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;

    internal class DocumentDiscoveryClient : IDocumentDiscoveryClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DocumentDiscoveryClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public DocumentDiscoveryClient(IIdentityServerAuthProviderSettings settings)
        {
            this.appSettings = settings;
        }

        public async Task<DocumentDiscoveryResult> GetAsync(string endpoint)
        {
            string document;

            IJsonServiceClient client = new JsonServiceClient(appSettings.AuthRealm);

            try
            {
                document = await client.GetAsync<string>(endpoint)
                                       .ConfigureAwait(false);
            }
            catch (AggregateException exception)
            {
                foreach (var ex in exception.InnerExceptions)
                {
                    Log.Error($"Error occurred requesting document data from {endpoint}", ex);
                }
                return null;
            }

            var configuration = new OpenIdConnectConfiguration(document);

            return new DocumentDiscoveryResult
            {
                AuthorizeUrl = configuration.AuthorizationEndpoint,
                IntrospectUrl = GetStringValue(document, "introspection_endpoint"),
                UserInfoUrl = configuration.UserInfoEndpoint,
                TokenUrl = configuration.TokenEndpoint,
                JwksUrl = configuration.JwksUri
            };
        }

        private static string GetStringValue(string document, string name)
        {
            var dictionary = document.FromJson<Dictionary<string, object>>();
            object obj;
            if (dictionary.TryGetValue(name, out obj))
            {
                return obj as string;
            }
            return null;
        }
    }
}
