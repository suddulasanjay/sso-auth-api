using Microsoft.EntityFrameworkCore;
using SSOAuthAPI.Data;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities.Constants;

namespace SSOAuthAPI.Services
{
    public class ProviderUserService : IProviderUserService
    {
        private readonly ApplicationDbContext _db;

        public ProviderUserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ProviderUser?> GetBySubjectIdAsync(string subjectId, int providerId)
        {
            return await _db.ProviderUsers.Include(pu => pu.User).FirstOrDefaultAsync(pu => pu.SubjectId == subjectId && pu.ProviderId == providerId && pu.Status == CommonStatus.Enabled);
        }

        public async Task<bool> ExistsAsync(int providerId, int userId)
        {
            return await _db.ProviderUsers.AnyAsync(pu => pu.ProviderId == providerId && pu.UserId == userId && pu.Status == CommonStatus.Enabled);
        }

        public async Task AddProviderUserAsync(ProviderUser providerUser)
        {
            _db.ProviderUsers.Add(providerUser);
            await _db.SaveChangesAsync();
        }
    }
}
