// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Enums
{
    public enum IdentityServerOpenIdAuthorizationFlowType
    {
        /// <summary>
        /// Authentication using the Hybrid Flow
        /// <see cref="http://openid.net/specs/openid-connect-core-1_0.html#HybridFlowAuth"/>
        /// </summary>
        Hybrid = 0,

        /// <summary>
        /// Authentication using the Authorization Code Flow
        /// <see cref="http://openid.net/specs/openid-connect-core-1_0.html#CodeFlowSteps"/>
        /// </summary>
        CodeFlow = 1
    }
}
