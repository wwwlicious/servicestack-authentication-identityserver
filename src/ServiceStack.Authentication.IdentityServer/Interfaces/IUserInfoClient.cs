namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Security.Claims;

    public interface IUserInfoClient
    {
        Task<IEnumerable<Claim>> GetClaims(string accessToken);
    }
}
