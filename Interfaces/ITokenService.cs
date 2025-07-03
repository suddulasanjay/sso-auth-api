using System.Security.Claims;

namespace SSOAuthAPI.Interfaces
{
    public interface ITokenService
    {
        Task RevokeTokens(ClaimsPrincipal claimsPrincipal);
        Task RevokeTokensByAuthorizationIdAsync(string? authid);
        Task RevokeTokensByAuthorizationIdAsync(IEnumerable<string> authids);
        public Task<ClaimsPrincipal?> ValidateToken(string accessToken, string applicationId);
    }
}
