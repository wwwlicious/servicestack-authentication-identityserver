// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Auth;
    using Configuration;
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

            var provider = new UserAuthProvider(new DictionarySettings())
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

            var provider = new UserAuthProvider(new DictionarySettings()) {CallbackUrl = url};

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(true);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
        }

        [Fact]
        public void IsInitialAuthenticateRequest_ReturnsFalseWhenValidUrlAndValidAccessToken()
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";
            const string accessToken = "A1234";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            A.CallTo(() => authTokensFake.AccessToken).Returns(accessToken);

            var introspectionClientFake = A.Fake<IIntrospectClient>();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).Returns(TokenValidationResult.Success);

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = introspectionClientFake,
                CallbackUrl = url
            };

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(false);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).MustHaveHappened();
        }

        [Fact]
        public void IsInitialAuthenticateRequest_ReturnsTrueWhenValidUrlAndInvalidAccessToken()
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";
            const string accessToken = "A1234";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            A.CallTo(() => authTokensFake.AccessToken).Returns(accessToken);

            var introspectionClientFake = A.Fake<IIntrospectClient>();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).Returns(TokenValidationResult.Error);

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = introspectionClientFake,
                CallbackUrl = url
            };

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(true);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).MustHaveHappened();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsInitialAuthenticateRequest_ReturnsTrueWhenValidUrlAndExpiredAccessTokenWithoutRefreshToken(string refreshToken)
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";
            const string accessToken = "A1234";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            A.CallTo(() => authTokensFake.AccessToken).Returns(accessToken);
            A.CallTo(() => authTokensFake.RefreshToken).Returns(refreshToken);

            var introspectionClientFake = A.Fake<IIntrospectClient>();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).Returns(TokenValidationResult.Expired);

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = introspectionClientFake,
                CallbackUrl = url
            };

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(true);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
            A.CallTo(() => authTokensFake.RefreshToken).MustHaveHappened();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).MustHaveHappened();
        }

        [Fact]
        public void IsInitialAuthenticateRequest_ReturnsTrueWhenValidUrlAndExpiredAccessTokenAndUnableToRefresh()
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";
            const string accessToken = "A1234";
            const string refreshToken = "B5678";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            A.CallTo(() => authTokensFake.AccessToken).Returns(accessToken);
            A.CallTo(() => authTokensFake.RefreshToken).Returns(refreshToken);

            var introspectionClientFake = A.Fake<IIntrospectClient>();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).Returns(TokenValidationResult.Expired);

            var refreshClientFake = A.Fake<IRefreshTokenClient>();
            A.CallTo(() => refreshClientFake.RefreshToken(refreshToken)).Returns(new TokenRefreshResult());

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = introspectionClientFake,
                RefreshTokenClient = refreshClientFake,
                CallbackUrl = url
            };

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(true);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => authTokensFake.AccessToken).MustHaveHappened();
            A.CallTo(() => authTokensFake.RefreshToken).MustHaveHappened();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).MustHaveHappened();
            A.CallTo(() => refreshClientFake.RefreshToken(refreshToken)).MustHaveHappened();
        }

        [Fact]
        public void IsInitialAuthenticateRequest_ReturnsFalseWhenValidUrlAndExpiredAccessTokenAndAbleToRefresh()
        {
            // Arrange
            const string url = "http://localhost:5000/Auth/IdentityServer";
            const string accessToken = "A1234";
            const string refreshToken = "B5678";

            const string newAccessToken = "C1234";
            const string newRefreshToken = "D5678";

            var httpRequestFake = A.Fake<IRequest>();
            A.CallTo(() => httpRequestFake.AbsoluteUri).Returns(url);

            var authTokensFake = A.Fake<IAuthTokens>();
            authTokensFake.AccessToken = accessToken;
            authTokensFake.RefreshToken = refreshToken;

            var introspectionClientFake = A.Fake<IIntrospectClient>();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).Returns(TokenValidationResult.Expired);

            var refreshClientFake = A.Fake<IRefreshTokenClient>();
            A.CallTo(() => refreshClientFake.RefreshToken(refreshToken)).Returns(new TokenRefreshResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = introspectionClientFake,
                RefreshTokenClient = refreshClientFake,
                CallbackUrl = url
            };

            // Act
            var result = provider.IsInitialAuthenticateRequest(httpRequestFake, authTokensFake).Result;

            // Assert
            result.Should().Be(false);

            authTokensFake.AccessToken.Should().Be(newAccessToken);
            authTokensFake.RefreshToken.Should().Be(newRefreshToken);

            A.CallTo(() => httpRequestFake.AbsoluteUri).MustHaveHappened();
            A.CallTo(() => introspectionClientFake.IsValidToken(accessToken)).MustHaveHappened();
            A.CallTo(() => refreshClientFake.RefreshToken(refreshToken)).MustHaveHappened();
        }

        [Fact]
        public void IsAuthorized_RequestNullSessionNull_ReturnsFalse()
        {
            var provider = new UserAuthProvider(new DictionarySettings());

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

            var provider = new UserAuthProvider(new DictionarySettings());

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

            var provider = new UserAuthProvider(new DictionarySettings());

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenNotValid_ReturnsFalse()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();
            var clientIntrospectionFake = A.Fake<IIntrospectClient>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = "A12345";

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).Returns(TokenValidationResult.Error);

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = clientIntrospectionFake
            };

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(false);

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).MustHaveHappened();
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenExpired_NoRefreshToken_ReturnsFalse()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();
            var clientIntrospectionFake = A.Fake<IIntrospectClient>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = "A12345";
            authTokenFake.RefreshToken = null;

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).Returns(TokenValidationResult.Expired);

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = clientIntrospectionFake
            };

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(false);

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).MustHaveHappened();
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenExpired_RefreshTokenInvalid_ReturnsFalse()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();
            var clientIntrospectionFake = A.Fake<IIntrospectClient>();
            var clientRefreshFake = A.Fake<IRefreshTokenClient>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = "A12345";
            authTokenFake.RefreshToken = "B6789";

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).Returns(TokenValidationResult.Expired);
            A.CallTo(() => clientRefreshFake.RefreshToken("B6789")).Returns(Task.FromResult(new TokenRefreshResult()));

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = clientIntrospectionFake,
                RefreshTokenClient = clientRefreshFake
            };

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(false);

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).MustHaveHappened();
            A.CallTo(() => clientRefreshFake.RefreshToken("B6789")).MustHaveHappened();
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenValid_ReturnsTrue()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();
            var clientIntrospectionFake = A.Fake<IIntrospectClient>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = "A12345";

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).Returns(TokenValidationResult.Success);

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = clientIntrospectionFake
            };

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(true);

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).MustHaveHappened();
        }

        [Fact]
        public void IsAuthorized_RequestNull_SessionAuthenticated_AccessTokenExpired_RefreshTokenValid_ReturnsTrue()
        {
            // Arrange
            var authSessionFake = A.Fake<IAuthSession>();
            var authTokenFake = A.Fake<IAuthTokens>();
            var clientIntrospectionFake = A.Fake<IIntrospectClient>();
            var clientRefreshFake = A.Fake<IRefreshTokenClient>();

            authSessionFake.IsAuthenticated = true;
            authTokenFake.AccessToken = "A12345";
            authTokenFake.RefreshToken = "B6789";

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).Returns(TokenValidationResult.Expired);
            A.CallTo(() => clientRefreshFake.RefreshToken("B6789")).Returns(Task.FromResult(new TokenRefreshResult
            {
                AccessToken = "C12345",
                RefreshToken = "E6789"
            }));

            var provider = new UserAuthProvider(new DictionarySettings())
            {
                IntrospectionClient = clientIntrospectionFake,
                RefreshTokenClient = clientRefreshFake
            };

            // Act
            var result = provider.IsAuthorized(authSessionFake, authTokenFake);

            result.Should().Be(true);

            authTokenFake.AccessToken.Should().Be("C12345");
            authTokenFake.RefreshToken.Should().Be("E6789");

            A.CallTo(() => clientIntrospectionFake.IsValidToken("A12345")).MustHaveHappened();
            A.CallTo(() => clientRefreshFake.RefreshToken("B6789")).MustHaveHappened();
        }
    }
}
