### Authenticate as a Service on behalf of a User - Quick Start

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
            AppSettings.SetImpersonationAuthProvider()                                  // Use the ImpersonationAuthProvider instead of the UserAuthProvider
                       .SetAuthRealm("http://identityserver:5000/")                     // The URL of the IdentityServer instance
                       .SetClientId("ServiceStack.SelfHost.Api")                        // The Client Identifier so that IdentityServer can identify the service
                       .SetClientSecret("F621F470-9731-4A25-80EF-67A6F7C5F4B8")         // The Client Secret so that IdentityServer can authorize the service
                       .SetScopes("openid ServiceStack.SelfHost.Api offline_access");
                       
            this.Plugins.Add(new IdentityServerAuthFeature());
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

To pre-authenticate a Service Client on the first ServiceStack instance so that it can make calls to our new second service, add the following to the IdentityServerAuthFeature constructor:
<pre>
<code>
    public class AppHost : AppSelfHostBase
    {
        public override void Configure(Container container)
        {
            AppSettings.SetUserAuthProvider()
            ...            
            this.Plugins.Add(new IdentityServerAuthFeature(defaultServiceClient: new JsonServiceClient("http://localhost:5003/"))); // The Url of the new second service
        }
    }
</code>
</pre>
If the above pre-authentication method is not used, the following must be added to the Authorization header of the http request:
<pre>
<code>
    Bearer *ACCESS_TOKEN*
</code>
</pre>
Where *ACCESS_TOKEN* is the AccessToken taken from the authenticated service stack session.  

This can be retrieved when inside a service stack service method using:
<pre>
<code>
    [Authenticate(IdentityServerAuthProvider.Name)]
    public class SomeService : Service
    {
        public object Any(RequestDTO request)
        {
            this.GetSession().GetOAuthTokens(IdentityServerAuthProvider.Name).AccessToken        
        }
    }    
</code>
</pre>
In addition, the URL of the calling service is required to confirm that the calling service is authorized to provide the above access token. This should be passed in the "Referer" http request header.


### IdentityServer Instance
Install the package to your IdentityServer Instance - what package? We don't have one yet....
<pre>
<code>
    PM> Install-Package ....whit?
</code>
</pre>

Add the following to the OWIN startup class of your IdentityServer instance:
<pre>
<code>
    public void Configuration(IAppBuilder app)
    {
        var factory = new IdentityServerServiceFactory();
        
        ...
        
        factory.CustomGrantValidators.Add(new Registration&lt;ICustomGrantValidator, ActAsUserGrantValidator&gt;());
        
        ...
    }
</code>
</pre>

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
        
        Flow = Flows.Custom,                                                // Uses the Custom flow so that the Custom Grant Validator is used
        
        AllowedCustomGrantTypes = new List&lt;string&gt;                    // The Custom Grant Validator we've added above
        {
            ActAsUserGrantValidator.GrantTypeName
        },
        
        ClientSecrets = new List&lt;Secret&gt;
        {
            new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())     // The Client Secret matching AppSettings.SetClientSecret() call
        },                                                                  // in the ServiceStack Setup

        AllowedScopes = new List&lt;Secret&gt;                              // Ensure the scope for the new client is referenced.
        {
            StandardScopes.OpenId.Name,
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
        Name = "ServiceStack.Api.SelfHost",
        Type = ScopeType.Identity,

        Claims = new List&lt;ScopeClaim&gt;
        {
            ...
        },

        ScopeSecrets = new List&lt;Secret&gt;
        {
            new Secret("F621F470-9731-4A25-80EF-67A6F7C5F4B8".Sha256())            
        }
    }
</pre>
</pre>

## What just happened?
Now when making a request to the exernal ServiceStack Instance the following should occur:

* Attempting to access a ServiceStack Service with the Authenticate attribute forces a redirect to the IdentityServer Auth Provider (added by the *IdentityServerAuthFeature()* plugin).
* The Auth Provider checks the current ServiceStack session for an *Access Token* issued by IdentityServer.
* Having yet to be issued with an Access Token, the Auth Provider needs to request an Access Token from IdentityServer.
* The Auth Provider inspects the incoming request for the AccessToken and the URL Refererral.
* The Auth Provider now uses the incoming AccessToken, the URL Referral, the *Client ID* and the *Client Secret* to request a new *Access Token* from IdentityServer using the [Token](https://identityserver.github.io/Documentation/docsv2/endpoints/token.html) endpoint.

* *Token Endpoint*
   * Checks the Service Stack Instance *Client Id* and *Client Secret* are valid.
   * Checks the Scopes requested are valid for the Client.
   * Validates the AccessToken provided
   * Validates that the URL Referrer provided is in the list of redirect urls for the Client that was awarded the AccessToken
   * The IdentityServer Instance issues the new *Access Token* for the user from the original AccessToken for the requested scopes.

* The ServiceStack Auth Provider now validates the *Access Token* with IdentityServer using the [Token Introspection](https://identityserver.github.io/Documentation/docsv2/endpoints/introspection.html) endpoint.

* *Introspection Endpoint*
    * Checks the Service Stack Instance *Client Id* and *Client Secret* are valid.
    * Checks the *Access Token* is valid.
    
* The ServiceStack Auth Provider now retrieves the Claims for the *Access Token* using the [User Info](https://identityserver.github.io/Documentation/docsv2/endpoints/userinfo.html) endpoint.

* *UserInfo Endpoint*
    * Checks the *Access Token* is valid.
    * Returns the Claims for the User that were part of the scopes that were requested when the initial Authorization took place.
    
* The ServiceStack Auth Provider populates the Service Stack Session with the Claims.