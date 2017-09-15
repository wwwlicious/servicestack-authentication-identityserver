// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    using System;
    using System.Collections.Generic;
    using Auth;
    using Configuration;
    using Enums;
    using Exceptions;
    using Extensions;
    using Interfaces;
    using Providers;
    using Web;

    public class IdentityServerAuthFeature : IPlugin, IIdentityServerAuthProviderSettings
    {
        private readonly ServiceClientBase defaultServiceClient;
        private readonly IAppSettings appSettings;
        private Func<IClientSecretStore> clientSecretStore;

        private IdentityServerAuthProviderType providerType;

        private string referrerUrl;

        public IdentityServerAuthFeature(IAppSettings appSettings = null, ServiceClientBase defaultServiceClient = null)
        {
            this.appSettings = appSettings ?? new AppSettings();
            this.defaultServiceClient = defaultServiceClient;

            this.providerType = IdentityServerAuthProviderType.UserAuthProvider;
            this.AuthorizationFlow = IdentityServerOpenIdAuthorizationFlowType.Hybrid;
        }

        public virtual void Register(IAppHost appHost)
        {
            ValidateCallbackRequirements(appHost);

            clientSecretStore = () => appHost.TryResolve<IClientSecretStore>() ?? new DefaultClientSecretStore(appSettings);

            var provider = ProviderFactory();

            appHost.LoadPlugin(
                new AuthFeature(() => new AuthUserSession(),
                    new IAuthProvider[]
                    {
                        provider
                    },
                    GetProviderLoginUrl(appHost))
            );

            if (defaultServiceClient == null) return;

            appHost.GetContainer().Register<IServiceClient>(defaultServiceClient);
            appHost.GlobalRequestFilters.Add(ImpersonateServiceClient);
        }

        private static string GetProviderLoginUrl(IAppHost appHost)
        {
            if (string.IsNullOrWhiteSpace(appHost.Config?.WebHostUrl))
            {                
                throw new ConfigurationException(
                    "appHost.Config.WebHostUrl must be set to use the Identity Server User Login plugin so that " +
                    "the service can sent it's full http://url:port to the Identity Server User Login");
            }

            var returnValue = appHost.Config.WebHostUrl.AppendUrlPaths("auth", IdentityServerAuthProvider.Name);
            return returnValue;
        }

        private void ValidateCallbackRequirements(IAppHost appHost)
        {
            var providersThatShouldValidateCallbackUrl = new List<IdentityServerAuthProviderType>
            {
              IdentityServerAuthProviderType.UserAuthProvider, 
              IdentityServerAuthProviderType.ResourcePasswordFlowProvider
            };

            if (!providersThatShouldValidateCallbackUrl.Contains(providerType)) return;

            if (appHost.Config?.WebHostUrl == null)
            {
                throw new ConfigurationException(
                    "appHost.Config.WebHostUrl must be set to use the Identity Server User Login plugin so that " +
                    "the service can sent it's full http://url:port to the Identity Server User Login");
            }

            referrerUrl = appHost.Config?.WebHostUrl.TidyUrl();

            appHost.AppSettings.Set($"oauth.{IdentityServerAuthProvider.Name}.CallbackUrl", referrerUrl.AppendUrlPaths("auth", IdentityServerAuthProvider.Name));
        }

        private IIdentityServerProvider ProviderFactory()
        {
            IIdentityServerProvider provider;

            switch (providerType)
            {
                case IdentityServerAuthProviderType.UserAuthProvider:
                    provider = new UserAuthProvider(this);
                    break;
                case IdentityServerAuthProviderType.ServiceProvider:
                    provider = new ServiceAuthProvider(this);
                    break;
              case IdentityServerAuthProviderType.ResourcePasswordFlowProvider:
                    provider = new ResourcePasswordFlowAuthProvider(this);
                    break;
                default:
                    provider = new ImpersonationAuthProvider(this);
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

#if NETSTANDARD1_6
            var authTokens = session.GetAuthTokens(IdentityServerAuthProvider.Name);
#elif NET45
            var authTokens = session.GetOAuthTokens(IdentityServerAuthProvider.Name);
#endif            
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

        public DocumentDiscoveryResult DiscoveryResult { get; set; }

        public IdentityServerAuthProviderType AuthProviderType
        {
            set { providerType = value; }
        }

        public IdentityServerOpenIdAuthorizationFlowType AuthorizationFlow { get; set; }

        public string AuthRealm
        {
            get { return appSettings.Get(ConfigKeys.AuthRealm, "http://127.0.0.1:8080/").TidyUrl(); }
            set { appSettings.Set(ConfigKeys.AuthRealm, value); }
        }

        public string AuthorizeUrl
        {
            get
            {
              var hasConfiguredValue = appSettings.Exists(ConfigKeys.AuthorizeUrl);
              var discoveryValue = DiscoveryResult?.AuthorizeUrl;              
              var settingsValue =  appSettings.Get(ConfigKeys.AuthorizeUrl, AuthRealm.AppendUrlPaths("connect", "authorize"));

              return !hasConfiguredValue && discoveryValue != null
                ? discoveryValue
                : settingsValue;
            }
            set { appSettings.Set(ConfigKeys.AuthorizeUrl, value); }
        }

        public string IntrospectUrl
        {
            get
            {
              var hasConfiguredValue = appSettings.Exists(ConfigKeys.IntrospectUrl);
              var discoveryValue = DiscoveryResult?.IntrospectUrl;
              var settingsValue =  appSettings.Get(ConfigKeys.IntrospectUrl, AuthRealm.AppendUrlPaths("connect", "introspect"));

              return !hasConfiguredValue && discoveryValue != null
                ? discoveryValue
                : settingsValue;
            }
            set { appSettings.Set(ConfigKeys.IntrospectUrl, value); }
        }

        public string UserInfoUrl
        {
            get
            {
              var hasConfiguredValue = appSettings.Exists(ConfigKeys.UserInfoUrl);
              var discoveryValue = DiscoveryResult?.UserInfoUrl;
              var settingsValue = appSettings.Get(ConfigKeys.UserInfoUrl, AuthRealm.AppendUrlPaths("connect", "userinfo"));

              return !hasConfiguredValue && discoveryValue != null
                ? discoveryValue
                : settingsValue;
            }
            set { appSettings.Set(ConfigKeys.UserInfoUrl, value); }
        }

        public string RequestTokenUrl
        {
            get
            {
              var hasConfiguredValue = appSettings.Exists(ConfigKeys.UserInfoUrl);
              var discoveryValue = DiscoveryResult?.TokenUrl;
              var settingsValue =  appSettings.Get(ConfigKeys.TokenUrl, AuthRealm.AppendUrlPaths("connect", "token"));

              return !hasConfiguredValue && discoveryValue != null
                ? discoveryValue
                : settingsValue;
            }
            set { appSettings.Set(ConfigKeys.TokenUrl, value); }
        }

        public string CallbackUrl
        {
            get { return appSettings.Get(ConfigKeys.CallbackUrl, referrerUrl.AppendUrlPaths("auth", "IdentityServer")); }
            set { appSettings.Set(ConfigKeys.CallbackUrl, value); }
        }

        public string ClientId
        {
            get { return appSettings.Get(ConfigKeys.ClientId, "ClientId"); }
            set { appSettings.Set(ConfigKeys.ClientId, value); }
        }

        public string ClientSecret
        {
            get { return clientSecretStore().GetSecretAsync(ClientId).Result; }
            set { appSettings.Set(ConfigKeys.ClientSecret, value); }
        }

        public string Username
        {
          get { return appSettings.Get(ConfigKeys.Username, string.Empty); }
          set { appSettings.Set(ConfigKeys.Username, value); }
        }

        public string Password
        {
          get { return appSettings.Get(ConfigKeys.Password, string.Empty); }
          set { appSettings.Set(ConfigKeys.Password, value); }
        }

        public string JwksUrl
        {
            get
            {
              var hasConfiguredValue = appSettings.Exists(ConfigKeys.JwksUrl);
              var discoveryValue = DiscoveryResult?.JwksUrl;
              var settingsValue =  appSettings.Get(ConfigKeys.JwksUrl, AuthRealm.AppendUrlPaths(".well-known"));

              return !hasConfiguredValue && discoveryValue != null
                ? discoveryValue
                : settingsValue;
            }
            set { appSettings.Set(ConfigKeys.JwksUrl, value); }
        }

        public string Scopes
        {
            get { return appSettings.Get(ConfigKeys.ClientScopes, "openid"); }
            set { appSettings.Set(ConfigKeys.ClientScopes, value); }
        }

        public IList<string> RoleClaimNames => appSettings.GetList(ConfigKeys.RoleClaims);

        public string RoleClaims
        {
            set { appSettings.Set(ConfigKeys.RoleClaims, value); }
        }

        public IList<string> PermissionClaimNames => appSettings.GetList(ConfigKeys.PermissionClaimNames);

        public string PermissionClaims
        {
            set { appSettings.Set(ConfigKeys.PermissionClaimNames, value); }
        }
    }
}
