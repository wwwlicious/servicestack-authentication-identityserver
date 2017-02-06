// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Enums
{
    public enum IdentityServerAuthProviderType
    {
        /// <summary>
        /// User Auth Provider - if the user is not authenticated, redirects to Identity Server for them to login
        /// </summary>
        UserAuthProvider = 0,

        /// <summary>
        /// Service Provider - requires that the service using a Client ID & Secret to authenticate
        /// </summary>
        ServiceProvider = 1,

        /// <summary>
        /// Impersonation Provider - requires that the requesting service provides an access token which is used to
        /// authenticate the user / service
        /// </summary>
        ImpersonationProvider = 2
    }
}
