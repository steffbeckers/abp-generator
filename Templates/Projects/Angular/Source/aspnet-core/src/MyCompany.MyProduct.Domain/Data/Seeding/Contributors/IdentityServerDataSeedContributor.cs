using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.IdentityServer.ApiResources;
using Volo.Abp.IdentityServer.ApiScopes;
using Volo.Abp.IdentityServer.Clients;
using Volo.Abp.IdentityServer.IdentityResources;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using ApiResource = Volo.Abp.IdentityServer.ApiResources.ApiResource;
using ApiScope = Volo.Abp.IdentityServer.ApiScopes.ApiScope;
using Client = Volo.Abp.IdentityServer.Clients.Client;

namespace MyCompany.MyProduct.Data.Seeding.Contributors
{
    public class IdentityServerDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IApiResourceRepository _apiResourceRepository;
        private readonly IApiScopeRepository _apiScopeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IConfiguration _configuration;
        private readonly ICurrentTenant _currentTenant;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IIdentityResourceDataSeeder _identityResourceDataSeeder;
        private readonly IPermissionDataSeeder _permissionDataSeeder;

        public IdentityServerDataSeedContributor(
            IApiResourceRepository apiResourceRepository,
            IApiScopeRepository apiScopeRepository,
            IClientRepository clientRepository,
            IConfiguration configuration,
            ICurrentTenant currentTenant,
            IGuidGenerator guidGenerator,
            IIdentityResourceDataSeeder identityResourceDataSeeder,
            IPermissionDataSeeder permissionDataSeeder)
        {
            _apiResourceRepository = apiResourceRepository;
            _apiScopeRepository = apiScopeRepository;
            _clientRepository = clientRepository;
            _configuration = configuration;
            _currentTenant = currentTenant;
            _guidGenerator = guidGenerator;
            _identityResourceDataSeeder = identityResourceDataSeeder;
            _permissionDataSeeder = permissionDataSeeder;
        }

        private async Task<ApiResource> CreateApiResourceAsync(string name, IEnumerable<string> claims)
        {
            ApiResource apiResource = await _apiResourceRepository.FindByNameAsync(name);
            if (apiResource == null)
            {
                apiResource = await _apiResourceRepository.InsertAsync(
                    new ApiResource(_guidGenerator.Create(), name, name + " API"), autoSave: true);
            }

            foreach (string claim in claims)
            {
                if (apiResource.FindClaim(claim) == null)
                {
                    apiResource.AddUserClaim(claim);
                }
            }

            return await _apiResourceRepository.UpdateAsync(apiResource);
        }

        private async Task CreateApiResourcesAsync()
        {
            string[] commonApiUserClaims = new[]
            {
                "email",
                "email_verified",
                "name",
                "phone_number",
                "phone_number_verified",
                "role"
            };

            await CreateApiResourceAsync("MyProduct", commonApiUserClaims);
        }

        private async Task<ApiScope> CreateApiScopeAsync(string name)
        {
            ApiScope apiScope = await _apiScopeRepository.GetByNameAsync(name);
            if (apiScope == null)
            {
                apiScope = await _apiScopeRepository.InsertAsync(
                    new ApiScope(_guidGenerator.Create(), name, name + " API"), autoSave: true);
            }

            return apiScope;
        }

        private async Task CreateApiScopesAsync()
        {
            await CreateApiScopeAsync("MyProduct");
        }

