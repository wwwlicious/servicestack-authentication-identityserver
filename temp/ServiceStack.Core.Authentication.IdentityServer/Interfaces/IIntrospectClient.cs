namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{
    using Enums;
    using System.Threading.Tasks;

    public interface IIntrospectClient
    {
        Task<TokenValidationResult> IsValidToken(string accessToken);
    }
}
