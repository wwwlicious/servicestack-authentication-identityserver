// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceAuthProvider.ServiceStack.SelfHost.ServiceInterface
{
    using System;
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer.Providers;
    using ServiceModel;

    [Authenticate(IdentityServerAuthProvider.Name)]
    public class SecureService : Service
    {
        public object Any(Secure request)
        {
            var serviceClient = new JsonServiceClient("http://localhost:5003/");
            serviceClient.Post(new Authenticate { provider = IdentityServerAuthProvider.Name });

            var response = serviceClient.Get(new Hello { Name = request.Name });
            return new SecureResponse { Result = response.Result };
        }
    }
}
