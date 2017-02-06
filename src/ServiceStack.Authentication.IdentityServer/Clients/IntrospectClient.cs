namespace ServiceStack.Authentication.IdentityServer.Clients
{
    using Enums;
    using IdentityModel.Client;
    using Interfaces;
    using Logging;
    using System.Threading.Tasks;

    internal class IntrospectClient : IIntrospectClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IntrospectClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public IntrospectClient(IIdentityServerAuthProviderSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public async Task<TokenValidationResult> IsValidToken(string accessToken)
        {
            var client = new IntrospectionClient(appSettings.IntrospectUrl);
            var result = await client.SendAsync(new IntrospectionRequest
            {
                Token = accessToken,

                ClientId = appSettings.ClientId,
                ClientSecret = appSettings.ClientSecret
            }).ConfigureAwait(false);

            if (result.IsError)
            {
                Log.Error($"An error occurred while validating the access token - {result.Error}");
                return TokenValidationResult.Error;
            }

            if (!result.IsActive)
            {
                Log.Error("Access token is not active");
                return TokenValidationResult.Expired;
            }

            return TokenValidationResult.Success;
        }
    }
}
