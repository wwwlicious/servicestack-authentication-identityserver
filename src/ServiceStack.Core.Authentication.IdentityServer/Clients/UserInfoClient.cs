namespace ServiceStack.Core.Authentication.IdentityServer.Clients
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using Interfaces;
    using Logging;

    internal class UserInfoClient : IUserInfoClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserInfoClient));

        private readonly IIdentityServerAuthProviderSettings appSettings;

        public UserInfoClient(IIdentityServerAuthProviderSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public async Task<IEnumerable<Claim>> GetClaims(string accessToken)
        {
            var client = new IdentityModel.Client.UserInfoClient(appSettings.UserInfoUrl);
            
            var response = await client.GetAsync(accessToken).ConfigureAwait(false);

            if (response.IsError)
            {
                Log.Error($"Error calling endpoint {appSettings.UserInfoUrl} - {response.Error}");
            }

            return response.Claims.Select(x => new Claim(x.Type, x.Value));
        }
    }
}
