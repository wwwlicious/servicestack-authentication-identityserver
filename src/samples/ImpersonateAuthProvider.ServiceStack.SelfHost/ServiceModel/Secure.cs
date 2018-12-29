// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ImpersonateAuthProvider.ServiceStack.SelfHost.ServiceModel
{
    using global::ServiceStack;

    [Route("/secure")]
    [Route("/secure/{Name}")]
    public class Secure : IReturn<SecureResponse>
    {
        public string Name { get; set; }
    }
    
    public class SecureResponse
    {
        public string Result { get; set; }
    }
}
