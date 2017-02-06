namespace ServiceStack.Authentication.IdentityServer.Tests
{
    using System.Collections.Generic;
    using Interfaces;

    internal class TestIdentityServerAuthProviderSettings : IIdentityServerAuthProviderSettings
    {
        public DocumentDiscoveryResult DiscoveryResult { get; set; }

        public string AuthRealm { get; set; }

        public string AuthorizeUrl { get; set; }

        public string IntrospectUrl { get; set; }

        public string UserInfoUrl { get; set; }

        public string RequestTokenUrl { get; set; }

        public string CallbackUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string JwksUrl { get; set; }

        public string Scopes { get; set; }

        public IList<string> RoleClaimNames { get; set; }

        public IList<string> PermissionClaimNames { get; set; }
    }
}
