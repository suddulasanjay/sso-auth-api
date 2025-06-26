using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SSOAuthAPI.Data;
using SSOAuthAPI.Data.Entities;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities.Constants;

namespace SSOAuthAPI.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _dbContext;

        public SessionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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

        public bool CheckMaxAgeForAuthorization(Session session, OpenIddictApplicationDescriptor application)
        {
            var maxAgeSettings = application.Settings.GetValueOrDefault("max_age");
            if (int.TryParse(maxAgeSettings, out int maxAge))
            {
                if (maxAge <= 0)
                    return true;

                if (DateTime.UtcNow < session.LastAuthenticated.AddSeconds(maxAge))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> CheckMaxAgeForAuthorization(Guid sessionId, OpenIddictApplicationDescriptor application)
        {
            var session = await GetSessionById(sessionId);
            return session != null && CheckMaxAgeForAuthorization(session, application);
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
