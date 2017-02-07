// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Auth;
    using Clients;
    using Interfaces;
    using Logging;

#if NETSTANDARD1_6    

    using System.IdentityModel.Tokens.Jwt;
    using IdentityModel;
    using Microsoft.IdentityModel.Tokens;

    class IdentityServerIdTokenValidator : IIdentityServerIdTokenValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IdentityServerIdTokenValidator));

        private readonly IIdentityServerAuthProviderSettings appSettings;
        private readonly TokenValidationParameters tokenValidationParameters;

        public IdentityServerIdTokenValidator(IIdentityServerAuthProviderSettings settings)
        {
            this.appSettings = settings;
            tokenValidationParameters = new TokenValidationParameters();
        }

        internal ISecurityTokenValidator SecurityTokenValidator { get; set; }

        internal IJsonWebKeyClient JsonWebKeyClient { get; set; }

        public async Task Init()
        {
            if (SecurityTokenValidator == null)
            {
                SecurityTokenValidator = new JwtSecurityTokenHandler();
            }

            if (JsonWebKeyClient == null)
            {
                JsonWebKeyClient = new JsonWebKeyClient(appSettings);
            }

            var keys = await JsonWebKeyClient.GetAsync();

            if (keys == null || keys.Count == 0)
            {
                Log.Warn($"Unable to load Json Web Kit Set from {appSettings.JwksUrl}");
            }
            else
            {
                tokenValidationParameters.IssuerSigningKeys = keys;
            }

            tokenValidationParameters.ValidAudience = appSettings.ClientId;

            var realms = new List<string>();
            var realm = appSettings.AuthRealm;
            realms.Add(realm.EndsWith("/") ? realm.TrimEnd('/') : $"{realm}/");
            realms.Add(realm);

            tokenValidationParameters.ValidIssuers = realms;
        }

        public bool IsValidIdToken(IAuthTokens authTokens, string idToken)
        {
            var jwtToken = new JwtSecurityToken(idToken);

            var idAuthTokens = authTokens as IdentityServerAuthTokens;
            if (idAuthTokens != null)
            {
                var nonce = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Nonce);
                if (nonce != null && nonce.Value != idAuthTokens.Nonce)
                {
                    Log.Error("Nonce in id_token does not match the nonce created for the login request - potential replay attack");
                    return false;
                }
            }

            SecurityToken validatedToken = null;

            try
            {
                SecurityTokenValidator.ValidateToken(idToken, tokenValidationParameters, out validatedToken);
            }
            catch (Exception exception)
            {
                Log.Error("Error validating JWT token", exception);
                return false;
            }

            if (validatedToken == null)
            {
                Log.Error("Unable to validate id_token");
                return false;
            }

            var jwt = validatedToken as JwtSecurityToken;
            if (jwt == null)
            {
                Log.Error("id_token is not a valid jwt token");
                return false;
            }

            return true;
        }
    }

#elif NET45

    using System.IdentityModel.Tokens;
    using IdentityModel;

    class IdentityServerIdTokenValidator : IIdentityServerIdTokenValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IdentityServerIdTokenValidator));

        private readonly IIdentityServerAuthProviderSettings appSettings;
        private readonly TokenValidationParameters tokenValidationParameters;

        public IdentityServerIdTokenValidator(IIdentityServerAuthProviderSettings settings)
        {
            this.appSettings = settings;
            tokenValidationParameters = new TokenValidationParameters();
        }

        internal ISecurityTokenValidator SecurityTokenValidator { get; set; }

        internal IJsonWebKeyClient JsonWebKeyClient { get; set; }

        public async Task Init()
        {
            if (SecurityTokenValidator == null)
            {
                SecurityTokenValidator = new JwtSecurityTokenHandler();
            }

            if (JsonWebKeyClient == null)
            {
                JsonWebKeyClient = new JsonWebKeyClient(appSettings);
            }

            var tokens = await JsonWebKeyClient.GetAsync();

            if (tokens == null || tokens.Count == 0)
            {
                Log.Warn($"Unable to load Json Web Kit Set from {appSettings.JwksUrl}");
            }
            else
            {
                tokenValidationParameters.IssuerSigningTokens = tokens;
            }

            tokenValidationParameters.ValidAudience = appSettings.ClientId;

            var realms = new List<string>();
            var realm = appSettings.AuthRealm;
            realms.Add(realm.EndsWith("/") ? realm.TrimEnd('/') : $"{realm}/");
            realms.Add(realm);

            tokenValidationParameters.ValidIssuers = realms;
        }

        public bool IsValidIdToken(IAuthTokens authTokens, string idToken)
        {
            var jwtToken = new JwtSecurityToken(idToken);

            var idAuthTokens = authTokens as IdentityServerAuthTokens;
            if (idAuthTokens != null)
            {
                var nonce = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Nonce);
                if (nonce != null && nonce.Value != idAuthTokens.Nonce)
                {
                    Log.Error("Nonce in id_token does not match the nonce created for the login request - potential replay attack");
                    return false;
                }
            }

            SecurityToken validatedToken = null;

            try
            {
                SecurityTokenValidator.ValidateToken(idToken, tokenValidationParameters, out validatedToken);
            }
            catch (Exception exception)
            {
                Log.Error("Error validating JWT token", exception);
                return false;
            }

            if (validatedToken == null)
            {
                Log.Error("Unable to validate id_token");
                return false;
            }

            var jwt = validatedToken as JwtSecurityToken;
            if (jwt == null)
            {
                Log.Error("id_token is not a valid jwt token");
                return false;
            }

            return true;
        }
    }
#endif
}
