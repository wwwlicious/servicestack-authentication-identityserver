namespace UserAuthProvider.ServiceStack.Core.SelfHost
{
    using System.IO;
    using Funq;
    using global::ServiceStack;
    using global::ServiceStack.Core.Authentication.IdentityServer;
    using global::ServiceStack.Core.Authentication.IdentityServer.Enums;
    using global::ServiceStack.Logging;
    using global::ServiceStack.Mvc;

    public class AppHost : AppHostBase
    {
        private readonly string serviceUrl;

        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost(string serviceUrl)
            : base("UserAuthProvider.ServiceStack.SelfHost", typeof(AppHost).GetAssembly())
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
            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = Path.GetFullPath(Path.Combine("~".MapServerPath(), "..", "..")),
#endif
                WebHostUrl = serviceUrl
            });
            this.Plugins.Add(new RazorFormat());
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
