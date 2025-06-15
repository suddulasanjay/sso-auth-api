using Microsoft.EntityFrameworkCore;
using SSOAuthAPI.Data;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities.Constants;

namespace SSOAuthAPI.Services
{
    public class UserClientService : IUserClientService
    {
        private readonly ApplicationDbContext _db;

        public UserClientService(ApplicationDbContext context)
        {
            _db = context;
        }

        public async Task<bool> ExistsAsync(int userId, string appId)
        {
            return await _db.UserClients.AnyAsync(uc => uc.UserId == userId && uc.AppId == appId && uc.Status == CommonStatus.Enabled);
        }

        public async Task AddUserClientAsync(UserClient userClient)
        {
            userClient.Status = CommonStatus.Enabled;
            _db.UserClients.Add(userClient);
            await _db.SaveChangesAsync();
        }

        public async Task<UserClient?> GetAsync(int userId, string appId)
        {
            return await _db.UserClients.Include(uc => uc.App).Include(uc => uc.User).FirstOrDefaultAsync(uc => uc.UserId == userId && uc.AppId == appId && uc.Status == CommonStatus.Enabled);
        }

        public async Task<bool> CanUserAccessClient(int userId, string appId, string requestedScope)
        {
            var entry = await _db.UserClients.AsNoTracking().FirstOrDefaultAsync(uc => uc.UserId == userId && uc.AppId == appId && uc.Status == CommonStatus.Enabled);

            if (entry is null)
                return false;

            if (string.IsNullOrWhiteSpace(entry.Scope))
                return string.IsNullOrWhiteSpace(requestedScope);

            var storedScopes = entry.Scope.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var requestedScopes = requestedScope.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            return requestedScopes.All(scope => storedScopes.Contains(scope));
        }

        public async Task UpdateScopeIfNeeded(int userId, string appId, string requestedScope)
        {
            if (string.IsNullOrWhiteSpace(requestedScope))
                return;

            var entry = await _db.UserClients
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.AppId == appId);

            if (entry == null)
                return;

            var existing = (entry.Scope ?? "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var incoming = requestedScope.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (incoming.Any(scope => !existing.Contains(scope)))
            {
                entry.Scope = string.Join(" ", existing.Union(incoming));
                await _db.SaveChangesAsync();
            }
        }
    }
}
