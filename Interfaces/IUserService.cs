using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Models.Security;

namespace SSOAuthAPI.Interfaces
{
    public interface IUserService
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByIdAsync(int userId);
        Task<User> CreateUserAsync(string email, string firstname, string lastname, int providerId);
        Task<User> CreateUserWithSignUp(SignupRequestDto signup);
        Task UpdateLoginTimeAsync(int userId);
        Task<Guid> LoginUserWithSession(User user);
        string? CheckLockAndVerifyPassword(User? user, string password);
    }
}
