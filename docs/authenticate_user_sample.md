# UserAuthProvider.ServiceStack.SelfHost

A demo project that authenticates a [ServiceStack](https://servicestack.net/) razor-based Client App using [IdentityServer](https://identityserver.github.io/)

## Overview
This demo project bring uses the Service Stack plugin ServiceStack.Authentication.IdentityServer that provides a ServiceStack AuthProvider that authenticates a user against an IdentityServer instance

When the project starts, you should be presented with a simple ServiceStack web app with a link that redirects to a secure service in ServiceStack. When you select the link you should be redirected to the IdentityServer instance that prompts you for login details.  Login using username "test@test.com" with password "password123".  You should then be redirected back to the ServiceStack web app and have access to the secure service (with Authenticate attribute) which displays the secure message.

## Prerequisites
The demo project must be run in conjunction with project IdentityServer3.SelfHost which is the IdentityServer instance configured to authenticate the ServiceStack Instance.