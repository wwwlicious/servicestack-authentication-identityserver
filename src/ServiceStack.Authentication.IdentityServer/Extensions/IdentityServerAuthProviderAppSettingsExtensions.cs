// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Extensions
{
    using Configuration;
    using Enums;
    using Providers;

    public static class IdentityServerAuthProviderAppSettingsExtensions
    {
        private static IAppSettings SetProviderType(this IAppSettings appSettings, IdentityServerAuthProviderType providerType)
        {
            appSettings.Set("oauth.provider", providerType);
            return appSettings;
        }

        public static IAppSettings SetUserAuthProvider(this IAppSettings appSettings) => appSettings.SetProviderType(IdentityServerAuthProviderType.UserAuthProvider);

        public static IAppSettings SetImpersonationAuthProvider(this IAppSettings appSettings) => appSettings.SetProviderType(IdentityServerAuthProviderType.ImpersonationProvider);

        public static IAppSettings SetServiceAuthProvider(this IAppSettings appSettings) => appSettings.SetProviderType(IdentityServerAuthProviderType.ServiceProvider);

        public static IdentityServerAuthProviderType GetProviderType(this IAppSettings appSettings) => appSettings.Get<IdentityServerAuthProviderType>("oauth.provider");
        
        public static IAppSettings SetOauthSetting(this IAppSettings appSettings, string name, string value)
        {
            appSettings.Set($"oauth.{IdentityServerAuthProvider.Name}.{name}", value);
            return appSettings;
        }

        public static string GetOauthSetting(this IAppSettings appSettings, string name) => appSettings.Get<string>($"oauth.{IdentityServerAuthProvider.Name}.{name}");        

        public static IAppSettings SetAuthRealm(this IAppSettings appSettings, string realm) => appSettings.SetOauthSetting("AuthRealm", realm);

        public static string GetAuthRealm(this IAppSettings appSettings) => appSettings.GetOauthSetting("AuthRealm");

        public static IAppSettings SetClientId(this IAppSettings appSettings, string clientId) => appSettings.SetOauthSetting("ClientId", clientId);

        public static string GetClientId(this IAppSettings appSettings) => appSettings.GetOauthSetting("ClientId");

        public static IAppSettings SetClientSecret(this IAppSettings appSettings, string clientSecret) => appSettings.SetOauthSetting("ClientSecret", clientSecret);

        public static IAppSettings SetScopes(this IAppSettings appSettings, string scopes) => appSettings.SetOauthSetting("Scopes", scopes);

        public static IAppSettings SetRoleClaims(this IAppSettings appSettings, string roles) => appSettings.SetOauthSetting("RoleClaimNames", roles);

        public static IAppSettings SetPermissionClaims(this IAppSettings appSettings, string permissions) => appSettings.SetOauthSetting("PermissionClaimNames", permissions);
    }
}
