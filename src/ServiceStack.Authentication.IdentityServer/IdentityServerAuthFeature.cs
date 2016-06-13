// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    using System;
    using Auth;
    using Configuration;
    using Enums;
    using Extensions;
    using Interfaces;
    using Providers;
    using Web;

    public class IdentityServerAuthFeature : IPlugin
    {
        private readonly ServiceClientBase defaultServiceClient;
        private IClientSecretStore clientSecretStore;

        private string referrerUrl;

        public IdentityServerAuthFeature(ServiceClientBase defaultServiceClient = null)
        {
            this.defaultServiceClient = defaultServiceClient;
        }

        public virtual void Register(IAppHost appHost)
        {
            ValidateCallbackRequirements(appHost);

            clientSecretStore = appHost.TryResolve<IClientSecretStore>() ?? new DefaultClientSecretStore(appHost.AppSettings);

            var provider = ProviderFactory(appHost.AppSettings, clientSecretStore);

            appHost.LoadPlugin(
                new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[]
                {
                    provider
                },
                $"{appHost.Config?.WebHostUrl}auth/{IdentityServerAuthProvider.Name}")
            );

            if (defaultServiceClient == null) return;

            appHost.GetContainer().Register<IServiceClient>(defaultServiceClient);
            appHost.GlobalRequestFilters.Add(ImpersonateServiceClient);
        }

        private void ValidateCallbackRequirements(IAppHost appHost)
        {
            if (appHost.AppSettings.GetProviderType() != IdentityServerAuthProviderType.UserAuthProvider) return;

            if (appHost.Config?.WebHostUrl == null)
            {
                throw new ApplicationException(
                    "appHost.Config.WebHostUrl must be set to use the Identity Server User Login plugin so that " +
                    "the service can sent it's full http://url:port to the Identity Server User Login");
            }

            referrerUrl = appHost.Config?.WebHostUrl;            
            appHost.AppSettings.Set($"oauth.{IdentityServerAuthProvider.Name}.CallbackUrl", $"{referrerUrl}auth/{IdentityServerAuthProvider.Name}");
        }

        private static IIdentityServerProvider ProviderFactory(IAppSettings appSettings, IClientSecretStore clientSecretStore)
        {
            IIdentityServerProvider provider;

            switch (appSettings.GetProviderType())
            {
                case IdentityServerAuthProviderType.UserAuthProvider:
                    provider = new UserAuthProvider(appSettings, clientSecretStore);
                    break;
                case IdentityServerAuthProviderType.ServiceProvider:
                    provider = new ServiceAuthProvider(appSettings, clientSecretStore);
                    break;
                default:
                    provider = new ImpersonationAuthProvider(appSettings, clientSecretStore);
                    break;
            }

            provider.Init().Wait();

            return provider;
        }

        private void ImpersonateServiceClient(IRequest request, IResponse response, object requestDto)
        {
            if (defaultServiceClient == null) return;

            var session = request.GetSession();
            if (session == null || !session.IsAuthenticated)
            {
                return;
            }

            var authTokens = session.GetOAuthTokens(IdentityServerAuthProvider.Name);
            if (string.IsNullOrEmpty(authTokens?.AccessToken))
            {
                return;
            }

            defaultServiceClient.Post(new Authenticate
            {
                provider = IdentityServerAuthProvider.Name,

                oauth_token = authTokens.AccessToken,
                oauth_verifier = $"{referrerUrl}auth/{IdentityServerAuthProvider.Name}"
            });
        }
    }
}
