using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using SSOAuthAPI.Data;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Utilities;
using SSOAuthAPI.Utilities.Constants;
using System.Reflection;
using System.Security.Claims;

namespace SSOAuthAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly OpenIddictValidationService _openIddictValidationService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IOpenIddictTokenManager _tokenManager;

        public TokenService(OpenIddictValidationService openIddictValidationService, ApplicationDbContext dbContext, IOpenIddictTokenManager tokenManager)
        {
            _openIddictValidationService = openIddictValidationService;
            _dbContext = dbContext;
            _tokenManager = tokenManager;
        }

        public async Task<ClaimsPrincipal?> ValidateToken(string accessToken, string applicationId)
        {
            ClaimsPrincipal principal;

            try
            {

                principal = await _openIddictValidationService.ValidateAccessTokenAsync(accessToken);

                var tokenObj = await _tokenManager.FindByIdAsync(
                        principal.FindFirstValue(OpenIddictConstants.Claims.Private.TokenId)!
                    )!;

                OpenIddictTokenDescriptor descriptor = new();
                await _tokenManager.PopulateAsync(descriptor, tokenObj!);

                var token = descriptor;

                if (token.ApplicationId != applicationId)
                    return null;

                if (token.Status == "revoked")
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }

            PreserveApplicationClaims((principal.Identity as ClaimsIdentity)!);

            return principal;
        }

        public async Task RevokeTokens(ClaimsPrincipal claimsPrincipal)
        {
            var authid = claimsPrincipal.FindFirstValue(OpenIddictConstants.Claims.Private.AuthorizationId);

            if (authid is null)
            {
                var tokenId = claimsPrincipal.FindFirstValue(OpenIddictConstants.Claims.Private.TokenId);

                if (tokenId is null)
                {
                    throw new Exception("Invalid Token");
                }

                var token = await _tokenManager.FindByIdAsync(tokenId);


                await _tokenManager.TryRevokeAsync(token);
            }
            else
            {
                await RevokeTokensByAuthorizationIdAsync(authid);
            }
        }

        public async Task RevokeTokensByAuthorizationIdAsync(string? authid)
        {
            await _tokenManager.RevokeByAuthorizationIdAsync(authid);
        }

        public async Task RevokeTokensByAuthorizationIdAsync(IEnumerable<string> authids)
        {
            // Revoke Tokens
            await _dbContext.Tokens.Where(x => authids.Contains(x.Authorization!.Id))
                .ExecuteUpdateAsync(x => x.SetProperty(x => x.Status, OpenIddictConstants.Statuses.Revoked));

            // Revoke authorizations
            await _dbContext.Authorizations.Where(a => authids.Contains(a.Id))
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.Status, OpenIddictConstants.Statuses.Revoked));
        }

        private void PreserveApplicationClaims(ClaimsIdentity identity)
        {
            // Get consts
            var props = typeof(CustomClaimTypes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(x => x.GetValue(null)?.ToString())
                .Concat([
                    OpenIddictConstants.Claims.Email,
                    OpenIddictConstants.Claims.Role,
                ]);

            var claimsToPreseve = props.Concat([OpenIddictConstants.Scopes.Email]);

            var claimsOnIdentity = identity.Claims.ToListedDictionary(x => x.Type);

            foreach (var claimToPreserve in claimsToPreseve)
            {
                if (claimsOnIdentity.ContainsKey(claimToPreserve))
                {
                    claimsOnIdentity[claimToPreserve].ForEach(claim =>
                        claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken
                    ));
                }
            }
        }
    }
}
