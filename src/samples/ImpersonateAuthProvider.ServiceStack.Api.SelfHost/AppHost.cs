// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ImpersonateAuthProvider.ServiceStack.Api.SelfHost
{
    using Funq;
    using global::ServiceStack;
    using global::ServiceStack.Authentication.IdentityServer;
    using global::ServiceStack.Authentication.IdentityServer.Enums;
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
                AuthProviderType = IdentityServerAuthProviderType.ImpersonationProvider,
                AuthRealm = "http://localhost:5000/",
                ClientId = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                ClientSecret = "a9c08d7b-ffc2-49f4-99c9-ce232d9f0cf6",
                Scopes = "openid ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                RoleClaims = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Role",
                PermissionClaims = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Permission"
            });
        }
    }
}
