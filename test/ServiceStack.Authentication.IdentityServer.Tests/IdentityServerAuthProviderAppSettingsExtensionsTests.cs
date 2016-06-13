// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Authentication.IdentityServer.Tests
{
    using Configuration;
    using Enums;
    using Extensions;
    using FluentAssertions;
    using Xunit;

    public class IdentityServerAuthProviderAppSettingsExtensionsTests
    {
        [Fact]
        public void SetUserAuthProvider_SetsCorrectAuthProvider()
        {
            // Arrange
            var appSettings = new DictionarySettings();

            // Act
            appSettings.SetUserAuthProvider();

            // Assert
            appSettings.Get<IdentityServerAuthProviderType>("oauth.provider")
                .Should()
                .Be(IdentityServerAuthProviderType.UserAuthProvider);
        }

        [Fact]
        public void SetImpersonationAuthProvider_SetsCorrectAuthProvider()
        {
            // Arrange
            var appSettings = new DictionarySettings();

            // Act
            appSettings.SetImpersonationAuthProvider();

            // Assert
            appSettings.Get<IdentityServerAuthProviderType>("oauth.provider")
                .Should()
                .Be(IdentityServerAuthProviderType.ImpersonationProvider);
        }

        [Fact]
        public void SetServiceAuthProvider_SetsCorrectAuthProvider()
        {
            // Arrange
            var appSettings = new DictionarySettings();

            // Act
            appSettings.SetServiceAuthProvider();

            // Assert
            appSettings.Get<IdentityServerAuthProviderType>("oauth.provider")
                .Should()
                .Be(IdentityServerAuthProviderType.ServiceProvider);
        }
    }
}
