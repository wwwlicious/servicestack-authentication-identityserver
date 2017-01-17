// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Tests
{
    using System.Threading.Tasks;
    using Auth;
    using Enums;
    using FakeItEasy;
    using FluentAssertions;
    using Interfaces;
    using Providers;
    using Web;
    using Xunit;

    public class UserAuthProviderTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("http://localhost:5000/Auth/")]
        [InlineData("http://localhost:5001/Auth/IdentityServer")]
        public void IsInitialAuthenticateRequest_ReturnsFalseWhenAbsoluteUriIsNotValid(string absoluteUri)
        {
            // Arrange
            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(absoluteUri);

            var authTokensFake = A.Fake<IAuthTokens>();

            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings())
            {
                CallbackUrl = "http://localhost:5000/Auth/IdentityServer"
            };
            
            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(false);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsInitialAuthenticateRequest_ReturnsTrueWhenValidUrlAndNoTokensFound(string accessToken)
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            A.CallTo(() => authTokensFake.AccessToken).Returns(accessToken);

            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings()) {CallbackUrl = url};

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(true);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
        }

        [Fact]
        public void IsInitialAuthenticateRequest_ReturnsFalseWhenValidUrlAndHasAccessToken()
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";
            const string accessToken = "A1234";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            A.CallTo(() => authTokensFake.AccessToken).Returns(accessToken);

            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings())
            {
                CallbackUrl = url
            };

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(false);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
        }

        [Fact]
        public void IsAuthorized_RequestNullSessionNull_ReturnsFalse()
        {
            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings());

            // Act
            var result = provider.IsAuthorized(null, null);

            result.Should().Be(false);
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionNotAuthenticated_ReturnsFalse()
        {
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();

            authSessionFake.IsAuthenticated = false;

            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings());

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(false);
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenNull_ReturnsFalse()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = null;

            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings());

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenValid_ReturnsTrue()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = "A12345";

            var provider = new UserAuthProvider(new TestIdentityServerAuthProviderSettings());

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(true);
        }
    }
}
