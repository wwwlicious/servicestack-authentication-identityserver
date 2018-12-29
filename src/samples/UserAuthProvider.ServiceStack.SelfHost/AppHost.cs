// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace UserAuthProvider.ServiceStack.SelfHost
{
    using System.IO;
    using Funq;
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer;
    using global::ServiceStack.Authentication.IdentityServer.Enums;
    using global::ServiceStack.Logging;
    using global::ServiceStack.Razor;
    using ServiceInterface;

    public class AppHost : AppSelfHostBase
    {

        private readonly string serviceUrl;

        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost(string serviceUrl)
            : base("UserAuthProvider.ServiceStack.SelfHost", typeof (MyServices).Assembly)
        {
            this.serviceUrl = serviceUrl;

            LogManager.LogFactory = new ConsoleLogFactory(true);
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            this.Plugins.Add(new RazorFormat());
            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = Path.GetFullPath(Path.Combine("~".MapServerPath(), "..", "..")),
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