namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;
    using Auth;

    public interface IIdentityServerIdTokenValidator
    {
        Task Init();

        bool IsValidIdToken(IAuthTokens authTokens, string idToken);
    }
}
