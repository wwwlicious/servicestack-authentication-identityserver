namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;

    public interface ITokenCredentialsClient
    {
        Task<TokenResult> RequestToken();
    }
}
