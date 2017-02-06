namespace ServiceStack.Authentication.IdentityServer.Extensions
{
    using System;
    using Configuration;
    using Enums;

    public static class IdentityServerAuthProviderAppSettingsExtensions
    {
        private static IAppSettings SetProviderType(this IAppSettings appSettings, IdentityServerAuthProviderType providerType)
        {
            appSettings.Set(ConfigKeys.OauthProvider, providerType);
            return appSettings;
        }

        [Obsolete("Use public property on IdentityServerAuthFeature.AuthProviderTyp=IdentityServerAuthProviderType.UserAuthProvider")]
        public static IAppSettings SetUserAuthProvider(this IAppSettings appSettings) => appSettings.SetProviderType(IdentityServerAuthProviderType.UserAuthProvider);

        [Obsolete("Use public property on IdentityServerAuthFeature.AuthProviderType=IdentityServerAuthProviderType.ImpersonationProvider")]
        public static IAppSettings SetImpersonationAuthProvider(this IAppSettings appSettings) => appSettings.SetProviderType(IdentityServerAuthProviderType.ImpersonationProvider);

        [Obsolete("Use public property on IdentityServerAuthFeature.AuthProviderType = IdentityServerAuthProviderType.ServiceProvide")]
        public static IAppSettings SetServiceAuthProvider(this IAppSettings appSettings) => appSettings.SetProviderType(IdentityServerAuthProviderType.ServiceProvider);

        [Obsolete("Use public property IdentityServerAuthFeature.AuthRealm")]
        public static IAppSettings SetAuthRealm(this IAppSettings appSettings, string realm)
        {
            appSettings.Set(ConfigKeys.AuthRealm, realm);
            return appSettings;
        }

        [Obsolete("Use public property IdentityServerAuthFeature.AuthRealm")]
        public static string GetAuthRealm(this IAppSettings appSettings) => appSettings.GetString(ConfigKeys.AuthRealm);

        [Obsolete("Use public property IdentityServerAuthFeature.ClientId")]
        public static IAppSettings SetClientId(this IAppSettings appSettings, string clientId)
        {
            appSettings.Set(ConfigKeys.ClientId, clientId);
            return appSettings;
        }

        [Obsolete("Use public property IdentityServerAuthFeature.ClientId")]
        public static string GetClientId(this IAppSettings appSettings) => appSettings.GetString(ConfigKeys.ClientId);

        [Obsolete("Use public property IdentityServerAuthFeature.ClientSecret")]
        public static IAppSettings SetClientSecret(this IAppSettings appSettings, string clientSecret)
        {
            appSettings.Set(ConfigKeys.ClientSecret, clientSecret);
            return appSettings;
        }

        [Obsolete("Use public property IdentityServerAuthFeature.Scopes")]
        public static IAppSettings SetScopes(this IAppSettings appSettings, string scopes)
        {
            appSettings.Set(ConfigKeys.ClientScopes, scopes);
            return appSettings;
        }

        [Obsolete("Use public property IdentityServerAuthFeature.RoleClaimNames")]
        public static IAppSettings SetRoleClaims(this IAppSettings appSettings, string roles)
        {
            appSettings.Set(ConfigKeys.RoleClaims, roles);
            return appSettings;
        }

        [Obsolete("Use public property IdentityServerAuthFeature.PermissionClaimNames")]
        public static IAppSettings SetPermissionClaims(this IAppSettings appSettings, string permissions)
        {
            appSettings.Set(ConfigKeys.PermissionClaimNames, permissions);
            return appSettings;
        }
    }
}
