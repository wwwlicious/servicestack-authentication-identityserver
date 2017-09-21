// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Tests
{
    using System.Collections.Generic;
    using Enums;
    using Interfaces;

    internal class TestIdentityServerAuthProviderSettings : IIdentityServerAuthProviderSettings
    {
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

        public string Username { get; set; }

        public string Password { get; set; }

        public IList<string> RoleClaimNames { get; set; }

        public IList<string> PermissionClaimNames { get; set; }

        public IdentityServerOpenIdAuthorizationFlowType AuthorizationFlow { get; set; }

        public DocumentDiscoveryResult DiscoveryResult { get; set; }
    }
}
