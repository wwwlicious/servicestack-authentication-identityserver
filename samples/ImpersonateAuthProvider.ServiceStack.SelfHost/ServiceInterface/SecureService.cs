// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ImpersonateAuthProvider.ServiceStack.SelfHost.ServiceInterface
{
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer.Providers;
    using ServiceModel;

    [Authenticate(IdentityServerAuthProvider.Name)]
    public class SecureService : Service
    {
        public IServiceClient ExternalServiceClient { get; set; }

        public object Any(Secure request)
        {
            var response = ExternalServiceClient.Get(new Hello {Name = request.Name});
            return new SecureResponse {Result = response.Result};
        }
    }
}
