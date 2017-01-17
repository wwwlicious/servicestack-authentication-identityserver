## Authenticate as a Service - Quick Start
### ServiceStack Instance
Install the package to your second Service Stack Instance:
```powershell
    PM> Install-Package ServiceStack.Authentication.IdentityServer
```

Add the following to your AppHost:
```csharp
public class AppHost : AppSelfHostBase
{
    public AppHost() : base("ServiceStack.SelfHost.Api", typeof (MyServices).Assembly) { }
        
        public override void Configure(Container container)
        {
            ...                          

            this.Plugins.Add(new IdentityServerAuthFeature 
            {
                AuthProviderType = IdentityServerAuthProviderType.ServiceProvider,  // Use the ServiceAuthProvider instead of the UserAuthProvider
                AuthRealm = "http://localhost:5000/",                               // The URL of the IdentityServer instance
                ClientId = "ServiceStack.SelfHost.Api",                             // The Client Identifier so that IdentityServer can identify the service
                ClientSecret = "6515372c-d1ab-457c-927a-13038007e977",              // The Client Secret so that IdentityServer can authorize the service
                Scopes = "servicescope"                                             // The Service must have a corresponding scope
            });
            
            ...
        }
}
```

To lock down a service so that the service has to be authenticated, again decorate it with the Authenticate Attribute.
```csharp
public class SomeService : Service
{
    [Authenticate(IdentityServerAuthProvider.Name)]
    public object Any()
    {
        ...
    }    
}
```

### IdentityServer3 Instance
Add the following Client to the Identity Server Client data store (example below is assuming IdentityServer In-Memory Clients is being used).
```csharp
new Client
{
    ClientName = "ServiceStack.SelfHost.Api",
    ClientId = "ServiceStack.SelfHost.Api",                             // The Client Identifier matching the AppSettings.SetClientId() call
                                                                            // in the ServiceStack AppHost Configure() method above        
    Enabled = true,
        
    AccessTokenType = AccessTokenType.Jwt,                              // The AccessToken encryption type
        
    Flow = Flows.ClientCredentials,                                     // Uses the Client Credentials flow

    ClientSecrets = new List&lt;Secret&gt;
    {
        new Secret("6515372c-d1ab-457c-927a-13038007e977".Sha256())     // The Client Secret matching AppSettings.SetClientSecret() call
    },                                                                  // in the ServiceStack Setup

    AllowedScopes = new List&lt;Secret&gt;                              // Ensure the scope for the new client is referenced.
    {
        "ServiceStack.Api.SelfHost"
    }
}
```

Add the following Scope to the Identity Server Scope data store (example below is assuming IdentityServer In-Memory Scopes is being used).
```csharp
new Scope
{
    Enabled = true,
    Name = "ServiceStack.Api.SelfHost",                                 // A Scope that matches the Client Id
    Type = ScopeType.Identity,

    ScopeSecrets = new List&lt;Secret&gt;                               // The Client Secret matching AppSettings.SetClientSecret() call
    {                                                                   // in the ServiceStack Setup
        new Secret("6515372c-d1ab-457c-927a-13038007e977".Sha256())
    }
}
```

### IdentityServer4 Instance
Add the following Client to the Identity Server Client data store (example below is assuming IdentityServer In-Memory Clients is being used).
new Client
{
    ClientName = "ServiceStack.SelfHost.Api",
    ClientId = "ServiceStack.SelfHost.Api",                             // The Client Identifier matching the AppSettings.SetClientId() call
                                                                        // in the ServiceStack AppHost Configure() method above        
    Enabled = true,
        
    AccessTokenType = AccessTokenType.Jwt,                              // The AccessToken encryption type
        
    AllowedGrantTypes = GrantTypes.ClientCredentials,                   // Uses the Client Credentials flow

    ClientSecrets = new List&lt;Secret&gt;
    {
        new Secret("6515372c-d1ab-457c-927a-13038007e977".Sha256())     // The Client Secret matching AppSettings.SetClientSecret() call
    },                                                                  // in the ServiceStack Setup

    AllowedScopes = new List&lt;Secret&gt;                              // Ensure the scope for the new client is referenced.
    {
        "ServiceStack.Api.SelfHost"
    }
}
```

Add the following ApiResource to the Identity Server Api Resource data store (example below is assuming IdentityServer In-Memory Api Resource is being used).
```csharp
new ApiResource
{
    Name = "ServiceStack.SelfHost.Api",
    Enabled = true,
    ApiSecrets = new List<Secret>
    {
        new Secret("6515372c-d1ab-457c-927a-13038007e977".Sha256())
    },
    Scopes = new List<Scope>
    {
        new Scope("ServiceStack.SelfHost.Api")
    }
}

## What just happened?
Now when making a request to the exernal ServiceStack Instance the following should occur:

* Attempting to access a ServiceStack Service with the Authenticate attribute forces a redirect to the IdentityServer Auth Provider (added by the *IdentityServerAuthFeature()* plugin).
* The Auth Provider checks the current ServiceStack session for an *Access Token* issued by IdentityServer.
* Having yet to be issued with an Access Token, the Auth Provider needs to request an Access Token from IdentityServer
* The ServiceStack Auth Provider now uses the *Client ID* and the *Client Secret* to request an *Access Token* from IdentityServer using the [Token](https://identityserver.github.io/Documentation/docsv2/endpoints/token.html) endpoint.

* *Token Endpoint*
   * Checks the Service Stack Instance *Client Id* and *Client Secret* are valid.
   * The IdentityServer Instance issues the *Access Token*.

* The ServiceStack Auth Provider now validates the *Access Token* with IdentityServer using the [Token Introspection](https://identityserver.github.io/Documentation/docsv2/endpoints/introspection.html) endpoint.

* *Introspection Endpoint*
    * Checks the Service Stack Instance *Client Id* and *Client Secret* are valid.
    * Checks the *Access Token* is valid.