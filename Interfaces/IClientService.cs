using SSOAuthAPI.Models.Application;

namespace SSOAuthAPI.Interfaces
{
    public interface IClientService
    {
        Task<ClientAppResponse> RegisterClientAsync(ClientDto dto);
        Task<ClientDto?> GetClientAsync(string clientId);
        Task<ClientDto?> EditClientAsync(ClientDto dto);
    }
}
