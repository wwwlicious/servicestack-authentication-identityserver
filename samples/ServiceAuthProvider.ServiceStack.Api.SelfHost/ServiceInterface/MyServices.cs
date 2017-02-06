// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceAuthProvider.ServiceStack.Api.SelfHost.ServiceInterface
{
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer.Providers;
    using ServiceModel;

    public class MyServices : Service
    {
        [Authenticate(IdentityServerAuthProvider.Name)]
        public object Any(Hello request)
        {
            return new HelloResponse
            {
                Result = $"Hello, {request.Name} I'm resource only Service Stack Instance!"
            };
        }
    }
}
