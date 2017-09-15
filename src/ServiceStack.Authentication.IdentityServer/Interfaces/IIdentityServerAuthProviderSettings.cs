// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using ServiceStack.Authentication.IdentityServer.Enums;

namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Collections.Generic;

    public interface IIdentityServerAuthProviderSettings
    {
        IdentityServerOpenIdAuthorizationFlowType AuthorizationFlow { get; set; }

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

        string Username { get; }

        string Password { get; }

        IList<string> RoleClaimNames { get; }

        IList<string> PermissionClaimNames { get; }
    }
}
