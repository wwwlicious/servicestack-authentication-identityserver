// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer4.SelfHost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using Contrib.ServiceStack;
    using IdentityModel;
    using IdentityServer4.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Test;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .WriteTo
               .LiterateConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}")
               .CreateLogger();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5000")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }

    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddIdentityServer()
                    .AddTemporarySigningCredential()
                    .AddInMemoryIdentityResources(IdentityResources.Get())
                    .AddInMemoryApiResources(ApiResources.Get())
                    .AddInMemoryClients(Clients.Get())
                    .AddTestUsers(Users.Get())
                    .AddExtensionGrantValidator<ActAsUserGrantValidator>();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            app.UseDeveloperExceptionPage();
            app.UseIdentityServer();
            
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }

    class Clients
    {
        public static List<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "UserAuthProvider.ServiceStack.SelfHost",
                    ClientId = "UserAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,

                    AccessTokenType = AccessTokenType.Jwt,
                    
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
                    },                    

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5001/auth/IdentityServer"
                    },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,

                        "UserAuthProvider.ServiceStack.SelfHost"
                    },

                    AllowOfflineAccess = true,

                    RequireConsent = false
                },

                // Service Web Client
                new Client
                {
                    ClientName = "ServiceAuthProvider.ServiceStack.SelfHost",
                    ClientId = "ServiceAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,

                    AccessTokenType = AccessTokenType.Jwt,

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("26631ded-6165-4bdd-900d-182028495a8c".Sha256())
                    },

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5001/auth/IdentityServer"
                    },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,

                        "ServiceAuthProvider.ServiceStack.SelfHost"
                    },

                    AllowOfflineAccess = true,

                    RequireConsent = false
                },

                new Client
                {
                    ClientName = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                    ClientId = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Jwt,

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("358bbaad-7921-4785-999e-adfbef1eb1d1".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "ServiceAuthProvider.ServiceStack.Api.SelfHost"
                    }
                },

                // Impersonate Web Client
                new Client
                {
                    ClientName = "ImpersonateAuthProvider.ServiceStack.SelfHost",
                    ClientId = "ImpersonateAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,

                    AccessTokenType = AccessTokenType.Jwt,

                    AllowedGrantTypes = GrantTypes.Hybrid,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("99e1ae38-866c-4ff4-b9e0-dcfaeb3dbb4a".Sha256())
                    },

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5001/auth/IdentityServer"
                    },

                    RequireConsent = false,

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,

                        "ImpersonateAuthProvider.ServiceStack.SelfHost"
                    },

                    AllowOfflineAccess = true
                },

                new Client
                {
                    ClientName = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    ClientId = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Jwt,

                    AllowedGrantTypes = GrantTypes.List(ActAsUserGrantValidator.GrantTypeName),

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("a9c08d7b-ffc2-49f4-99c9-ce232d9f0cf6".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "ImpersonateAuthProvider.ServiceStack.Api.SelfHost"
                    }
                }
            };
        }
    }

    class IdentityResources
    {
        public static IList<IdentityResource> Get()
        {
            return new List<IdentityResource>
            {
                new IdentityServer4.Models.IdentityResources.OpenId(),
                new IdentityServer4.Models.IdentityResources.Profile(),
                new IdentityServer4.Models.IdentityResources.Email(),
                           
                new IdentityResource
                {
                    Name = "UserAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Subject,
                        JwtClaimTypes.PreferredUserName
                    }
                },

                new IdentityResource
                {
                    Name = "ServiceAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Subject,
                        JwtClaimTypes.PreferredUserName
                    }
                },               

                new IdentityResource
                {
                    Name = "ImpersonateAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Subject,
                        JwtClaimTypes.PreferredUserName
                    }
                },

                new IdentityResource
                {
                    Name = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    UserClaims = new List<string>
                    {
                        "ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Role",
                        "ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Permission"
                    }
                }
            };
        }
    }

    class ApiResources
    {
        public static IList<ApiResource> Get()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("358bbaad-7921-4785-999e-adfbef1eb1d1".Sha256())
                    },
                    Scopes = new List<Scope>
                    {
                        new Scope("ServiceAuthProvider.ServiceStack.Api.SelfHost")
                    }
                },

                new ApiResource
                {
                    Name = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("a9c08d7b-ffc2-49f4-99c9-ce232d9f0cf6".Sha256())
                    },
                    Scopes = new List<Scope>
                    {
                        new Scope("ImpersonateAuthProvider.ServiceStack.Api.SelfHost")
                        {
                            UserClaims = new List<string>
                            {
                                "ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Role",
                                "ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Permission"
                            }
                        }
                    }
                }
            };
        }
    }

    class Users
    {
        public static List<TestUser> Get()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = new Guid("93797286-1c85-483a-b238-fe09cd40f210").ToString(),
                    Username = "test@test.com",
                    Password = "password123",
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Email, "test@test.com"),
                        new Claim(JwtClaimTypes.GivenName, "Boaby"),
                        new Claim(JwtClaimTypes.FamilyName, "Fyffe"),

                        new Claim("ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Role", "Manager"),
                        new Claim("ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Role", "Buyer"),

                        new Claim("ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Permission", "CanSeeAllOrders"),
                        new Claim("ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Permission", "CanBuyStuff")
                    }
                }
            };
        }
    }
}
