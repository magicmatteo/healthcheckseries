using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace HealthCheck;

public class ExpensiveHealthMonitor(IHttpClientFactory httpClientFactory) : IHealthCheck, IHostedService, IDisposable
{
    private Timer _timer;
    private long _checkMs;
    public bool Healthy { get; private set; } = true;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CheckDependencies, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
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

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Health Endpoint called!");
        return Task.FromResult(Healthy
            ? HealthCheckResult.Healthy($"ExpensiveDependency is Healthy - last check took {_checkMs}ms.")
            : HealthCheckResult.Unhealthy("ExpensiveDependency is Unhealthy"));
    }
    
    private async void CheckDependencies(object state)
    {
        var watch = Stopwatch.StartNew();
        // Heavy checks go here (async calls, etc.)
        // We could have as many as we want here to ultimately determine health
        Healthy = await ProbeSlowPokeApi();
        watch.Stop();
        _checkMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Ran PokeApiHealthCheck in the background. " + 
                          $"Took {_checkMs} ms.");
    }
    
    private async Task<bool> ProbeSlowPokeApi()
    {
        var httpClient = httpClientFactory.CreateClient();
        
        // Simulate some delay in the request
        await Task.Delay(700);
        
        var response = await httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/slowpoke");
        return response.IsSuccessStatusCode;
    }
}