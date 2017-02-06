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
