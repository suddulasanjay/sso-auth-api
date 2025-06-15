using SSOAuthAPI.Data.Entities;

namespace SSOAuthAPI.Interfaces
{
    public interface IProviderUserService
    {
        Task<ProviderUser?> GetBySubjectIdAsync(string subjectId, int providerId);
        Task<bool> ExistsAsync(int providerId, int userId);
        Task AddProviderUserAsync(ProviderUser providerUser);
    }
}
