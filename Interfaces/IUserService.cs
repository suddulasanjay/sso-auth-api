using SSOAuthAPI.Data.Entities;

namespace SSOAuthAPI.Interfaces
{
    public interface IUserService
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByIdAsync(int userId);
        Task<User> CreateUserAsync(string email, string firstname, string lastname, int providerId);
        Task UpdateLoginTimeAsync(int userId);
        Task<Guid> LoginUserWithSession(User user);
    }
}
