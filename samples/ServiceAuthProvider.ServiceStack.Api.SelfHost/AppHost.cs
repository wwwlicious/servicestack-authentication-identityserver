// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceAuthProvider.ServiceStack.Api.SelfHost
{
    using Funq;
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer;
    using global::ServiceStack.Authentication.IdentityServer.Enums;
    using global::ServiceStack.Logging;
    using ServiceInterface;

    public class AppHost : AppSelfHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("ImpersonateAuthProvider.ServiceStack.Api.SelfHost", typeof(MyServices).Assembly)
        {
            LogManager.LogFactory = new ConsoleLogFactory(true);
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            this.Plugins.Add(new IdentityServerAuthFeature
            {
                AuthProviderType = IdentityServerAuthProviderType.ServiceProvider,
                AuthRealm = "http://localhost:5000/",
                ClientId = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                ClientSecret = "358bbaad-7921-4785-999e-adfbef1eb1d1",
                Scopes = "ServiceAuthProvider.ServiceStack.Api.SelfHost"
            });
        }
    }
}
