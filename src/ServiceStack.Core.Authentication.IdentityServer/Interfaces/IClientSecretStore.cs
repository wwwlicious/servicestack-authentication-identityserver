
namespace ServiceStack.Core.Authentication.IdentityServer.Interfaces
{
    using System.Threading.Tasks;

    public interface IClientSecretStore
    {
        Task<string> GetSecretAsync(string clientId);
    }
}
