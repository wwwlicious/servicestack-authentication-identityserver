namespace ServiceStack.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;

    public interface IDocumentDiscoveryClient
    {
        Task<DocumentDiscoveryResult> GetAsync(string endPoint);
    }
}