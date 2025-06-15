using Microsoft.EntityFrameworkCore;
using SSOAuthAPI.Data;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities.Constants;

namespace SSOAuthAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
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

        public async Task UpdateLoginTimeAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;
            user.LastLoginAt = DateTime.UtcNow;
            user.ModifiedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

    }
}
