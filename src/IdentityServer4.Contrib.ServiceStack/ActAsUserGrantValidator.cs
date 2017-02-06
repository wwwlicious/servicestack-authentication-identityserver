// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer4.Contrib.ServiceStack
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityModel;
    using Models;
    using Validation;

    public class ActAsUserGrantValidator : IExtensionGrantValidator
    {
        public const string GrantTypeName = "act-as-user";

        private readonly ITokenValidator validator;

        /// <summary>Constructor</summary>
        /// <param name="validator">Token Validator</param>
        public ActAsUserGrantValidator(ITokenValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>Validates the Custom grant request</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userAccessToken = context.Request.Raw.Get("access_token");
            var clientRefererToken = context.Request.Raw.Get("client_referer");

            if (string.IsNullOrWhiteSpace(userAccessToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            if (string.IsNullOrWhiteSpace(clientRefererToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            var result = await validator.ValidateAccessTokenAsync(userAccessToken).ConfigureAwait(false);
            if (result.IsError || result.Claims == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            var subjectClaim = result.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (subjectClaim == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            var client = result.Client;
            if (client == null || !client.RedirectUris.Any(x => x.IndexOf(clientRefererToken, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);                
            }

            context.Result = new GrantValidationResult(subjectClaim.Value, "access_token");
        }

        public string GrantType => GrantTypeName;
    }
}
