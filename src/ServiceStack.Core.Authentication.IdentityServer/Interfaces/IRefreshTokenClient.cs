namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{    
    using System.Threading.Tasks;

    public interface IRefreshTokenClient
    {
        Task<TokenRefreshResult> RefreshToken(string refreshToken);
    }
}
