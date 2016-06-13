// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Interfaces;

    public class IdentityServerAuthProviderSettings : IIdentityServerAuthProviderSettings
    {
        private readonly string providerName;
        private readonly IClientSecretStore clientSecretStore;             

        public IdentityServerAuthProviderSettings(string providerName, IAppSettings appSettings, IClientSecretStore clientSecretStore)
        {
            this.providerName = providerName;
            AppSettings = appSettings;
            this.clientSecretStore = clientSecretStore;
        }

        public IdentityServerAuthProviderSettings(IAppSettings appSettings)
            : this(Providers.IdentityServerAuthProvider.Name, appSettings, new DefaultClientSecretStore(appSettings))
        {
            
        }

        public IAppSettings AppSettings { get; set; }

        public DocumentDiscoveryResult DiscoveryResult { get; set; }

        public string AuthRealm => AppSettings.Get($"oauth.{providerName}.AuthRealm", "http://127.0.0.1:8080/");

        public string AuthorizeUrl
        {
            get
            {
                if (DiscoveryResult != null) return DiscoveryResult.AuthorizeUrl;
                
                return AppSettings.Get($"oauth.{providerName}.AuthorizeUrl", $"{AuthRealm}connect/authorize");
            }
        }

        public string IntrospectUrl
        {
            get
            {
                if (DiscoveryResult != null) return DiscoveryResult.IntrospectUrl;
                
                return AppSettings.Get($"oauth.{providerName}.IntrospectUrl", $"{AuthRealm}connect/introspect");
            }
        }

        public string UserInfoUrl
        {
            get
            {
                if (DiscoveryResult != null) return DiscoveryResult.UserInfoUrl;
                
                return AppSettings.Get($"oauth.{providerName}.UserInfoUrl", $"{AuthRealm}connect/userinfo");
            }
        }

        public string RequestTokenUrl
        {
            get
            {
                if (DiscoveryResult != null) return DiscoveryResult.TokenUrl;

                return AppSettings.Get($"oauth.{providerName}.TokenUrl", $"{AuthRealm}connect/token");
            }
        }

        public string CallbackUrl => AppSettings.Get($"oauth.{providerName}.CallbackUrl", $"{AuthRealm}auth/{providerName}");
        
        public string ClientId => AppSettings.Get($"oauth.{providerName}.ClientId", "ClientId");

        public string ClientSecret => clientSecretStore.GetSecretAsync(ClientId).Result;

        public string JwksUrl
        {
            get
            {
                if (DiscoveryResult != null) return DiscoveryResult.JwksUrl;

                return $"{AuthRealm}..well-known/......";
            }
        }

        public string Scopes => AppSettings.Get($"oauth.{providerName}.Scopes", "openid");

        public IList<string> RoleClaimNames
        {
            get
            {
                var roleClaimNames = AppSettings.Get($"oauth.{providerName}.RoleClaimNames", "role");
                return roleClaimNames.Split(',', ' ').ToList();
            }
        }

        public IList<string> PermissionClaimNames
        {
            get
            {
                var permissionClaimNames = AppSettings.Get($"oauth.{providerName}.PermissionClaimNames", "permission");
                return permissionClaimNames.Split(',', ' ').ToList();
            }
        }
    }
}
