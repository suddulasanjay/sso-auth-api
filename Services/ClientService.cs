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
            if (string.IsNullOrEmpty(client.DisplayName))
            {
                throw new Exception("Display Name is Required");
            }

            string secret = client.ClientSecret ?? MiscUtility.RandomString(15);
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                DisplayName = client.DisplayName,
                ClientSecret = secret
            };

            ApplyClientSettings(descriptor, client);

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
            OpenIddictApplicationDescriptor descriptor = new();
            await _applicationManager.PopulateAsync(descriptor, app);
            var client = new ClientDto
            {
                ClientId = descriptor.ClientId!,
                ClientSecret = descriptor.ClientSecret,
                DisplayName = descriptor.DisplayName!,
                RedirectUris = descriptor.RedirectUris.Select(x => x.ToString()).ToList(),
                PostLogoutRedirectUris = descriptor.PostLogoutRedirectUris.Select(x => x.ToString()).ToList(),
                Permissions = descriptor.Permissions.ToList(),
                EnableClientCredentials = descriptor.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials),
                CanGrantRefreshTokens = descriptor.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken)
            };

            return client;
        }

        public async Task<ClientDto?> EditClientAsync(ClientDto client)
        {
            var app = await _applicationManager.FindByClientIdAsync(client.ClientId);
            if (app is null) return null;

            // Build a descriptor from the existing app
            OpenIddictApplicationDescriptor descriptor = new();
            await _applicationManager.PopulateAsync(descriptor, app);

            // Update descriptor fields
            if (!string.IsNullOrWhiteSpace(client.DisplayName))
            {
                descriptor.DisplayName = client.DisplayName;
            }

            ApplyClientSettings(descriptor, client);

            // Update the application
            await _applicationManager.UpdateAsync(app, descriptor);

            var updatedClient = new ClientDto
            {
                ClientId = descriptor.ClientId!,
                ClientSecret = descriptor.ClientSecret,
                DisplayName = descriptor.DisplayName!,
                RedirectUris = descriptor.RedirectUris.Select(x => x.ToString()).ToList(),
                PostLogoutRedirectUris = descriptor.PostLogoutRedirectUris.Select(x => x.ToString()).ToList(),
                Permissions = descriptor.Permissions.ToList(),
                EnableClientCredentials = descriptor.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials),
                CanGrantRefreshTokens = descriptor.Permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken)
            };


            return updatedClient;
        }

        private void ApplyClientSettings(OpenIddictApplicationDescriptor descriptor, ClientDto client)
        {
            descriptor.Permissions.Clear();
            descriptor.RedirectUris.Clear();
            descriptor.PostLogoutRedirectUris.Clear();

            //Required base permissions
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Email);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);

            // Conditional permissions
            if (client.EnableClientCredentials)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (client.CanGrantRefreshTokens)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            // Custom permissions
            foreach (var permission in client.Permissions)
            {
                descriptor.Permissions.Add(permission);
            }

            // Redirect URIs
            foreach (var uri in client.RedirectUris)
            {
                descriptor.RedirectUris.Add(new Uri(uri));
            }

            // Post-Logout URIs
            foreach (var uri in client.PostLogoutRedirectUris)
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
            }
        }

    }
}
