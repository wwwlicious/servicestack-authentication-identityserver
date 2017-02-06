namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;

    public interface IJsonWebKeyClient
    {
        Task<IList<SecurityKey>> GetAsync();
    }
}
