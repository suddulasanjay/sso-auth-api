using SSOAuthAPI.Data.Entities;

namespace SSOAuthAPI.Interfaces
{
    public interface IUserClientService
    {
        Task<bool> ExistsAsync(int userId, string appId);
        Task AddUserClientAsync(UserClient userClient);
        Task<UserClient?> GetAsync(int userId, string appId);
        Task<bool> CanUserAccessClient(int userId, string appId, string requestedScope);
    }

}
