namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;

    public interface IActAsUserGrantTokenClient
    {
        Task<string> RequestCode(string accessToken, string accessTokenUrl);
    }
}
