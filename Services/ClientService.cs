using OpenIddict.Abstractions;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Application;
using SSOAuthAPI.Utilities;

namespace SSOAuthAPI.Services
{
    public class ClientService : IClientService
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        public ClientService(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager;
        }

        public async Task<ClientAppResponse> RegisterClientAsync(ClientDto client)
        {
            string secret = client.ClientSecret ?? MiscUtility.RandomString(15);
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                DisplayName = client.DisplayName,
                ClientSecret = secret,
                Permissions =   {
                                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                                    OpenIddictConstants.Permissions.Endpoints.Token,
                                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                                    OpenIddictConstants.Permissions.Scopes.Email,
                                    OpenIddictConstants.Permissions.Scopes.Profile
                                }
            };

            foreach (var uri in client.RedirectUris)
            {
                descriptor.RedirectUris.Add(new Uri(uri));
            }

            foreach (var uri in client.PostLogoutRedirectUris)
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
            }

            if (client.EnableClientCredentials)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            }

            foreach (var permission in client.Permissions)
                descriptor.Permissions.Add(permission);

            await _applicationManager.CreateAsync(descriptor);

            return new ClientAppResponse
            {
                ClientSecret = secret,
                ClientId = client.ClientId,
            };
        }

        public async Task<ClientDto?> GetClientAsync(string clientId)
        {
            var app = await _applicationManager.FindByClientIdAsync(clientId);
            if (app is null) return null;

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(app, descriptor);

            var client = new ClientDto
            {
                ClientId = descriptor.ClientId!,
                DisplayName = descriptor.DisplayName!,
                ClientSecret = descriptor.ClientSecret,
                PostLogoutRedirectUris = descriptor.PostLogoutRedirectUris.Select(uri => uri.ToString()).ToList(),
                RedirectUris = descriptor.RedirectUris.Select(uri => uri.ToString()).ToList(),
                Permissions = descriptor.Permissions.ToList(),
                EnableClientCredentials = descriptor.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials)
            };

            return client;
        }
    }
}
