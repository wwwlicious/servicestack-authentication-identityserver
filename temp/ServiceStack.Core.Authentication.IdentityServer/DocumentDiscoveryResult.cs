namespace ServiceStack.Core.Authentication.IdentityServer
{
    public class DocumentDiscoveryResult
    {
        public string AuthorizeUrl { get; set; }

        public string IntrospectUrl { get; set; }

        public string UserInfoUrl { get; set; }

        public string TokenUrl { get; set; }

        public string JwksUrl { get; set; }
    }
}
