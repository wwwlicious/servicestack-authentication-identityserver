namespace ServiceStack.Authentication.IdentityServer
{
    using System.Threading.Tasks;
    using Configuration;
    using Interfaces;

    class DefaultClientSecretStore : IClientSecretStore
    {
        private readonly IAppSettings appSettings;

        public DefaultClientSecretStore(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public Task<string> GetSecretAsync(string clientId)
        {
            return Task.FromResult(appSettings.GetString(ConfigKeys.ClientSecret));
        }
    }
}
