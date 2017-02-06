namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IJsonWebKeyClient
    {
#if NETSTANDARD1_6
        Task<IList<Microsoft.IdentityModel.Tokens.SecurityKey>> GetAsync();
#elif NET45
        Task<IList<System.IdentityModel.Tokens.SecurityToken>> GetAsync();
#endif
    }
}
