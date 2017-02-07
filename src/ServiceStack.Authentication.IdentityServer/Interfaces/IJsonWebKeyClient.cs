// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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