        private async Task<Client> CreateClientAsync(
            string name,
            IEnumerable<string> scopes,
            IEnumerable<string> grantTypes,
            string secret = null,
            string redirectUri = null,
            string postLogoutRedirectUri = null,
            string frontChannelLogoutUri = null,
            bool requireClientSecret = true,
            bool requirePkce = false,
            IEnumerable<string> permissions = null,
            IEnumerable<string> corsOrigins = null)
        {
            Client client = await _clientRepository.FindByClientIdAsync(name);
            if (client == null)
            {
                client = await _clientRepository.InsertAsync(
                    new Client(_guidGenerator.Create(), name)
                    {
                        ClientName = name,
                        ProtocolType = "oidc",
                        Description = name,
                        AlwaysIncludeUserClaimsInIdToken = true,
                        AllowOfflineAccess = true,
                        AbsoluteRefreshTokenLifetime = 31536000, //365 days
                        AccessTokenLifetime = 31536000, //365 days
                        AuthorizationCodeLifetime = 300,
                        IdentityTokenLifetime = 300,
                        RequireConsent = false,
                        FrontChannelLogoutUri = frontChannelLogoutUri,
                        RequireClientSecret = requireClientSecret,
                        RequirePkce = requirePkce
                    }, autoSave: true);
            }

            foreach (string scope in scopes)
            {
                if (client.FindScope(scope) == null)
                {
                    client.AddScope(scope);
                }
            }

            foreach (string grantType in grantTypes)
            {
                if (client.FindGrantType(grantType) == null)
                {
                    client.AddGrantType(grantType);
                }
            }

            if (!secret.IsNullOrEmpty())
            {
                if (client.FindSecret(secret) == null)
                {
                    client.AddSecret(secret);
                }
            }

            if (redirectUri != null)
            {
                if (client.FindRedirectUri(redirectUri) == null)
                {
                    client.AddRedirectUri(redirectUri);
                }
            }

            if (postLogoutRedirectUri != null)
            {
                if (client.FindPostLogoutRedirectUri(postLogoutRedirectUri) == null)
                {
                    client.AddPostLogoutRedirectUri(postLogoutRedirectUri);
                }
            }

            if (permissions != null)
            {
                await _permissionDataSeeder.SeedAsync(
                    ClientPermissionValueProvider.ProviderName, name, permissions, null);
            }

            if (corsOrigins != null)
            {
                foreach (string origin in corsOrigins)
                {
                    if (!origin.IsNullOrWhiteSpace() && (client.FindCorsOrigin(origin) == null))
                    {
                        client.AddCorsOrigin(origin);
                    }
                }
            }

            return await _clientRepository.UpdateAsync(client);
        }

        private async Task CreateClientsAsync()
        {
            string[] commonScopes = new[] { "email", "openid", "profile", "role", "phone", "address", "MyProduct" };

            IConfigurationSection configurationSection = _configuration.GetSection("IdentityServer:Clients");

            // Console Test / Angular Client
            string consoleAndAngularClientId = configurationSection["MyProduct_App:ClientId"];
            if (!consoleAndAngularClientId.IsNullOrWhiteSpace())
            {
                string webClientRootUrl = configurationSection["MyProduct_App:RootUrl"]?.TrimEnd('/');

                await CreateClientAsync(
                    name: consoleAndAngularClientId, scopes: commonScopes,
                    grantTypes: new[] { "password", "client_credentials", "authorization_code" },
                    secret: (configurationSection["MyProduct_App:ClientSecret"] ?? "1q2w3e*").Sha256(),
                    requireClientSecret: false, redirectUri: webClientRootUrl, postLogoutRedirectUri: webClientRootUrl,
                    corsOrigins: new[] { webClientRootUrl.RemovePostFix("/") });
            }

            // Postman Client
            string postmanClientId = configurationSection["MyProduct_Postman:ClientId"];
            if (!postmanClientId.IsNullOrWhiteSpace())
            {
                string postmanRootUrl = configurationSection["MyProduct_Postman:RootUrl"].TrimEnd('/');

                await CreateClientAsync(
                    name: postmanClientId, scopes: commonScopes, grantTypes: new[] { "authorization_code" },
                    secret: configurationSection["MyProduct_Postman:ClientSecret"]?.Sha256(),
                    requireClientSecret: false, redirectUri: $"https://oauth.pstmn.io/v1/callback",
                    corsOrigins: new[] { postmanRootUrl.RemovePostFix("/") });
            }

            // Swagger Client
            string swaggerClientId = configurationSection["MyProduct_Swagger:ClientId"];
            if (!swaggerClientId.IsNullOrWhiteSpace())
            {
                string swaggerRootUrl = configurationSection["MyProduct_Swagger:RootUrl"].TrimEnd('/');

                await CreateClientAsync(
                    name: swaggerClientId, scopes: commonScopes, grantTypes: new[] { "authorization_code" },
                    secret: configurationSection["MyProduct_Swagger:ClientSecret"]?.Sha256(),
                    requireClientSecret: false, redirectUri: $"{swaggerRootUrl}/swagger/oauth2-redirect.html",
                    corsOrigins: new[] { swaggerRootUrl.RemovePostFix("/") });
            }
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            using (_currentTenant.Change(context?.TenantId))
            {
                await _identityResourceDataSeeder.CreateStandardResourcesAsync();
                await CreateApiResourcesAsync();
                await CreateApiScopesAsync();
                await CreateClientsAsync();
            }
        }
    }
}
