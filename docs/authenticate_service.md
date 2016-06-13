## Authenticate as a Service - Quick Start
### ServiceStack Instance
Install the package to your second Service Stack Instance:
<pre>
<code>
    PM> Install-Package ServiceStack.Authentication.IdentityServer
</code>
</pre>

Add the following to your AppHost:
<pre>
<code>
    public class AppHost : AppSelfHostBase
    {
        public AppHost() : base("ServiceStack.SelfHost.Api", typeof (MyServices).Assembly) { }
        
        public override void Configure(Container container)
        {
            ...
            
            AppSettings.SetServiceAuthProvider()                                    // Use the ServiceAuthProvider instead of the UserAuthProvider
                       .SetAuthRealm("http://localhost:5000/")                      // The URL of the IdentityServer instance
                       .SetClientId("ServiceStack.SelfHost.Api")                    // The Client Identifier so that IdentityServer can identify the service
                       .SetClientSecret("6515372c-d1ab-457c-927a-13038007e977")     // The Client Secret so that IdentityServer can authorize the service
                       .SetScopes("servicescope");                                  // The Service must have a corresponding scope
                       
            this.Plugins.Add(new IdentityServerAuthFeature());
            
            ...
        }
    }
</code>
</pre>

To lock down a service so that the service has to be authenticated, again decorate it with the Authenticate Attribute.
<pre>
<code>
    public class SomeService : Service
    {
        [Authenticate(IdentityServerAuthProvider.Name)]
        public object Any()
        {
            ...
        }    
    }
</code>
</pre>

### IdentityServer Instance
Add the following Client to the Identity Server Client data store (example below is assuming IdentityServer In-Memory Clients is being used).
<pre>
<code>
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
</code>
</pre>

Add the following Scope to the Identity Server Scope data store (example below is assuming IdentityServer In-Memory Scopes is being used).
<pre>
<code>
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
</code>
</pre>

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