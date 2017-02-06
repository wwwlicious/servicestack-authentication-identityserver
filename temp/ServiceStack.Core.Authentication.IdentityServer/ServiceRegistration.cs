namespace ServiceStack.Core.Authentication.IdentityServer
{
    using System.Collections.Generic;

    public class ServiceRegistration
    {
        public string ClientId { get; set; }

        public string Secret { get; set; }

        public List<string> Roles { get; set; }

        public List<string> Permissions { get; set; }
    }
}
