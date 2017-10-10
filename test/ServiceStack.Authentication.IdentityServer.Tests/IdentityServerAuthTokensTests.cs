using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using ServiceStack.Auth;
using ServiceStack.Authentication.IdentityServer.Providers;
using ServiceStack.Text;
using Xunit;

namespace ServiceStack.Authentication.IdentityServer.Tests
{
    public class IdentityServerAuthTokensTests
    {
        private readonly IAuthSession _session;

        public IdentityServerAuthTokensTests()
        {
            var subjectNameClaim = new Claim(ClaimsIdentity.DefaultNameClaimType, "test subject name");
            var subject = new ClaimsIdentity();
            subject.AddClaim(subjectNameClaim);

            var simpleClaim = new Claim("testsimpleclaimtype", "testsimplevalue");

            var complexClaim = new Claim(
                "testcomplexclaim",
                "testcomplexvalue",
                "testcomplextype",
                "testcomplexissuer",
                "testcomplexriginalissues",
                subject)
            {
                Properties = { { "testcomplexpropertykey", "testcomplextpropertyvalue" } }
            };

            var token = new IdentityServerAuthTokens
            {
                UserId = subject.Name,
                Claims = new Dictionary<string, string>
        {
          { ClaimsIdentity.DefaultNameClaimType, subjectNameClaim.Value },
          { simpleClaim.Type, simpleClaim.Value},

          //TODO: Originally IdentityServerAuthTokens had full System.Security.Claim objects here
          //      but since they can't be serialized, and only type/value was used anyway this was changed
          { complexClaim.Type, complexClaim.Value},
        }
            };

            var session = new AuthUserSession
            {
                UserAuthId = token.UserId
            };
            session.ProviderOAuthAccess.Add(token);
            _session = session;
        }

        [Fact]
        public void SessionWithIdentityServerAuthTokens_CanBeSerialized()
        {
            using (JsConfig.With(dateHandler: DateHandler.ISO8601, skipDateTimeConversion: true))
            {
                var serialized = JsonSerializer.SerializeToString(_session);
                Debug.WriteLine(serialized.IndentJson());
                serialized.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void SessionWithIdentityServerAuthTokens_CanBeDeserialized()
        {
            using (JsConfig.With(dateHandler: DateHandler.ISO8601, skipDateTimeConversion: true))
            {
                var serialized = JsonSerializer.SerializeToString(_session);
                Debug.WriteLine(serialized.IndentJson());
                var deserialized = JsonSerializer.DeserializeFromString<IAuthSession>(serialized);
                deserialized.ShouldBeEquivalentTo(_session);

                var tokens = deserialized.ProviderOAuthAccess.Single() as IdentityServerAuthTokens;
                tokens.Claims.Should().HaveCount(3);
            }
        }

        [Fact]
        public void SessionWithIdentityServerAuthTokens_CanBeRoundtripSerialized()
        {
            using (JsConfig.With(dateHandler: DateHandler.ISO8601, skipDateTimeConversion: true))
            {
                var serialized = JsonSerializer.SerializeToString(_session);
                Debug.WriteLine(serialized.IndentJson());
                var deserialized = JsonSerializer.DeserializeFromString<IAuthSession>(serialized);
                var serialized2 = JsonSerializer.SerializeToString(deserialized);
                serialized2.Should().Be(serialized);
            }
        }
    }
}
