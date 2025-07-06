using OpenIddict.Abstractions;

namespace SSOAuthAPI.Services.Jobs
{
    public interface IOpenIddictCleanupService
    {
        Task CleanupAsync();
    }

    public class OpenIddictCleanupService : IOpenIddictCleanupService
    {
        private readonly IOpenIddictTokenManager _tokenManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;

        public OpenIddictCleanupService(
            IOpenIddictTokenManager tokenManager,
            IOpenIddictAuthorizationManager authorizationManager)
        {
            _tokenManager = tokenManager;
            _authorizationManager = authorizationManager;
        }

        public async Task CleanupAsync()
        {
            var now = DateTimeOffset.UtcNow;
            await _tokenManager.PruneAsync(now);
            await _authorizationManager.PruneAsync(now);
        }
    }
}
