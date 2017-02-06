// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ImpersonateAuthProvider.ServiceStack.Api.SelfHost.ServiceInterface
{
    using global::ServiceStack;
    using ServiceModel;

    public class MyServices : Service
    {
        [RequiredRole("Manager")]
        [RequiredPermission("CanBuyStuff")]
        public object Any(Hello request)
        {
            var session = this.GetSession();

            if (session.HasPermission("CanSeeAllOrders"))
            {
                return new HelloResponse
                {
                    Result = $"Whoooooaaaa! {request.Name}, you must be a big deal as you have the CanSeeAllOrders permission"
                };
            }
            else
            {
                return new HelloResponse
                {
                    Result = $"Hello, {request.Name} I'm in a separate Service Stack Instance!"
                };
            }
        }
    }
}
