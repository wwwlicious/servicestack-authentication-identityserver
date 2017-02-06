namespace ServiceStack.Core.Authentication.IdentityServer
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Auth;

    public class IdentityServerAuthTokens : AuthTokens
    {
        public IdentityServerAuthTokens()
        {
            Claims = new List<Claim>();
        }

        public string IdToken { get; set; }

        public string Code { get; set; }

        public string Issuer { get; set; }

        public string Subject { get; set; }

        public string Audience { get; set; }

        public string Expiration { get; set; }

        public string IssuedAt { get; set; }

        public string AuthenticationTime { get; set; }

        public string Nonce { get; set; }

        public IList<Claim> Claims { get; set; }
    }
}
