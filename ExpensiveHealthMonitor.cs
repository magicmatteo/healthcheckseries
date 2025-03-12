using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck;

public class ExpensiveHealthMonitor : IHealthCheck, IHostedService, IDisposable
{
    private Timer _timer;
    public bool Healthy { get; private set; } = true;
    private IHttpClientFactory _httpClientFactory;
    
    public ExpensiveHealthMonitor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CheckDependencies, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    private async void CheckDependencies(object state)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        // Heavy checks go here (async calls, etc.)
        // We could have as many as we want here to ultimately determine health
        Healthy = await ProbeSlowPokeApi();
        watch.Stop();
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Ran PokeApiHealthCheck in the background. Took {watch.ElapsedMilliseconds} ms.");
    }
    
    private async Task<bool> ProbeSlowPokeApi()
    {
        var httpClient = _httpClientFactory.CreateClient();
        
        // Simulate some delay in the request
        await Task.Delay(700);
        
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
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Health Endpoint called!");
        return Task.FromResult(Healthy ? HealthCheckResult.Healthy("Healthy") : HealthCheckResult.Unhealthy("Unhealthy"));
    }
}