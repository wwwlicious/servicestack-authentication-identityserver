// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ImpersonateAuthProvider.ServiceStack.SelfHost
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
            : base("ImpersonateAuthProvider.ServiceStack.SelfHost", typeof (MyServices).Assembly)
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
            //Config examples
            //this.Plugins.Add(new PostmanFeature());
            //this.Plugins.Add(new CorsFeature());

            this.Plugins.Add(new RazorFormat());
            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = Path.GetFullPath(Path.Combine("~".MapServerPath(), "..", "..")),
#endif
                WebHostUrl = serviceUrl
            });

            this.Plugins.Add(new IdentityServerAuthFeature(AppSettings, new JsonServiceClient("http://localhost:5003/"))
            {
                AuthProviderType = IdentityServerAuthProviderType.UserAuthProvider,
                AuthRealm = "http://localhost:5000/",
                ClientId = "ImpersonateAuthProvider.ServiceStack.SelfHost",
                ClientSecret = "99e1ae38-866c-4ff4-b9e0-dcfaeb3dbb4a",
                Scopes = "openid profile ImpersonateAuthProvider.ServiceStack.SelfHost email offline_access"
            });
        }
    }
}