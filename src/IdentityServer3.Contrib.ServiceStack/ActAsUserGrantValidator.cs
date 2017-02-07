// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.ServiceStack
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Services;
    using Core.Validation;

    /// <summary>
    /// Custom Grant Validator that takes an already valid access_token that has been created by another
    /// client and gets the subject (user) for that token.  It then creates a new access token with the
    /// scopes that have been requested.
    /// </summary>
    public class ActAsUserGrantValidator : ICustomGrantValidator
    {
        public const string GrantTypeName = "act-as-user";

        private readonly TokenValidator validator;

        /// <summary>Constructor</summary>
        /// <param name="validator">Token Validator</param>
        public ActAsUserGrantValidator(TokenValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>Validates the Custom grant request</summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var userAccessToken = request.Raw.Get("access_token");
            var clientRefererToken = request.Raw.Get("client_referer");
            if (string.IsNullOrWhiteSpace(userAccessToken))
            {
                return new CustomGrantValidationResult(Constants.TokenErrors.InvalidRequest);
            }
            if (string.IsNullOrWhiteSpace(clientRefererToken))
            {
                return new CustomGrantValidationResult(Constants.TokenErrors.InvalidRequest);
            }

            var result = await validator.ValidateAccessTokenAsync(userAccessToken).ConfigureAwait(false);
            if (result.IsError || result.Claims == null)
            {
                return new CustomGrantValidationResult(Constants.TokenErrors.InvalidRequest);
            }

            var subjectClaim = result.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Subject);
            if (subjectClaim == null)
            {
                return new CustomGrantValidationResult(Constants.TokenErrors.InvalidRequest);
            }

            var client = result.Client;
            if (client == null || !client.RedirectUris.Any(x => x.IndexOf(clientRefererToken, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return new CustomGrantValidationResult(Constants.TokenErrors.InvalidRequest);
            }            

            return new CustomGrantValidationResult(subjectClaim.Value, "access_token");
        }

        public string GrantType => GrantTypeName;
    }
}
