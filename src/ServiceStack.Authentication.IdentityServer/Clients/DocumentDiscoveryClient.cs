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
            IJsonServiceClient client = new JsonServiceClient(appSettings.AuthRealm);

#if NETSTANDARD1_6
            string document;
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

            var configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration(document);
#elif NET45
            Dictionary<string, object> document;            
            try
            {
                document = await client.GetAsync<Dictionary<string, object>>(endpoint)
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
            
            var configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnectConfiguration(document);
#endif

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

        private static string GetStringValue(Dictionary<string, object> document, string name)
        {
            object obj;
            if (document.TryGetValue(name, out obj))
            {
                return obj as string;
            }
            return null;
        }
    }
}
