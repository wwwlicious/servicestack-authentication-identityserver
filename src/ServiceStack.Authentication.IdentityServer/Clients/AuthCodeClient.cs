namespace ServiceStack.Authentication.IdentityServer.Clients
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Interfaces;

    public class AuthCodeClient : IAuthCodeClient
    {
        private readonly IIdentityServerAuthProviderSettings settings;

        public AuthCodeClient(IIdentityServerAuthProviderSettings settings)
        {
            this.settings = settings;
        }

        public async Task<TokenRefreshResult> RequestCode(string code, string callbackUrl)
        {
            var client = new TokenClient(settings.RequestTokenUrl, settings.ClientId, settings.ClientSecret);
            var result = await client.RequestAsync(new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", callbackUrl}
            }).ConfigureAwait(false);

            if (result.IsError)
            {
                return new TokenRefreshResult();
            }

            return new TokenRefreshResult { AccessToken = result.AccessToken, RefreshToken = result.RefreshToken };
        }
    }
}
