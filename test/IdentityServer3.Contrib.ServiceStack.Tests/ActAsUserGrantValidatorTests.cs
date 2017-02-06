// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.Contrib.ServiceStack.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Specialized;
    using System.Security.Claims;
    using Core;
    using Core.Configuration;
    using Core.Models;
    using Core.Services.InMemory;
    using Core.Validation;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ActAsUserGrantValidatorTests
    {
        [Fact]
        public void ValidateAsync_ReturnsInvalidRequestErrorWhenAccessTokenNotFound()
        {
            // Arrange
            var tokenRequest = new ValidatedTokenRequest { Raw = new NameValueCollection() };
            var validator = new ActAsUserGrantValidator(null);

            // Act
            var result = validator.ValidateAsync(tokenRequest).Result;

            // Assert
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);
        }

        [Fact]
        public void ValidateAsync_ReturnsInvalidRequestErrorWhenAccessTokenNotValid()
        {
            // Arrange
            var tokenRequest = new ValidatedTokenRequest { Raw = new NameValueCollection() };
            tokenRequest.Raw.Add("access_token", "A12345");
            tokenRequest.Raw.Add("client_referer", "http://localhost:12345");

            var tokenValidatorFake = createTokenValidatorFake;
            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null))
             .Returns(Task.FromResult(new TokenValidationResult { IsError = true }));

            var validator = new ActAsUserGrantValidator(tokenValidatorFake);

            // Act
            var result = validator.ValidateAsync(tokenRequest).Result;

            // Assert
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);

            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null)).MustHaveHappened();
        }

        [Fact]
        public void ValidateAsync_ReturnsInvalidRequestWhenClaimsNull()
        {
            // Arrange
            var tokenRequest = new ValidatedTokenRequest { Raw = new NameValueCollection() };
            tokenRequest.Raw.Add("access_token", "A12345");
            tokenRequest.Raw.Add("client_referer", "http://localhost:12345");

            var tokenValidatorFake = createTokenValidatorFake;
            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null))
             .Returns(Task.FromResult(new TokenValidationResult { IsError = false, Claims = null }));

            var validator = new ActAsUserGrantValidator(tokenValidatorFake);

            // Act
            var result = validator.ValidateAsync(tokenRequest).Result;

            // Assert
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);

            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null)).MustHaveHappened();
        }

        [Fact]
        public void ValidateAsync_ReturnsInvalidRequestWhenSubjectClaimNotFound()
        {
            // Arrange
            var tokenRequest = new ValidatedTokenRequest { Raw = new NameValueCollection() };
            tokenRequest.Raw.Add("access_token", "A12345");
            tokenRequest.Raw.Add("client_referer", "http://localhost:12345");

            var tokenValidatorFake = createTokenValidatorFake;
            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null))
             .Returns(Task.FromResult(new TokenValidationResult { IsError = false, Claims = new List<Claim>() }));

            var validator = new ActAsUserGrantValidator(tokenValidatorFake);

            // Act
            var result = validator.ValidateAsync(tokenRequest).Result;

            // Assert
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);

            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null)).MustHaveHappened();
        }

        [Fact]
        public void ValidateAsync_ReturnsInvalidRequestWhenClientRefererDoesNotMatch()
        {
            // Arrange
            var tokenRequest = new ValidatedTokenRequest
            {
                Raw = new NameValueCollection(),
                Scopes = new List<string> { "scope1", "scope2", "scope3" }
            };
            tokenRequest.Raw.Add("access_token", "A12345");
            tokenRequest.Raw.Add("client_referer", "http://localhost:12345");

            var tokenValidatorFake = createTokenValidatorFake;
            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null))
             .Returns(Task.FromResult(new TokenValidationResult
             {
                 IsError = false,
                 Claims = new List<Claim>
                 {
                    new Claim(Constants.ClaimTypes.Subject, "sun123")
                 },
                 Client = new Client { RedirectUris = new List<string> { "http://piratebay:12345" } }
             }));

            var validator = new ActAsUserGrantValidator(tokenValidatorFake);

            // Act
            var result = validator.ValidateAsync(tokenRequest).Result;

            // Assert
            result.Error.Should().Be(Constants.TokenErrors.InvalidRequest);

            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null)).MustHaveHappened();
        }

        [Fact]
        public void ValidateAsync_ReturnsResultWhenSubjectClaimFound()
        {
            // Arrange
            var tokenRequest = new ValidatedTokenRequest
            {
                Raw = new NameValueCollection(),
                Scopes = new List<string> { "scope1", "scope2", "scope3" }
            };
            tokenRequest.Raw.Add("access_token", "A12345");
            tokenRequest.Raw.Add("client_referer", "http://localhost:12345");

            var tokenValidatorFake = createTokenValidatorFake;
            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null))
             .Returns(Task.FromResult(new TokenValidationResult
             {
                 IsError = false,
                 Claims = new List<Claim>
                 {
                    new Claim(Constants.ClaimTypes.Subject, "sun123")
                 },
                 Client = new Client { RedirectUris = new List<string> { "http://localhost:12345" } }
             }));

            var validator = new ActAsUserGrantValidator(tokenValidatorFake);

            // Act
            var result = validator.ValidateAsync(tokenRequest).Result;

            // Assert
            result.Principal.Identity.AuthenticationType.Should().Be("access_token");
            result.Principal.Claims.First().Type.Should().Be(Constants.ClaimTypes.Subject);
            result.Principal.Claims.First().Value.Should().Be("sun123");

            A.CallTo(() => tokenValidatorFake.ValidateAccessTokenAsync("A12345", null)).MustHaveHappened();
        }

        private readonly TokenValidator createTokenValidatorFake = A.Fake<TokenValidator>(x => x.WithArgumentsForConstructor(() => new TokenValidator(new IdentityServerOptions(), new InMemoryClientStore(null), new InMemoryTokenHandleStore(), null)));
    }
}
