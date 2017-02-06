// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace UserAuthProvider.ServiceStack.Core.SelfHost
{
    using Funq;
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer;
    using global::ServiceStack.Authentication.IdentityServer.Enums;
    using global::ServiceStack.Mvc;

    public class AppHost : AppHostBase
    {
        private readonly string serviceUrl;

        public AppHost(string serviceUrl)
         : base("UserAuthProvider.ServiceStack.Core.SelfHost", typeof(AppHost).GetAssembly())
        {
            this.serviceUrl = serviceUrl;
        }

        public override void Configure(Container container)
        {
            this.Plugins.Add(new RazorFormat());

            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
#endif
                WebHostUrl = serviceUrl
            });

            this.Plugins.Add(new IdentityServerAuthFeature
            {
                AuthProviderType = IdentityServerAuthProviderType.UserAuthProvider,
                AuthRealm = "http://localhost:5000/",
                ClientId = "UserAuthProvider.ServiceStack.SelfHost",
                ClientSecret = "F621F470-9731-4A25-80EF-67A6F7C5F4B8",
                Scopes = "openid profile UserAuthProvider.ServiceStack.SelfHost email offline_access"
            });
        }
    }
}
