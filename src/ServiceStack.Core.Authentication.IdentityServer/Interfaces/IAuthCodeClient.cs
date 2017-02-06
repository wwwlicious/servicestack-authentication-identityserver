namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;

    public interface IAuthCodeClient
    {
        Task<TokenRefreshResult> RequestCode(string code, string callbackUrl);
    }
}
