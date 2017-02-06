namespace ServiceStack.Core.Authentication.IdentityServer
{
    using System;

    public class TokenRefreshResult : TokenResult
    {
        public string RefreshToken { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
