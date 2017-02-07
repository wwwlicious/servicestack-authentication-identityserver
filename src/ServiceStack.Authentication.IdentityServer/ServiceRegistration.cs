// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    using System.Collections.Generic;

    public class ServiceRegistration
    {
        public string ClientId { get; set; }

        public string Secret { get; set; }

        public List<string> Roles { get; set; }

        public List<string> Permissions { get; set; }
    }
}
