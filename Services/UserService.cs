using CryptoHelper;
using Microsoft.EntityFrameworkCore;
using SSOAuthAPI.Data;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Security;
using SSOAuthAPI.Utilities.Constants;

namespace SSOAuthAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ISessionService _sessionService;
        private readonly ApplicationDbContext _db;
        public UserService(ISessionService sessionService, ApplicationDbContext db)
        {
            _sessionService = sessionService;
            _db = db;
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Status == CommonStatus.Enabled);
        }

        public async Task<User?> FindByIdAsync(int userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Status == CommonStatus.Enabled);
        }

        public async Task<User> CreateUserAsync(string email, string firstname, string lastname, int providerId)
        {
            var user = new User
            {
                Email = email,
                FirstName = firstname,
                LastName = lastname,
                ProviderId = providerId,
                Verified = true,
                VerifiedAt = DateTime.UtcNow,
                Status = CommonStatus.Enabled,
                CreatedTime = DateTime.UtcNow,
                ModifiedTime = DateTime.UtcNow,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User> CreateUserWithSignUp(SignupRequestDto signup)
        {
            var user = new User
            {
                Email = signup.Email,
                FirstName = signup.FirstName,
                LastName = signup.LastName,
                ProviderId = (int)IdentityProvider.Custom,
                PasswordHash = string.IsNullOrEmpty(signup.Password) ? "" : Crypto.HashPassword(signup.Password),
                PasswordModifiedTime = string.IsNullOrEmpty(signup.Password) ? null : DateTime.UtcNow,
                //verification process is pending
                Verified = true,
                VerifiedAt = DateTime.UtcNow,
                Status = CommonStatus.Enabled,
                CreatedTime = DateTime.UtcNow,
                ModifiedTime = DateTime.UtcNow,
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateLoginTimeAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;
            user.LastLoginAt = DateTime.UtcNow;
            user.ModifiedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<Guid> LoginUserWithSession(User user)
        {
            var sessionId = await _sessionService.BeginNewUserSession(user.Id);

            await UpdateLoginTimeAsync(user.Id);

            return sessionId;
        }

        public string? CheckLockAndVerifyPassword(User? user, string password)
        {

            if (user is null || !user.Verified)
            {
                return "User does not exist";
            }

            if (user.Status == CommonStatus.Disabled)
            {
                return "This account is disabled";
            }

            //locking mechanism implementation

            if (!Crypto.VerifyHashedPassword(user.PasswordHash, password))
            {
                string message;

                //logic failed login attempts
                message = "Incorrect Password";

                _db.SaveChanges();

                return message;
            }

            user.LastLoginAt = DateTime.UtcNow;
            _db.SaveChanges();

            return null;
        }
    }
}
