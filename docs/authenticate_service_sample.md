# ServiceAuthProvider.ServiceStack.Api.SelfHost

A demo project that provides a [ServiceStack](https://servicestack.net/) api instance that needs to authenticate itself as a resource with an IdentityServer instance.

## Overview
This demo project bring uses the Service Stack plugin ServiceStack.Authentication.IdentityServer that provides a ServiceStack AuthProvider that authenticates a user against an IdentityServer instance.

When the project starts, you should be presented with a simple ServiceStack web app with a link that redirects to a secure service in ServiceStack. When you select the link you should be redirected to the IdentityServer instance that prompts you for login details.  Login using username "test@test.com" with password "password123".  You should then be redirected back to the ServiceStack web app and have access to the secure service (with Authenticate attribute) which displays the secure message.  A secure form is presented on the page that when text is entered in the field, a request is sent to a second ServiceStack service instance that will need to authenticate itself with IdentityServer before being able to return a response.

## Prerequisites
The demo project must be run in conjunction with the following projects:
* IdentityServer3.SelfHost - an IdentityServer instance configured to authenticate the ServiceStack Instances.
* ServiceAuthProvider.ServiceStack.SelfHost - a ServiceStack razor-based Client App that provides the user interface to the Api project