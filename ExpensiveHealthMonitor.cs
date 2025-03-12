using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck;

public class ExpensiveHealthMonitor : IHealthCheck, IHostedService, IDisposable
{
    private Timer _timer;
    public bool Healthy { get; private set; } = true;
    private ILogger<ExpensiveHealthMonitor> _logger;
    private IHttpClientFactory _httpClientFactory;
    public ExpensiveHealthMonitor(IHttpClientFactory httpClientFactory, ILogger<ExpensiveHealthMonitor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CheckDependencies, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    private async void CheckDependencies(object state)
    {
        // Heavy checks go here (async calls, etc.)
        Healthy = await ProbeDependencies();
    }
    
    private async Task<bool> ProbeDependencies()
    {
        _logger.LogDebug("Running PokeApiHealthCheck in the background...");
        Console.WriteLine("Running PokeApiHealthCheck in the background...");
        var httpClient = _httpClientFactory.CreateClient(); 
        var response = await httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon");

        return response.IsSuccessStatusCode;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        _logger.LogDebug("Health check endpoint called");
        return Task.FromResult(Healthy ? HealthCheckResult.Healthy("Healthy") : HealthCheckResult.Unhealthy("Unhealthy"));
    }
}