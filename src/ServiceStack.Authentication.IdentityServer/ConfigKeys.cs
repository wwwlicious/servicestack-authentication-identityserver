// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    internal class ConfigKeys
    {
        public const string OauthProvider = "oauth.provider";

        public const string AuthRealm = "oauth.IdentityServer.AuthRealm";

        public const string ClientId = "oauth.IdentityServer.ClientId";

        public const string ClientSecret = "oauth.IdentityServer.ClientSecret";

        public const string ClientScopes = "oauth.IdentityServer.Scopes";

        public const string RoleClaims = "oauth.IdentityServer.RoleClaimNames";

        public const string PermissionClaimNames = "oauth.IdentityServer.PermissionClaimNames";

        public const string AuthorizeUrl = "oauth.IdentityServer.AuthorizeUrl";

        public const string IntrospectUrl = "oauth.IdentityServer.IntrospectUrl";

        public const string UserInfoUrl = "oauth.IdentityServer.UserInfoUrl";

        public const string TokenUrl = "oauth.IdentityServer.TokenUrl";

        public const string CallbackUrl = "oauth.IdentityServer.CallbackUrl";

        public const string JwksUrl = "oauth.IdentityServer.JwksUrl";
    }
}
