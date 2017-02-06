namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{
    using System.Collections.Generic;

    public interface IIdentityServerAuthProviderSettings
    {
        DocumentDiscoveryResult DiscoveryResult { get; set; }

        string AuthRealm { get; }

        string AuthorizeUrl { get; }

        string IntrospectUrl { get; }

        string UserInfoUrl { get; }

        string RequestTokenUrl { get; }

        string CallbackUrl { get; }


        string ClientId { get; }

        string ClientSecret { get; }

        string JwksUrl { get; }

        string Scopes { get; }

        IList<string> RoleClaimNames { get; }

        IList<string> PermissionClaimNames { get; }
    }
}
