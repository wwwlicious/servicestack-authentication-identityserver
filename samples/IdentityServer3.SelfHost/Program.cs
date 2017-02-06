// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer3.SelfHost
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using Contrib.ServiceStack;
    using Core;
    using Core.Configuration;
    using Core.Models;
    using Core.Services;
    using Core.Services.InMemory;
    using Microsoft.Owin.Hosting;
    using Owin;
    using Serilog;
    using Client = Core.Models.Client;

    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .LiterateConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            using (WebApp.Start<Startup>("http://localhost:5000"))
            {
                Console.WriteLine("Identity Server running....");
                Console.ReadLine();
            }
        }
    }

    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var factory = new IdentityServerServiceFactory()
                .UseInMemoryClients(Clients.Get())
                .UseInMemoryScopes(Scopes.Get())
                .UseInMemoryUsers(Users.Get());

            factory.CustomGrantValidators.Add(new Registration<ICustomGrantValidator, ActAsUserGrantValidator>());

            var options = new IdentityServerOptions
            {
                SigningCertificate = LoadCertificate(),
                
                Factory = factory,

                RequireSsl = false
            };
            
            app.UseIdentityServer(options);
        }

        private X509Certificate2 LoadCertificate()
        {
            using (var stream = typeof (Startup).Assembly.GetManifestResourceStream("IdentityServer3.SelfHost.idsrv3test.pfx"))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return new X509Certificate2(bytes, "idsrv3test");
            }
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

                    Flow = Flows.Hybrid,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
                    },

                    AllowAccessToAllScopes = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5001/auth/IdentityServer"
                    },

                    RequireConsent = false
                },

                // Service Web Client
                new Client
                {
                    ClientName = "ServiceAuthProvider.ServiceStack.SelfHost",
                    ClientId = "ServiceAuthProvider.ServiceStack.SelfHost",
                    Enabled = true,

                    AccessTokenType = AccessTokenType.Jwt,

                    Flow = Flows.Hybrid,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("26631ded-6165-4bdd-900d-182028495a8c".Sha256())
                    },

                    AllowAccessToAllScopes = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5001/auth/IdentityServer"
                    },

                    RequireConsent = false
                },

                new Client
                {
                    ClientName = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                    ClientId = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Jwt,

                    Flow = Flows.ClientCredentials,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("358bbaad-7921-4785-999e-adfbef1eb1d1".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
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

                    Flow = Flows.Hybrid,

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("99e1ae38-866c-4ff4-b9e0-dcfaeb3dbb4a".Sha256())
                    },

                    AllowAccessToAllScopes = true,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:5001/auth/IdentityServer"
                    },

                    RequireConsent = false
                },
                
                new Client
                {
                    ClientName = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    ClientId = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Jwt,

                    Flow = Flows.Custom,

                    AllowedCustomGrantTypes = new List<string>
                    {
                        ActAsUserGrantValidator.GrantTypeName
                    },

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("a9c08d7b-ffc2-49f4-99c9-ce232d9f0cf6".Sha256())
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        "ImpersonateAuthProvider.ServiceStack.Api.SelfHost"
                    }
                }
            };
        } 
    }

    class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>(StandardScopes.All)
            {                
                StandardScopes.OfflineAccess,
                new Scope
                {
                    Enabled = true,
                    Name = "UserAuthProvider.ServiceStack.SelfHost",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Subject),
                        new ScopeClaim(Constants.ClaimTypes.PreferredUserName)
                    },
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())
                    }
                },

                // ServiceAuthProvider
                new Scope
                {
                    Enabled = true,
                    Name = "ServiceAuthProvider.ServiceStack.SelfHost",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Subject),
                        new ScopeClaim(Constants.ClaimTypes.PreferredUserName)
                    },
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("26631ded-6165-4bdd-900d-182028495a8c".Sha256())
                    }
                },

                new Scope
                {
                    Enabled = true,
                    Name = "ServiceAuthProvider.ServiceStack.Api.SelfHost",
                    Type = ScopeType.Resource,

                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("358bbaad-7921-4785-999e-adfbef1eb1d1".Sha256())
                    }
                },

                // Impersonation Auth Provider
                new Scope
                {
                    Enabled = true,
                    Name = "ImpersonateAuthProvider.ServiceStack.SelfHost",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Subject),
                        new ScopeClaim(Constants.ClaimTypes.PreferredUserName)
                    },
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("99e1ae38-866c-4ff4-b9e0-dcfaeb3dbb4a".Sha256())
                    }
                },

                new Scope
                {
                    Enabled = true,
                    Name = "ImpersonateAuthProvider.ServiceStack.Api.SelfHost",
                    Type = ScopeType.Identity,

                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Role"),
                        new ScopeClaim("ImpersonateAuthProvider.ServiceStack.Api.SelfHost.Permission"),
                    },

                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("99e1ae38-866c-4ff4-b9e0-dcfaeb3dbb4a".Sha256()),
                        new Secret("a9c08d7b-ffc2-49f4-99c9-ce232d9f0cf6".Sha256())
                    }
                }
            };
        }
    }

    class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = new Guid("93797286-1c85-483a-b238-fe09cd40f210").ToString(),
                    Username = "test@test.com",
                    Password = "password123",
                    Enabled = true,
                    Claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.Email, "test@test.com"),
                        new Claim(Constants.ClaimTypes.GivenName, "Boaby"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Fyffe"),

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
