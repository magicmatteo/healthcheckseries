using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck;

public class CustomHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    private IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        using var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://pokeapi.co/api/v2/pokemon", cancellationToken);
            
        return response.IsSuccessStatusCode ? HealthCheckResult.Healthy("PokeAPI is healthy") 
            : HealthCheckResult.Unhealthy("PokeAPI is unhealthy");
    }
}