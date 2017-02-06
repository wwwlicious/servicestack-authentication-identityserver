namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;
    using Enums;

    public interface IIntrospectClient
    {
        Task<TokenValidationResult> IsValidToken(string accessToken);
    }
}
