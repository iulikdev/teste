using System.Net.Http.Headers;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Identity.Client;

namespace TeamsCallCenter.Api.Bot.Services;

public class AuthenticationProvider : IRequestAuthenticationProvider
{
    private readonly string _appId;
    private readonly string _appSecret;
    private readonly IGraphLogger _logger;
    private readonly IConfidentialClientApplication _clientApp;

    public AuthenticationProvider(string appId, string appSecret, IGraphLogger logger)
    {
        _appId = appId;
        _appSecret = appSecret;
        _logger = logger;

        _clientApp = ConfidentialClientApplicationBuilder
            .Create(_appId)
            .WithClientSecret(_appSecret)
            .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdMultipleOrgs)
            .Build();
    }

    public async Task AuthenticateOutboundRequestAsync(HttpRequestMessage request, string tenant)
    {
        const string resource = "https://graph.microsoft.com/.default";
        var scopes = new[] { resource };

        try
        {
            var result = await _clientApp
                .AcquireTokenForClient(scopes)
                .WithAuthority($"https://login.microsoftonline.com/{tenant}")
                .ExecuteAsync()
                .ConfigureAwait(false);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to acquire token");
            throw;
        }
    }

    public async Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequestMessage request)
    {
        var token = request.Headers.Authorization?.Parameter;

        if (string.IsNullOrEmpty(token))
        {
            return new RequestValidationResult { IsValid = false };
        }

        // In production, validate the token properly
        // For now, we trust the token if it exists
        return new RequestValidationResult
        {
            IsValid = true,
            TenantId = "common"
        };
    }
}
