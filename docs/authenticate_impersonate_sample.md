# ImpersonateAuthProvider.ServiceStack.Api.SelfHost

A demo project that provides a [ServiceStack](https://servicestack.net/) api instance that needs to impersonate a user that has logged into another application and is trying to access a secured api service requiring specific a specific role and specific permission.

## Overview
This demo project bring uses the following plugins:
* ServiceStack.Authentication.IdentityServer - a ServiceStack plugin that registers an AuthProvider that authenticates a user against an IdentityServer instance
* IdentityServer3.Contrib.ServiceStack - an IdentityServer3 plugin that provides a custom token authentication endpoint that allows a ServiceStack instance to retrieve credentials from IdentityServer that impersonate a calling user.

When the project starts, you should be presented with a simple ServiceStack web app with a link that redirects to a secure service in ServiceStack. When you select the link you should be redirected to the IdentityServer instance that prompts you for login details.  Login using username "test@test.com" with password "password123".  You should then be redirected back to the ServiceStack web app and have access to the secure service (with Authenticate attribute) which displays the secure message.  A secure form is presented on the page that when text is entered in the field, a request is sent to a second ServiceStack service instance that will need to impersonate the calling user.

## Prerequisites
The demo project must be run in conjunction with the following projects:
* IdentityServer3.SelfHost - an IdentityServer instance configured to authenticate the ServiceStack Instances.
* ImpersonateAuthProvider.ServiceStack.SelfHost - a ServiceStack razor-based Client App that provides the user interface and passes the Access Token that will be used to impersonate the user to the Api project