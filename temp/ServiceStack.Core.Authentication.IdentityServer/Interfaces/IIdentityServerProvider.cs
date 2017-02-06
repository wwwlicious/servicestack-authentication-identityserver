namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{   
    using System.Threading.Tasks;
    using Auth;

    public interface IIdentityServerProvider : IAuthProvider
    {
        Task Init();

        IDocumentDiscoveryClient DocumentDiscoveryClient { get; }

        IIntrospectClient IntrospectionClient { get; }

        IRefreshTokenClient RefreshTokenClient { get; }

        IUserInfoClient UserInfoClient { get; }
    }
}
