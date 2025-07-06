using Microsoft.EntityFrameworkCore;
using SSOAuthAPI.Data;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Services.Jobs;
using SSOAuthAPI.Utilities.Constants;

namespace SSOAuthAPI.Services
{
    public class SessionService : ISessionService
    {
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IOpenIddictCleanupService _openIddictCleanupService;

        public SessionService(ITokenService tokenService, ApplicationDbContext dbContext, IOpenIddictCleanupService openIddictCleanupService)
        {
            _tokenService = tokenService;
            _dbContext = dbContext;
            _openIddictCleanupService = openIddictCleanupService;
        }

        public async Task<Guid> BeginNewUserSession(int userId)
        {
            await this.LogoutAllExistingSessions(userId);

            var sessionId = Guid.NewGuid();

            var session = new Session
            {
                Id = sessionId,
                UserId = userId,
                LastAccessTime = DateTime.UtcNow,
                Status = CommonStatus.Enabled,
                CreatedTime = DateTime.UtcNow,
                ModifiedTime = DateTime.UtcNow,
                LastAuthenticated = DateTime.UtcNow,
            };

            _dbContext.Sessions.Add(session);

            await _dbContext.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _openIddictCleanupService.CleanupAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            });

            return sessionId;
        }

        public async Task<Session?> GetSessionById(Guid sessionId)
        {
            return await _dbContext.Sessions.FindAsync(sessionId);
        }

        public async Task<bool> ValidateSession(string? sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return false;

            if (Guid.TryParse(sessionId, out var sessionGuid))
            {
                var session = await _dbContext.Sessions
                    .Where(x => x.Id == sessionGuid && x.Status == CommonStatus.Enabled)
                    .FirstOrDefaultAsync();

                if (session is null)
                    return false;

                session.LastAccessTime = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> EndSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return false;

            if (Guid.TryParse(sessionId, out var sessionGuid))
            {
                var session = await _dbContext.Sessions
                    .Where(x => x.Id == sessionGuid && x.Status == CommonStatus.Enabled)
                    .FirstOrDefaultAsync();

                if (session == null) return false;

                if (session.AuthorizationIds is not null)
                {

                    await _tokenService.RevokeTokensByAuthorizationIdAsync(session.AuthorizationIds);
                }

                session.Status = CommonStatus.Disabled;
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task AddAuthorizationToSession(string? authId, string? sessionId)
        {
            if (Guid.TryParse(sessionId, out var sessionGuid))
            {
                var session = await _dbContext.Sessions.FirstOrDefaultAsync(x => x.Id == sessionGuid);
                if (session != null)
                {
                    var list = session.AuthorizationIds ?? new();
                    list.Add(authId!);
                    session.AuthorizationIds = list;
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task Reauthenticate(Session session)
        {
            session.LastAuthenticated = DateTime.UtcNow;
            _dbContext.Update(session);
            await _dbContext.SaveChangesAsync();
        }

        public async Task LogoutAllExistingSessions(int userId)
        {
            var sessions = await _dbContext.Sessions
                .Where(x => x.UserId == userId && x.Status == CommonStatus.Enabled)
                .ExecuteUpdateAsync(x => x.SetProperty(x => x.Status, CommonStatus.Deleted));
        }
    }
}
