using OpenIddict.Abstractions;
using SSOAuthAPI.Data.Entities;
namespace SSOAuthAPI.Interfaces
{
    public interface ISessionService
    {
        Task<Session?> GetSessionById(Guid sessionId);
        Task<bool> ValidateSession(string? sessionId);
        Task<bool> CheckMaxAgeForAuthorization(Guid sessionId, OpenIddictApplicationDescriptor application);
        bool CheckMaxAgeForAuthorization(Session session, OpenIddictApplicationDescriptor application);
        Task<Guid> BeginNewUserSession(int userId);
        Task<bool> EndSession(string sessionId);
        Task AddAuthorizationToSession(string? authId, string? sessionId);
        Task Reauthenticate(Session session);
        Task LogoutAllExistingSessions(int userId);
    }
}
