// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    public class DocumentDiscoveryResult
    {
        public string AuthorizeUrl { get; set; }

        public string IntrospectUrl { get; set; }

        public string UserInfoUrl { get; set; }

        public string TokenUrl { get; set; }

        public string JwksUrl { get; set; }
    }
}
